using System.Collections.Generic;
using System.ComponentModel;

namespace NF.Tool.PatchNoteMaker.Common
{
    public sealed class TemplateModel
    {
        public bool IsRenderTitle { get; set; }

        public VersionData VersionData { get; set; }

        // PatchNoteType or Category or Definition
        [Description("key is Definition.Name")]
        public Dictionary<string, PatchNoteConfig.PatchNoteType> DefinitionDic { get; set; } = new Dictionary<string, PatchNoteConfig.PatchNoteType>();


        //    data[section_name][category_name] = categories
        [Description("key is Section.Name")]
        public Fragment SectionDic { get; set; } = new Fragment();

        // issues_by_category[section_name][category_name] = [
        // render_issue(issue_format, i)
        //        for i in sorted(category_issues, key= issue_key)]
        public Dictionary<string, Dictionary<string, List<string>>> IssuesByCategory { get; set; } = new Dictionary<string, Dictionary<string, List<string>>>();

        private TemplateModel(VersionData versionData, bool isRenderTitle, Fragment fragment)
        {
            IsRenderTitle = isRenderTitle;
            VersionData = versionData;
            SectionDic = fragment;
            DefinitionDic = new Dictionary<string, PatchNoteConfig.PatchNoteType>
            {
                { "Category1", new PatchNoteConfig.PatchNoteType { Name = "Category 1 Name" } }
             };
            IssuesByCategory = new Dictionary<string, Dictionary<string, List<string>>>
            {
                { "Section1", new Dictionary<string, List<string>> { { "Category1", new List<string> { "Issue1" } } } }
            };
        }

        public static TemplateModel Create(VersionData versionData, bool isRenderTitle, Fragment fragment)
        {
            TemplateModel ret = new TemplateModel(versionData, isRenderTitle, fragment);
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

    public class Fragment
    {
        Dictionary<string, Section> _dic = new Dictionary<string, Section>();

        public void AddSection(Section section)
        {
            _dic[section.Name] = section;
        }

        public bool ContainsKey(string sectionName)
        {
            return _dic.ContainsKey(sectionName);
        }
    }

    public class Section
    {
        public string Name { get; }

        // (category, (text, issuelist))
        private readonly Dictionary<string, Dictionary<string, List<string>>> _dic = new Dictionary<string, Dictionary<string, List<string>>>();

        public Section(string name)
        {
            Name = name;
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
