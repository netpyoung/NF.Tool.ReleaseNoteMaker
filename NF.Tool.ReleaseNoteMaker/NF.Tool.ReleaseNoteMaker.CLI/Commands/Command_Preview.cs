using NF.Tool.ReleaseNoteMaker.CLI.Impl;
using NF.Tool.ReleaseNoteMaker.Common;
using NF.Tool.ReleaseNoteMaker.Common.Config;
using NF.Tool.ReleaseNoteMaker.Common.Fragments;
using NF.Tool.ReleaseNoteMaker.Common.Template;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace NF.Tool.ReleaseNoteMaker.CLI.Commands
{
    [Description($"Preview a release note.")]
    internal sealed class Command_Preview : AsyncCommand<Command_Preview.Settings>
    {
        internal sealed class Settings : CommandSettings
        {
            [Description("Build fragment in directory. Default to current directory.")]
            [CommandOption("--dir")]
            public string Directory { get; set; } = string.Empty;

            [Description(Const.DESCRIPTION_CONFIG)]
            [CommandOption("--config")]
            public string Config { get; set; } = string.Empty;

            [Description("Pass a custom project name.")]
            [CommandOption("--name")]
            public string ProjectName { get; set; } = string.Empty;

            [Description("Render the news fragments using given version.")]
            [CommandOption("--version")]
            [DefaultValue("PREVIEW")]
            public string ProjectVersion { get; set; } = string.Empty;

            [Description("Render the news fragments using the given date.")]
            [CommandOption("--date")]
            public string ProjectDate { get; set; } = string.Empty;

            internal (string ProjectName, string ProjectVersion, string ProjectDate) GetProjectInfo()
            {
                return (ProjectName, ProjectVersion, ProjectDate);
            }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings setting)
        {
            Exception? exOrNull = Utils.GetConfig(setting.Directory, setting.Config, out string baseDirectory, out ReleaseNoteConfig config);
            if (exOrNull is not null)
            {
                AnsiConsole.WriteException(exOrNull, ExceptionFormats.ShortenEverything);
                return 1;
            }

            (Exception? projectDataResultExOrNull, ProjectData projectData) = ProjectData.GetProjectData(config, setting.GetProjectInfo());
            if (projectDataResultExOrNull is not null)
            {
                AnsiConsole.WriteException(projectDataResultExOrNull, ExceptionFormats.ShortenEverything);
                return 1;
            }

            //AnsiConsole.MarkupLine("[green]*[/] Finding news fragments...");
            (Exception? fragmentResultExOrNull, FragmentResult fragmentResult) = FragmentFinder.FindFragments(baseDirectory, config, isStrictMode: false);
            if (fragmentResultExOrNull != null)
            {
                AnsiConsole.WriteException(fragmentResultExOrNull, ExceptionFormats.ShortenEverything);
                return 1;
            }

            List<FragmentContent> splitted = FragmentFinder.SplitFragments(fragmentResult.FragmentContents, config);
            string rendered;
            using (ScopedFileDeleter deleter = ScopedFileDeleter.Using())
            {
                // AnsiConsole.MarkupLine("[green]*[/] Loading template...");
                string templatePath;
                if (!string.IsNullOrEmpty(config.Maker.TemplateFilePath))
                {
                    if (Path.IsPathRooted(config.Maker.TemplateFilePath))
                    {
                        templatePath = config.Maker.TemplateFilePath;
                    }
                    else
                    {
                        templatePath = Path.Combine(baseDirectory, config.Maker.TemplateFilePath);
                    }
                }
                else
                {
                    string tempFilePath = deleter.Register(Utils.ExtractResourceToTempFilePath(Const.DEFAULT_TEMPLATE_T4_FILENAME));
                    templatePath = tempFilePath;
                }

                // AnsiConsole.MarkupLine("[green]*[/] Rendering news fragments...");
                (Exception? renderExOrNull, string text) = await TemplateRenderer.RenderFragments(templatePath, config, projectData, splitted);
                if (renderExOrNull != null)
                {
                    AnsiConsole.WriteException(renderExOrNull);
                    return 1;
                }
                rendered = text;
            }

            string topLine = Utils.GetTopLine(config, projectData);
            string content = $"{topLine}{rendered}";

            //AnsiConsole.MarkupLine("[green]*[/] show draft...");
            AnsiConsole.WriteLine(content);
            return 0;
        }
    }
}
