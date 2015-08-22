using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace JackCompiler
{
    //TODO: Add support for // and /**/ comments

    internal sealed class Tokenizer
    {
        private TextReader _source;
        private bool _done;
        private Token _currentToken;
        private int _currentLine = 1;
        
        public Tokenizer(TextReader source)
        {
            Contract.Requires(source != null);

            _source = source;
            _done = false;
            Advance();
        }

        private char NextChar()
        {
            int c = _source.Read();
            if (c == -1)
                throw new EndOfStreamException("Expected more data");
            if (c == '\n')
            {
                ++_currentLine;
            }
            return (char) c;
        }
        private char LookAhead()
        {
            return (char)_source.Peek();
        }
        private bool IsAtEnd
        {
            get
            {
                return _source.Peek() == -1;
            }
        }
        private void EatWhitespace()
        {
            while (Syntax.IsWhitespace(LookAhead()))
            {
                NextChar();
            }
        }
        private string EatWhile(Predicate<char> pred)
        {
            string result = String.Empty;
            while (pred(LookAhead()))
            {
                result += NextChar();
            }
            return result;
        }

        public int CurrentLine { get { return _currentLine; } }

        public void Advance()
        {
            EatWhitespace();

            if (IsAtEnd)
            {
                _done = true;
                return;
            }
            char nextChar = NextChar();
            if (Syntax.IsSymbol(nextChar))
            {
                //This token is going to be a symbol. There are three special
                //look-ahead cases for '<=', '>=', and '!='.
                if ((new[] { '<', '>', '!' }.Contains(nextChar)) && LookAhead() == '=')
                {
                    NextChar();//Eat the '='
                    _currentToken = new Token(TokenType.Symbol, nextChar + "=");
                }
                else
                {
                    _currentToken = new Token(TokenType.Symbol, nextChar.ToString());
                }
            }
            else if (Syntax.IsNumber(nextChar))
            {
                //This token is going to be an integer constant.
                string intConst = nextChar.ToString();
                intConst += EatWhile(Syntax.IsNumber);

                int result;
                if (!int.TryParse(intConst, out result))
                {
                    throw new CompilationException(
                        "Integer constant must be in range [0,2147483648), but got: "
                        + intConst, _currentLine);
                }

                _currentToken = new Token(TokenType.IntConst, intConst);
            }
            else if (Syntax.IsCharOrdinalStart(nextChar))
            {
                //This is an addition to the 'standard' Jack language. We
                //support also character ordinals of the form 'H' or of the
                //form '\nnn' where nnn are decimal digits. They are translated
                //to integer constants as far as the parser is concerned.
                char marker = NextChar();
                if (marker == '\\')
                {
                    string code = EatWhile(Syntax.IsNumber);
                    if (code.Length != 3)
                    {
                        throw new CompilationException("Expected: \\nnn where n are decimal digits", _currentLine);
                    }
                    int value = int.Parse(code);
                    if (value >= 256)
                    {
                        throw new CompilationException("Character ordinal is out of range [0,255]", _currentLine);
                    }
                    _currentToken = new Token(TokenType.IntConst, value.ToString());
                }
                else
                {
                    _currentToken = new Token(TokenType.IntConst, ((int)marker).ToString());
                }
                NextChar();//Swallow the end of the character ordinal
            }
            else if (Syntax.IsStringConstantStart(nextChar))
            {
                //This token is going to be a string constant.
                string strConst = EatWhile(c => !Syntax.IsStringConstantStart(c));
                NextChar();//Swallow the end of the string constant
                _currentToken = new Token(TokenType.StrConst, strConst);
            }
            else if (Syntax.IsStartOfKeywordOrIdent(nextChar))
            {
                //This is going to be a keyword or an identifier. We can't
                //tell what it's going to be until we accumulate more of it.
                //We need to read until we encounter whitespace.
                string keywordOrIdent = nextChar.ToString();
                keywordOrIdent += EatWhile(Syntax.IsPartOfKeywordOrIdent);
                if (Syntax.IsKeyword(keywordOrIdent))
                {
                    _currentToken = new Token(TokenType.Keyword, keywordOrIdent);
                }
                else
                {
                    _currentToken = new Token(TokenType.Ident, keywordOrIdent);
                }
            }
            else
            {
                throw new CompilationException("Unexpected character: " + nextChar, _currentLine);
            }
        }

        public bool HasNext
        {
            get
            {
                return !_done;
            }
        }

        public Token LookAheadToken
        {
            get { return _currentToken; }
        }

        public Token Next()
        {
            Token temp = LookAheadToken;
            Advance();
            return temp;
        }
    }
}