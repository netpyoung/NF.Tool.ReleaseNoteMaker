using NF.Tool.PatchNoteMaker.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    public sealed class FragmentBasename
    {
        // example: "release-2.0.1.doc.10"
        // issue: release-2.0.1
        // category: doc
        // retryCount: 10
        public string Issue { get; set; }
        public string Category { get; init; }
        public int RetryCount { get; set; }

        public FragmentBasename(string issue, string category, int retryCount)
        {
            Issue = issue;
            Category = category;
            RetryCount = retryCount;
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
        public static (Exception? exOrNull, FragmentResult result) FindFragments(string baseDirectory, PatchNoteConfig config, bool isStrictMode)
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
            List<FragmentFile> fragmentFiles = new List<FragmentFile>(30);
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

                List<FragmentContent.SectionFragment> fileContents = new List<FragmentContent.SectionFragment>();
                foreach (string fileName in files.Select(x => Path.GetFileName(x)))
                {
                    if (ignoredFileNameSet.Any(pattern => IsMatch(fileName.ToLower(), pattern)))
                    {
                        continue;
                    }

                    FragmentBasename? fragmentBaseNameOrNull = ParseNewFragmentBasenameOrNull(fileName, config.Types);
                    if (fragmentBaseNameOrNull == null)
                    {
                        if (isStrictMode)
                        {
                            PatchNoteMakerException ex = new PatchNoteMakerException($"Invalid news fragment name: {fileName}\nIf this filename is deliberate, add it to 'ignore' in your configuration.");
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
                            fragmentBaseName.RetryCount = orphanFragmentCounter[fragmentBaseName.Category]++;
                        }
                    }

                    if (!string.IsNullOrEmpty(config.Maker.IssuePattern))
                    {
                        if (!Regex.IsMatch(fragmentBaseName.Issue, config.Maker.IssuePattern))
                        {
                            PatchNoteMakerException ex = new PatchNoteMakerException($"Issue name '{fragmentBaseName.Issue}' does not match the configured pattern, '{config.Maker.IssuePattern}'");
                            return (ex, FragmentResult.Default());
                        }
                    }

                    string fullFilename = Path.Combine(sectionDir, fileName);
                    fragmentFiles.Add(new FragmentFile { FileName = fullFilename, Category = fragmentBaseName.Category });

                    string data = File.ReadAllText(fullFilename);
                    if (fileContents.Find(x => x.FragmentBasename == fragmentBaseName) != null)
                    {
                        PatchNoteMakerException ex = new PatchNoteMakerException($"Multiple files for {fragmentBaseName.Issue}.{fragmentBaseName.Category} in {sectionDir}");
                        return (ex, FragmentResult.Default());
                    }

                    fileContents.Add(new FragmentContent.SectionFragment(fragmentBaseName, data));
                }

                fragmentContents.Add(new FragmentContent { SectionName = sectionName, SectionFragments = fileContents });
            }

            return (null, new FragmentResult { FragmentContents = fragmentContents, FragmentFiles = fragmentFiles });
        }

        private static bool IsMatch(string input, string pattern)
        {
            return Regex.IsMatch(input, "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$");
        }

        private static FragmentBasename? ParseNewFragmentBasenameOrNull(string basename, List<PatchNoteConfig.PatchNoteType> types)
        {
            // basename: "release-2.0.1.doc.10"
            // FragmentBasename
            //   - issue: release-2.0.1
            //   - category: doc
            //   - retryCount: 10

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
                if (i == 0)
                {
                    return null;
                }

                if (types.Find(x => Utils.IsSameIgnoreCase(x.Name, parts[i])) == null)
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

                int retryCount = 0;
                if (i + 1 < parts.Length)
                {
                    if (int.TryParse(parts[i + 1], out int parsedCount))
                    {
                        retryCount = parsedCount;
                    }
                }

                return new FragmentBasename(issue, category, retryCount);
            }
        }

        public static Fragment SplitFragments(List<FragmentContent> fragmentContents, List<PatchNoteConfig.PatchNoteType> definitions, bool isAllBullets)
        {
            Fragment fragment = new Fragment();
            foreach (FragmentContent fragmentContent in fragmentContents)
            {
                string sectionName = fragmentContent.SectionName;
                Section section = new Section(sectionName);
                foreach (FragmentContent.SectionFragment sectionFragment in fragmentContent.SectionFragments)
                {
                    string category = sectionFragment.FragmentBasename.Category;
                    string content = sectionFragment.Data;
                    string issue = sectionFragment.FragmentBasename.Issue;

                    if (isAllBullets)
                    {
                        // TODO(pyoung): content = indent(content.strip(), "  ")[2:]
                    }
                    else
                    {
                        content = content.TrimStart();
                    }


                    if (!string.IsNullOrEmpty(issue)
                        && !definitions.Find(x => Utils.IsSameIgnoreCase(x.Name, sectionFragment.FragmentBasename.Category))!.IsShowcontent)
                    {
                        content = string.Empty;
                    }

                    section.AddIssue(category, content, issue);
                }

                fragment.AddSection(section);
            }
            return fragment;
        }
    }
}