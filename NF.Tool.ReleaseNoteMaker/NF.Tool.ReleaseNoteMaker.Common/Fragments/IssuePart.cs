using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace NF.Tool.ReleaseNoteMaker.Common.Fragments
{
    public sealed record class IssuePart(bool IsDigit, bool HasDigit, string NonDigitPart, int Number) : IComparable<IssuePart>
    {
        public static IssuePart IssueKey(string issue)
        {
            if (string.IsNullOrEmpty(issue))
            {
                return new IssuePart(false, false, string.Empty, -1);
            }

            if (issue.All(char.IsDigit))
            {
                return new IssuePart(true, true, string.Empty, int.Parse(issue));
            }

            Match match = Regex.Match(issue, @"\d+");
            if (!match.Success)
            {
                return new IssuePart(false, false, issue, -1);
            }

            string nonDigitPart = string.Concat(issue.AsSpan(0, match.Index), issue.AsSpan(match.Index + match.Length));
            return new IssuePart(false, true, nonDigitPart, int.Parse(match.Value));
        }

        public int CompareTo(IssuePart? other)
        {
            if (other is null)
            {
                return 1;
            }

            if (IsDigit != other.IsDigit)
            {
                return IsDigit.CompareTo(other.IsDigit);
            }

            if (HasDigit != other.HasDigit)
            {
                return HasDigit.CompareTo(other.HasDigit);
            }

            int nonDigitComparison = string.Compare(NonDigitPart, other.NonDigitPart, StringComparison.Ordinal);
            if (nonDigitComparison != 0)
            {
                return nonDigitComparison;
            }

            return Number.CompareTo(other.Number);
        }

        public static bool operator <([NotNull] IssuePart a, IssuePart b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >([NotNull] IssuePart a, IssuePart b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator <=([NotNull] IssuePart a, IssuePart b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator >=([NotNull] IssuePart a, IssuePart b)
        {
            return a.CompareTo(b) >= 0;
        }
    }
}
