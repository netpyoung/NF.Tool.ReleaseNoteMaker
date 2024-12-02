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
    [Description($"Generate a release note.")]
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

            HashSet<string> fullPaths = changedFiles.Select(Path.GetFullPath).ToHashSet();
            AnsiConsole.MarkupLine("Looking at these files:");
            AnsiConsole.MarkupLine("----");
            foreach ((int Index, string Item) in fullPaths.OrderBy(x => x).Index())
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
            if (fullPaths.Contains(newsFileFpath))
            {
                AnsiConsole.MarkupLine("Checks SKIPPED: news file changes detected.");
                return Task.FromResult(0);
            }

            HashSet<string> fragments = new HashSet<string>(result.FragmentFiles.Count);
            HashSet<string> unchecked_fragments = new HashSet<string>(result.FragmentFiles.Count);
            foreach (FragmentFile x in result.FragmentFiles)
            {
                string fileName = x.FileName;
                string category = x.Category;

                if (config.Types.Find(x => x.Category == category)!.IsCheck)
                {
                    _ = fragments.Add(fileName);
                }
                else
                {
                    _ = unchecked_fragments.Add(fileName);
                }
            }

            string[] fragments_in_branch = fullPaths.Intersect(fragments).ToArray();
            if (fragments_in_branch.Length > 0)
            {
                AnsiConsole.MarkupLine("Found:");
                foreach ((int Index, string Item) in fragments_in_branch.OrderBy(x => x).Index())
                {
                    AnsiConsole.MarkupLine($"{Index + 1}. {Item}");
                }
                return Task.FromResult(0);
            }

            if (unchecked_fragments.Count == 0)
            {
                AnsiConsole.MarkupLine("No new newsfragments found on this branch.");
                return Task.FromResult(1);
            }

            string[] unchecked_fragments_in_branch = fullPaths.Intersect(unchecked_fragments).ToArray();
            AnsiConsole.MarkupLine("Found newsfragments of unchecked types in the branch:");
            foreach ((int Index, string Item) in unchecked_fragments_in_branch.OrderBy(x => x).Index())
            {
                AnsiConsole.MarkupLine($"{Index + 1}. {Item}");
            }
            return Task.FromResult(1);
        }
    }
}
