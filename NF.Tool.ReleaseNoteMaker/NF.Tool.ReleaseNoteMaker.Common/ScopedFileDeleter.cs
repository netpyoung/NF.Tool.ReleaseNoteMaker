using System;
using System.Collections.Generic;
using System.IO;

namespace NF.Tool.ReleaseNoteMaker.Common
{
    public readonly record struct ScopedFileDeleter : IDisposable
    {
        private readonly List<string> _willDeleteFilePaths;

        [Obsolete("Do not use constructor", error: true)]
        public ScopedFileDeleter()
        {
            throw new InvalidOperationException();
        }

        public ScopedFileDeleter(List<string> list)
        {
            _willDeleteFilePaths = list;
        }

        public static ScopedFileDeleter Using()
        {
            return new ScopedFileDeleter(new List<string>(20));
        }

        public readonly string Register(string path)
        {
            _willDeleteFilePaths.Add(path);
            return path;
        }

        public readonly void Dispose()
        {
            foreach (string path in _willDeleteFilePaths)
            {
                File.Delete(path);
            }
            _willDeleteFilePaths.Clear();
        }
    }
}
