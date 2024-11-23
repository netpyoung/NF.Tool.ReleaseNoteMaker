using NF.Tool.PatchNoteMaker.Common.Config;
using NF.Tool.PatchNoteMaker.Common.Fragments;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NF.Tool.PatchNoteMaker.Common.Template
{
    public sealed class TemplateModel
    {
        public bool IsRenderTitle { get; private set; }

        public VersionData VersionData { get; private set; }

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
}
