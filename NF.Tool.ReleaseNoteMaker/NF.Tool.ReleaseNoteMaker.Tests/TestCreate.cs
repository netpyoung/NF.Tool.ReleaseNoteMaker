using NF.Tool.ReleaseNoteMaker.CLI;

namespace NF.Tool.ReleaseNoteMaker.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class TestCreate
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

        [TestMethod]
        [DeploymentItem("Template.tt", "ChangeLog.d/")]
        [DeploymentItem("ReleaseNote.config.toml")]
        public async Task TestSplit()
        {
            string[] arr = ["create", "123.feature.md"];
            int result = await Program.Main(arr);
            Assert.AreEqual(0, result);
            Assert.IsTrue(File.Exists("ChangeLog.d/123.feature.md"));
        }
    }
}
