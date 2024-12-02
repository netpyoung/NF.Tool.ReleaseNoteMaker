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
            File.Copy("Template.tt", $"{testDirectory}/Template.tt");
            File.Copy("ReleaseNote.config.toml", $"{testDirectory}/ReleaseNote.config.toml");
            Directory.SetCurrentDirectory(testDirectory);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Directory.SetCurrentDirectory(TestContext.DeploymentDirectory!);
        }

        [TestMethod]
        public async Task TestGitFails()
        {
            string branch = "main";
            Cmd.Call("git", $"init --initial-branch={branch}");
            Cmd.Call("git", "config user.name user");
            Cmd.Call("git", "config user.email user@example.com");
            Cmd.Call("git", "add .");
            string message = "Initial Commit";
            Cmd.Call("git", $"commit -m {message}");
            Cmd.Call("git", "checkout -b otherbranch");

            TestConsole c = new TestConsole();
            AnsiConsole.Console = c;
            string[] args = ["check", "--compare-with", "hblaugh"];
            int result = await Program.Main(args);
            Assert.AreEqual(1, result);
            Assert.IsTrue(c.Output.Contains("git produced output while failing"));
            Assert.IsTrue(c.Output.Contains("hblaugh"));
        }
    }
}
