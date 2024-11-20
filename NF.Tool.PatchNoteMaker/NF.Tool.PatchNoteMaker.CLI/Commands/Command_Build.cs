using NF.Tool.PatchNoteMaker.CLI.Impl;
using NF.Tool.PatchNoteMaker.Common;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

            public override ValidationResult Validate()
            {
                if (string.IsNullOrEmpty(ProjectVersion))
                {
                    return ValidationResult.Error("Required: --version");
                }
                return ValidationResult.Success();
            }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings setting)
        {
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
            else if (!string.IsNullOrEmpty(config.Maker.Package))
            {
                throw new NotImplementedException();
            }
            else
            {
                AnsiConsole.MarkupLine("'--version' is required since the config file does not contain 'version' or 'package'.");
                return 1;
            }

            Console.WriteLine("Finding news fragments...");
            TemplateModel model = DummyTemplateModel();

            (Exception? fragmentResultExOrNull, FragmentResult fragmentResult) = FragmentFinder.FindFragments(baseDirectory, config, strict: false);
            if (fragmentResultExOrNull != null)
            {
                AnsiConsole.WriteException(fragmentResultExOrNull, ExceptionFormats.ShortenEverything);
                return 1;
            }

            Console.WriteLine("Rendering news fragments...");
            List<string> willDeleteFilePaths = new List<string>(20);
            string templatePath;
            if (!string.IsNullOrEmpty(config.Maker.TemplateFilePath))
            {
                templatePath = config.Maker.TemplateFilePath;
            }
            else
            {
                templatePath = Utils.ExtractResourceText(Const.DEFAULT_TEMPLATE_FILENAME);
                willDeleteFilePaths.Add(templatePath);
            }

            string outputPath;
            if (setting.IsDraft)
            {
                outputPath = Path.GetTempFileName();
            }
            else
            {
                outputPath = config.Maker.OutputFileName;
            }

            await TemplateRenderer.Render(templatePath, config, model, outputPath);
            if (setting.IsDraft)
            {
                string output = File.ReadAllText(outputPath);
                Console.WriteLine(output);
                willDeleteFilePaths.Add(outputPath);
            }

            foreach (string willDeleteFilePath in willDeleteFilePaths)
            {
                File.Delete(willDeleteFilePath);
            }
            return 0;
        }

        public static TemplateModel DummyTemplateModel()
        {
            TemplateModel model = new TemplateModel
            {
                RenderTitle = true,
                VersionData = new VersionData
                {
                    Name = "MyApp",
                    Version = "1.0.0",
                    Date = "2024-11-18"
                },
                SectionDic = new Dictionary<string, Section> {
                    {
                        "Section1", new Section
                        {
                            Sections = new Dictionary<string, Dictionary<string, List<string>>>
                            {
                                {
                                    "Category1", new Dictionary<string, List<string>>
                                    {
                                        { "Text1", new List<string> { "Issue1", "Issue2" } }
                                    }
                                }
                            },
                            IssuesByCategory = new Dictionary<string, List<string>>
                            {
                                { "Category1", new List<string> { "Issue1", "Issue2" } }
                            }
                        }
                    }
                },
                DefinitionDic = new Dictionary<string, Definition>
                {
                    { "Category1", new Definition { Name = "Category 1 Name" } }
                },
                IssuesByCategory = new Dictionary<string, Dictionary<string, List<string>>>
                {
                    { "Section1", new Dictionary<string, List<string>> { { "Category1", new List<string> { "Issue1" } } } }
                }
            };
            return model;
        }
    }
}
