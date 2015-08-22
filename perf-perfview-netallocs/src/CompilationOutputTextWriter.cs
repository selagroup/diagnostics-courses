using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    class CompilationOutputTextWriter : TextWriter
    {
        public string FinalText { get; private set; }

        public CompilationOutputTextWriter()
        {
            FinalText = "";
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override void Write(char value)
        {
            FinalText += value;
        }

        public override string ToString()
        {
            return FinalText;
        }
    }
}
