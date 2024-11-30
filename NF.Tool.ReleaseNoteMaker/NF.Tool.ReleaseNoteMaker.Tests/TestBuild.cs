using NF.Tool.ReleaseNoteMaker.CLI;

namespace NF.Tool.ReleaseNoteMaker.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class TestBuild
    {

        [TestMethod]
        [DeploymentItem("SampleData/Case001", "ChangeLog.d/")]
        [DeploymentItem("Template.tt")]
        [DeploymentItem("ReleaseNote.config.toml")]
        public async Task TestCommand()
        {
            string[] args = ["build", "--draft", "--date", "01-01-2001", "--version", "1.0.0", "--name", "HelloWorld"];
            int result = await Program.Main(args);
            Assert.AreEqual(0, result);
        }
    }
}
