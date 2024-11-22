namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    internal static class Const
    {
        public const string DEFAULT_CONFIG_FILENAME = "PatchNote.config.toml";
        public const string DEFAULT_TEMPLATE_FILENAME = "Template.tt";
        public const string DEFAULT_NEWS_CONTENT = @"

# Please write your news content. Lines starting with '#' will be ignored, and
# an empty message aborts.
";
        public const string DESCRIPTION_CONFIG = @"Pass a custom config file at FILE_PATH.
Default: towncrier.toml or pyproject.toml file, 
if both files exist, the first will take precedence.";

        public static readonly string[] FRAGMENT_IGNORE_FILELIST = [
            ".gitignore",
            ".gitkeep",
            ".keep",
            "readme",
            "readme.md",
            "readme.rst"
        ];
    }
}
