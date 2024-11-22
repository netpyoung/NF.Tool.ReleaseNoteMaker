using NF.Tool.PatchNoteMaker.Common;
using System.IO;

namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    public sealed class FragmentsPath
    {
        private readonly string _baseDirectory;
        private readonly string _appendDirectory;
        private readonly PatchNoteConfig _config;

        public FragmentsPath(string baseDirectory, PatchNoteConfig config)
        {
            _config = config;
            if (!string.IsNullOrEmpty(config.Maker.Directory))
            {
                _baseDirectory = Path.Combine(baseDirectory, config.Maker.Directory);
                _appendDirectory = string.Empty;
            }
            else
            {
                _baseDirectory = baseDirectory;
                _appendDirectory = "newsfragments";
            }
        }

        public string Resolve(string section)
        {
            return Path.Combine(_baseDirectory, section);
        }

        public string GetDirectory(string section)
        {
            PatchNoteConfig.PatchNoteSection s = _config.Sections.Find(x => Utils.IsSameIgnoreCase(x.Path, section))!;
            string dir = Path.Combine(_baseDirectory, s.Path, _appendDirectory);
            return dir;

        }
    }
}