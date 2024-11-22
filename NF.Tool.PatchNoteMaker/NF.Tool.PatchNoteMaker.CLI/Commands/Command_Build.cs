using NF.Tool.PatchNoteMaker.CLI.Impl;
using NF.Tool.PatchNoteMaker.Common;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NF.Tool.PatchNoteMaker.CLI.Commands
{
    [Description($"Generate a patch note.")]
    internal sealed class Command_Build : AsyncCommand<Command_Build.Settings>
    {
        public sealed class Settings : CommandSettings
        {
            [Description("[yellow][[Required]][/] Render the news fragments using given version.")]
            [CommandOption("--version")]
            public string ProjectVersion { get; set; } = string.Empty;

            [Description(Const.DESCRIPTION_CONFIG)]
            [CommandOption("--config")]
            public string Config { get; set; } = string.Empty;

            [Description("Build fragment in directory. Default to current directory.")]
            [CommandOption("--dir")]
            public string Directory { get; set; } = string.Empty;

            [Description("Render the news fragments to standard output. \nDon't write to files, don't check versions.")]
            [CommandOption("--draft")]
            public bool IsDraft { get; set; }

            [Description("Pass a custom project name.")]
            [CommandOption("--name")]
            public string ProjectName { get; set; } = string.Empty;

            [Description("Render the news fragments using the given date.")]
            [CommandOption("--date")]
            public string ProjectDate { get; set; } = string.Empty;

            [Description("Do not ask for confirmation to remove news fragments")]
            [CommandOption("--yes")]
            public bool IsAnswerYes { get; set; }

            [Description("Do not ask for confirmations. But keep news fragments.")]
            [CommandOption("--keep")]
            public bool IsAnswerKeep { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings setting)
        {
            // TODO(pyoung): handle stderr
            //AnsiConsole.Console = AnsiConsole.Create(new AnsiConsoleSettings
            //{
            //    Out = new AnsiConsoleOutput(Console.Error)
            //});
            Exception? exOrNull = Utils.GetConfig(setting.Directory, setting.Config, out string baseDirectory, out PatchNoteConfig config);
            if (exOrNull is not null)
            {
                AnsiConsole.WriteException(exOrNull, ExceptionFormats.ShortenEverything);
                return 1;
            }

            string projectVersion;
            if (!string.IsNullOrEmpty(setting.ProjectVersion))
            {
                projectVersion = setting.ProjectVersion;
            }
            else if (!string.IsNullOrEmpty(config.Maker.Version))
            {
                projectVersion = config.Maker.Version;
            }
            else
            {
                AnsiConsole.MarkupLine("[green]'--version'[/] is required since the config file does not contain 'version.");
                return 1;
            }

            string projectName;
            if (!string.IsNullOrEmpty(config.Maker.Name))
            {
                projectName = config.Maker.Name;
            }
            else
            {
                projectName = string.Empty;
            }

            string projectDate;
            if (!string.IsNullOrEmpty(setting.ProjectDate))
            {
                projectDate = setting.ProjectDate;
            }
            else
            {
                projectDate = DateTime.Today.ToString("yyyy-MM-dd");
            }

            VersionData versionData = new VersionData(projectName, projectVersion, projectDate);

            AnsiConsole.MarkupLine("[green]*[/] Finding news fragments...");
            (Exception? fragmentResultExOrNull, FragmentResult fragmentResult) = FragmentFinder.FindFragments(baseDirectory, config, isStrictMode: false);
            if (fragmentResultExOrNull != null)
            {
                AnsiConsole.WriteException(fragmentResultExOrNull, ExceptionFormats.ShortenEverything);
                return 1;
            }
            Fragment fragment = FragmentFinder.SplitFragments(fragmentResult.FragmentContents, config.Types, isAllBullets: true);
            bool isRenderTitle = string.IsNullOrEmpty(config.Maker.TitleFormat);

            // TODO(pyoung): impl fill model
            TemplateModel model = TemplateModel.Create(
                versionData,
                isRenderTitle,
                fragment
            );


            using (ScopedFileDeleter deleter = new ScopedFileDeleter())
            {
                AnsiConsole.MarkupLine("[green]*[/] Loading template...");
                string templatePath;
                if (!string.IsNullOrEmpty(config.Maker.TemplateFilePath))
                {
                    templatePath = config.Maker.TemplateFilePath;
                }
                else
                {
                    string tempFilePath = deleter.Register(Utils.ExtractResourceToTempFilePath(Const.DEFAULT_TEMPLATE_FILENAME));
                    templatePath = tempFilePath;
                }

                AnsiConsole.MarkupLine("[green]*[/] Rendering news fragments...");
                string renderOutputPath = deleter.Register(Path.GetTempFileName());
                await TemplateRenderer.Render(templatePath, config, model, renderOutputPath);
                string rendered = await File.ReadAllTextAsync(renderOutputPath);

                string topLine;
                if (!string.IsNullOrEmpty(config.Maker.TitleFormat))
                {
                    topLine = string.Format($"{config.Maker.TitleFormat}\n", projectName, projectVersion, projectDate);
                }
                else
                {
                    topLine = string.Empty;
                }
                string content = $"{topLine}{rendered}";

                if (setting.IsDraft)
                {
                    AnsiConsole.MarkupLine("[green]*[/] show draft...");
                    AnsiConsole.WriteLine(content);
                    return 0;
                }

                AnsiConsole.MarkupLine("[green]*[/] Writing to newsfile...");
                // TODO(pyoung): impl append_to_newsfile
                //  append_to_newsfile(
                //base_directory,
                //news_file,
                //config.start_string,
                //top_line,
                //content,
                File.Move(renderOutputPath, config.Maker.OutputFileName, overwrite: true);
            }

            string newsFile = string.Empty;
            AnsiConsole.MarkupLine("[green]*[/] Staging newsfile...");
            GitHelper.StageNewsfile(baseDirectory, newsFile);

            string[] fragmentFpaths = fragmentResult.FragmentFiles.Select(x => x.FileName).ToArray();
            if (fragmentFpaths.Length == 0)
            {
                AnsiConsole.MarkupLine("No news fragments to remove. Skipping!");
            }
            else if (setting.IsAnswerKeep)
            {
                AnsiConsole.MarkupLine("Keeping the following files:");
                foreach (string x in fragmentFpaths)
                {
                    AnsiConsole.WriteLine(x);
                }
            }
            else if (setting.IsAnswerYes)
            {
                AnsiConsole.MarkupLine("Removing the following files:");
                foreach (string x in fragmentFpaths)
                {
                    AnsiConsole.WriteLine(x);
                }

                AnsiConsole.MarkupLine("[green]*[/] Removing news fragments...");
                GitHelper.RemoveFiles(fragmentFpaths);
            }
            else
            {
                AnsiConsole.MarkupLine("I want to remove the following files:");
                foreach (string x in fragmentFpaths)
                {
                    AnsiConsole.WriteLine(x);
                }

                if (AnsiConsole.Confirm("Is it okay if I remove those files?"))
                {
                    AnsiConsole.MarkupLine("[green]*[/] Removing news fragments...");
                    GitHelper.RemoveFiles(fragmentFpaths);
                }
            }

            AnsiConsole.MarkupLine("[green]*[/] Done!");
            return 0;
        }
    }
}
