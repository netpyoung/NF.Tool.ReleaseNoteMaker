using NF.Tool.PatchNoteMaker.CLI.Impl;

namespace NF.Tool.PatchNoteMaker.Tests;

[TestClass]
public sealed class Test1
{
    [TestMethod]
    [DataRow("123.feature", new string[] { "feature" }, "123", "feature", 0)]
    [DataRow("123.feature.1", new string[] { "feature" }, "123", "feature", 1)]
    [DataRow("123.feature.1.ext", new string[] { "feature" }, "123", "feature", 1)]
    [DataRow("123.feature.ext", new string[] { "feature" }, "123", "feature", 0)]
    [DataRow("baz.feature.ext", new string[] { "feature" }, "baz", "feature", 0)]
    [DataRow("baz.1.2.feature", new string[] { "feature" }, "baz.1.2", "feature", 0)]
    [DataRow("baz.1.2.feature.3", new string[] { "feature" }, "baz.1.2", "feature", 3)]
    [DataRow("  007.feature", new string[] { "feature" }, "7", "feature", 0)]
    [DataRow("  007.feature.3", new string[] { "feature" }, "7", "feature", 3)]
    [DataRow("+orphan.feature", new string[] { "feature" }, "+orphan", "feature", 0)]
    [DataRow("+123_orphan.feature", new string[] { "feature" }, "+123_orphan", "feature", 0)]
    [DataRow("+orphan_123.feature", new string[] { "feature" }, "+orphan_123", "feature", 0)]
    [DataRow("+12.3_orphan.feature", new string[] { "feature" }, "+12.3_orphan", "feature", 0)]
    [DataRow("+orphan_12.3.feature", new string[] { "feature" }, "+orphan_12.3", "feature", 0)]
    [DataRow("+123.feature", new string[] { "feature" }, "+123", "feature", 0)]
    public void TestMethod1(string baseName, string[] categorySeq, string x, string y, int z)
    {
        FragmentBasename? actual = FragmentFinder.ParseNewFragmentBasenameOrNull(baseName, categorySeq);
        FragmentBasename expected = new FragmentBasename(x, y, z);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow("README.ext", new string[] { "feature" })]
    [DataRow("README", new string[] { "feature" })]
    [DataRow("baz.1.2.notfeature", new string[] { "feature" })]
    public void TestMethod1(string baseName, string[] categorySeq)
    {
        FragmentBasename? actual = FragmentFinder.ParseNewFragmentBasenameOrNull(baseName, categorySeq);
        Assert.IsNull(actual);
    }
}
