using NF.Tool.ReleaseNoteMaker.CLI.Impl;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        public override async Task<int> ExecuteAsync(CommandContext context, Settings setting, CancellationToken cancellationToken)
        {
            StringBuilder errSb = new StringBuilder();
            string newConfigFilePath = setting.FileName;

            if (File.Exists(newConfigFilePath))
            {
                _ = errSb.AppendLine($"FileName [yellow]{newConfigFilePath}[/] already exists.");
            }

            string templateFileName = Const.DEFAULT_TEMPLATE_T4_FILENAME;
            string templatePath = $"ChangeLog.d/{templateFileName}";
            if (File.Exists(templatePath))
            {
                _ = errSb.AppendLine($"FileName [yellow]{templatePath}[/] already exists.");
            }

            string errStr = errSb.ToString();
            if (!string.IsNullOrEmpty(errStr))
            {
                AnsiConsole.Markup(errStr);
                return 1;
            }

            string configFileTempPath = Utils.ExtractResourceToTempFilePath(Const.DEFAULT_CONFIG_FILENAME);
            File.Move(configFileTempPath, newConfigFilePath);
            _ = Directory.CreateDirectory("ChangeLog.d");

            string templateFileTempPath = Utils.ExtractResourceToTempFilePath(templateFileName);
            File.Move(templateFileTempPath, templatePath);

            bool isNeedToCreateChagelogFile = true;
            {
                // CHANGELOG.md
                foreach (string file in Directory.EnumerateFiles(Directory.GetCurrentDirectory()))
                {
                    if (string.Equals(Path.GetFileName(file), "CHANGELOG.md", StringComparison.OrdinalIgnoreCase))
                    {
                        isNeedToCreateChagelogFile = false;
                        break;
                    }
                }

                if (isNeedToCreateChagelogFile)
                {
                    string chagelogFileTempPath = Utils.ExtractResourceToTempFilePath("CHANGELOG.md");
                    File.Move(chagelogFileTempPath, "CHANGELOG.md");
                }
            }

            {
                // display layout

                AnsiConsole.WriteLine("Initialized");
                Tree root = new Tree("./");
                _ = root.AddNode($"{Const.DEFAULT_CONFIG_FILENAME}");
                if (isNeedToCreateChagelogFile)
                {
                    _ = root.AddNode("CHANGELOG.md");
                }

                TreeNode changelogD = root.AddNode("[blue]ChangeLog.d/[/]");
                _ = changelogD.AddNode($"{templateFileName}");
                AnsiConsole.Write(root);
            }

            return 0;
        }
    }
}
