using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    internal sealed class TextEditorHelper
    {
        public static async Task<string?> OpenAndReadTemporaryFile(string tempFilename, string initialContent)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), tempFilename);

            File.WriteAllText(tempFilePath, initialContent);

            DateTime lastWriteTime = File.GetLastWriteTime(tempFilePath);

            string editor = GetEditorName();

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = editor,
                    Arguments = tempFilePath,
                    UseShellExecute = false
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                return null;
            }

            if (File.GetLastWriteTime(tempFilePath) == lastWriteTime)
            {
                return null;
            }

            string txt = File.ReadAllText(tempFilePath);
            File.Delete(tempFilePath);
            string filteredTxt = ProcessContent(txt);
            return filteredTxt;
        }
        public static string ProcessContent(string content)
        {
            string[] allLines = content.Split('\n');

            IEnumerable<string> filteredLines = allLines
                .Where(line => !line.TrimStart().StartsWith("#"))
                .Select(line => line.TrimEnd());

            return string.Join("\n", filteredLines).Trim();
        }

        public static string GetEditorName()
        {
            string? visualEditorOrNull = Environment.GetEnvironmentVariable("VISUAL");
            if (!string.IsNullOrEmpty(visualEditorOrNull))
            {
                return visualEditorOrNull;
            }

            string? defaultEditorOrNull = Environment.GetEnvironmentVariable("EDITOR");
            if (!string.IsNullOrEmpty(defaultEditorOrNull))
            {
                return defaultEditorOrNull;
            }

            if (OperatingSystem.IsWindows())
            {
                return "notepad";
            }

            foreach (string editorOption in new string[] { "sensible-editor", "vim", "nano" })
            {
                if (IsCommandAvailable(editorOption))
                {
                    return editorOption;
                }
            }
            return "vi";
        }

        private static bool IsCommandAvailable(string command)
        {
            try
            {
                using Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "which",
                        Arguments = command,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
