using NF.Tool.PatchNoteMaker.CLI.Impl;
using NF.Tool.PatchNoteMaker.Common;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NF.Tool.PatchNoteMaker.CLI.Commands
{
    [Description("Create a new fragment.")]
    internal sealed class Command_Create : AsyncCommand<Command_Create.Settings>
    {
        public sealed class Settings : CommandSettings
        {
            [Description("Create fragment in directory. Default to current directory.")]
            [CommandOption("--dir")]
            public string Directory { get; set; } = default!;

            [Description(Const.DESCRIPTION_CONFIG)]
            [CommandOption("--config")]
            public string Config { get; set; } = default!;

            [Description("Sets the content of the new fragment.")]
            [CommandOption("--content")]
            public string? ContentOrNull { get; set; }

            [Description("The section to create the fragment for.")]
            [CommandOption("--section")]
            public string Section { get; set; } = default!;

            [Description("Open an editor for writing the newsfragment content.")]
            [CommandOption("--edit")]
            public bool IsEdit { get; set; }

            [Description("Fragment FileName")]
            [CommandArgument(0, "[FileName]")]
            public string FileName { get; set; } = string.Empty;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings setting)
        {
            Exception? ex = Utils.GetConfig(setting.Directory, setting.Config, out string baseDirectory, out PatchNoteConfig config);
            if (ex is not null)
            {
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
                return 1;
            }

            string section = string.Empty;
            {
                // handle setting.Section
                if (!string.IsNullOrEmpty(setting.Section))
                {
                    section = setting.Section;
                }
                else
                {
                    foreach (PatchNoteConfig.PatchNoteSection configSection in config.Sections)
                    {
                        if (string.IsNullOrEmpty(configSection.Path))
                        {
                            section = configSection.Name;
                            break;
                        }
                    }
                }

                if (config.Sections.Find(x => x.Name == section) == null)
                {
                    Console.Error.WriteLine($"Error: Section '{section}' is invalid. Expected one of: {string.Join(", ", config.Sections.Select(x => x.Name))}");
                    return 1;
                }
            }


            bool isEdit = setting.IsEdit;
            string fileName;
            {
                // handle setting.FileName
                if (!string.IsNullOrEmpty(setting.FileName))
                {
                    fileName = setting.FileName;
                }
                else
                {
                    if (string.IsNullOrEmpty(setting.Section))
                    {
                        List<PatchNoteConfig.PatchNoteSection> sections = config.Sections;
                        if (sections.Count > 1)
                        {
                            section = AnsiConsole.Prompt(
                                new SelectionPrompt<string>()
                                    .Title("Pick a [green]section[/]: ")
                                    .AddChoices(sections.Select(x => x.Name))
                                    .UseConverter((x) => string.IsNullOrEmpty(x) ? "(primary)" : x)
                            );
                        }
                    }
                    string issueNumber = AnsiConsole.Prompt(
                        new TextPrompt<string>("Issue number:")
                        .DefaultValue("+")
                    );
                    string fragmentType = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Pick a Fragment [green]Type[/]: ")
                            .AddChoices(config.Types.Select(x => x.Name))
                    ).ToLower();
                    fileName = $"{issueNumber}.{fragmentType}.md";
                    if (setting.ContentOrNull is null)
                    {
                        isEdit = true;
                    }
                }

                {
                    // validate fileName
                    string[] split = fileName.Split(".");
                    string s = $@"Expected filename [green]'{fileName}'[/] to be of format '{{name}}.{{type}}', 
where '{{name}}' is an arbitrary slug
and '{{type}}' is one of: [green]{string.Join(", ", config.Types.Select(x => x.Name))}[/].";
                    if (split.Length < 2)
                    {
                        AnsiConsole.MarkupLine(s);
                        return 1;
                    }
                    string fileExtension = split[split.Length - 1];
                    string fragmentType = split[split.Length - 2];
                    if (config.Types.Find(x => string.Compare(x.Name, fragmentType, StringComparison.OrdinalIgnoreCase) == 0) == null)
                    {
                        AnsiConsole.MarkupLine(s);
                        return 1;
                    }
                }
            }

            string content;
            {
                // handle: setting.ContentOrNull 
                if (setting.ContentOrNull != null)
                {
                    content = setting.ContentOrNull;
                }
                else
                {
                    content = Const.DEFAULT_NEWS_CONTENT;
                }
            }

            FragmentsPath fragmentPath = new FragmentsPath(baseDirectory, config);
            string fragmentDirectory = fragmentPath.GetDirectory(section);
            string segmentFilePath = GetSegmentFilePath(fragmentDirectory, fileName);
            if (isEdit)
            {
                string? editorContentOrNull = await TextEditorHelper.OpenAndReadTemporaryFile($"TEMP_{Path.GetFileName(segmentFilePath)}", content);
                if (editorContentOrNull is null)
                {
                    AnsiConsole.MarkupLine($"Abort writing conrent to [red]{segmentFilePath}[/]");
                    return 1;
                }
                content = editorContentOrNull;
            }

            Directory.CreateDirectory(fragmentDirectory);
            File.WriteAllText(segmentFilePath, content);
            AnsiConsole.MarkupLine($"Created news fragment at {segmentFilePath}");
            return 0;
        }

        private static string GetSegmentFilePath(string fragmentsDirectory, string fileName)
        {
            int retry = 0;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string fileExtension = Path.GetExtension(fileName);

            string segmentFile = Path.Combine(fragmentsDirectory, $"{fileNameWithoutExtension}{fileExtension}");
            while (File.Exists(segmentFile))
            {
                retry++;
                segmentFile = Path.Combine(fragmentsDirectory, $"{fileNameWithoutExtension}.{retry}{fileExtension}");
            }

            return segmentFile;
        }
    }
}
