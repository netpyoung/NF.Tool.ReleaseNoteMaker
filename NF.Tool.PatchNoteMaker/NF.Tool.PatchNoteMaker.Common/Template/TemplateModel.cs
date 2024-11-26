using System.Collections.Generic;

namespace NF.Tool.PatchNoteMaker.Common.Template
{
    public sealed record class Section(string DisplayName, List<Category> Categories);
    public sealed record class Category(string DisplayName, List<Content> Contents, List<string> CategoryIssues)
    {
#pragma warning disable CA1024
        public List<string> GetAllIssues()
        {
            return CategoryIssues;
        }
#pragma warning restore CA1024
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
