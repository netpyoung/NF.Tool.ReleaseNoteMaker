using Mono.TextTemplating;
using NF.Tool.ReleaseNoteMaker.Common.Config;

namespace NF.Tool.ReleaseNoteMaker.Common.Template
{
    public sealed class PatchNoteTemplateGenerator(PatchNoteConfig config, TemplateModel templateModel) : TemplateGenerator()
    {
        public PatchNoteConfig Config { get; init; } = config;
        public TemplateModel TemplateModel { get; init; } = templateModel;
    }
}
