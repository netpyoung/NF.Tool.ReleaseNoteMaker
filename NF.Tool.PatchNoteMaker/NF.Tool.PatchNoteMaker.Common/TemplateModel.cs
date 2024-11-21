using System.Collections.Generic;
using System.ComponentModel;

namespace NF.Tool.PatchNoteMaker.Common
{
    public sealed class TemplateModel
    {
        public bool RenderTitle { get; set; }

        public VersionData VersionData { get; set; }

        // PatchNoteType or Category or Definition
        [Description("key is Definition.Name")]
        public Dictionary<string, PatchNoteConfig.PatchNoteType> DefinitionDic { get; set; } = new Dictionary<string, PatchNoteConfig.PatchNoteType>();


        //    data[section_name][category_name] = categories
        [Description("key is Section.Name")]
        public Dictionary<string, Section> SectionDic { get; set; } = new Dictionary<string, Section>();

        // issues_by_category[section_name][category_name] = [
        // render_issue(issue_format, i)
        //        for i in sorted(category_issues, key= issue_key)]
        public Dictionary<string, Dictionary<string, List<string>>> IssuesByCategory { get; set; } = new Dictionary<string, Dictionary<string, List<string>>>();

        private TemplateModel(VersionData versionData)
        {
            RenderTitle = true;
            VersionData = versionData;
            SectionDic = new Dictionary<string, Section> {
                {
                    "Section1", new Section
                    {
                        Sections = new Dictionary<string, Dictionary<string, List<string>>>
                        {
                            {
                                "Category1", new Dictionary<string, List<string>>
                                {
                                    { "Text1", new List<string> { "Issue1", "Issue2" } }
                                }
                            }
                        }
                    }
                }
            };
            DefinitionDic = new Dictionary<string, PatchNoteConfig.PatchNoteType>
            {
                { "Category1", new PatchNoteConfig.PatchNoteType { Name = "Category 1 Name" } }
             };
            IssuesByCategory = new Dictionary<string, Dictionary<string, List<string>>>
            {
                { "Section1", new Dictionary<string, List<string>> { { "Category1", new List<string> { "Issue1" } } } }
            };
        }

        public static TemplateModel Create(VersionData versionData)
        {
            TemplateModel ret = new TemplateModel(versionData);
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

    public sealed class Section
    {
        // category_name] = categories
        public Dictionary<string, Dictionary<string, List<string>>> Sections { get; set; } = new Dictionary<string, Dictionary<string, List<string>>>();
    }
}
