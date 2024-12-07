using NF.Tool.ReleaseNoteMaker.CLI;
using NF.Tool.ReleaseNoteMaker.CLI.Impl;
using Spectre.Console;
using Spectre.Console.Testing;

namespace NF.Tool.ReleaseNoteMaker.Tests
{
    [TestClass]
    [DoNotParallelize]
    public sealed class TestCheck
    {
        public required TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            string testName = TestContext.TestName!;
            string testDirectory = Path.Combine(TestContext.DeploymentDirectory!, testName);
            Directory.CreateDirectory(testDirectory);
            Directory.CreateDirectory($"{testDirectory}/ChangeLog.d");
            File.Copy("ChangeLog.d/Template.tt", $"{testDirectory}/ChangeLog.d/Template.tt");
            File.Copy("ReleaseNote.config.toml", $"{testDirectory}/ReleaseNote.config.toml");
            Directory.SetCurrentDirectory(testDirectory);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Directory.SetCurrentDirectory(TestContext.DeploymentDirectory!);
        }

        void Commit(string commitMessage)
        {
            Cmd.Call("git", "add .");
            Cmd.Call("git", $"commit -m \"{commitMessage}\"");
        }

        void CreateProject(string initialBranch)
        {
            Cmd.Call("git", $"init --initial-branch={initialBranch}");
            Cmd.Call("git", "config user.name user");
            Cmd.Call("git", "config user.email user@example.com");
            Commit("Initial Commit");
            Cmd.Call("git", "checkout -b otherbranch");
        }

        [TestMethod]
        [DeploymentItem("Template.tt", "ChangeLog.d/")]
        [DeploymentItem("ReleaseNote.config.toml")]
        public async Task TestGitFails()
        {
            CreateProject(initialBranch: "main");

            TestConsole c = new TestConsole();
            AnsiConsole.Console = c;
            string[] args = ["check", "--compare-with", "hblaugh"];
            int result = await Program.Main(args);
            Assert.AreEqual(1, result);
            Assert.IsTrue(c.Output.Contains("git produced output while failing"));
            Assert.IsTrue(c.Output.Contains("hblaugh"));
        }

        [TestMethod]
        [DeploymentItem("Template.tt", "ChangeLog.d/")]
        [DeploymentItem("ReleaseNote.config.toml")]
        public async Task TestNoChangesMade()
        {
            CreateProject(initialBranch: "main");

            TestConsole c = new TestConsole();
            AnsiConsole.Console = c;
            string[] args = ["check", "--compare-with", "main"];
            int result = await Program.Main(args);
            Assert.AreEqual(0, result);
            Assert.AreEqual("On main branch, or no diffs, so no newsfragment required.\n", c.Output);
        }

        [TestMethod]
        [DeploymentItem("Template.tt", "ChangeLog.d/")]
        [DeploymentItem("ReleaseNote.config.toml")]
        public async Task TestNoChangesMadeConfigPath()
        {
            File.Move("ReleaseNote.config.toml", "not-pyproject.toml");
            CreateProject(initialBranch: "main");

            TestConsole c = new TestConsole();
            AnsiConsole.Console = c;
            string[] args = ["check", "--compare-with", "main", "--config", "not-pyproject.toml"];
            int result = await Program.Main(args);
            Assert.AreEqual(0, result);
            Assert.AreEqual("On main branch, or no diffs, so no newsfragment required.\n", c.Output);
        }

        [TestMethod]
        [DeploymentItem("Template.tt", "ChangeLog.d/")]
        [DeploymentItem("ReleaseNote.config.toml")]
        public async Task TestFragmentExists()
        {
            CreateProject(initialBranch: "main");

            File.WriteAllText("helloworld.txt", "hello world");
            Commit("add a file");

            Directory.CreateDirectory("ChangeLog.d");
            string fpath = Path.GetFullPath("ChangeLog.d/1234.feature");
            File.WriteAllText(fpath, "Adds gravity back");
            Commit("add a newsfragment");

            TestConsole c = new TestConsole();
            AnsiConsole.Console = c;
            AnsiConsole.Profile.Width = 255;
            string[] args = ["check", "--compare-with", "main"];
            int result = await Program.Main(args);
            Assert.AreEqual(0, result);
            Assert.IsTrue(c.Output.EndsWith($"Found:\n    1. {fpath}\n"));
        }

        // TODO(pyoung): from test_fragment_exists_but_not_in_check
    }
}
