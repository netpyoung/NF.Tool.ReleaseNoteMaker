using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NF.Tool.ReleaseNoteMaker.CLI.Impl
{
    internal static class GitHelper
    {

        public static void StageNewsfile(string fpath)
        {
            _ = Cmd.Call("git", $@"add ""{fpath}""");
        }

        public static void RemoveFiles(string[] fragmentFpaths)
        {
            if (fragmentFpaths.Length == 0)
            {
                return;
            }

            string[] gitFragmentFpaths = Cmd.Call("git", $"ls-files {string.Join(' ', fragmentFpaths)}").Split("\n");
            gitFragmentFpaths = gitFragmentFpaths.Where(File.Exists).Select(Path.GetFullPath).ToArray();
            _ = Cmd.Call("git", $"rm --quiet --force {string.Join(' ', gitFragmentFpaths)}");

            string[] unknownFragmentFpaths = fragmentFpaths.Except(gitFragmentFpaths).ToArray();
            foreach (string x in unknownFragmentFpaths)
            {
                File.Delete(x);
            }
        }
    }

    internal static class Cmd
    {
        public static string Call(string fileName, string arguments)
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;

                    _ = process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string outputErr = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    return output;
                }
            }
            catch
            {
                return string.Empty;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        public static (Exception? exOrNull, int exitCode, string stdOut, string stdErr) Call2(string fileName, string arguments)
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;

                    _ = process.Start();
                    string stdOut = process.StandardOutput.ReadToEnd();
                    string stdErr = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    int exitCode = process.ExitCode;
                    return (null, exitCode, stdOut, stdErr);
                }
            }
            catch (Exception ex)
            {
                return (ex, 0, string.Empty, string.Empty);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }
}
