using Mono.TextTemplating;

namespace NF.Tool.PatchNoteMaker.Common
{
    public sealed class PatchNoteTemplateGenerator : TemplateGenerator
    {
        public PatchNoteConfig Config { get; init; }
        public TemplateModel TemplateModel { get; init; }

        public PatchNoteTemplateGenerator(PatchNoteConfig config, TemplateModel templateModel) : base()
        {
            Config = config;
            TemplateModel = templateModel;
        }
    }
}
