namespace NF.Tool.ReleaseNoteMaker.Common.Config
{
    public sealed class TomlModel
    {
        public PatchNoteConfig PatchNote { get; set; } = new PatchNoteConfig();
    }
}
