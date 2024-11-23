using NF.Tool.PatchNoteMaker.Common.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NF.Tool.PatchNoteMaker.Common
{
    public sealed class TemplateModel
    {
        public bool IsRenderTitle { get; set; }

        public VersionData VersionData { get; set; }

        [Description("key is Definition.Name")]
        public Dictionary<string, PatchNoteType> DefinitionDic { get; } = new Dictionary<string, PatchNoteType>();

        [Description("key is Section.Name")]
        public Fragment SectionDic { get; set; } = new Fragment();

        public Dictionary<string, Dictionary<string, List<string>>> IssuesByCategory { get; } = new Dictionary<string, Dictionary<string, List<string>>>();

        private TemplateModel(VersionData versionData, bool isRenderTitle, Fragment fragment, List<PatchNoteType> types)
        {
            IsRenderTitle = isRenderTitle;
            VersionData = versionData;
            SectionDic = fragment;
            DefinitionDic = types.ToDictionary(x => x.Category, x => x, StringComparer.OrdinalIgnoreCase);
            IssuesByCategory = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Section1", new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase) { { "Category1", new List<string> { "Issue1" } } } }
            };
        }

        public static TemplateModel Create(VersionData versionData, bool isRenderTitle, Fragment fragment, List<PatchNoteType> types)
        {
            TemplateModel ret = new TemplateModel(versionData, isRenderTitle, fragment, types);
            return ret;
        }
    }

    public sealed class VersionData
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;

        public VersionData(string name, string version, string date)
        {
            Name = name;
            Version = version;
            Date = date;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return $"{Version} ({Date})";
            }
            return $"{Name} {Version} ({Date})";
        }
    }

    public sealed class Fragment : IEnumerable<KeyValuePair<string, Section>>
    {
        Dictionary<string, Section> _dic = new Dictionary<string, Section>();

        public void AddSection([NotNull] Section section)
        {
            _dic[section.Name] = section;
        }

        public bool ContainsKey(string sectionName)
        {
            return _dic.ContainsKey(sectionName);
        }

        public IEnumerator<KeyValuePair<string, Section>> GetEnumerator()
        {
            return _dic.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dic.GetEnumerator();
        }
    }

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
