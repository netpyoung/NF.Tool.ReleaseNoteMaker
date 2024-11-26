using NF.Tool.PatchNoteMaker.Common.Config;
using NF.Tool.PatchNoteMaker.Common.Fragments;

namespace NF.Tool.PatchNoteMaker.Tests
{
    [TestClass]
    public class FormatterTests
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

            List<PatchNoteType> definitions = new List<PatchNoteType>
            {
                new PatchNoteType{ Category= "feature", IsShowContent=true},
                new PatchNoteType{ Category= "bugfix", IsShowContent=true},
                new PatchNoteType{ Category= "misc", IsShowContent=false},
            };
            PatchNoteConfig config = new PatchNoteConfig();
            config.Maker.IsAllBullets = true;
            config.Types.AddRange(definitions);
            List<FragmentContent> y = FragmentFinder.SplitFragments(a, config);
            CollectionAssert.AreEqual(b, y);
        }
    }
}