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
using System.Threading.Tasks;

namespace NF.Tool.ReleaseNoteMaker.CLI.Commands
{
    [Description($"Checks files changed.")]
    internal sealed class Command_Check : AsyncCommand<Command_Check.Settings>
    {
        internal sealed class Settings : CommandSettings
        {
            [Description("Checks files changed running git diff --name-only BRANCH...\nBRANCH is the branch to be compared with.")]
            [CommandOption("--compare-with")]
            [DefaultValue("origin/main")]
            public string CompareWith { get; set; } = string.Empty;

            [Description(Const.DESCRIPTION_CONFIG)]
            [CommandOption("--config")]
            public string Config { get; set; } = string.Empty;

            [Description("Build fragment in directory. Default to current directory.")]
            [CommandOption("--dir")]
            public string Directory { get; set; } = string.Empty;
        }

        public override Task<int> ExecuteAsync(CommandContext context, Settings setting)
        {
            Exception? exOrNull = Utils.GetConfig(setting.Directory, setting.Config, out string baseDirectory, out ReleaseNoteConfig config);
            if (exOrNull is not null)
            {
                AnsiConsole.WriteException(exOrNull, ExceptionFormats.ShortenEverything);
                return Task.FromResult(1);
            }

            (Exception? exOrNull, int exitCode, string stdOut, string stdErr) callResult = Cmd.Call2("git", $"diff --name-only {setting.CompareWith}...");
            if (callResult.exOrNull != null)
            {
                AnsiConsole.WriteException(callResult.exOrNull);
                return Task.FromResult(1);
            }

            if (callResult.exitCode != 0)
            {
                AnsiConsole.MarkupLine("git produced output while failing:");
                AnsiConsole.WriteLine(callResult.stdErr);
                return Task.FromResult(1);
            }

            string[] changedFiles = callResult.stdOut.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (changedFiles.Length == 0)
            {
                AnsiConsole.MarkupLine($"On [green]{setting.CompareWith}[/] branch, or no diffs, so no newsfragment required.");
                return Task.FromResult(0);
            }

            HashSet<string> fullPathSet = changedFiles.Select(Path.GetFullPath).ToHashSet();
            AnsiConsole.MarkupLine("Looking at these files:");
            AnsiConsole.MarkupLine("----");
            foreach ((int Index, string Item) in fullPathSet.OrderBy(x => x).Index())
            {
                AnsiConsole.MarkupLine($"{Index + 1}. {Item}");
            }
            AnsiConsole.MarkupLine("----");

            (Exception? fragmentResultExOrNull, FragmentResult result) = FragmentFinder.FindFragments(setting.Directory, config, isStrictMode: true);
            if (fragmentResultExOrNull != null)
            {
                AnsiConsole.WriteException(fragmentResultExOrNull, ExceptionFormats.ShortenEverything);
                return Task.FromResult(1);
            }

            string newsFileFpath = Path.GetFullPath(Path.Combine(setting.Directory, config.Maker.OutputFileName));
            if (fullPathSet.Contains(newsFileFpath))
            {
                AnsiConsole.MarkupLine("Checks SKIPPED: news file changes detected.");
                return Task.FromResult(0);
            }

            HashSet<string> fragmentCheckSet = new HashSet<string>(result.FragmentFiles.Count);
            HashSet<string> fragmentUnCheckSet = new HashSet<string>(result.FragmentFiles.Count);
            foreach (FragmentFile x in result.FragmentFiles)
            {
                string fileName = x.FileName;
                string category = x.Category;

                ReleaseNoteType releseNoteType = config.Types.Find(x => x.Category == category)!;
                if (releseNoteType.IsCheck)
                {
                    _ = fragmentCheckSet.Add(fileName);
                }
                else
                {
                    _ = fragmentUnCheckSet.Add(fileName);
                }
            }

            string[] inBranchfragments = fullPathSet.Intersect(fragmentCheckSet).ToArray();
            if (inBranchfragments.Length > 0)
            {
                AnsiConsole.MarkupLine("Found:");
                foreach ((int Index, string Item) in inBranchfragments.OrderBy(x => x).Index())
                {
                    AnsiConsole.MarkupLine($"{Index + 1}. {Item}");
                }
                return Task.FromResult(0);
            }

            if (fragmentUnCheckSet.Count == 0)
            {
                AnsiConsole.MarkupLine("No new newsfragments found on this branch.");
                return Task.FromResult(1);
            }

            string[] inBranchUncheckedFragments = fullPathSet.Intersect(fragmentUnCheckSet).ToArray();
            AnsiConsole.MarkupLine("Found newsfragments of unchecked types in the branch:");
            foreach ((int Index, string Item) in inBranchUncheckedFragments.OrderBy(x => x).Index())
            {
                AnsiConsole.MarkupLine($"{Index + 1}. {Item}");
            }
            return Task.FromResult(1);
        }
    }
}
