using NF.Tool.ReleaseNoteMaker.CLI.Impl;
using NF.Tool.ReleaseNoteMaker.Common;
using NF.Tool.ReleaseNoteMaker.Common.Config;
using SmartFormat;
using SmartFormat.Core.Settings;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NF.Tool.ReleaseNoteMaker.CLI.Commands
{
    [Description($"Read Release Note.")]
    internal sealed class Command_Read : AsyncCommand<Command_Read.Settings>
    {
        internal sealed class Settings : CommandSettings
        {
            [Description("Build fragment in directory. Default to current directory.")]
            [CommandOption("--dir")]
            public string Directory { get; set; } = string.Empty;

            [Description(Const.DESCRIPTION_CONFIG)]
            [CommandOption("--config")]
            public string Config { get; set; } = string.Empty;

            [Description("Path of release note.")]
            [CommandOption("--path")]
            public string Path { get; set; } = string.Empty;

            [Description("Read release note using given version.")]
            [CommandOption("--version")]
            public string ProjectVersion { get; set; } = string.Empty;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings setting, CancellationToken cancellationToken)
        {
            Exception? exOrNull = Utils.GetConfig(setting.Directory, setting.Config, out string baseDirectory, out ReleaseNoteConfig config);
            if (exOrNull is not null)
            {
                AnsiConsole.WriteException(exOrNull, ExceptionFormats.ShortenEverything);
                return 1;
            }

            string path = config.Maker.OutputFileName;
            if (!string.IsNullOrEmpty(setting.Path))
            {
                path = setting.Path;
            }

            if (!File.Exists(path))
            {
                ReleaseNoteMakerException ex = new ReleaseNoteMakerException($"!File.Exists({path}))");
                AnsiConsole.WriteException(ex);
                return 1;
            }

            string rawStr = await File.ReadAllTextAsync(path, cancellationToken);
            rawStr = rawStr.Replace("\r\n", "\n").Replace('\r', '\n').Trim();

            string versionPattern = config.Reader.VersionPattern;
            string titlePattern = config.Reader.TitlePattern;

            SmartFormatter formatter = Smart.CreateDefaultSmartFormat(new SmartSettings { Parser = new ParserSettings { ConvertCharacterStringLiterals = false } });
            string pattern = formatter.Format(titlePattern, new { VersionPattern = versionPattern });

            MatchCollection mc = Regex.Matches(rawStr, pattern, RegexOptions.Multiline);
            if (mc.Count == 0)
            {
                ReleaseNoteMakerException ex = new ReleaseNoteMakerException("Match Count == 0");
                AnsiConsole.WriteException(ex);
                return 1;
            }

            List<Match> matches = new List<Match>(mc.Count + 1);
            matches.AddRange(mc);
            matches.Add(Match.Empty);

            List<ReleaseInfo> releaseInfos = matches.Zip(matches.Skip(1), (first, second) =>
            {
                int from = first.Index;
                int to = second.Index - first.Index;
                if (second == Match.Empty)
                {
                    to = rawStr.Length - first.Index;
                }
                string full = rawStr.Substring(from, to);
                string[] splitted = full.Split('\n');

                string title = splitted.First();
                string content = string.Join('\n', splitted.Skip(1)).Trim();
                Match match = Regex.Match(title, pattern);
                string version = match.Groups["version"].Value;

                return new ReleaseInfo(title, version, content);
            }).ToList();

            ReleaseInfo? findOrNull = releaseInfos.Find(x => x.Version == setting.ProjectVersion);
            if (findOrNull is null)
            {
                ReleaseNoteMakerException ex = new ReleaseNoteMakerException("Fail to find version");
                AnsiConsole.WriteException(ex);
                return 1;
            }

            Console.WriteLine(findOrNull.Content);
            return 0;
        }
        private sealed record class ReleaseInfo(string Title, string Version, string Content);
    }
}