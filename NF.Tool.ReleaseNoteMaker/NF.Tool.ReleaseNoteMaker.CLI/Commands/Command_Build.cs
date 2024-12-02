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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NF.Tool.ReleaseNoteMaker.CLI.Commands
{
    [Description($"Generate a release note.")]
    internal sealed class Command_Build : AsyncCommand<Command_Build.Settings>
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

            [Description("[yellow][[Required]][/] Render the news fragments using given version.")]
            [CommandOption("--version")]
            public string ProjectVersion { get; set; } = string.Empty;

            [Description("Render the news fragments using the given date.")]
            [CommandOption("--date")]
            public string ProjectDate { get; set; } = string.Empty;

            [Description("Render the news fragments to standard output.\nDon't write to files, don't check versions.")]
            [CommandOption("--draft")]
            public bool IsDraft { get; set; }

            [Description("Do not ask for confirmation to remove news fragments")]
            [CommandOption("--yes")]
            public bool IsAnswerYes { get; set; }

            [Description("Do not ask for confirmations. But keep news fragments.")]
            [CommandOption("--keep")]
            public bool IsAnswerKeep { get; set; }

            internal (string ProjectName, string ProjectVersion, string ProjectDate) GetProjectInfo()
            {
                return (ProjectName, ProjectVersion, ProjectDate);
            }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings setting)
        {
            // TODO(pyoung): handle stderr
            //AnsiConsole.Console = AnsiConsole.Create(new AnsiConsoleSettings
            //{
            //    Out = new AnsiConsoleOutput(Console.Error)
            //});
            Exception? exOrNull = Utils.GetConfig(setting.Directory, setting.Config, out string baseDirectory, out ReleaseNoteConfig config);
            if (exOrNull is not null)
            {
                AnsiConsole.WriteException(exOrNull, ExceptionFormats.ShortenEverything);
                return 1;
            }

            (Exception? versionDataResultExOrNull, VersionData versionData) = VersionData.GetVersionData(config, setting.GetProjectInfo());
            if (versionDataResultExOrNull is not null)
            {
                AnsiConsole.WriteException(versionDataResultExOrNull, ExceptionFormats.ShortenEverything);
                return 1;
            }

            AnsiConsole.MarkupLine("[green]*[/] Finding news fragments...");
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
                (Exception? renderExOrNull, string text) = await TemplateRenderer.RenderFragments(templatePath, config, versionData, splitted);
                if (renderExOrNull != null)
                {
                    AnsiConsole.WriteException(renderExOrNull);
                    return 1;
                }
                rendered = text;
            }

            string topLine;
            if (!string.IsNullOrEmpty(config.Maker.TitleFormat))
            {
                topLine = string.Format($"{config.Maker.TitleFormat}\n", versionData.Name, versionData.Version, versionData.Date);
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

            string newsFileName;
            if (config.Maker.IsSingleFile)
            {
                newsFileName = config.Maker.OutputFileName;
            }
            else
            {
                newsFileName = string.Format(config.Maker.OutputFileName, versionData.Name, versionData.Version, versionData.Date);
            }

            string newsFileFpath = Path.Combine(baseDirectory, newsFileName);
            AnsiConsole.MarkupLine($"[green]*[/] Writing to newsfile...");
            {
                TextPath txtPath = new TextPath(newsFileFpath)
                     .RootColor(Color.Red)
                     .SeparatorColor(Color.Green)
                     .StemColor(Color.Blue)
                     .LeafColor(Color.Yellow);
                AnsiConsole.Write($"{nameof(newsFileFpath)}: ");
                AnsiConsole.Write(txtPath);
                AnsiConsole.WriteLine();
                Exception? appendToNewsFileExOrNull = await AppendToNewsFile(config, topLine, content, newsFileFpath);
                if (appendToNewsFileExOrNull != null)
                {
                    AnsiConsole.WriteException(appendToNewsFileExOrNull);
                    return 1;
                }
            }

            AnsiConsole.MarkupLine("[green]*[/] Staging newsfile...");
            GitHelper.StageNewsfile(newsFileFpath);

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

        internal static async Task<Exception?> AppendToNewsFile(ReleaseNoteConfig config, string topLine, string content, string newsfileFpath)
        {
            ExtractBaseHeaderAndContent(newsfileFpath, config, out string baseHeader, out string baseContent);
            if (!string.IsNullOrEmpty(topLine)
                && baseContent.Contains(topLine))
            {
                return new ReleaseNoteMakerException("It seems you've already produced newsfiles for this version?");
            }

            StringBuilder sb = new StringBuilder();
            _ = sb.Append(baseHeader);
            if (!string.IsNullOrEmpty(baseContent))
            {
                _ = sb.Append(content);
                _ = sb.Append(baseContent);
            }
            else
            {
                _ = sb.Append(content.TrimEnd());
                _ = sb.Append('\n');
            }
            ReleaseNoteConfigMaker.E_END_OF_LINE eofType = config.Maker.EndOfLine;
            string newContent = NormalizeEndOfLine(sb.ToString(), eofType);
            await File.WriteAllTextAsync(newsfileFpath, newContent);
            return null;
        }

        private static string NormalizeEndOfLine(string content, ReleaseNoteConfigMaker.E_END_OF_LINE eofType)
        {
            string normalized = content.Replace("\r\n", "\n").Replace('\r', '\n');
            switch (eofType)
            {
                case ReleaseNoteConfigMaker.E_END_OF_LINE.CRLF:
                    return normalized.Replace("\n", "\r\n");
                case ReleaseNoteConfigMaker.E_END_OF_LINE.LF:
                    return normalized;
                case ReleaseNoteConfigMaker.E_END_OF_LINE.ENVIRONMENT:
                    return normalized.Replace("\n", Environment.NewLine);
                default:
                    return string.Empty;
            }
        }

        private static void ExtractBaseHeaderAndContent(string path, ReleaseNoteConfig config, out string baseHeader, out string baseContent)
        {
            if (!config.Maker.IsSingleFile)
            {
                baseHeader = string.Empty;
                baseContent = string.Empty;
                return;
            }

            if (!File.Exists(path))
            {
                baseHeader = string.Empty;
                baseContent = string.Empty;
                return;
            }

            string startString = config.Maker.StartString;
            string txt = NormalizeEndOfLine(File.ReadAllText(path), ReleaseNoteConfigMaker.E_END_OF_LINE.LF);
            int index = txt.IndexOf(startString);
            if (index == -1)
            {
                baseHeader = string.Empty;
                baseContent = txt;
                return;
            }

            baseHeader = $"{txt.Substring(0, index).TrimEnd()}\n\n{startString}\n";
            baseContent = txt.Substring(index + startString.Length).TrimStart();
        }
    }
}
