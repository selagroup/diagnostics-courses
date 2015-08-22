using System;

namespace JackCompiler
{
    internal sealed class CompilationException : Exception
    {
        public int LineNumber { get; private set; }

        public CompilationException(string message, int lineNumber = -1)
            : base(message)
        {
            LineNumber = lineNumber;
        }
    }
}