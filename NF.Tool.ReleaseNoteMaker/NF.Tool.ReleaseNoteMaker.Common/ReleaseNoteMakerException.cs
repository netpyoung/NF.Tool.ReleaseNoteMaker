using System;

namespace NF.Tool.ReleaseNoteMaker.Common
{
    public sealed class ReleaseNoteMakerException : Exception
    {
        public override string StackTrace { get; }

        public ReleaseNoteMakerException(string message) : base(message)
        {
            StackTrace = Environment.StackTrace;
        }

        public ReleaseNoteMakerException() : base()
        {
            StackTrace = Environment.StackTrace;
        }

        public ReleaseNoteMakerException(string message, Exception innerException) : base(message, innerException)
        {
            StackTrace = Environment.StackTrace;
        }
    }
}
