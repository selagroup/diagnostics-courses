using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JackCompiler
{
    internal sealed class LabelStack
    {
        private readonly string _prefix;
        private int _current;
        private readonly Stack<string> _stack = new Stack<string>();

        public LabelStack(string prefix)
        {
            _prefix = prefix;
        }

        public string Push()
        {
            _stack.Push(_prefix + _current++);
            return _stack.Peek();
        }

        public string Pop()
        {
            return _stack.Pop();
        }
    }
}
