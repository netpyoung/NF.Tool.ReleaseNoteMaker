using System;
using System.Collections.Generic;
using System.IO;

namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    public record struct ScopedFileDeleter : IDisposable
    {
        private readonly List<string> _willDeleteFilePaths = new List<string>(20);
        public ScopedFileDeleter()
        {
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
