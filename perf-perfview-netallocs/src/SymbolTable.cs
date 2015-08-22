using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace JackCompiler
{
    internal enum SubroutineKind
    {
        None = 0,
        Constructor,
        Function,
        Method
    }

    internal sealed class Subroutine
    {
        public SubroutineKind Kind { get; private set; }
        public string ClassName { get; private set; }
        public string Name { get; private set; }
        public string ReturnType { get; private set; }
        public IList<Symbol> Parameters { get; private set; }
        public IList<Symbol> Locals { get; private set; }

        public Subroutine(SubroutineKind kind, string className, string name, string returnType)
        {
            Kind = kind;
            ClassName = className;
            Name = name;
            ReturnType = returnType;
            Parameters = new List<Symbol>();
            Locals = new List<Symbol>();
        }
    }

    internal enum SymbolKind
    {
        Static,
        Field,
        Local,
        Parameter
    }

    internal sealed class Symbol
    {
        public string Name { get; private set; }
        public string Type { get; private set; }
        public SymbolKind Kind { get; private set; }
        public string DeclaringClassName { get; private set; }

        public Symbol(string symType, string symName,
                      SymbolKind symKind, string declaringClassName)
        {
            Type = symType;
            Name = symName;
            Kind = symKind;
            DeclaringClassName = declaringClassName;
        }
    }

    internal sealed class SymbolTable
    {
        private Dictionary<string,Symbol> _symbols = new Dictionary<string, Symbol>();

        [Pure]
        public bool HasSymbol(string symName)
        {
            return _symbols.ContainsKey(symName);
        }

        public void AddSymbol(Symbol symbol)
        {
            Contract.Requires(symbol != null &&
                              symbol.Name != null &&
                              symbol.Type != null);
            Contract.Requires(!HasSymbol(symbol.Name));

            _symbols.Add(symbol.Name, symbol);
        }

        public Symbol GetSymbol(string symName)
        {
            Contract.Requires(HasSymbol(symName));
            
            return _symbols[symName];
        }

        public void Clear()
        {
            _symbols.Clear();
        }
    }
}