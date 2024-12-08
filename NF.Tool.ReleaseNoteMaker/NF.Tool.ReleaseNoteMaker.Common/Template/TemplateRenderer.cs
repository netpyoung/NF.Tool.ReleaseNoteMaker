using NF.Tool.ReleaseNoteMaker.Common.Config;
using NF.Tool.ReleaseNoteMaker.Common.Fragments;
using SmartFormat;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NF.Tool.ReleaseNoteMaker.Common.Template
{
    public sealed class TemplateRenderer
    {
        public static async Task<(Exception? exOrNull, string text)> Render(string templatePath, ReleaseNoteConfig config, TemplateModel templateModel)
        {
            string tempFilePath = Path.GetTempFileName();
            string assemblyLocation = typeof(ReleaseNoteTemplateGenerator).Assembly.Location;

            ReleaseNoteTemplateGenerator generator = new ReleaseNoteTemplateGenerator(config, templateModel);
            generator.Refs.Add(assemblyLocation);
            generator.Imports.Add(typeof(ReleaseNoteTemplateGenerator).Namespace);
            generator.Imports.Add(typeof(ReleaseNoteConfig).Namespace);

            bool isSuccess = await generator.ProcessTemplateAsync(templatePath, tempFilePath);
            if (!isSuccess)
            {
                StringBuilder sb = new StringBuilder();
                _ = sb.AppendLine("Template Render Error");
                foreach (CompilerError err in generator.Errors)
                {
                    _ = sb.AppendLine(err.ToString());
                }
                ReleaseNoteMakerException ex = new ReleaseNoteMakerException(sb.ToString());
                return (ex, string.Empty);
            }

            string text = await File.ReadAllTextAsync(tempFilePath);
            File.Delete(tempFilePath);
            return (null, text);
        }

        public static async Task<(Exception?, string)> RenderFragments(string templateFpath, [NotNull] ReleaseNoteConfig config, ProjectData projectData, List<FragmentContent> fragmentContents)
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
                    foreach (IGrouping<string, FragmentContent> grpData in grpCategory.OrderBy(x => IssuePart.IssueKey(x.FragmentBasename.Issue)).GroupBy(x => x.Data))
                    {
                        List<string> trimedIssues = grpData.Select(x => x.FragmentBasename.Issue).Where(x => !string.IsNullOrEmpty(x)).OrderBy(IssuePart.IssueKey).ToList();
                        xs.Add(new Content(grpData.Key, trimedIssues));
                    }

                    List<Content> contents = new List<Content>(xs.Count);
                    foreach (Content x in xs.OrderBy(EntryKey))
                    {
                        List<string> formattedIssues = x.Issues.Select(x => RenderIssue(issueFormat, x)).ToList();
                        string text = AppendNewlinesIfTrailingCodeBlock(x.Text);
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
                            List<string> issues = grpCategory.Select(x => x.FragmentBasename.Issue).Where(x => !string.IsNullOrEmpty(x)).OrderBy(IssuePart.IssueKey).ToList();
                            List<string> formattedIssues = issues.Select(x => RenderIssue(issueFormat, x)).ToList();
                            categories.Add(new Category(categoryDisplayName, contents, formattedIssues));
                        }
                    }
                }

                sections.Add(new Section(grpSection.Key, categories));
            }

            // TODO(pyoung): handle - underlines, topUnderline

            TemplateModel model = new TemplateModel(isRenderTitle, projectData, sections);
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
                    _ = sb.Append(TextWrap(line, 79, GetIndent(line, isAllBullets)));
                    _ = sb.Append('\n');
                }
                else
                {
                    _ = sb.Append(line);
                    _ = sb.Append('\n');
                }
            }

            string result = sb.ToString();
            return (null, result);
        }

        private static (string, IssuePart) EntryKey(Content c)
        {
            if (c.Issues.Count == 0)
            {
                return (c.Text, IssuePart.IssueKey(string.Empty));
            }

            return (string.Empty, IssuePart.IssueKey(c.Issues.First()));
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
            StringBuilder sb = new StringBuilder();
            int currentLineLength = 0;

            for (int i = 0; i < words.Length; ++i)
            {
                string word = words[i];

                _ = sb.Append(word);
                currentLineLength += word.Length;

                if (i == words.Length - 1)
                {
                    break;
                }

                if (currentLineLength + words[i + 1].Length >= width)
                {
                    _ = sb.Append('\n');
                    _ = sb.Append(subsequentIndent);
                    currentLineLength = subsequentIndent.Length;
                }
                else
                {
                    _ = sb.Append(' ');
                    currentLineLength += 1;
                }
            }

            string ret = sb.ToString().TrimEnd();
            return ret;
        }

        public static string RenderIssue(string issueFormat, string issue)
        {
            if (!string.IsNullOrEmpty(issueFormat))
            {
                string renderedIssue = Smart.Format(issueFormat, new { Issue = issue });
                return renderedIssue;
            }

            if (int.TryParse(issue, out int issueNumber))
            {
                string renderedIssue = $"#{issueNumber}";
                return renderedIssue;
            }
            return issue;
        }
        public static string AppendNewlinesIfTrailingCodeBlock(string text)
        {
            string indentedText = @"  [ \t]+[^\n]*";
            string emptyOrIndentedTextLines = $"(({indentedText})?\n)*";
            string regex = @"::\n\n" + emptyOrIndentedTextLines + indentedText + "$";
            bool isTrailingCodeBlock = Regex.IsMatch(text, regex);
            if (isTrailingCodeBlock)
            {
                return $"{text}\n\n ";
            }

            return text;
        }
    }
}
