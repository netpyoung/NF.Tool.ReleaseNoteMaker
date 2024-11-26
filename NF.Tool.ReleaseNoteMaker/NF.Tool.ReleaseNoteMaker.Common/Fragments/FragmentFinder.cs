using NF.Tool.ReleaseNoteMaker.Common.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NF.Tool.ReleaseNoteMaker.Common.Fragments
{
    public sealed class FragmentFinder
    {
        public static readonly string[] FRAGMENT_IGNORE_FILELIST = [
            ".gitignore",
            ".gitkeep",
            ".keep",
            "readme",
            "readme.md",
            "readme.rst"
        ];

        public static (Exception? exOrNull, FragmentResult result) FindFragments(string baseDirectory, [NotNull] PatchNoteConfig config, bool isStrictMode)
        {
            HashSet<string> ignoredFileNameSet = new HashSet<string>(FRAGMENT_IGNORE_FILELIST, StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(config.Maker.TemplateFilePath))
            {
                _ = ignoredFileNameSet.Add(Path.GetFileName(config.Maker.TemplateFilePath).ToLower());
            }

            foreach (string filename in config.Maker.Ignores)
            {
                _ = ignoredFileNameSet.Add(filename.ToLower());
            }

            FragmentPath getSectionPath = FragmentPath.Get(baseDirectory, config);
            Dictionary<string, int> orphanFragmentCounter = new Dictionary<string, int>(config.Sections.Count);

            List<FragmentContent> fragmentContents = new List<FragmentContent>(config.Sections.Count);
            List<FragmentFile> fragmentFiles = new List<FragmentFile>(30);
            foreach (PatchNoteSection section in config.Sections)
            {
                string sectionDisplayName = section.DisplayName;
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

                foreach (string fileName in files.Select(x => Path.GetFileName(x)))
                {
                    if (ignoredFileNameSet.Any(pattern => IsMatch(fileName.ToLower(), pattern)))
                    {
                        continue;
                    }

                    FragmentBasename? fragmentBaseNameOrNull = ParseNewFragmentBasenameOrNull(fileName, config.Types.Select(x => x.Category));
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
                            if (!orphanFragmentCounter.ContainsKey(fragmentBaseName.Category))
                            {
                                orphanFragmentCounter[fragmentBaseName.Category] = 0;
                            }
                            int retryCount = orphanFragmentCounter[fragmentBaseName.Category]++;

                            fragmentBaseName = new FragmentBasename(Issue: string.Empty, fragmentBaseName.Category, retryCount);
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

                    string fullFileName = Path.Combine(sectionDir, fileName);
                    fragmentFiles.Add(new FragmentFile(fullFileName, fragmentBaseName.Category));

                    string data = File.ReadAllText(fullFileName);
                    if (fragmentContents.Exists(x => x.FragmentBasename == fragmentBaseName))
                    {
                        PatchNoteMakerException ex = new PatchNoteMakerException($"Multiple files for {fragmentBaseName.Issue}.{fragmentBaseName.Category} in {sectionDir}");
                        return (ex, FragmentResult.Default());
                    }

                    fragmentContents.Add(new FragmentContent(sectionDisplayName, fragmentBaseName, data));
                }
            }

            return (null, new FragmentResult { FragmentContents = fragmentContents, FragmentFiles = fragmentFiles });
        }

        private static bool IsMatch(string input, string pattern)
        {
            return Regex.IsMatch(input, "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$");
        }

        internal static FragmentBasename? ParseNewFragmentBasenameOrNull(string basename, IEnumerable<string> categorySeq)
        {
            HashSet<string> categorySet = categorySeq.ToHashSet();
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

                if (!categorySet.Contains(parts[i]))
                {
                    i--;
                    continue;
                }

                string category = parts[i];
                string issue = string.Join(".", parts.Take(i)).Trim();

                if (issue.All(char.IsDigit))
                {
                    if (int.TryParse(issue, out int issueAsInt))
                    {
                        issue = issueAsInt.ToString();
                    }
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

        public static List<FragmentContent> SplitFragments([NotNull] List<FragmentContent> fragmentContents, [NotNull] PatchNoteConfig config)
        {
            List<PatchNoteType> definitions = config.Types;
            bool isAllBullets = config.Maker.IsAllBullets;

            List<FragmentContent> ret = new List<FragmentContent>(fragmentContents.Count);
            foreach (FragmentContent fragmentContent in fragmentContents)
            {
                string content;
                if (isAllBullets)
                {
                    string indented = Indent(fragmentContent.Data.Trim(), "  ");
                    if (indented.Length > 2)
                    {
                        content = indented.Substring(2);
                    }
                    else
                    {
                        content = string.Empty;
                    }
                }
                else
                {
                    content = fragmentContent.Data.TrimStart();
                }


                if (!string.IsNullOrEmpty(fragmentContent.FragmentBasename.Issue))
                {
                    if (!definitions.Find(x => x.Category == fragmentContent.FragmentBasename.Category)!.IsShowContent)
                    {
                        content = string.Empty;
                    }
                }

                FragmentContent newFragmentContent = fragmentContent with { Data = content };
                ret.Add(newFragmentContent);
            }
            return ret;
        }

        public static string Indent([NotNull] string text, string prefix)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            StringBuilder result = new StringBuilder();
            IEnumerable<string> lines = Regex.Matches(text, ".*?(\r\n|\n|\r|$)").Select(m => m.Value);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    _ = result.Append(prefix);
                }
                _ = result.Append(line);
            }

            return result.ToString();
        }
    }
}