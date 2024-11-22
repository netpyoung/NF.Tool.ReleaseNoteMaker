using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    internal static class GitHelper
    {

        public static void StageNewsfile(string baseDirectory, string path)
        {
            string filePath = Path.Combine(baseDirectory, path);
            Call("git", $@"add ""{filePath}""");
        }

        public static void RemoveFiles(string[] fragmentFpaths)
        {
            if (fragmentFpaths.Length == 0)
            {
                return;
            }

            string[] gitFragmentFpaths = Call("git", $"ls-files {string.Join(' ', fragmentFpaths)}").Split("\n");
            gitFragmentFpaths = gitFragmentFpaths.Where(x => File.Exists(x)).Select(x => Path.GetFullPath(x)).ToArray();
            Call("git", $"rm --quiet --force {string.Join(' ', gitFragmentFpaths)}");

            string[] unknownFragmentFpaths = fragmentFpaths.Except(gitFragmentFpaths).ToArray();
            foreach (string x in unknownFragmentFpaths)
            {
                File.Delete(x);
            }
        }

        public static string Call(string fileName, string arguments)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;

                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return output;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
