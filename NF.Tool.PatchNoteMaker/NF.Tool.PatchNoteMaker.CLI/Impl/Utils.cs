using NF.Tool.PatchNoteMaker.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Tomlyn;
using Tomlyn.Syntax;

namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    internal static class Utils
    {
        public static string ExtractResourceText(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string tempFilePath = Path.GetTempFileName();
            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName)!)
            {
                Debug.Assert(resourceStream != null);
                using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
            return tempFilePath;
        }

        public static Exception? GetConfig(string directory, string configPath, out string baseDirectory, out PatchNoteConfig config)
        {
            // TODO(pyoung): refactoring without throw
            if (string.IsNullOrEmpty(configPath))
            {
                return TraverseToParentForConfig(directory, out baseDirectory, out config);
            }

            string configFullPath = Path.GetFullPath(configPath);
            if (!string.IsNullOrEmpty(directory))
            {
                baseDirectory = Path.GetFullPath(directory);
            }
            else
            {
                baseDirectory = Path.GetDirectoryName(configFullPath)!;
            }

            if (!File.Exists(configFullPath))
            {
                throw new PatchNoteMakerException($"Configuration file '{configFullPath}' not found.");
            }

            config = LoadConfigFromFile(configFullPath);
            return null;
        }

        private static Exception? TraverseToParentForConfig(string path, out string directory, out PatchNoteConfig config)
        {
            string startDirectory;
            if (!string.IsNullOrEmpty(path))
            {
                startDirectory = path;
            }
            else
            {
                startDirectory = Directory.GetCurrentDirectory();
            }

            directory = startDirectory;
            while (true)
            {
                string configPath = Path.Combine(directory, Const.DEFAULT_CONFIG_FILENAME);
                if (File.Exists(configPath))
                {
                    config = LoadConfigFromFile(configPath);
                    return null;
                }

                DirectoryInfo? parentOrNull = Directory.GetParent(directory);
                if (parentOrNull == null)
                {
                    config = new PatchNoteConfig();
                    return new PatchNoteMakerException($"No configuration file found. Looked back from: {startDirectory}");
                }
                directory = parentOrNull.FullName;
            }
        }

        private static PatchNoteConfig LoadConfigFromFile(string configFile)
        {
            string configText = File.ReadAllText(configFile);
            TomlModelOptions option = new TomlModelOptions();
            option.ConvertFieldName = StringIdentity;
            option.ConvertPropertyName = StringIdentity;

            bool isSuccess = Toml.TryToModel(configText, out TomlModel? modelOrNull, out DiagnosticsBag? diagostics, options: option);
            if (!isSuccess)
            {
                Console.Error.WriteLine($"configFile: {configFile}");
                foreach (DiagnosticMessage x in diagostics!)
                {
                    Console.Error.WriteLine(x);
                }
                Environment.Exit(1);
            }

            TomlModel model = modelOrNull!;
            PatchNoteConfig patchNoteConfig = model.PatchNote;
            return patchNoteConfig;
        }

        private static string StringIdentity(string x)
        {
            return x;
        }
    }
}
