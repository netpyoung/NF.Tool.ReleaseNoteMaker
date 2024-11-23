using Mono.TextTemplating;
using NF.Tool.PatchNoteMaker.Common.Config;

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
