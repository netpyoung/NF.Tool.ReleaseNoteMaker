using Microsoft.VisualStudio.TextTemplating;
using NF.Tool.ReleaseNoteMaker.Common.Config;
using System.Diagnostics.CodeAnalysis;

namespace NF.Tool.ReleaseNoteMaker.Common.Template
{
    public static class ExEngineHost
    {
        public static ReleaseNoteConfig GetConfig([NotNull] this ITextTemplatingEngineHost host)
        {
            ReleaseNoteTemplateGenerator x = (ReleaseNoteTemplateGenerator)host;
            return x.Config;
        }

        public static TemplateModel GetTemplateModel([NotNull] this ITextTemplatingEngineHost host)
        {
            ReleaseNoteTemplateGenerator x = (ReleaseNoteTemplateGenerator)host;
            return x.TemplateModel;
        }
    }
}
