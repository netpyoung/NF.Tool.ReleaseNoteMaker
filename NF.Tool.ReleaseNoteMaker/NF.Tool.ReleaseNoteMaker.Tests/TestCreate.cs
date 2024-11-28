using NF.Tool.ReleaseNoteMaker.CLI;

namespace NF.Tool.ReleaseNoteMaker.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class TestCreate
    {
        [TestMethod]
        [DeploymentItem("Template.tt")]
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
