using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace NF.Tool.PatchNoteMaker.Common.Fragments
{
    public sealed record class SplitFragment(string SectionDisplayName, string Category, string TrimedData, List<string> RenderedIssues);

    public sealed record class IssueParts(bool IsDigit, bool HasDigit, string NonDigitPart, int Number) : IComparable<IssueParts>
    {
        public static IssueParts IssueKey(string issue)
        {
            if (string.IsNullOrEmpty(issue))
            {
                return new IssueParts(false, false, string.Empty, -1);
            }

            if (issue.All(char.IsDigit))
            {
                return new IssueParts(true, true, string.Empty, int.Parse(issue));
            }

            Match match = Regex.Match(issue, @"\d+");
            if (!match.Success)
            {
                return new IssueParts(false, false, issue, -1);
            }

            string nonDigitPart = string.Concat(issue.AsSpan(0, match.Index), issue.AsSpan(match.Index + match.Length));
            return new IssueParts(false, true, nonDigitPart, int.Parse(match.Value));
        }

        public int CompareTo(IssueParts? other)
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

        public static bool operator <([NotNull] IssueParts a, IssueParts b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >([NotNull] IssueParts a, IssueParts b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator <=([NotNull] IssueParts a, IssueParts b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator >=([NotNull] IssueParts a, IssueParts b)
        {
            return a.CompareTo(b) >= 0;
        }
    }

    public sealed class Issue
    {
        public static string RenderIssue(string issueFormat, string issue)
        {
            if (!string.IsNullOrEmpty(issueFormat))
            {
                return string.Format(issueFormat, issue);
            }

            if (int.TryParse(issue, out int issueNumber))
            {
                return $"#{issueNumber}";
            }
            return issue;
        }
        public static string AppendNewlinesIfTrailingCodeBlock(string text)
        {
            string indentedText = @"  [ \t]+[^\n]*";
            string emptyOrIndentedTextLines = $"(({indentedText})?\n)*";
            string regex = @"::\n\n" + emptyOrIndentedTextLines + indentedText + "$";
            bool isTrailingCodeBlock = Regex.IsMatch(text, regex);
            if (isTrailingCodeBlock)
            {
                return $"{text}\n\n";
            }

            return text;
        }
    }

    public sealed class Fragment : IEnumerable<SplitFragment>
    {
        private readonly Dictionary<(string sectionDisplayName, string category, string content), SplitFragment> _dic2 = [];

        internal void Add(string sectionDisplayName, string category, string content, string issue)
        {
            (string sectionDisplayName, string category, string content) key = (sectionDisplayName, category, content);
            if (_dic2.TryGetValue(key, out SplitFragment? sf))
            {
                sf.RenderedIssues.Add(issue);
            }
            else
            {
                _dic2.Add(key, new SplitFragment(sectionDisplayName, category, content, [issue]));
            }
        }

        public IEnumerator<SplitFragment> GetEnumerator()
        {
            return _dic2.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dic2.Values.GetEnumerator();
        }
    }
}
