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
    }
}
