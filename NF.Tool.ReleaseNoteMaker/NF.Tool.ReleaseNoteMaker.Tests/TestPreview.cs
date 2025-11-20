using NF.Tool.ReleaseNoteMaker.CLI;
using Spectre.Console;
using Spectre.Console.Testing;

namespace NF.Tool.ReleaseNoteMaker.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class TestPreview
    {
        public required TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            string testName = TestContext.TestName!;
            string testDirectory = Path.Combine(TestContext.DeploymentDirectory!, testName);
            Directory.CreateDirectory(testDirectory);
            Directory.CreateDirectory($"{testDirectory}/ChangeLog.d");
            File.Copy("ChangeLog.d/Template.t4", $"{testDirectory}/ChangeLog.d/Template.t4");
            File.Copy("ReleaseNote.config.toml", $"{testDirectory}/ReleaseNote.config.toml");
            CopyDirectory("ChangeLog.d", $"{testDirectory}/ChangeLog.d", recursive: true);
            Directory.SetCurrentDirectory(testDirectory);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Directory.SetCurrentDirectory(TestContext.DeploymentDirectory!);
        }

        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, overwrite: true);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        [TestMethod]
        [DeploymentItem("SampleData/Case001", "ChangeLog.d/")]
        [DeploymentItem("Template.t4", "ChangeLog.d/")]
        [DeploymentItem("ReleaseNote.config.toml")]
        public async Task TestCommand()
        {
            TestConsole c = new TestConsole();
            AnsiConsole.Console = c;

            string[] args = ["preview", "--date", "01-01-2001", "--version", "1.0.0", "--name", "HelloWorld"];
            int result = await Program.Main(args);
            Assert.AreEqual(0, result);

            string expected = """
## HelloWorld 1.0.0 (01-01-2001)


### Main

#### Features

- Baz levitation (baz)
- Baz fix levitation (fix-1.2)
- Adds levitation (#123)
- Extends levitation (#124)
- An orphaned feature ending with a dotted number
- An orphaned feature ending with a number
- An orphaned feature starting with a dotted number
- An orphaned feature starting with a number
- Another orphaned feature
- Orphaned feature




""".Replace("\r\n", "\n");
            Assert.AreEqual(expected, c.Output);
        }
    }
}
