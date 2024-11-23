using System.Collections.Generic;

namespace NF.Tool.PatchNoteMaker.Common.Fragments
{
    public sealed class Section
    {
        public string Name { get; }
        public int Count => _dic.Count;

        // (category, (text, issuelist))
        private readonly Dictionary<string, Dictionary<string, List<string>>> _dic = new Dictionary<string, Dictionary<string, List<string>>>();

        public Section(string name)
        {
            Name = name;
        }

        public Dictionary<string, Dictionary<string, List<string>>>.KeyCollection Keys => _dic.Keys;

        public Dictionary<string, List<string>> this[string category]
        {
            get => _dic[category];
        }

        public void AddIssue(string category, string content, string issue)
        {
            if (!_dic.TryGetValue(category, out Dictionary<string, List<string>>? texts))
            {
                texts = new Dictionary<string, List<string>>();
                _dic[category] = texts;
            }
            if (!texts.TryGetValue(content, out List<string>? issues))
            {
                issues = new List<string>();
                texts[content] = issues;
            }
            if (!string.IsNullOrEmpty(issue))
            {
                issues.Add(issue);
                issues.Sort();
            }
        }
    }
}
