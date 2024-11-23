using System;
using System.Collections.Generic;
using System.IO;

namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    public record struct ScopedFileDeleter : IDisposable
    {
        private readonly List<string> _willDeleteFilePaths;

        [Obsolete("", error: true)]
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

        public string Register(string path)
        {
            _willDeleteFilePaths.Add(path);
            return path;
        }

        public void Dispose()
        {
            foreach (string path in _willDeleteFilePaths)
            {
                File.Delete(path);
            }
            _willDeleteFilePaths.Clear();
        }
    }
}
