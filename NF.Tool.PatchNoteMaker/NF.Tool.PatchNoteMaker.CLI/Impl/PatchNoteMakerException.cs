using System;

namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    internal sealed class PatchNoteMakerException : Exception
    {
        public new string StackTrace { get; init; }

        public PatchNoteMakerException(string message) : base(message)
        {
            StackTrace = Environment.StackTrace;
        }
    }
}
