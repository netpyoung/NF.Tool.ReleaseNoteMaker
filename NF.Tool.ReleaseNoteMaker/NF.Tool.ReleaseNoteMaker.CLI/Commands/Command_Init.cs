using NF.Tool.ReleaseNoteMaker.CLI.Impl;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NF.Tool.ReleaseNoteMaker.CLI.Commands
{
    [Description($"Init release-note setup.")]
    internal sealed class Command_Init : AsyncCommand<Command_Init.Settings>
    {
        internal sealed class Settings : CommandSettings
        {
            [Description("Specify config file name.")]
            [CommandOption("--file")]
            [DefaultValue(Const.DEFAULT_CONFIG_FILENAME)]
            public string FileName { get; set; } = string.Empty;
        }

        public override Task<int> ExecuteAsync(CommandContext context, Settings setting)
        {
            StringBuilder errSb = new StringBuilder();
            string newConfigFilePath = setting.FileName;

            if (File.Exists(newConfigFilePath))
            {
                _ = errSb.AppendLine($"FileName [yellow]{newConfigFilePath}[/] already exists.");
            }

            string templatePath = $"ChangeLog.d/{Const.DEFAULT_TEMPLATE_FILENAME}";
            if (File.Exists(templatePath))
            {
                _ = errSb.AppendLine($"FileName [yellow]{templatePath}[/] already exists.");
            }

            string errStr = errSb.ToString();
            if (!string.IsNullOrEmpty(errStr))
            {
                AnsiConsole.Markup(errStr);
                return Task.FromResult(1);
            }

            string configFileTempPath = Utils.ExtractResourceToTempFilePath(Const.DEFAULT_CONFIG_FILENAME);
            File.Move(configFileTempPath, newConfigFilePath);

            _ = Directory.CreateDirectory("ChangeLog.d");

            string templateFileTempPath = Utils.ExtractResourceToTempFilePath(Const.DEFAULT_TEMPLATE_FILENAME);
            File.Move(templateFileTempPath, templatePath);

            {
                // display layout

                AnsiConsole.WriteLine("Initialized");
                Tree root = new Tree("./");
                _ = root.AddNode($"{Const.DEFAULT_CONFIG_FILENAME}");
                TreeNode changelogD = root.AddNode("[blue]ChangeLog.d/[/]");
                _ = changelogD.AddNode($"{Const.DEFAULT_TEMPLATE_FILENAME}");
                AnsiConsole.Write(root);
            }

            return Task.FromResult(0);
        }
    }
}
