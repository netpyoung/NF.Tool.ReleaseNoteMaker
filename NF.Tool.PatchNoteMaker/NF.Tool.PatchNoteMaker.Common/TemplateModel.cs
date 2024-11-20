using System.Collections.Generic;
using System.ComponentModel;

namespace NF.Tool.PatchNoteMaker.Common
{
    public sealed class TemplateModel
    {
        public bool RenderTitle { get; set; }

        public VersionData VersionData { get; set; } = new VersionData();

        [Description("key is Section.Name")]
        public Dictionary<string, Section> SectionDic { get; set; } = new Dictionary<string, Section>();

        [Description("key is Definition.Name")]
        public Dictionary<string, Definition> DefinitionDic { get; set; } = new Dictionary<string, Definition>();

        public Dictionary<string, Dictionary<string, List<string>>> IssuesByCategory { get; set; } = new Dictionary<string, Dictionary<string, List<string>>>();
    }

    public sealed class VersionData
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
    }

    public sealed class Section
    {
        public Dictionary<string, Dictionary<string, List<string>>> Sections { get; set; } = new Dictionary<string, Dictionary<string, List<string>>>();
        public Dictionary<string, List<string>> IssuesByCategory { get; set; } = new Dictionary<string, List<string>>();
    }

    public sealed class Definition
    {
        public string Name { get; set; } = string.Empty;
    }
}
