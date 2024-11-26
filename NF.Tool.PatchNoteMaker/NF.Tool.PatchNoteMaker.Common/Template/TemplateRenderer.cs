using NF.Tool.PatchNoteMaker.Common.Config;
using NF.Tool.PatchNoteMaker.Common.Fragments;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NF.Tool.PatchNoteMaker.Common.Template
{
    public sealed class TemplateRenderer
    {
        public static async Task<(Exception? exOrNull, string text)> Render(string templatePath, PatchNoteConfig config, TemplateModel templateModel)
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

        public static async Task<Exception?> Render(string templatePath, PatchNoteConfig config, TemplateModel templateModel, string outputPath)
        {
            string assemblyLocation = typeof(PatchNoteTemplateGenerator).Assembly.Location;

            PatchNoteTemplateGenerator generator = new PatchNoteTemplateGenerator(config, templateModel);
            generator.Refs.Add(assemblyLocation);
            generator.Imports.Add(typeof(PatchNoteTemplateGenerator).Namespace);
            generator.Imports.Add(typeof(PatchNoteConfig).Namespace);

            bool isSuccess = await generator.ProcessTemplateAsync(templatePath, outputPath);
            if (!isSuccess)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Template Render Error");
                foreach (CompilerError err in generator.Errors)
                {
                    sb.AppendLine(err.ToString());
                }
                return new PatchNoteMakerException(sb.ToString());
            }

            return null;
        }


        public static async Task<(Exception?, string)> RenderFragments(string templateFpath, [NotNull] PatchNoteConfig config, VersionData versionData, List<FragmentContent> fragmentContents)
        {
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
                    List<Content> contents = new List<Content>();
                    foreach (IGrouping<string, FragmentContent> grpData in grpCategory.OrderBy(x => IssueParts.IssueKey(x.FragmentBasename.Issue)).GroupBy(x => x.Data))
                    {
                        List<string> issues = grpData.Select(x => x.FragmentBasename.Issue).OrderBy(IssueParts.IssueKey).ToList();
                        List<string> formattedIssues = issues.Select(x => Issue.RenderIssue(issueFormat, x)).ToList();
                        string text = Issue.AppendNewlinesIfTrailingCodeBlock(grpData.Key);
                        contents.Add(new Content(text, formattedIssues));
                    }
                    string category = grpCategory.Key;
                    string categoryDisplayName = config.Types.Find(x => x.Category == category)!.DisplayName;
                    categories.Add(new Category(categoryDisplayName, contents));
                }

                sections.Add(new Section(grpSection.Key, categories));
            }


            //entries = entries
            //    .OrderBy(entryKey)
            //    .ThenBy(e => isAllBullets ? 0 : bulletKey(e.text))
            //    .ToList();

            // underlines,
            // topUnderline,
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
                    sb.AppendLine(TextWrap(line, 79, GetIndent(line, isAllBullets)));
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            string result = sb.ToString();
            return (null, result);
        }

        private static int bulletKey(string text)
        {
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

        private static string TextWrap(string text, int width, string subsequentIndent)
        {
            string[] words = text.Split(' ');
            StringBuilder line = new StringBuilder();
            int currentLineLength = 0;

            foreach (string word in words)
            {
                if (currentLineLength + word.Length + 1 > width)
                {
                    line.AppendLine();
                    line.Append(subsequentIndent);
                    currentLineLength = 0;
                }
                line.Append(word + " ");
                currentLineLength += word.Length + 1;
            }

            return line.ToString().TrimEnd();
        }
    }
}
