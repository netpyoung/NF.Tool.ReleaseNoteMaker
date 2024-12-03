using System.Collections.Generic;
using System.Diagnostics;

namespace NF.Tool.ReleaseNoteMaker.Common.Template
{
    public sealed record class TemplateModel(bool IsRenderTitle, ProjectData ProjectData, List<Section> Sections);

    [DebuggerDisplay("Section: {DisplayName}")]
    public sealed record class Section(string DisplayName, List<Category> Categories);

    [DebuggerDisplay("Category: {DisplayName}")]
    public sealed record class Category(string DisplayName, List<Content> Contents, List<string> CategoryIssues)
    {
#pragma warning disable CA1024
        public List<string> GetAllIssues()
        {
            return CategoryIssues;
        }
#pragma warning restore CA1024
    }

    [DebuggerDisplay("Content: {Text}")]
    public sealed record class Content(string Text, List<string> Issues)
    {
        public void Deconstruct(out string text, out List<string> issues)
        {
            text = Text;
            issues = Issues;
        }
    }
}
