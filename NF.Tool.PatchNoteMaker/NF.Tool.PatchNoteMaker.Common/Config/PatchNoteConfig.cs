using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NF.Tool.PatchNoteMaker.Common.Config
{
    public sealed class PatchNoteConfig
    {
        public PatchNoteConfigMaker Maker { get; private set; } = new PatchNoteConfigMaker();
        [DataMember(Name = "Section")]
        public List<PatchNoteSection> Sections { get; } = new List<PatchNoteSection>(20);
        [DataMember(Name = "Type")]
        public List<PatchNoteType> Types { get; } = new List<PatchNoteType>(20);
    }

    public sealed class PatchNoteConfigMaker
    {
        public string Name { get; set; } = string.Empty;
        public string Directory { get; set; } = string.Empty;
        public string OutputFileName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string TemplateFilePath { get; set; } = string.Empty;
        public List<string> Ignores { get; } = [];
        public string OrphanPrefix { get; set; } = "+";
        public string IssuePattern { get; set; } = string.Empty;
        public string IssueFormat { get; set; } = string.Empty;
        public string TitleFormat { get; set; } = string.Empty;
        public string StartString { get; set; } = "<!-- release notes start -->\n";
        public bool IsWrap { get; set; }
        public bool IsAllBullets { get; set; }

        // config.package_dir
        // public string PackageDirectory{ get; set; } = string.Empty;
    }

    public sealed class PatchNoteSection
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    public sealed class PatchNoteType
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsShowContent { get; set; }
        public bool IsCheck { get; set; }
    }
}
