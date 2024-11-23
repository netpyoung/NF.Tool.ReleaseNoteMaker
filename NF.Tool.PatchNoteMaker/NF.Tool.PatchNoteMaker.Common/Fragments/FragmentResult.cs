using System.Collections.Generic;

namespace NF.Tool.PatchNoteMaker.Common.Fragments
{
    public sealed class FragmentResult
    {
        public required List<FragmentContent> FragmentContents { get; init; }
        public required List<FragmentFile> FragmentFiles { get; init; }

        public static FragmentResult Default()
        {
            return new FragmentResult
            {
                FragmentContents = new List<FragmentContent>(),
                FragmentFiles = new List<FragmentFile>()
            };
        }
    }

    public sealed record class FragmentContent(string SectionName, List<SectionFragment> SectionFragments);

    public sealed record class FragmentFile(string FileName, string Category);

    public sealed record class SectionFragment(FragmentBasename FragmentBasename, string Data);

    public sealed record class FragmentBasename
    {
        // example: "release-2.0.1.doc.10"
        // issue: release-2.0.1
        // category: doc
        // retryCount: 10
        public string Issue { get; set; }
        public string Category { get; init; }
        public int RetryCount { get; set; }

        public FragmentBasename(string issue, string category, int retryCount)
        {
            Issue = issue;
            Category = category;
            RetryCount = retryCount;
        }
    }
}