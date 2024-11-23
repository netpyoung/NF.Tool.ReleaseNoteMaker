using System;

namespace NF.Tool.PatchNoteMaker.Common
{
    public sealed class PatchNoteMakerException : Exception
    {
        public override string StackTrace { get; }

        public PatchNoteMakerException(string message) : base(message)
        {
            StackTrace = Environment.StackTrace;
        }

        public PatchNoteMakerException() : base()
        {
            StackTrace = Environment.StackTrace;
        }

        public PatchNoteMakerException(string message, Exception innerException) : base(message, innerException)
        {
            StackTrace = Environment.StackTrace;
        }
    }
}
