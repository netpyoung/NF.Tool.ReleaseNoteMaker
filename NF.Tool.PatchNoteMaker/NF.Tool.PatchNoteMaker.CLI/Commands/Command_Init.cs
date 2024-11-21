using NF.Tool.PatchNoteMaker.CLI.Impl;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace NF.Tool.PatchNoteMaker.CLI.Commands
{
    [Description($"Create a new config file.")]
    internal sealed class Command_Init : AsyncCommand<Command_Init.Settings>
    {
        public sealed class Settings : CommandSettings
        {
            [Description("Config file name.")]
            [CommandOption("--file")]
            public string FileName { get; set; } = string.Empty;
        }

        public override Task<int> ExecuteAsync(CommandContext context, Settings setting)
        {
            string newConfigFilePath;
            if (!string.IsNullOrEmpty(setting.FileName))
            {
                newConfigFilePath = setting.FileName;
            }
            else
            {
                newConfigFilePath = Const.DEFAULT_CONFIG_FILENAME;
            }

            if (File.Exists(newConfigFilePath))
            {
                AnsiConsole.MarkupLine($"FileName [yellow]{newConfigFilePath}[/] already exists.");
                return Task.FromResult(1);
            }

            string path = Utils.ExtractResourceToTempFilePath(Const.DEFAULT_CONFIG_FILENAME);
            File.Move(path, newConfigFilePath);

            return Task.FromResult(0);
        }
    }
}
