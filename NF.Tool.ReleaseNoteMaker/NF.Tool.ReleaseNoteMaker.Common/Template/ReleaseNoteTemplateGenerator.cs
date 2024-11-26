using Mono.TextTemplating;
using NF.Tool.ReleaseNoteMaker.Common.Config;

namespace NF.Tool.ReleaseNoteMaker.Common.Template
{
    public sealed class ReleaseNoteTemplateGenerator(ReleaseNoteConfig config, TemplateModel templateModel) : TemplateGenerator()
    {
        public ReleaseNoteConfig Config { get; init; } = config;
        public TemplateModel TemplateModel { get; init; } = templateModel;
    }
}
