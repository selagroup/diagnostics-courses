namespace JackCompiler
{
    internal enum TokenType
    {
        Keyword,
        Symbol,
        Ident,
        IntConst,
        StrConst
    }

    internal sealed class Token
    {
        public TokenType Type { get; private set; }
        public string Value { get; private set; }

        public Token(TokenType tokType, string tokValue)
        {
            Type = tokType;
            Value = tokValue;
        }

        public override string ToString()
        {
            return Type + "\t\t" + Value;
        }
    }
}