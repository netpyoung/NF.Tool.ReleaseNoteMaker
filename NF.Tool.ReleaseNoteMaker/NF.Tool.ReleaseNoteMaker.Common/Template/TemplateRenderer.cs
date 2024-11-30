using NF.Tool.ReleaseNoteMaker.Common.Config;
using NF.Tool.ReleaseNoteMaker.Common.Fragments;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NF.Tool.ReleaseNoteMaker.Common.Template
{
    public sealed class TemplateRenderer
    {
        public static async Task<(Exception? exOrNull, string text)> Render(string templatePath, ReleaseNoteConfig config, TemplateModel templateModel)
        {
            string tempFilePath = Path.GetTempFileName();
            Exception? exOrNull = await Render(templatePath, config, templateModel, tempFilePath);
            if (exOrNull != null)
            {
                return (exOrNull, string.Empty);
            }

            string text = await File.ReadAllTextAsync(tempFilePath);
            File.Delete(tempFilePath);
            return (null, text);
        }

        public static async Task<Exception?> Render(string templatePath, ReleaseNoteConfig config, TemplateModel templateModel, string outputPath)
        {
            string assemblyLocation = typeof(ReleaseNoteTemplateGenerator).Assembly.Location;

            ReleaseNoteTemplateGenerator generator = new ReleaseNoteTemplateGenerator(config, templateModel);
            generator.Refs.Add(assemblyLocation);
            generator.Imports.Add(typeof(ReleaseNoteTemplateGenerator).Namespace);
            generator.Imports.Add(typeof(ReleaseNoteConfig).Namespace);

            bool isSuccess = await generator.ProcessTemplateAsync(templatePath, outputPath);
            if (!isSuccess)
            {
                StringBuilder sb = new StringBuilder();
                _ = sb.AppendLine("Template Render Error");
                foreach (CompilerError err in generator.Errors)
                {
                    _ = sb.AppendLine(err.ToString());
                }
                return new ReleaseNoteMakerException(sb.ToString());
            }

            return null;
        }


        public static async Task<(Exception?, string)> RenderFragments(string templateFpath, [NotNull] ReleaseNoteConfig config, VersionData versionData, List<FragmentContent> fragmentContents)
        {
            // TODO(pyoung): handle - underlines, topUnderline
            //    top_underline = config.underlines[0],
            //    config.underlines[1:],
            //string topUnderline = "=";
            //List<string> underlines = new List<string> { "", "" };

            string issueFormat = config.Maker.IssueFormat;
            bool isWrap = config.Maker.IsWrap;
            bool isAllBullets = config.Maker.IsAllBullets;
            bool isRenderTitle = string.IsNullOrEmpty(config.Maker.TitleFormat);

            List<Section> sections = new List<Section>(config.Sections.Count);
            foreach (IGrouping<string, FragmentContent> grpSection in fragmentContents.GroupBy(x => x.SectionDisplayName).OrderBy(grp => config.Sections.FindIndex(x => x.DisplayName == grp.Key)))
            {
                List<Category> categories = new List<Category>(config.Types.Count);
                foreach (IGrouping<string, FragmentContent> grpCategory in grpSection.GroupBy(x => x.FragmentBasename.Category).OrderBy(grp => config.Types.FindIndex(x => x.Category == grp.Key)))
                {
                    List<Content> xs = new List<Content>(grpCategory.Count());
                    foreach (IGrouping<string, FragmentContent> grpData in grpCategory.OrderBy(x => IssueParts.IssueKey(x.FragmentBasename.Issue)).GroupBy(x => x.Data))
                    {
                        List<string> trimedIssues = grpData.Select(x => x.FragmentBasename.Issue).Where(x => !string.IsNullOrEmpty(x)).OrderBy(IssueParts.IssueKey).ToList();
                        xs.Add(new Content(grpData.Key, trimedIssues));
                    }

                    List<Content> contents = new List<Content>(xs.Count);
                    foreach (Content x in xs.OrderBy(EntryKey))
                    {
                        List<string> formattedIssues = x.Issues.Select(x => Issue.RenderIssue(issueFormat, x)).ToList();
                        string text = Issue.AppendNewlinesIfTrailingCodeBlock(x.Text);
                        Content c = new Content(text, formattedIssues);
                        contents.Add(c);
                    }

                    if (!isAllBullets)
                    {
                        contents = contents.OrderBy(BulletKey).ToList();
                    }

                    string category = grpCategory.Key;
                    ReleaseNoteType? releaseNoteTypeOrNull = config.Types.Find(x => x.Category == category);
                    if (releaseNoteTypeOrNull != null)
                    {
                        string categoryDisplayName = releaseNoteTypeOrNull.DisplayName;
                        {
                            List<string> issues = grpCategory.Select(x => x.FragmentBasename.Issue).Where(x => !string.IsNullOrEmpty(x)).OrderBy(IssueParts.IssueKey).ToList();
                            List<string> formattedIssues = issues.Select(x => Issue.RenderIssue(issueFormat, x)).ToList();
                            categories.Add(new Category(categoryDisplayName, contents, formattedIssues));
                        }
                    }
                }

                sections.Add(new Section(grpSection.Key, categories));
            }

            // TODO(pyoung): handle - underlines, topUnderline

            TemplateModel model = new TemplateModel(isRenderTitle, versionData, sections);
            (Exception? exOrNull, string renderedText) = await Render(templateFpath, config, model);
            if (exOrNull != null)
            {
                return (exOrNull, string.Empty);
            }

            StringBuilder sb = new StringBuilder();
            string[] lines = renderedText.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
            foreach (string line in lines)
            {
                if (isWrap)
                {
                    _ = sb.AppendLine(TextWrap(line, 79, GetIndent(line, isAllBullets)));
                }
                else
                {
                    _ = sb.AppendLine(line);
                }
            }

            string result = sb.ToString();
            return (null, result);
        }

        private static (string, IssueParts) EntryKey(Content c)
        {
            if (c.Issues.Count == 0)
            {
                return (c.Text, IssueParts.IssueKey(string.Empty));
            }

            return (string.Empty, IssueParts.IssueKey(c.Issues.First()));
        }

        private static int BulletKey(Content c)
        {
            string text = c.Text;
            if (string.IsNullOrEmpty(text))
            {
                return -1;
            }

            if (text.StartsWith("- "))
            {
                return 0;
            }

            if (text.StartsWith("* "))
            {
                return 1;
            }

            if (text.StartsWith("#. "))
            {
                return 2;
            }

            return 3;
        }

        private static string GetIndent(string text, bool allBullets)
        {
            if (allBullets || text.StartsWith("- ") || text.StartsWith("* "))
            {
                return "  ";
            }

            if (text.StartsWith("#. "))
            {
                return "   ";
            }

            return string.Empty;
        }

        internal static string TextWrap(string text, int width, string subsequentIndent)
        {
            string[] words = text.Split(' ');
            StringBuilder line = new StringBuilder();
            int currentLineLength = 0;

            for (int i = 0; i < words.Length; ++i)
            {
                string word = words[i];

                _ = line.Append(word);
                currentLineLength += word.Length;

                if (i == words.Length - 1)
                {
                    break;
                }

                if (currentLineLength + words[i + 1].Length >= width)
                {
                    _ = line.AppendLine();
                    _ = line.Append(subsequentIndent);
                    currentLineLength = subsequentIndent.Length;
                }
                else
                {
                    _ = line.Append(' ');
                    currentLineLength += 1;
                }
            }

            string ret = line.ToString().TrimEnd();
            return ret;
        }
    }
}
