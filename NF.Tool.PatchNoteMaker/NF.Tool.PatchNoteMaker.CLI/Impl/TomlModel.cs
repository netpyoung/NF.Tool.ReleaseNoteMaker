using NF.Tool.PatchNoteMaker.Common;

namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    public sealed class TomlModel
    {
        public PatchNoteConfig PatchNote { get; set; } = new PatchNoteConfig();
    }
}
