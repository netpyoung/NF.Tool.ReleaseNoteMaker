using Microsoft.VisualStudio.TextTemplating;

namespace NF.Tool.PatchNoteMaker.Common
{
    public static class ExEngineHost
    {
        public static PatchNoteConfig GetConfig(this ITextTemplatingEngineHost host)
        {
            PatchNoteTemplateGenerator x = (PatchNoteTemplateGenerator)host;
            return x.Config;
        }

        public static TemplateModel GetTemplateModel(this ITextTemplatingEngineHost host)
        {
            PatchNoteTemplateGenerator x = (PatchNoteTemplateGenerator)host;
            return x.TemplateModel;
        }
    }
}
