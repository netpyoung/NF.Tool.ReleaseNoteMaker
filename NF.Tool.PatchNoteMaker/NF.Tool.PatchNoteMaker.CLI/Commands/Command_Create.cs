using NF.Tool.PatchNoteMaker.CLI.Impl;
using NF.Tool.PatchNoteMaker.Common.Config;
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

            [Description("The section display name to create the fragment for.")]
            [CommandOption("--section")]
            public string Section { get; set; } = default!;

            [Description("Open an editor for writing the newsfragment content.")]
            [CommandOption("--edit")]
            public bool IsEditMode { get; set; }

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

            string sectionDisplayName = string.Empty;
            string sectionPath;
            {
                // handle setting.Section
                if (!string.IsNullOrEmpty(setting.Section))
                {
                    sectionDisplayName = setting.Section;
                }
                else
                {
                    foreach (PatchNoteSection configSection in config.Sections)
                    {
                        if (string.IsNullOrEmpty(configSection.Path))
                        {
                            sectionDisplayName = configSection.DisplayName;
                            break;
                        }
                    }
                }

                PatchNoteSection? sectionOrNull = config.Sections.Find(x => Utils.IsSameIgnoreCase(x.DisplayName, sectionDisplayName));
                if (sectionOrNull is null)
                {
                    AnsiConsole.WriteLine($"Error: Section '{sectionDisplayName}' is invalid. Expected one of: {string.Join(", ", config.Sections.Select(x => x.DisplayName))}");
                    return 1;
                }
                sectionPath = sectionOrNull.Path;
            }


            bool isEdit = setting.IsEditMode;
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
                        List<PatchNoteSection> sections = config.Sections;
                        if (sections.Count > 1)
                        {
                            int displayNameMax = sections.Max(x => x.DisplayName.Length);
                            PatchNoteSection patchNoteSection = AnsiConsole.Prompt(
                                new SelectionPrompt<PatchNoteSection>()
                                    .Title("Pick a [green]Section[/]: ")
                                    .AddChoices(sections)
                                    .UseConverter((x) =>
                                    {
                                        return string.Format($"{{0,-{displayNameMax}}} | Path: {{1}}/{{2}}", x.DisplayName, config.Maker.Directory, x.Path);
                                    })
                            );
                            sectionPath = patchNoteSection.Path;
                        }
                    }
                    string issueName = AnsiConsole.Prompt(
                        new TextPrompt<string>("Issue Name : \nex) + / +hello / 123 / baz.1.2 \nDefault:")
                            .DefaultValue("+")
                    );
                    string typeCategory;
                    {
                        int displayNameMax = config.Types.Max(x => x.DisplayName.Length);

                        PatchNoteType patchNoteType = AnsiConsole.Prompt(
                            new SelectionPrompt<PatchNoteType>()
                                .Title("Pick a Fragment [green]Type[/]: ")
                                .AddChoices(config.Types)
                                .UseConverter((x) =>
                                {
                                    if (string.IsNullOrEmpty(sectionPath))
                                    {
                                        return string.Format($"{{0,-{displayNameMax}}} | {{1}}/{{2}}.{{3}}.md", x.DisplayName, config.Maker.Directory, issueName, x.Category);
                                    }
                                    else
                                    {
                                        return string.Format($"{{0,-{displayNameMax}}} |  {{1}}/{{2}}/{{3}}.{{4}}.md", x.DisplayName, config.Maker.Directory, sectionPath, issueName, x.Category);
                                    }
                                })
                        );
                        typeCategory = patchNoteType.Category;
                    }

                    fileName = $"{issueName}.{typeCategory}.md";
                    if (setting.ContentOrNull is null)
                    {
                        isEdit = true;
                    }
                }

                if (!_IsValidFileName(config, fileName))
                {
                    AnsiConsole.MarkupLine($@"Expected filename [green]'{fileName}'[/] to be of format '{{issueName}}.{{typeCategory}}', 
where '{{issueName}}' is an arbitrary slug
and '{{typeCategory}}' is one of: [green]{string.Join(", ", config.Types.Select(x => x.DisplayName))}[/].");
                    return 1;
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

            FragmentsPath fragmentPath = FragmentsPath.Get(baseDirectory, config);
            string fragmentDirectory = fragmentPath.GetDirectory(sectionPath);
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
            await File.WriteAllTextAsync(segmentFilePath, content);
            AnsiConsole.MarkupLine($"Created news fragment at {segmentFilePath}");
            return 0;
        }

        private static bool _IsValidFileName(PatchNoteConfig config, string fileName)
        {
            string[] split = fileName.Split(".");
            if (split.Length < 2)
            {
                return false;
            }

            string category = split[split.Length - 2];
            return config.Types.Exists(x => x.Category == category);
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
