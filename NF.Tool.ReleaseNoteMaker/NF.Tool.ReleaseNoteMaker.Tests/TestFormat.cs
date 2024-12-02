using NF.Tool.ReleaseNoteMaker.Common.Config;
using NF.Tool.ReleaseNoteMaker.Common.Fragments;
using NF.Tool.ReleaseNoteMaker.Common.Template;

namespace NF.Tool.ReleaseNoteMaker.Tests
{
    [TestClass]
    public class TestFormat
    {
        [TestMethod]
        public void TestSplit()
        {
            List<FragmentContent> a = new List<FragmentContent>
            {
                new FragmentContent("", new FragmentBasename("1", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("baz", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("2", "feature", 0), "Foo added."),
                new FragmentContent("", new FragmentBasename("5", "feature", 0), "Foo added.    \n"),
                new FragmentContent("", new FragmentBasename("6", "bugfix", 0), "Foo added."),
                new FragmentContent("Web", new FragmentBasename("3", "bugfix", 0), "Web fixed.    "),
                new FragmentContent("Web", new FragmentBasename("4", "feature", 0), "Foo added."),
            };

            List<FragmentContent> b = new List<FragmentContent>
            {
                new FragmentContent("", new FragmentBasename("1", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("baz", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("2", "feature", 0), "Foo added."),
                new FragmentContent("", new FragmentBasename("5", "feature", 0), "Foo added."),
                new FragmentContent("", new FragmentBasename("6", "bugfix", 0), "Foo added."),
                new FragmentContent("Web", new FragmentBasename("3", "bugfix", 0), "Web fixed."),
                new FragmentContent("Web", new FragmentBasename("4", "feature", 0), "Foo added."),
            };

            List<ReleaseNoteType> definitions = new List<ReleaseNoteType>
            {
                new ReleaseNoteType{ Category= "feature", IsShowContent=true},
                new ReleaseNoteType{ Category= "bugfix", IsShowContent=true},
                new ReleaseNoteType{ Category= "misc", IsShowContent=false},
            };
            ReleaseNoteConfig config = new ReleaseNoteConfig();
            config.Maker.IsAllBullets = true;
            config.Types.AddRange(definitions);
            List<FragmentContent> y = FragmentFinder.SplitFragments(a, config);
            CollectionAssert.AreEqual(b, y);
        }

        [TestMethod]
        [DataRow(new string[] { "2", "#11", "#3", "gh-10", "gh-4", "omega", "alpha" }, new string[] { "alpha", "omega", "#3", "#11", "gh-4", "gh-10", "2" })]
        [DataRow(new string[] { "2", "72", "9" }, new string[] { "2", "9", "72" })]

        [DataRow(new string[] { "baz", "2", "9", "72", "3", "4" }, new string[] { "baz", "2", "3", "4", "9", "72" })]
        public void TestIssueKey(string[] arr, string[] expected)
        {
            string[] actual = arr.OrderBy(IssueParts.IssueKey).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("Template.tt", "ChangeLog.d/")]
        public async Task TestBasic()
        {
            List<FragmentContent> a = new List<FragmentContent>
            {
                new FragmentContent("", new FragmentBasename("142", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("1", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("9", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("bar", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("4", "feature", 0), "Stuff!"),
                new FragmentContent("", new FragmentBasename("2", "feature", 0), "Foo added."),
                new FragmentContent("", new FragmentBasename("72", "feature", 0), "Foo added."),
                new FragmentContent("", new FragmentBasename("9", "feature", 0), "Foo added."),
                new FragmentContent("", new FragmentBasename("3", "feature", 0), "Multi-line\nhere"),
                new FragmentContent("", new FragmentBasename("baz", "feature", 0), "Fun!"),
                new FragmentContent("Names", FragmentBasename.Empty, ""),
                new FragmentContent("Web", new FragmentBasename("3", "bugfix", 0), "Web fixed."),
                new FragmentContent("Web", new FragmentBasename("2", "bugfix", 0), "Multi-line bulleted\n- fix\n- here"),
            };

            List<ReleaseNoteType> definitions = new List<ReleaseNoteType>
            {
                new ReleaseNoteType{ DisplayName= "Features", Category= "feature", IsShowContent=true},
                new ReleaseNoteType{ DisplayName= "Bugfixes", Category = "bugfix", IsShowContent=true},
                new ReleaseNoteType{ DisplayName= "Misc", Category= "misc", IsShowContent=false},
            };
            ReleaseNoteConfig config = new ReleaseNoteConfig();
            config.Maker.IsAllBullets = true;
            config.Maker.IsWrap = true;
            config.Types.AddRange(definitions);
            List<FragmentContent> splitted = FragmentFinder.SplitFragments(a, config);
            string templatePath = "ChangeLog.d/Template.tt";
            VersionData versionData = new VersionData("MyProject", "1.0", "never");
            (Exception? renderExOrNull, string text) = await TemplateRenderer.RenderFragments(templatePath, config, versionData, splitted);
            Assert.IsNull(renderExOrNull);
            string expected = """
# MyProject 1.0 (never)

### Features

- Fun! (baz)
- Foo added. (#2, #9, #72)
- Multi-line
  here (#3)
- Stuff! (#4)

### Misc

- bar, #1, #9, #142


## Names

No significant changes.


## Web

### Bugfixes

- Multi-line bulleted
  - fix
  - here

  (#2)
- Web fixed. (#3)



""".Replace("\r\n", "\n");
            Assert.AreEqual(expected, text);


            string issueFormat = "[{0}]: https://github.com/twisted/towncrier/issues/{0}";
            config.Maker.IssueFormat = issueFormat;
            (renderExOrNull, text) = await TemplateRenderer.RenderFragments(templatePath, config, versionData, splitted);
            Assert.IsNull(renderExOrNull);

            expected = """
# MyProject 1.0 (never)

### Features

- Fun! ([baz])
- Foo added. ([2], [9], [72])
- Multi-line
  here ([3])
- Stuff! ([4])

[baz]: https://github.com/twisted/towncrier/issues/baz
[2]: https://github.com/twisted/towncrier/issues/2
[3]: https://github.com/twisted/towncrier/issues/3
[4]: https://github.com/twisted/towncrier/issues/4
[9]: https://github.com/twisted/towncrier/issues/9
[72]: https://github.com/twisted/towncrier/issues/72

### Misc

- [bar], [1], [9], [142]

[bar]: https://github.com/twisted/towncrier/issues/bar
[1]: https://github.com/twisted/towncrier/issues/1
[9]: https://github.com/twisted/towncrier/issues/9
[142]: https://github.com/twisted/towncrier/issues/142


## Names

No significant changes.


## Web

### Bugfixes

- Multi-line bulleted
  - fix
  - here

  ([2])
- Web fixed. ([3])

[2]: https://github.com/twisted/towncrier/issues/2
[3]: https://github.com/twisted/towncrier/issues/3



""".Replace("\r\n", "\n");
            Assert.AreEqual(expected, text);
        }

        [TestMethod]
        [DeploymentItem("Template.tt", "ChangeLog.d/")]
        public async Task TestIssueFormat()
        {
            string file = "ChangeLog.d/Template.tt";
            Assert.IsTrue(File.Exists(file), "deployment failed: " + file +
                " did not get deployed");
            List<FragmentContent> fragments = new List<FragmentContent>
            {
                new FragmentContent("", new FragmentBasename("142", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("1", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("9", "misc", 0), ""),
                new FragmentContent("", new FragmentBasename("bar", "misc", 0), ""),
            };
            List<ReleaseNoteType> definitions = new List<ReleaseNoteType>
            {
                new ReleaseNoteType{ DisplayName= "Misc", Category= "misc", IsShowContent=false},
            };


            ReleaseNoteConfig config = new ReleaseNoteConfig();
            config.Maker.IsAllBullets = true;
            config.Maker.IsWrap = true;
            config.Maker.IssueFormat = "xx{0}";
            config.Types.AddRange(definitions);

            List<FragmentContent> splitted = FragmentFinder.SplitFragments(fragments, config);

            string templatePath = "ChangeLog.d/Template.tt";
            VersionData versionData = new VersionData("MyProject", "1.0", "never");
            (Exception? renderExOrNull, string text) = await TemplateRenderer.RenderFragments(templatePath, config, versionData, splitted);
            Assert.IsNull(renderExOrNull);
            string expected = """
# MyProject 1.0 (never)

### Misc

- xxbar, xx1, xx9, xx142



""".Replace("\r\n", "\n");
            Assert.AreEqual(expected, text);
        }


        [TestMethod]
        [DeploymentItem("Template.tt", "ChangeLog.d/")]
        public async Task TestLineWrapping()
        {
            List<FragmentContent> fragments = new List<FragmentContent>
            {
                new FragmentContent("", new FragmentBasename("1", "feature", 0), """
                    asdf asdf asdf asdf looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong newsfragment.
                    """),
                new FragmentContent("", new FragmentBasename("2", "feature", 0), $"https://google.com/q=?{new string('-', 100)}"),
                new FragmentContent("", new FragmentBasename("3", "feature", 0), string.Concat(Enumerable.Repeat("a ", 80))),
            };

            List<ReleaseNoteType> definitions = new List<ReleaseNoteType>
            {
                new ReleaseNoteType{ DisplayName= "Features", Category= "feature", IsShowContent=true},
            };


            ReleaseNoteConfig config = new ReleaseNoteConfig();
            config.Maker.IsAllBullets = true;
            config.Maker.IsWrap = true;
            config.Types.AddRange(definitions);

            List<FragmentContent> splitted = FragmentFinder.SplitFragments(fragments, config);

            string templatePath = "ChangeLog.d/Template.tt";
            VersionData versionData = new VersionData("MyProject", "1.0", "never");
            (Exception? renderExOrNull, string text) = await TemplateRenderer.RenderFragments(templatePath, config, versionData, splitted);
            Assert.IsNull(renderExOrNull);

            string expected = """
# MyProject 1.0 (never)

### Features

- asdf asdf asdf asdf
  looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong
  newsfragment. (#1)
-
  https://google.com/q=?----------------------------------------------------------------------------------------------------
  (#2)
- a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a
  a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a
  a a (#3)



""".Replace("\r\n", "\n");
            Assert.AreEqual(expected, text);
        }


        [TestMethod]
        [DeploymentItem("Template.tt", "ChangeLog.d/")]
        public async Task TestLineWrappingDisabled()
        {
            List<FragmentContent> fragments = new List<FragmentContent>
            {
                new FragmentContent("", new FragmentBasename("1", "feature", 0), """
                    asdf asdf asdf asdf looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong newsfragment.
                    """),
                new FragmentContent("", new FragmentBasename("2", "feature", 0), $"https://google.com/q=?{new string('-', 100)}"),
                new FragmentContent("", new FragmentBasename("3", "feature", 0), string.Concat(Enumerable.Repeat("a ", 80))),
            };

            List<ReleaseNoteType> definitions = new List<ReleaseNoteType>
            {
                new ReleaseNoteType{ DisplayName= "Features", Category= "feature", IsShowContent=true},
            };


            ReleaseNoteConfig config = new ReleaseNoteConfig();
            config.Maker.IsAllBullets = true;
            config.Maker.IsWrap = false;
            config.Types.AddRange(definitions);

            List<FragmentContent> splitted = FragmentFinder.SplitFragments(fragments, config);

            string templatePath = "ChangeLog.d/Template.tt";
            VersionData versionData = new VersionData("MyProject", "1.0", "never");
            (Exception? renderExOrNull, string text) = await TemplateRenderer.RenderFragments(templatePath, config, versionData, splitted);
            Assert.IsNull(renderExOrNull);

            string expected = """
# MyProject 1.0 (never)

### Features

- asdf asdf asdf asdf looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong newsfragment. (#1)
- https://google.com/q=?---------------------------------------------------------------------------------------------------- (#2)
- a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a (#3)



""".Replace("\r\n", "\n");
            Assert.AreEqual(expected, text);
        }

        [TestMethod]
        public void TestTextWrap()
        {
            string sample = "- asdf asdf asdf asdf looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong newsfragment. (#1)";

            string expected = """
- asdf asdf asdf asdf
  looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong
  newsfragment. (#1)
""".Replace("\r\n", "\n");

            string actual = TemplateRenderer.TextWrap(sample, 79, "  ");
            Assert.AreEqual(expected, actual);

            sample = "- a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a (#3)";

            expected = """
- a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a
  a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a a
  a a (#3)
""".Replace("\r\n", "\n");

            actual = TemplateRenderer.TextWrap(sample, 79, "  ");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("Template.tt", "ChangeLog.d/")]
        public async Task TestTrailingBlock()
        {
            List<FragmentContent> fragments = new List<FragmentContent>
            {
                new FragmentContent("", new FragmentBasename("1", "feature", 0),
                    "this fragment has a trailing code block::\n\n"
                    + "    def foo(): ...\n\n"
                    + "   \n"
                    + "    def bar(): ..."),
                new FragmentContent("", new FragmentBasename("2", "feature", 0),
                    "this block is not trailing::\n\n"
                    + "    def foo(): ...\n"
                    + "    def bar(): ...\n\n"
                    + "so we can append the issue number directly after this"),
            };

            List<ReleaseNoteType> definitions = new List<ReleaseNoteType>
            {
                new ReleaseNoteType{ DisplayName= "Features", Category= "feature", IsShowContent=true},
            };


            ReleaseNoteConfig config = new ReleaseNoteConfig();
            config.Maker.IsAllBullets = true;
            config.Maker.IsWrap = true;
            config.Types.AddRange(definitions);

            List<FragmentContent> splitted = FragmentFinder.SplitFragments(fragments, config);

            string templatePath = "ChangeLog.d/Template.tt";
            VersionData versionData = new VersionData("MyProject", "1.0", "never");
            (Exception? renderExOrNull, string actual) = await TemplateRenderer.RenderFragments(templatePath, config, versionData, splitted);
            Assert.IsNull(renderExOrNull);

            string expected = """
# MyProject 1.0 (never)

### Features

- this fragment has a trailing code block::

      def foo(): ...


      def bar(): ...

  (#1)
- this block is not trailing::

      def foo(): ...
      def bar(): ...

  so we can append the issue number directly after this (#2)



""".Replace("\r\n", "\n");
            Assert.AreEqual(expected, actual);
        }
    }
}