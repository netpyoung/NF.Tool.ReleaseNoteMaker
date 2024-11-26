using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NF.Tool.PatchNoteMaker.Common.Fragments
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public sealed class FragmentResult : IEquatable<FragmentResult>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
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

        public bool Equals(FragmentResult? other)
        {
            if (other is null)
            {
                return false;
            }

            if (!Enumerable.SequenceEqual(FragmentFiles, other.FragmentFiles))
            {
                return false;
            }

            return Enumerable.SequenceEqual(FragmentContents, other.FragmentContents);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as FragmentResult);
        }
    }

    public sealed record class FragmentContent(string SectionDisplayName, FragmentBasename FragmentBasename, string Data);

    public sealed record class FragmentFile(string FileName, string Category);

    [DebuggerDisplay("{Issue}/{Category}/{RetryCount}")]
    public sealed record class FragmentBasename(string Issue, string Category, int RetryCount)
    {
        // example: "release-2.0.1.doc.10"
        // issue: release-2.0.1
        // category: doc
        // retryCount: 10
    }
}