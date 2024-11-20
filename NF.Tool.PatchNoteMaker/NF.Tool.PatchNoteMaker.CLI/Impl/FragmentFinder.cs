using NF.Tool.PatchNoteMaker.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    public sealed class FragmentBasename
    {
        public string Issue { get; set; }
        public string Category { get; init; }
        public int RetryCounter { get; set; }

        public FragmentBasename(string issue, string category, int retryCounter)
        {
            Issue = issue;
            Category = category;
            RetryCounter = retryCounter;
        }

        public static FragmentBasename Invalid()
        {
            return new FragmentBasename(string.Empty, string.Empty, 0);
        }
    }
    public sealed class FragmentContent
    {
        public string SectionName { get; set; } = string.Empty;
        public List<SectionFragment> SectionFragments { get; set; } = new List<SectionFragment>();

        public sealed record class SectionFragment(FragmentBasename FragmentBasename, string Data);
    }

    public sealed class FragmentFile
    {
        public required string FileName { get; init; }
        public required string Category { get; init; }
    }

    public sealed class FragmentResult
    {
        public required List<FragmentContent> FragmentContents { get; init; }
        public required List<FragmentFile> FragmentFiles { get; init; }

        public static FragmentResult Default()
        {
            return new FragmentResult
            {
                FragmentContents = new List<FragmentContent>(),
                FragmentFiles = new List<FragmentFile>()
            };
        }
    }

    public sealed class FragmentFinder
    {

        [Description("Key: SectionName")]
        //  section_name, section_fragments(issue, category, counter), content // fragment_files(filename, category)
        public static
            (Exception? exOrNull, FragmentResult result)
            FindFragments
            (string baseDirectory, PatchNoteConfig config, bool strict)
        {
            HashSet<string> ignoredFileNameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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
                ignoredFileNameSet.Add(Path.GetFileName(config.Maker.TemplateFilePath).ToLower());
            }

            foreach (string filename in config.Maker.Ignores)
            {
                ignoredFileNameSet.Add(filename.ToLower());
            }

            FragmentsPath getSectionPath = new FragmentsPath(baseDirectory, config);
            Dictionary<string, int> orphanFragmentCounter = new Dictionary<string, int>(config.Sections.Count);

            List<FragmentContent> fragmentContents = new List<FragmentContent>(config.Sections.Count);
            List<FragmentFile> fragmentFiles = new List<FragmentFile>(config.Sections.Count);
            foreach (PatchNoteConfig.PatchNoteSection section in config.Sections)
            {
                string sectionName = section.Name;
                string sectionDir = getSectionPath.Resolve(section.Path);

                string[] files;
                try
                {
                    files = Directory.GetFiles(sectionDir);
                }
                catch (DirectoryNotFoundException)
                {
                    files = [];
                }

                List<FragmentContent.SectionFragment> fileContent = new List<FragmentContent.SectionFragment>();

                foreach (string file in files)
                {
                    string basename = Path.GetFileName(file);
                    if (ignoredFileNameSet.Any(pattern => IsMatch(basename.ToLower(), pattern)))
                    {
                        continue;
                    }

                    FragmentBasename? fragmentBaseNameOrNull = ParseNewFragmentBasenameOrNull(basename, config.Types);
                    if (fragmentBaseNameOrNull == null)
                    {
                        if (strict)
                        {
                            InvalidOperationException ex = new InvalidOperationException($"Invalid news fragment name: {basename}\nIf this filename is deliberate, add it to 'ignore' in your configuration.");
                            return (ex, FragmentResult.Default());
                        }
                        continue;
                    }

                    FragmentBasename fragmentBaseName = fragmentBaseNameOrNull;
                    if (!string.IsNullOrEmpty(config.Maker.OrphanPrefix))
                    {
                        if (fragmentBaseName.Issue.StartsWith(config.Maker.OrphanPrefix))
                        {
                            fragmentBaseName.Issue = "";
                            if (!orphanFragmentCounter.ContainsKey(fragmentBaseName.Category))
                            {
                                orphanFragmentCounter[fragmentBaseName.Category] = 0;
                            }
                            fragmentBaseName.RetryCounter = orphanFragmentCounter[fragmentBaseName.Category]++;
                        }
                    }

                    if (!string.IsNullOrEmpty(config.Maker.IssuePattern))
                    {
                        if (!Regex.IsMatch(fragmentBaseName.Issue, config.Maker.IssuePattern))
                        {
                            InvalidOperationException ex = new InvalidOperationException($"Issue name '{fragmentBaseName.Issue}' does not match the configured pattern, '{config.Maker.IssuePattern}'");
                            return (ex, FragmentResult.Default());
                        }
                    }

                    string fullFilename = Path.Combine(sectionDir, basename);
                    fragmentFiles.Add(new FragmentFile { FileName = fullFilename, Category = fragmentBaseName.Category });

                    string data = File.ReadAllText(fullFilename);
                    if (fileContent.Find(x => x.FragmentBasename == fragmentBaseName) != null)
                    {
                        InvalidOperationException ex = new InvalidOperationException($"Multiple files for {fragmentBaseName.Issue}.{fragmentBaseName.Category} in {sectionDir}");
                        return (ex, FragmentResult.Default());
                    }

                    fileContent.Add(new FragmentContent.SectionFragment(fragmentBaseName, data));
                }

                fragmentContents.Add(new FragmentContent { SectionName = sectionName, SectionFragments = fileContent });
            }

            return (null, new FragmentResult { FragmentContents = fragmentContents, FragmentFiles = fragmentFiles });
        }

        private static bool IsMatch(string input, string pattern)
        {
            return Regex.IsMatch(input, "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$");
        }

        private static FragmentBasename? ParseNewFragmentBasenameOrNull(string basename, List<PatchNoteConfig.PatchNoteType> types)
        {
            if (string.IsNullOrWhiteSpace(basename))
            {
                return null;
            }

            string[] parts = basename.Split('.');
            if (parts.Length == 1)
            {
                return null;
            }

            int i = parts.Length - 1;
            while (true)
            {
                if (i == 1)
                {
                    return null;
                }

                if (types.Find(x => x.Name == parts[i]) == null)
                {
                    i--;
                    continue;
                }

                string category = parts[i];
                string issue = string.Join(".", parts.Take(i)).Trim();

                if (int.TryParse(issue, out int issueAsInt))
                {
                    issue = issueAsInt.ToString();
                }

                int retryCounter = 0;
                if (i + 1 < parts.Length)
                {
                    if (int.TryParse(parts[i + 1], out int parsedCounter))
                    {
                        retryCounter = parsedCounter;
                    }
                }

                return new FragmentBasename(issue, category, retryCounter);
            }
        }
    }
}