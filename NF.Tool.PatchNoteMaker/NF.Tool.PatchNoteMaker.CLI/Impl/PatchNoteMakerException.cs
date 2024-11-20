using System;

namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    internal sealed class PatchNoteMakerException : Exception
    {
        public PatchNoteMakerException(string message) : base(message)
        {
        }
    }
}
