using System;

namespace NF.Tool.PatchNoteMaker.CLI.Impl
{
    internal sealed class PatchNoteMakerException : Exception
    {
        public override string StackTrace { get; }

        public PatchNoteMakerException(string message) : base(message)
        {
            StackTrace = Environment.StackTrace;
        }
    }
}
