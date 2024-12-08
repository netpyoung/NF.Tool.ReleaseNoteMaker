using NF.Tool.ReleaseNoteMaker.CLI.Impl;
using NF.Tool.ReleaseNoteMaker.Common.Config;
using NF.Tool.ReleaseNoteMaker.Common.Fragments;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NF.Tool.ReleaseNoteMaker.CLI.Commands
{
    [Description("Create a new fragment.")]
    internal sealed class Command_Create : AsyncCommand<Command_Create.Settings>
    {
        internal sealed class Settings : CommandSettings
        {
            [Description("Create fragment in directory. Default to current directory.")]
            [CommandOption("--dir")]
            public string Directory { get; set; } = string.Empty;

            [Description(Const.DESCRIPTION_CONFIG)]
            [CommandOption("--config")]
            public string Config { get; set; } = string.Empty;

            [Description("Sets the content of the new fragment.")]
            [CommandOption("--content")]
            [DefaultValue(Const.DEFAULT_NEWS_CONTENT)]
            public string Content { get; set; } = string.Empty;

            [Description("The section display name to create the fragment for.")]
            [CommandOption("--section")]
            public string Section { get; set; } = string.Empty;

            [Description("Open an editor for writing the newsfragment content.")]
            [CommandOption("--edit")]
            public bool IsEditMode { get; set; }

            [Description("Fragment FileName")]
            [CommandArgument(0, "[FileName]")]
            public string FileName { get; set; } = string.Empty;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings setting)
        {
            Exception? ex = Utils.GetConfig(setting.Directory, setting.Config, out string baseDirectory, out ReleaseNoteConfig config);
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
                    foreach (ReleaseNoteSection configSection in config.Sections)
                    {
                        if (string.IsNullOrEmpty(configSection.Path))
                        {
                            sectionDisplayName = configSection.DisplayName;
                            break;
                        }
                    }
                }

                ReleaseNoteSection? sectionOrNull = config.Sections.Find(x => Utils.IsSameIgnoreCase(x.DisplayName, sectionDisplayName));
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
                        List<ReleaseNoteSection> sections = config.Sections;
                        if (sections.Count > 1)
                        {
                            int displayNameMax = sections.Max(x => x.DisplayName.Length);
                            ReleaseNoteSection releaseNoteSection = AnsiConsole.Prompt(
                                new SelectionPrompt<ReleaseNoteSection>()
                                    .Title("Pick a [green]Section[/]: ")
                                    .AddChoices(sections)
                                    .UseConverter((x) =>
                                    {
                                        return string.Format($"{{0,-{displayNameMax}}} | Path: {{1}}/{{2}}", x.DisplayName, config.Maker.Directory, x.Path);
                                    })
                            );
                            sectionPath = releaseNoteSection.Path;
                        }
                    }
                    string issueName = AnsiConsole.Prompt(
                        new TextPrompt<string>("Issue Name : \nex) + / +hello / 123 / baz.1.2 \nDefault:")
                            .DefaultValue("+")
                    );
                    string typeCategory;
                    {
                        int displayNameMax = config.Types.Max(x => x.DisplayName.Length);

                        ReleaseNoteType releaseNoteType = AnsiConsole.Prompt(
                            new SelectionPrompt<ReleaseNoteType>()
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
                        typeCategory = releaseNoteType.Category;
                    }

                    fileName = $"{issueName}.{typeCategory}.md";
                    if (setting.Content is null)
                    {
                        isEdit = true;
                    }
                }

                if (!IsValidFileName(config, fileName))
                {
                    AnsiConsole.MarkupLine($@"Expected fileName [green]'{fileName}'[/] to be of format '{{issueName}}.{{typeCategory}}', 
where '{{issueName}}' is an arbitrary slug
and '{{typeCategory}}' is one of: [green]{string.Join(", ", config.Types.Select(x => x.DisplayName))}[/].");
                    return 1;
                }
            }

            FragmentPath fragmentPath = FragmentPath.Get(baseDirectory, config);
            string fragmentDirectory = fragmentPath.GetDirectory(sectionPath);
            string segmentFilePath = GetSegmentFilePath(fragmentDirectory, fileName);
            string content;
            {
                content = setting.Content!;

                if (isEdit)
                {
                    string initialContent = $"{content}\n{Const.DEFAULT_EDIT_NEWS_CONTENT}";
                    string? editorContentOrNull = await TextEditorHelper.OpenAndReadTemporaryFile($"TEMP_{Path.GetFileName(segmentFilePath)}", initialContent);
                    if (editorContentOrNull is null)
                    {
                        AnsiConsole.MarkupLine($"Abort writing conrent to [red]{segmentFilePath}[/]");
                        return 1;
                    }

                    string[] allLines = editorContentOrNull.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
                    StringBuilder sb = new StringBuilder(editorContentOrNull.Length);
                    foreach (string line in allLines)
                    {
                        if (line.StartsWith('#'))
                        {
                            continue;
                        }

                        string x = line.TrimEnd();
                        _ = sb.AppendLine(x);
                    }

                    content = sb.ToString().Trim();
                }
            }

            _ = Directory.CreateDirectory(fragmentDirectory);
            await File.WriteAllTextAsync(segmentFilePath, content);
            AnsiConsole.MarkupLine($"Created news fragment at {segmentFilePath}");
            return 0;
        }

        private static bool IsValidFileName(ReleaseNoteConfig config, string fileName)
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
