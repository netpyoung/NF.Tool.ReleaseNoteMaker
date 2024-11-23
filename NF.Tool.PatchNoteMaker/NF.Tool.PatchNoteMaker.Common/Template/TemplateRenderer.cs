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


        public static async Task<(Exception?, string)> RenderFragments(
            string templateFpath,
            [NotNull] PatchNoteConfig config,
            VersionData versionData,
            [NotNull] Fragment fragment,
            bool isRenderTitle)
        {
            //    top_underline = config.underlines[0],
            //    config.underlines[1:],
            //string topUnderline = "=";
            //List<string> underlines = new List<string> { "", "" };

            string issueFormat = config.Maker.IssueFormat;
            bool isWrap = config.Maker.IsWrap;
            bool isAllBullets = config.Maker.IsAllBullets;

            // dic/str sectionName, str categoryName, [categories str appendnewlinecodeblock [str formattedIssues]]
            Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> data = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();

            // dic/str sectionName categoryName issues.orderd.renderd.
            Dictionary<string, Dictionary<string, List<string>>> issuesByCategory = new Dictionary<string, Dictionary<string, List<string>>>();

            foreach ((string sectionName, Section section) in fragment)
            {
                data[sectionName] = new Dictionary<string, Dictionary<string, List<string>>>();
                issuesByCategory[sectionName] = new Dictionary<string, List<string>>();

                foreach (string categoryName in section.Keys)
                {
                    HashSet<string> categoryIssues = new HashSet<string>();
                    List<(string text, List<string> issues)> entries = new List<(string text, List<string> issues)>();

                    foreach ((string text, List<string> issues) in section[categoryName])
                    {
                        entries.Add((text, issues.OrderBy(issueKey).ToList()));
                        foreach (string issue in issues)
                        {
                            categoryIssues.Add(issue);
                        }
                    }

                    entries = entries
                        .OrderBy(entryKey)
                        .ThenBy(e => isAllBullets ? 0 : bulletKey(e.text))
                        .ToList();

                    Dictionary<string, List<string>> categories = new Dictionary<string, List<string>>();
                    foreach ((string text, List<string> issues) in entries)
                    {
                        List<string> formattedIssues = issues.Select(issue => RenderIssue(issueFormat, issue)).ToList();
                        categories[AppendNewlinesIfTrailingCodeBlock(text)] = formattedIssues;
                    }

                    data[sectionName][categoryName] = categories;
                    issuesByCategory[sectionName][categoryName] = categoryIssues
                        .OrderBy(issueKey)
                        .Select(issue => RenderIssue(issueFormat, issue))
                        .ToList();
                }
            }

            // underlines,
            // topUnderline,
            // sections = data,
            // issuesByCategory
            TemplateModel model = TemplateModel.Create(versionData, isRenderTitle, fragment, config.Types);
            (Exception? exOrNull, string renderedText) = await Render(templateFpath, config, model);
            if (exOrNull != null)
            {
                return (exOrNull, string.Empty);
            }

            StringBuilder sb = new StringBuilder();
            foreach (string line in renderedText.Split('\n'))
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

        private static string RenderIssue(string format, string issue)
        {
            if (!string.IsNullOrEmpty(format))
            {
                return string.Format(format, issue);
            }
            return issue;
        }

        private static string AppendNewlinesIfTrailingCodeBlock(string text)
        {
            return text;
        }

        private static int issueKey(string issue)
        {
            return int.Parse(issue.TrimStart('#'));
        }
        private static int entryKey((string text, List<string> issues) entry)
        {
            return entry.text.Length;
        }

        private static int bulletKey(string text)
        {
            return text.StartsWith("- ") || text.StartsWith("* ") ? 1 : 0;
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

            return "";
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
