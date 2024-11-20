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

        public static (string baseDirectory, PatchNoteConfig config) GetConfig(string directory, string configPath)
        {
            // TODO(pyoung): refactoring without throw
            if (string.IsNullOrEmpty(configPath))
            {
                return TraverseToParentForConfig(directory);
            }

            string configFullPath = Path.GetFullPath(configPath);
            string baseDirectory;
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

            PatchNoteConfig config = LoadConfigFromFile(configFullPath);
            return (baseDirectory, config);
        }

        private static (string directory, PatchNoteConfig config) TraverseToParentForConfig(string path)
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

            string directory = startDirectory;
            while (true)
            {
                string configPath = Path.Combine(directory, Const.DEFAULT_CONFIG_FILENAME);
                if (File.Exists(configPath))
                {
                    PatchNoteConfig config = LoadConfigFromFile(configPath);
                    return (directory, config);
                }

                DirectoryInfo? parentOrNull = Directory.GetParent(directory);
                if (parentOrNull == null)
                {
                    throw new PatchNoteMakerException($"No configuration file found. Looked back from: {startDirectory}");
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
