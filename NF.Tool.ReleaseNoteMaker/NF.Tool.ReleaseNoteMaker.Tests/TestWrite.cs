using NF.Tool.ReleaseNoteMaker.CLI;
using NF.Tool.ReleaseNoteMaker.CLI.Commands;
using NF.Tool.ReleaseNoteMaker.Common.Config;
using NF.Tool.ReleaseNoteMaker.Common.Fragments;
using NF.Tool.ReleaseNoteMaker.Common.Template;
using Spectre.Console;
using Spectre.Console.Testing;

namespace NF.Tool.ReleaseNoteMaker.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class TestWrite
    {
        [TestInitialize()]
        public void Init()
        {
            File.Delete("ChangeLog.md");
        }

        [TestMethod]
        [DeploymentItem("Template.tt")]
        [DeploymentItem("ReleaseNote.config.toml")]
        public async Task TestAppendAtTop()
        {
            List<FragmentContent> fragments = new List<FragmentContent>
            {
                new FragmentContent("", new FragmentBasename("142", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("1", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("4", "feature", 0), "Stuff!"),
                new FragmentContent("", new FragmentBasename("4", "feature", 1), "Second Stuff!"),
                new FragmentContent("", new FragmentBasename("2", "feature", 0), "Foo added."),
                new FragmentContent("", new FragmentBasename("72", "feature", 0), "Foo added."),
                new FragmentContent("Names", FragmentBasename.Empty, string.Empty),
                new FragmentContent("Web", new FragmentBasename("3", "bugfix", 0), "Web fixed."),
            };
            List<ReleaseNoteType> definitions = new List<ReleaseNoteType>
            {
                new ReleaseNoteType{ DisplayName= "Features", Category= "feature", IsShowContent=true},
                new ReleaseNoteType{ DisplayName= "Bugfixes", Category= "bugfix", IsShowContent=true},
                new ReleaseNoteType{ DisplayName= "Misc", Category= "misc", IsShowContent=true},
            };

            ReleaseNoteConfig config = new ReleaseNoteConfig();
            config.Maker.IsAllBullets = true;
            config.Maker.IsWrap = true;
            config.Maker.EndOfLine = ReleaseNoteConfigMaker.E_END_OF_LINE.LF;
            config.Types.AddRange(definitions);

            List<FragmentContent> splitted = FragmentFinder.SplitFragments(fragments, config);

            string templatePath = "Template.tt";
            VersionData versionData = new VersionData("MyProject", "1.0", "never");
            (Exception? renderExOrNull, string text) = await TemplateRenderer.RenderFragments(templatePath, config, versionData, splitted);
            Assert.IsNull(renderExOrNull);

            await File.WriteAllTextAsync("ChangeLog.md", "Old text.\n");

            Exception? ex = await Command_Build.AppendToNewsFile(config, "release notes start", text, "ChangeLog.md");
            Assert.IsNull(ex);

            string expected = """
# MyProject 1.0 (never)

### Features

- Foo added. (#2, #72)
- Stuff! (#4)
- Second Stuff! (#4)

### Misc

- #1, #142


## Names

No significant changes.


## Web

### Bugfixes

- Web fixed. (#3)


Old text.

""".Replace("\r\n", "\n");

            string actual = await File.ReadAllTextAsync("ChangeLog.md");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("Template.tt")]
        [DeploymentItem("ReleaseNote.config.toml")]
        public async Task TestAppendAtTopWithHint()
        {
            List<FragmentContent> fragments = new List<FragmentContent>
            {
                new FragmentContent("", new FragmentBasename("142", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("1", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("4", "feature", 0), "Stuff!"),
                new FragmentContent("", new FragmentBasename("2", "feature", 0), "Foo added."),
                new FragmentContent("", new FragmentBasename("72", "feature", 0), "Foo added."),
                new FragmentContent("", new FragmentBasename("99", "feature", 0), string.Concat(Enumerable.Repeat("Foo! ", 100))),
                new FragmentContent("Names", FragmentBasename.Empty, string.Empty),
                new FragmentContent("Web", new FragmentBasename("3", "bugfix", 0), "Web fixed."),
            };
            List<ReleaseNoteType> definitions = new List<ReleaseNoteType>
            {
                new ReleaseNoteType{ DisplayName= "Features", Category= "feature", IsShowContent=true},
                new ReleaseNoteType{ DisplayName= "Bugfixes", Category= "bugfix", IsShowContent=true},
                new ReleaseNoteType{ DisplayName= "Misc", Category= "misc", IsShowContent=true},
            };

            string topLine = string.Empty;
            string startString = ".. towncrier release notes start\n";

            ReleaseNoteConfig config = new ReleaseNoteConfig();
            config.Maker.IsAllBullets = true;
            config.Maker.IsWrap = true;
            config.Maker.StartString = startString;
            config.Maker.EndOfLine = ReleaseNoteConfigMaker.E_END_OF_LINE.LF;
            config.Types.AddRange(definitions);

            List<FragmentContent> splitted = FragmentFinder.SplitFragments(fragments, config);

            string templatePath = "Template.tt";
            VersionData versionData = new VersionData("MyProject", "1.0", "never");
            (Exception? renderExOrNull, string text) = await TemplateRenderer.RenderFragments(templatePath, config, versionData, splitted);
            Assert.IsNull(renderExOrNull);

            await File.WriteAllTextAsync("ChangeLog.md", """
Hello there! Here is some info.

.. towncrier release notes start
Old text.
""");

            Exception? ex = await Command_Build.AppendToNewsFile(config, topLine, text, "ChangeLog.md");
            Assert.IsNull(ex);

            string expected = """
Hello there! Here is some info.

.. towncrier release notes start

# MyProject 1.0 (never)

### Features

- Foo added. (#2, #72)
- Stuff! (#4)
- Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo!
  Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo!
  Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo!
  Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo!
  Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo!
  Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo!
  Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! Foo! (#99)

### Misc

- #1, #142


## Names

No significant changes.


## Web

### Bugfixes

- Web fixed. (#3)


Old text.
""".Replace("\r\n", "\n");

            string actual = await File.ReadAllTextAsync("ChangeLog.md");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("Template.tt")]
        [DeploymentItem("ReleaseNote.config.toml")]
        public async Task TestMultipleFileNoStartString()
        {
            List<FragmentContent> fragments = new List<FragmentContent>
            {
            };
            List<ReleaseNoteType> definitions = new List<ReleaseNoteType>
            {
            };

            string topLine = string.Empty;
            string startString = string.Empty;

            ReleaseNoteConfig config = new ReleaseNoteConfig();
            config.Maker.IsAllBullets = true;
            config.Maker.IsWrap = true;
            config.Maker.StartString = startString;
            config.Maker.EndOfLine = ReleaseNoteConfigMaker.E_END_OF_LINE.LF;
            config.Types.AddRange(definitions);

            List<FragmentContent> splitted = FragmentFinder.SplitFragments(fragments, config);

            string templatePath = "Template.tt";
            VersionData versionData = new VersionData("MyProject", "1.0", "never");
            (Exception? renderExOrNull, string text) = await TemplateRenderer.RenderFragments(templatePath, config, versionData, splitted);
            Assert.IsNull(renderExOrNull);

            Exception? ex = await Command_Build.AppendToNewsFile(config, topLine, text, "ChangeLog.md");
            Assert.IsNull(ex);

            string expected = """
# MyProject 1.0 (never)

""".Replace("\r\n", "\n");

            string actual = await File.ReadAllTextAsync("ChangeLog.md");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("Template.tt")]
        [DeploymentItem("ReleaseNote.config.toml")]
        [DeploymentItem("SampleData/Case001/123.feature", "ChangeLog.d/")]
        public async Task TestWithTitleFormatDuplicateVersionRaise()
        {
            Assert.IsTrue(File.Exists("ChangeLog.d/123.feature"));

            File.WriteAllText("ReleaseNote.config.toml", """
[ReleaseNote.Maker]
Directory = "ChangeLog.d"
OutputFileName = "{0}-notes.md"
TemplateFilePath = "Template.tt"
TitleFormat = "{0} {1} ({2})"
""");

            string[] args = [
                "build",
                "--version", "7.8.9",
                "--name", "foo",
                "--date", "01-01-2001",
                "--yes",
            ];

            int result = await Program.Main(args);
            Assert.AreEqual(0, result);
            Console.Write(File.ReadAllText("{0}-notes.md"));
            Assert.IsTrue(File.Exists("ChangeLog.d/123.feature"));



            TestConsole c = new TestConsole();
            AnsiConsole.Console = c;
            result = await Program.Main(args);
            Assert.AreEqual(1, result);

            string expected = "already produced newsfiles for this version";
            Assert.IsTrue(c.Output.Contains(expected));
        }
    }
}