using System;
using System.Collections.Generic;

namespace JackCompiler
{
    internal static class Syntax
    {
        private static HashSet<string> _keywords;
        private static HashSet<char> _symbols;
        static Syntax()
        {
            _keywords = new HashSet<string>
                            {
                                "if", "let", "do", "else", "while",
                                "return", "true", "false", "null",
                                "this", "class", "constructor",
                                "function", "method", "field", "static",
                                "var", "int", "char", "boolean"
                            };
            _symbols = new HashSet<char>
                           {
                               '(', ')', '{', '}', '[', ']',
                               '<', '>', '=', ';', '!', '&',
                               '|', '+', '-', '*', '/', '.',
                               ',', '%'
                           };
        }

        public static bool IsKeyword(string token)
        {
            return _keywords.Contains(token);
        }
        public static bool IsSymbol(char token)
        {
            return _symbols.Contains(token);
        }
        public static bool IsNumber(char token)
        {
            return Char.IsDigit(token);
        }
        public static bool IsCharOrdinalStart(char token)
        {
            return token == '\'';
        }
        public static bool IsStringConstantStart(char token)
        {
            return token == '"';
        }
        public static bool IsWhitespace(char token)
        {
            return Char.IsWhiteSpace(token);
        }
        public static bool IsPartOfKeywordOrIdent(char token)
        {
            return Char.IsLetterOrDigit(token) || token == '_';
        }
        public static bool IsStartOfKeywordOrIdent(char token)
        {
            return Char.IsLetter(token) || token == '_';
        }
    }
}