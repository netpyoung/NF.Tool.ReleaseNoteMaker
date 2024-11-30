using NF.Tool.ReleaseNoteMaker.CLI;
using Spectre.Console;
using Spectre.Console.Testing;

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
            TestConsole c = new TestConsole();
            AnsiConsole.Console = c;

            string[] args = ["build", "--draft", "--date", "01-01-2001", "--version", "1.0.0", "--name", "HelloWorld"];
            int result = await Program.Main(args);
            Assert.AreEqual(0, result);

            string expected = @"* Finding news fragments...
* Loading template...
* Rendering news fragments...
* show draft...
# HelloWorld 1.0.0 (01-01-2001)


## Main

### Features

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



";
            Assert.AreEqual(expected.Replace(Environment.NewLine, "\n"), c.Output);
        }
    }
}
