using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NF.Tool.ReleaseNoteMaker.Common.Config
{
    public sealed class ReleaseNoteConfig
    {
        public ReleaseNoteConfigMaker Maker { get; } = new ReleaseNoteConfigMaker();
        public ReleaseNoteConfigReader Reader { get; } = new ReleaseNoteConfigReader();
        [DataMember(Name = "Section")]
        public List<ReleaseNoteSection> Sections { get; } = new List<ReleaseNoteSection>(20);
        [DataMember(Name = "Type")]
        public List<ReleaseNoteType> Types { get; } = new List<ReleaseNoteType>(20);
    }

    public sealed class ReleaseNoteConfigMaker
    {
        public string OutputFileName { get; set; } = "CHANGELOG.md";
        public string Directory { get; set; } = "ChangeLog.d";
        public string TemplateFilePath { get; set; } = "ChangeLog.d/Template.tt";
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string CsprojPath { get; set; } = string.Empty;
        public List<string> Ignores { get; } = [];
        public string OrphanPrefix { get; set; } = "+";
        public string IssuePattern { get; set; } = string.Empty;

        public string IssueFormat { get; set; } = string.Empty;

        public string TitleFormat { get; set; } = string.Empty;
        public string StartString { get; set; } = "<!-- release notes start -->\n";
        public bool IsWrap { get; set; } = false;
        public bool IsAllBullets { get; set; } = false;

        public bool IsSingleFile { get; set; } = true;

        public E_END_OF_LINE EndOfLine { get; set; } = E_END_OF_LINE.LF;

        public enum E_END_OF_LINE
        {
            LF = 0,
            CRLF = 1,
            ENVIRONMENT = 2,
        }
    }

    public sealed class ReleaseNoteConfigReader
    {
        // ref:
        // - https://semver.org/
        // - https://regex101.com/r/vkijKf/1/
        public string VersionPattern { get; set; } = "(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.(0|[1-9]\\d*)(?:-((?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\\.(?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\\+([0-9a-zA-Z-]+(?:\\.[0-9a-zA-Z-]+)*))?";
        public string TitlePattern { get; set; } = "^## .*(?<version>{VersionPattern})";
    }

    public sealed class ReleaseNoteSection
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    [DebuggerDisplay("<ReleaseNoteType| category: {Category}>")]
    public sealed class ReleaseNoteType
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsShowContent { get; set; } = true;
        public bool IsCheck { get; set; } = true;
    }
}
