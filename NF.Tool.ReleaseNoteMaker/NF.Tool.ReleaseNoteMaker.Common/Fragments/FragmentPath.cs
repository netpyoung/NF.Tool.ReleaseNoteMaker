using NF.Tool.ReleaseNoteMaker.Common.Config;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace NF.Tool.ReleaseNoteMaker.Common.Fragments
{
    public readonly record struct FragmentPath
    {
        private readonly string _baseDirectory;
        private readonly string _appendDirectory;
        private readonly ReleaseNoteConfig _config;

        public static FragmentPath Get(string baseDirectory, [NotNull] ReleaseNoteConfig config)
        {
            return new FragmentPath(baseDirectory, config);
        }

        [Obsolete("Do not use constructor", error: true)]
        public FragmentPath()
        {
            throw new InvalidOperationException();
        }

        private FragmentPath(string baseDirectory, ReleaseNoteConfig config)
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

        public string GetDirectory(string sectionPath)
        {
            ReleaseNoteSection? sectionOrNull = _config.Sections.Find(x => x.Path == sectionPath)!;
            Debug.Assert(sectionOrNull != null, $"sectionOrNull != null | sectionPath: {sectionPath}");

            string dir = Path.Combine(_baseDirectory, sectionOrNull.Path, _appendDirectory);
            return dir;
        }
    }
}