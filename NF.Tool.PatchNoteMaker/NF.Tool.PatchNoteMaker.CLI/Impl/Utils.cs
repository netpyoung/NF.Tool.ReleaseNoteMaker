using NF.Tool.PatchNoteMaker.Common;
using NF.Tool.PatchNoteMaker.Common.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Tomlyn;
using Tomlyn.Syntax;

namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    internal static class Utils
    {
        public static string ExtractResourceToTempFilePath(string resourceName)
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
            if (string.IsNullOrEmpty(configPath))
            {
                return TraverseToParentForConfig(directory, out baseDirectory, out config);
            }

            string configFpath = Path.GetFullPath(configPath);
            if (!string.IsNullOrEmpty(directory))
            {
                baseDirectory = Path.GetFullPath(directory);
            }
            else
            {
                baseDirectory = Path.GetDirectoryName(configFpath)!;
            }

            if (!File.Exists(configFpath))
            {
                config = new PatchNoteConfig();
                return new PatchNoteMakerException($"Configuration file '{configFpath}' not found.");
            }

            (Exception? exOrNull, config) = LoadConfigFromFile(configFpath);
            return exOrNull;
        }

        private static Exception? TraverseToParentForConfig(string path, out string directoryFpath, out PatchNoteConfig config)
        {
            string startDirectoryFpath;
            if (!string.IsNullOrEmpty(path))
            {
                startDirectoryFpath = Path.GetFullPath(path);
            }
            else
            {
                startDirectoryFpath = Directory.GetCurrentDirectory();
            }

            directoryFpath = startDirectoryFpath;
            while (true)
            {
                string configFpath = Path.Combine(directoryFpath, Const.DEFAULT_CONFIG_FILENAME);
                if (File.Exists(configFpath))
                {
                    (Exception? exOrNull, config) = LoadConfigFromFile(configFpath);
                    return exOrNull;
                }

                DirectoryInfo? parentOrNull = Directory.GetParent(directoryFpath);
                if (parentOrNull == null)
                {
                    config = new PatchNoteConfig();
                    return new PatchNoteMakerException($"No configuration file found. Looked back from: {startDirectoryFpath}");
                }
                directoryFpath = parentOrNull.FullName;
            }
        }

        private static (Exception? exOrNull, PatchNoteConfig config) LoadConfigFromFile(string configFpath)
        {
            string configText = File.ReadAllText(configFpath);
            TomlModelOptions option = new TomlModelOptions
            {
                ConvertFieldName = StringIdentity,
                ConvertPropertyName = StringIdentity
            };

            bool isSuccess = Toml.TryToModel(configText, out TomlModel? modelOrNull, out DiagnosticsBag? diagostics, options: option);
            if (!isSuccess)
            {
                StringBuilder sb = new StringBuilder();
                _ = sb.AppendLine($"TOML Parsing Error: configFpath={configFpath}");
                foreach (DiagnosticMessage x in diagostics!)
                {
                    _ = sb.AppendLine(x.ToString());
                }
                return (new PatchNoteMakerException(sb.ToString()), new PatchNoteConfig());
            }

            TomlModel model = modelOrNull!;
            PatchNoteConfig patchNoteConfig = model.PatchNote;
            if (patchNoteConfig.Sections.Count == 0)
            {
                patchNoteConfig.Sections.Add(new PatchNoteSection { DisplayName = "", Path = "" });
            }
            return (null, patchNoteConfig);
        }

        private static string StringIdentity(string x)
        {
            return x;
        }

        public static bool IsSameIgnoreCase(string x, string y)
        {
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }
    }
}
