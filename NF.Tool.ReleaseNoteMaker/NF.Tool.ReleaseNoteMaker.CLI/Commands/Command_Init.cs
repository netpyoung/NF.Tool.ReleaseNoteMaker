using NF.Tool.ReleaseNoteMaker.CLI.Impl;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace NF.Tool.ReleaseNoteMaker.CLI.Commands
{
    [Description($"Create a new config file.")]
    internal sealed class Command_Init : AsyncCommand<Command_Init.Settings>
    {
        internal sealed class Settings : CommandSettings
        {
            [Description("Config file name.")]
            [CommandOption("--file")]
            [DefaultValue(Const.DEFAULT_CONFIG_FILENAME)]
            public string FileName { get; set; } = string.Empty;
        }

        public override Task<int> ExecuteAsync(CommandContext context, Settings setting)
        {
            string newConfigFilePath = setting.FileName;

            if (File.Exists(newConfigFilePath))
            {
                AnsiConsole.MarkupLine($"FileName [yellow]{newConfigFilePath}[/] already exists.");
                return Task.FromResult(1);
            }

            string configFileTempPath = Utils.ExtractResourceToTempFilePath(Const.DEFAULT_CONFIG_FILENAME);
            File.Move(configFileTempPath, newConfigFilePath);

            _ = Directory.CreateDirectory("ChangeLog.d");

            string templateFileTempPath = Utils.ExtractResourceToTempFilePath(Const.DEFAULT_TEMPLATE_FILENAME);
            File.Move(templateFileTempPath, $"ChangeLog.d/{Const.DEFAULT_TEMPLATE_FILENAME}");

            return Task.FromResult(0);
        }
    }
}
