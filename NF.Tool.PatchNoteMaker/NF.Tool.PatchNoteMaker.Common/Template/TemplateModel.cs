using System.Collections.Generic;
using System.Linq;

namespace NF.Tool.PatchNoteMaker.Common.Template
{
    public sealed record class Section(string DisplayName, List<Category> Categories);
    public sealed record class Category(string DisplayName, List<Content> Contents)
    {
        public List<string> GetAllIssues()
        {
            List<string> ret = Contents.SelectMany(x => x.Issues).ToList();
            return ret;
        }
    }
    public sealed record class Content(string Text, List<string> Issues)
    {
        public void Deconstruct(out string text, out List<string> issues)
        {
            text = Text;
            issues = Issues;
        }
    }

    public sealed record class TemplateModel(bool IsRenderTitle, VersionData VersionData, List<Section> Sections);
}
