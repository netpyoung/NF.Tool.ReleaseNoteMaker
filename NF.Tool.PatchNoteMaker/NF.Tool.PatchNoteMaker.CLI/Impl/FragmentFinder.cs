using NF.Tool.PatchNoteMaker.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    public sealed class FragmentFinder
    {
        public static
            (Dictionary<string, Dictionary<(string?, string?, int), string>>, List<(string, string?)>)
            FindFragments
            (string baseDirectory, PatchNoteConfig config, bool strict)
        {
            HashSet<string> ignoredFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".gitignore",
                ".gitkeep",
                ".keep",
                "readme",
                "readme.md",
                "readme.rst"
            };

            if (!string.IsNullOrEmpty(config.Maker.TemplateFilePath))
            {
                ignoredFiles.Add(Path.GetFileName(config.Maker.TemplateFilePath));
            }

            foreach (string filename in config.Maker.Ignores)
            {
                ignoredFiles.Add(filename.ToLower());
            }

            FragmentsPath getSectionPath = new FragmentsPath(baseDirectory, config);

            Dictionary<string, Dictionary<(string?, string?, int), string>> content = new Dictionary<string, Dictionary<(string?, string?, int), string>>();
            List<(string, string?)> fragmentFiles = new List<(string, string?)>();
            Dictionary<string?, int> orphanFragmentCounter = new Dictionary<string?, int>();

            foreach (PatchNoteConfig.PatchNoteSection section in config.Sections)
            {
                string key = section.Name;
                string sectionDir = getSectionPath.Resolve(section.Path);

                string[] files;
                try
                {
                    files = Directory.GetFiles(sectionDir);
                }
                catch (DirectoryNotFoundException)
                {
                    files = Array.Empty<string>();
                }

                Dictionary<(string?, string?, int), string> fileContent = new Dictionary<(string?, string?, int), string>();

                foreach (string file in files)
                {
                    string basename = Path.GetFileName(file);
                    if (ignoredFiles.Any(pattern => IsMatch(basename.ToLower(), pattern)))
                    {
                        continue;
                    }

                    (string issue, string category, int counter) = ParseNewFragmentBasename(basename, config.Types);
                    if (category == null)
                    {
                        if (strict && issue == null)
                        {
                            throw new InvalidOperationException(
                                $"Invalid news fragment name: {basename}\n" +
                                "If this filename is deliberate, add it to 'ignore' in your configuration.");
                        }
                        continue;
                    }

                    if (config.Maker.OrphanPrefix != null && issue != null && issue.StartsWith(config.Maker.OrphanPrefix))
                    {
                        issue = "";
                        if (!orphanFragmentCounter.ContainsKey(category))
                        {
                            orphanFragmentCounter[category] = 0;
                        }
                        counter = orphanFragmentCounter[category]++;
                    }

                    if (!string.IsNullOrEmpty(config.Maker.IssuePattern) &&
                        issue != null &&
                        !Regex.IsMatch(issue, config.Maker.IssuePattern))
                    {
                        throw new InvalidOperationException(
                            $"Issue name '{issue}' does not match the configured pattern, '{config.Maker.IssuePattern}'");
                    }

                    string fullFilename = Path.Combine(sectionDir, basename);
                    fragmentFiles.Add((fullFilename, category));
                    string data = File.ReadAllText(fullFilename);

                    (string? issue, string category, int counter) keyTuple = (issue, category, counter);
                    if (fileContent.ContainsKey(keyTuple))
                    {
                        throw new InvalidOperationException(
                            $"Multiple files for {issue}.{category} in {sectionDir}");
                    }

                    fileContent[keyTuple] = data;
                }

                content[key] = fileContent;
            }

            return (content, fragmentFiles);
        }

        private static bool IsMatch(string input, string pattern)
        {
            return Regex.IsMatch(input, "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$");
        }

        private static (string issue, string category, int counter) ParseNewFragmentBasename(string basename, List<PatchNoteConfig.PatchNoteType> types)
        {
            throw new NotImplementedException();
        }
    }
}