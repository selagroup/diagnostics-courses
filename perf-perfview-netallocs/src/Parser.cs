using System;
using System.Linq;
using System.Diagnostics.Contracts;

namespace JackCompiler
{
    //TODO: Support multiple classes in the same file
    //TODO: Add 'for' and 'switch' statements

    internal sealed class Parser
    {
        private Tokenizer _tokenizer;
        private ICodeGenerator _codeGenerator;
        
        private string _currentClassName = String.Empty;
        private Subroutine _currentSub;
        private bool _wasLastStatementReturn;

        private SymbolTable _classSymTable = new SymbolTable();
        private SymbolTable _methodSymTable = new SymbolTable();

        public Parser(Tokenizer tokenizer, ICodeGenerator codeGenerator)
        {
            Contract.Requires(tokenizer != null);
            Contract.Requires(codeGenerator != null);

            _tokenizer = tokenizer;
            _codeGenerator = codeGenerator;

            _codeGenerator.InitSymbolTables(
                classSymTable: _classSymTable,
                methodSymTable: _methodSymTable);
        }

        private void Expected(string what, string got = null)
        {
            ThrowCompilationException(
                "Expected: " + what +
                (got == null ? "" : (", got: " + got)));
        }
        private void Match(Token token)
        {
            if (!_tokenizer.HasNext)
                Expected(token.Value);
            
            Token actual = _tokenizer.Next();
            if (actual.Type != token.Type ||
                actual.Value != token.Value)
            {
                Expected(token.Value);
            }
        }
        private void MatchCurrent(Token token)
        {
            if (_tokenizer.LookAheadToken.Type != token.Type ||
                _tokenizer.LookAheadToken.Value != token.Value)
            {
                Expected(token.Value);
            }
        }
        private Token LookAheadToken
        {
            get { return _tokenizer.LookAheadToken; }
        }
        private Token NextToken()
        {
            if (_tokenizer.HasNext)
                return _tokenizer.Next();
            return null;
        }

        private void ThrowCompilationException(string error)
        {
            throw new CompilationException(error, _tokenizer.CurrentLine);
        }

        //program ::= class-dec
        public void Parse()
        {
            ParseClass();
        }

        //class-dec ::= 'class' <class-name> '{' <class-var-dec>* <sub-dec>* '}'
        private void ParseClass()
        {
            Match(new Token(TokenType.Keyword, "class"));
            
            Token className = NextToken();
            if (className == null)
                Expected("class name");

            if (className.Type != TokenType.Ident)
                Expected("identifier");
            
            _currentClassName = className.Value;
            _codeGenerator.BeginClass(className.Value);

            Match(new Token(TokenType.Symbol, "{"));
            ParseClassVarDecls();
            ParseSubDecls();
            Match(new Token(TokenType.Symbol, "}"));
            _codeGenerator.EndClass();

            _currentClassName = String.Empty;
            _classSymTable.Clear();
        }
        [Pure]
        private bool IsNextTokenClassVarDecl()
        {
            return _tokenizer.LookAheadToken.Type == TokenType.Keyword &&
                    (_tokenizer.LookAheadToken.Value == "static" ||
                    _tokenizer.LookAheadToken.Value == "field");
        }
        [Pure]
        private bool IsNextTokenLocalVarDecl()
        {
            return _tokenizer.LookAheadToken.Type == TokenType.Keyword &&
                    _tokenizer.LookAheadToken.Value == "var";
        }
        [Pure]
        private bool IsNextTokenSubDecl()
        {
            return _tokenizer.LookAheadToken.Type == TokenType.Keyword &&
                    (_tokenizer.LookAheadToken.Value == "constructor" ||
                    _tokenizer.LookAheadToken.Value == "function" ||
                    _tokenizer.LookAheadToken.Value == "method");            
        }

        //class-var-dec ::= ('static' | 'field') <type> <var-name> (',' <var-name>)* ';'
        private void ParseClassVarDecls()
        {
            while (IsNextTokenClassVarDecl())
            {
                ParseClassVarDecl();
            }
        }
        private void ParseClassVarDecl()
        {
            Contract.Requires(IsNextTokenClassVarDecl());

            Token varKind = NextToken();
            bool isStatic = varKind.Value == "static";
            Token varType = NextToken();
            Token varName = NextToken();

            do
            {
                if (isStatic)
                {
                    Symbol variable = new Symbol(
                        varType.Value, varName.Value, SymbolKind.Static, _currentClassName);
                    _classSymTable.AddSymbol(variable);
                    _codeGenerator.StaticDeclaration(variable);
                }
                else
                {
                    Symbol variable = new Symbol(
                        varType.Value, varName.Value, SymbolKind.Field, _currentClassName);
                    _classSymTable.AddSymbol(variable);
                    _codeGenerator.FieldDeclaration(variable);
                }
            } while (LookAheadToken.Value == ",");

            Match(new Token(TokenType.Symbol, ";"));
        }

        //sub-dec ::= ('constructor' | 'function' | 'method') ('void' | <type>) <sub-name>
        //            '(' <formal-param-list> ')' <sub-body>
        private void ParseSubDecls()
        {
            while (IsNextTokenSubDecl())
            {
                ParseSubDecl();
            }
        }
        private void ParseSubDecl()
        {
            Contract.Requires(IsNextTokenSubDecl());

            Token subKind = NextToken();
            Token returnType = NextToken();
            Token subName = NextToken();

            if (subKind.Type != TokenType.Keyword ||
                !new[]{"constructor", "method", "function"}.Contains(subKind.Value))
            {
                ThrowCompilationException("Expected: 'constructor', 'method', or 'function', but got: " + subKind.Value);
            }
            if (returnType.Type != TokenType.Keyword &&
                returnType.Type != TokenType.Ident)
            {
                ThrowCompilationException("Invalid return type '" + returnType.Value + "'");
            }
            if (subName.Type != TokenType.Ident)
            {
                ThrowCompilationException("Invalid subroutine name '" + subName.Value + "'");
            }

            _currentSub = new Subroutine(
                (SubroutineKind) Enum.Parse(typeof (SubroutineKind), subKind.Value, ignoreCase: true),
                _currentClassName, subName.Value, returnType.Value);

            if (_currentSub.Kind == SubroutineKind.Constructor &&
                _currentSub.ReturnType != _currentClassName)
            {
                ThrowCompilationException("Constructor must return '" + _currentClassName + "'");
            }

            Match(new Token(TokenType.Symbol, "("));
            ParseFormalParamList();
            Match(new Token(TokenType.Symbol, ")"));

            //NOTE: We notify the code generator of the new subroutine only
            //after we have its local variable declarations.

            ParseSubBody();

            _currentSub = null;
            _methodSymTable.Clear();
        }

        //formal-param-list ::= ((<type> <var-name>) (',' <type> <var-name>)*)?
        private void ParseFormalParamList()
        {
            while (LookAheadToken.Value != ")")
            {
                Token paramType = NextToken();
                Token paramName = NextToken();

                if (paramType.Type != TokenType.Keyword &&
                    paramType.Type != TokenType.Ident)
                {
                    ThrowCompilationException("Invalid parameter type '" + paramType.Value + "'");
                }
                if (paramName.Type != TokenType.Ident)
                {
                    ThrowCompilationException("Invalid parameter name '" + paramName.Value + "'");
                }

                Symbol parameter = new Symbol(paramType.Value, paramName.Value,
                    SymbolKind.Parameter, _currentClassName);
                _currentSub.Parameters.Add(parameter);
                _methodSymTable.AddSymbol(parameter);

                if (LookAheadToken.Value == ",")
                {
                    NextToken();//Skip the comma
                }
            }
        }

        //sub-body ::= '{' <local-var-decl>* <statement>* '}'
        private void ParseSubBody()
        {
            Match(new Token(TokenType.Symbol, "{"));
            ParseLocalVarDecls();
            
            //Notify the code generator -- now _currentSub has full information
            //about the subroutine's parameters and local variables.
            switch (_currentSub.Kind)
            {
                case SubroutineKind.Constructor:
                    _codeGenerator.ConstructorDeclaration(_currentSub);
                    break;
                case SubroutineKind.Function:
                    _codeGenerator.FunctionDeclaration(_currentSub);
                    break;
                case SubroutineKind.Method:
                    _codeGenerator.MethodDeclaration(_currentSub);
                    break;
                default:
                    ThrowCompilationException("Invalid value for _currentSub.Kind: " + _currentSub.Kind);
                    break;
            }
            
            ParseStatements();
            
            //Make sure the method ends with 'return'. If the method is void,
            //return an an arbitrary value. This could be left to the CG's
            //discretion as well.
            if (!_wasLastStatementReturn)
            {
                if (_currentSub.ReturnType == "void")
                {
                    _codeGenerator.IntConst(0);
                    _codeGenerator.Return();
                    //NOTE: This is a deviation from the original Jack syntax,
                    //in which all methods must end with a 'return' statement,
                    //even void methods.
                }
                else
                {
                    ThrowCompilationException("Non-void method must return a value");
                }
            }
            _wasLastStatementReturn = false;
            _codeGenerator.EndSubroutine();

            //This is the last thing we do because it may advance us to the
            //next line, but we want accurate compilation errors.
            Match(new Token(TokenType.Symbol, "}"));
        }
        //local-var-decl ::= 'var' <type> <var-name> (',' <var-name>)* ';'
        private void ParseLocalVarDecls()
        {
            while (IsNextTokenLocalVarDecl())
            {
                ParseLocalVarDecl();
            }
        }
        private void ParseLocalVarDecl()
        {
            Contract.Requires(IsNextTokenLocalVarDecl());

            Match(new Token(TokenType.Keyword, "var"));
            Token varType = NextToken();

            while (true)
            {
                Token varName = NextToken();

                Symbol variable = new Symbol(
                    varType.Value, varName.Value, SymbolKind.Local, _currentClassName);

                //Locals can't clash with parameter names. This will fail when
                //adding to the symbol table, but crash explicitly:
                if (_methodSymTable.HasSymbol(variable.Name))
                {
                    ThrowCompilationException(
                        "Variable name '" + variable.Name +
                        "' conflicts with existing local variable or formal parameter");
                }

                _methodSymTable.AddSymbol(variable);
                _currentSub.Locals.Add(variable);

                //NOTE: We don't notify the CG of the local declaration,
                //because we include this information when declaring the whole
                //subroutine to the CG.

                if (LookAheadToken.Value != ",")
                {
                    break;
                }
                Match(new Token(TokenType.Symbol, ","));
            }

            Match(new Token(TokenType.Symbol, ";"));
        }

        private void ParseStatements()
        {
            while (LookAheadToken.Value != "}")
            {
                ParseStatement();
            }
        }

        //statement ::= <let-statement> | <if-statement> | <while-statement> | <do-statement> | <return-statement>
        private void ParseStatement()
        {
            _wasLastStatementReturn = false;
            Token keyword = LookAheadToken;
            if (keyword.Type != TokenType.Keyword)
            {
                ThrowCompilationException("Expected a statement, got '" + keyword.Value + "'");
            }
            switch (keyword.Value)
            {
                case "let":
                    ParseLetStatement();
                    break;
                case "if":
                    ParseIfStatement();
                    break;
                case "while":
                    ParseWhileStatement();
                    break;
                case "do":
                    ParseDoStatement();
                    break;
                case "return":
                    ParseReturnStatement();
                    break;
                default:
                    ThrowCompilationException("Unrecognized statement '" + keyword.Value + "'");
                    break;
            }
        }

        //return-statement ::= 'return' <expression>? ';'
        private void ParseReturnStatement()
        {
            Match(new Token(TokenType.Keyword, "return"));
            if (LookAheadToken.Value != ";")
            {
                if (_currentSub.ReturnType == "void")
                {
                    ThrowCompilationException("A void subroutine may not return a value");
                }
                //Constructors may only return 'this' and nothing more fancy:
                if (_currentSub.Kind == SubroutineKind.Constructor &&
                    LookAheadToken.Value != "this")
                {
                    ThrowCompilationException("A constructor must return 'this'");
                }
                ParseExpression();
            }
            else
            {
                if (_currentSub.ReturnType != "void")
                {
                    ThrowCompilationException("Non-void subroutine must return a value");
                }
                //As a convention, void methods still return the value 0.
                _codeGenerator.IntConst(0);
            }
            _codeGenerator.Return();
            Match(new Token(TokenType.Symbol, ";"));
            _wasLastStatementReturn = true;
        }

        //do-statement ::= 'do' <sub-call> ';'
        private void ParseDoStatement()
        {
            Match(new Token(TokenType.Keyword, "do"));
            ParseSubCall();
            Match(new Token(TokenType.Symbol, ";"));
            _codeGenerator.DiscardReturnValueFromLastCall();
        }

        //while-statement ::= 'while' '(' <expression> ')' '{' <statement>* '}
        private void ParseWhileStatement()
        {
            //TODO: Consider 'break' support, but requires knowing the label
            //to which we should goto in case of a break (storing it at the
            //class level in a stack, because we could have nested scopes).
            
            Match(new Token(TokenType.Keyword, "while"));
            Match(new Token(TokenType.Symbol, "("));
            _codeGenerator.BeginWhile();
            ParseExpression();
            _codeGenerator.WhileCondition();
            Match(new Token(TokenType.Symbol, ")"));
            Match(new Token(TokenType.Symbol, "{"));
            ParseStatements();
            Match(new Token(TokenType.Symbol, "}"));
            _codeGenerator.EndWhile();
        }

        //if-statement ::= 'if' '(' <expression> ')' '{' <statement>* '}' ('else' '{' <statement>* '}')?
        private void ParseIfStatement()
        {
            Match(new Token(TokenType.Keyword, "if"));
            Match(new Token(TokenType.Symbol, "("));
            ParseExpression();
            _codeGenerator.BeginIf();
            Match(new Token(TokenType.Symbol, ")"));
            Match(new Token(TokenType.Symbol, "{"));
            ParseStatements();
            Match(new Token(TokenType.Symbol, "}"));
            _codeGenerator.PossibleElse();
            if (LookAheadToken.Value == "else")
            {
                Match(new Token(TokenType.Keyword, "else"));
                Match(new Token(TokenType.Symbol, "{"));
                ParseStatements();
                Match(new Token(TokenType.Symbol, "}"));
            }
            _codeGenerator.EndIf();
        }

        private Symbol GetClosestSymbol(string symName)
        {
            if (_methodSymTable.HasSymbol(symName))
                return _methodSymTable.GetSymbol(symName);
            if (_classSymTable.HasSymbol(symName))
                return _classSymTable.GetSymbol(symName);
            return null;
        }
        private void VerifyArrayAccessAllowed(Token varName)
        {
            Symbol variable = GetClosestSymbol(varName.Value);
            if (variable.Type != "Array")
            {
                ThrowCompilationException("Only arrays may be indexed with '[]'");
            }
        }

        //let-statement ::= 'let' <var-name> ('[' <expression> ']')? '=' <expression> ';'
        private void ParseLetStatement()
        {
            Match(new Token(TokenType.Keyword, "let"));
            
            Token varName = NextToken();
            if (GetClosestSymbol(varName.Value) == null)
            {
                ThrowCompilationException("Undefined variable '" + varName.Value + "'");
            }
            if (!_methodSymTable.HasSymbol(varName.Value))
            {
                Symbol variable = _classSymTable.GetSymbol(varName.Value);
                if (variable.Kind == SymbolKind.Field &&
                    _currentSub.Kind != SubroutineKind.Constructor &&
                    _currentSub.Kind != SubroutineKind.Method)
                {
                    ThrowCompilationException("Instance fields may be used only from a method or a constructor");
                }
            }

            bool withArrayIndex = false;
            if (LookAheadToken.Value == "[")
            {
                VerifyArrayAccessAllowed(varName);
                withArrayIndex = true;
                Match(new Token(TokenType.Symbol, "["));
                ParseExpression();
                Match(new Token(TokenType.Symbol, "]"));
            }
            Match(new Token(TokenType.Symbol, "="));
            ParseExpression();
            _codeGenerator.Assignment(varName, withArrayIndex);
            Match(new Token(TokenType.Symbol, ";"));

            //TODO: Type checking, but this is really hard . . . (we need
            //to propagate the type of the LHS expression so that we can
            //verify that it's the same as the RHS, and if RHS is an array
            //access then we have no way of knowing what should come from the
            //array, unless we start tracking all array types and not just
            //use 'Array' for all arrays)
        }

        private bool IsNextTokenLogicalOp()
        {
            return new[] {"&", "|"}.Contains(LookAheadToken.Value);
        }
        private bool IsNextTokenRelationalOp()
        {
            return new[] { "<", "<=", ">", ">=", "=", "!=" }.Contains(LookAheadToken.Value);
        }
        private bool IsNextTokenAddOp()
        {
            return new[] { "+", "-" }.Contains(LookAheadToken.Value);
        }
        private bool IsNextTokenMulOp()
        {
            return new[] { "*", "/", "%" }.Contains(LookAheadToken.Value);
        }

        //This BNF takes into consideration operator precedence:
        //  expression ::= <relexpr> (('&' | '|') <relexpr>)*
        //  relexpr    ::= <addexpr> (('<' | '<=' | '>' | '>=' | '=' | '!=') <addexpr>)*
        //  addexpr    ::= <mulexpr> (('+' | '-') <mulexpr>)*
        //  mulexpr    ::= <term>    (('*' | '/' | '%') <term>)*
        //  term is defined as before, see ParseTerm; thanks to that definition,
        //  unary operators have the highest precedence, as desired.
        private void ParseExpression()
        {
            //After this method completes, the code generator should have
            //available to it the result of the expression's evaluation (e.g.
            //on the stack or in a predefined register).

            ParseRelationalExpression();
            while (IsNextTokenLogicalOp())
            {
                Token logicalOp = NextToken();
                ParseRelationalExpression();
                switch (logicalOp.Value)
                {
                    case "&":
                        _codeGenerator.And();
                        break;
                    case "|":
                        _codeGenerator.Or();
                        break;
                    default:
                        ThrowCompilationException("Unexpected logical operator " + logicalOp.Value);
                        break;
                }
            }
        }
        private void ParseRelationalExpression()
        {
            ParseAddExpression();
            while (IsNextTokenRelationalOp())
            {
                Token relationalOp = NextToken();
                ParseAddExpression();
                switch (relationalOp.Value)
                {
                    case "<":
                        _codeGenerator.Less();
                        break;
                    case ">":
                        _codeGenerator.Greater();
                        break;
                    case "=":
                        _codeGenerator.Equal();
                        break;
                    case "<=":
                        _codeGenerator.LessOrEqual();
                        break;
                    case ">=":
                        _codeGenerator.GreaterOrEqual();
                        break;
                    case "!=":
                        _codeGenerator.NotEqual();
                        break;
                    default:
                        ThrowCompilationException("Unexpected relational operator " + relationalOp.Value);
                        break;
                }
            }
        }
        private void ParseAddExpression()
        {
            ParseMulExpression();
            while (IsNextTokenAddOp())
            {
                Token addOp = NextToken();
                ParseMulExpression();
                switch (addOp.Value)
                {
                    case "+":
                        _codeGenerator.Add();
                        break;
                    case "-":
                        _codeGenerator.Sub();
                        break;
                    default:
                        ThrowCompilationException("Unexpected add operator " + addOp.Value);
                        break;
                }
            }
        }
        private void ParseMulExpression()
        {
            ParseTerm();
            while (IsNextTokenMulOp())
            {
                Token mulOp = NextToken();
                ParseTerm();
                switch (mulOp.Value)
                {
                    case "*":
                        _codeGenerator.Mul();
                        break;
                    case "/":
                        _codeGenerator.Div();
                        break;
                    case "%":
                        _codeGenerator.Mod();
                        break;
                    default:
                        ThrowCompilationException("Unexpected mul operator " + mulOp.Value);
                        break;
                }
            }
        }

        private bool IsNextTokenKeywordConst()
        {
            return new[] {"true", "false", "null", "this"}.Contains(LookAheadToken.Value);
        }
        private bool IsNextTokenUnaryOp()
        {
            return new[] {"-", "!"}.Contains(LookAheadToken.Value);
        }
        //term ::= <int-const> | <str-const> | <keyword-const> | <var-name> |
        //         <var-name> '[' <expression> ']' | <sub-call> | '(' expression ')' |
        //         ('-' | '!') <term>
        private void ParseTerm()
        {
            if (LookAheadToken.Type == TokenType.IntConst)
            {
                Token intConst = NextToken();
                _codeGenerator.IntConst(int.Parse(intConst.Value));
            }
            else if (LookAheadToken.Type == TokenType.StrConst)
            {
                Token strConst = NextToken();
                _codeGenerator.StrConst(strConst.Value);
            }
            else if (IsNextTokenKeywordConst())
            {
                Token keywordConst = NextToken();
                switch (keywordConst.Value)
                {
                    case "true":
                        _codeGenerator.True();
                        break;
                    case "false":
                        _codeGenerator.False();
                        break;
                    case "null":
                        _codeGenerator.Null();
                        break;
                    case "this":
                        if (_currentSub.Kind != SubroutineKind.Method &&
                            _currentSub.Kind != SubroutineKind.Constructor)
                        {
                            ThrowCompilationException("'this' can be used only in a method or a constructor");
                        }
                        _codeGenerator.This();
                        break;
                    default:
                        ThrowCompilationException("Unexpected keyword constant '" + keywordConst.Value + "'");
                        break;
                }
            }
            else if (LookAheadToken.Type == TokenType.Ident)
            {
                //This is either a variable name, an array access, or a
                //subroutine call. To know which it's going to be, we need
                //the token after the variable name.
                Token varName = NextToken();
                if (LookAheadToken.Value == ".")
                {
                    ParseSubCall(varName);
                }
                else
                {
                    //Now is the time to verify that we have a valid variable:
                    Symbol symbol = GetClosestSymbol(varName.Value);
                    if (symbol == null)
                    {
                        ThrowCompilationException("Undefined identifier '" + varName.Value + "'");
                    }

                    bool withArrayIndex = false;
                    if (LookAheadToken.Value == "[")
                    {
                        withArrayIndex = true;
                        VerifyArrayAccessAllowed(varName);
                        Match(new Token(TokenType.Symbol, "["));
                        ParseExpression();
                        Match(new Token(TokenType.Symbol, "]"));
                    }
                    _codeGenerator.VariableRead(varName, withArrayIndex);
                }
            }
            else if (LookAheadToken.Value == "(")
            {
                Match(new Token(TokenType.Symbol, "("));
                ParseExpression();
                Match(new Token(TokenType.Symbol, ")"));
            }
            else if (IsNextTokenUnaryOp())
            {
                Token unaryOp = NextToken();
                ParseTerm();
                switch (unaryOp.Value)
                {
                    case "-":
                        _codeGenerator.Negate();
                        break;
                    case "!":
                        _codeGenerator.Not();
                        break;
                    default:
                        ThrowCompilationException("Unexpected unary operator: " + unaryOp.Value);
                        break;
                }
            }
            else
            {
                ThrowCompilationException("Expected a term, got: " + LookAheadToken.Value);
            }
        }

        //sub-call ::= <sub-name> '(' <expr-list> ')' |
        //             (<class-name> | <var-name>) '.' <sub-name> '(' <expr-list> ')'
        //expr-list ::= (<expression> (',' <expression>)*)?
        private void ParseSubCall(Token firstPart = null)
        {
            //NOTE: Because the parser may inadvertently swallow the first part
            //of the subroutine call expression (i.e. the subroutine name,
            //class name, or variable name), there's an option for this method
            //to accept it explicitly instead of reading it from the tokenizer.
            //
            //After this method completes, the code generator should have
            //available to it the result of the subroutine's invocation (e.g.
            //on the stack or in a predefined register). If the subroutine
            //call is not part of an expression (i.e. it's part of a 'do'
            //statement), then this result should be discarded via an explicit
            //call to the code generator.

            if (firstPart == null)
            {
                firstPart = NextToken();
            }
            if (firstPart.Type != TokenType.Ident)
            {
                ThrowCompilationException("Expected an identifier, got: " + firstPart.Value);
            }
            string classNameOfSubroutine = null;
            string subroutineName = null;
            bool isInstanceMethod = false;
            Symbol classOrVarName = GetClosestSymbol(firstPart.Value);
            if (classOrVarName == null)
            {
                if (LookAheadToken.Value == "(")
                {
                    //If this is the case, this is a direct method call within
                    //the current class. The current subroutine has to be
                    //non-static (i.e. a method or a constructor) for us to
                    //dispatch a method call. Because we're a one-pass compiler,
                    //we can't verify that the method actually exists.
                    if (_currentSub.Kind != SubroutineKind.Constructor &&
                        _currentSub.Kind != SubroutineKind.Method)
                    {
                        ThrowCompilationException("Method calls are allowed only within a method or a constructor");
                    }
                    classNameOfSubroutine = _currentClassName;
                    subroutineName = firstPart.Value;
                    isInstanceMethod = true;
                }
                else if (LookAheadToken.Value == ".")
                {
                    //This is a function (or constructor) call on a class name.
                    //We don't know about all class names, so we have to assume
                    //that this class exists. The assembler will tell us if
                    //we're wrong. Alternatively, we could implement a multi-
                    //pass compiler that goes over only declarations, or require
                    //all subroutines and classes to be declared before they
                    //can be called.
                    //NOTE: If we want to do inlining as an optimization, we
                    //must perform two passes (or else we don't know what's in
                    //the subroutine we're placing inline :-)).
                    Match(new Token(TokenType.Symbol, "."));
                    Token subName = NextToken();
                    if (subName.Type != TokenType.Ident)
                    {
                        ThrowCompilationException("Expected an identifier, got: " + subName.Value);
                    }
                    classNameOfSubroutine = firstPart.Value;
                    subroutineName = subName.Value;
                    isInstanceMethod = false;
                }
                else
                {
                    ThrowCompilationException("Unexpected: " + LookAheadToken.Value);
                }
            }
            else
            {
                //This is a method call on a variable. We must be able to tell
                //the class name by looking at the variable's type. Note that
                //built-in types that we recognize as keywords don't have any
                //methods, so this is an error we can flag easily.
                classNameOfSubroutine = classOrVarName.Type;
                if (Syntax.IsKeyword(classNameOfSubroutine))
                {
                    ThrowCompilationException("Can't call methods on built-in types");
                }
                Match(new Token(TokenType.Symbol, "."));
                Token subName = NextToken();
                if (subName.Type != TokenType.Ident)
                {
                    ThrowCompilationException("Expected an identifier, got: " + subName.Value);
                }
                subroutineName = subName.Value;
                isInstanceMethod = true;
            }
            Match(new Token(TokenType.Symbol, "("));
            //If this is an instance method, we need to supply 'this' as the
            //first parameter to be extracted on the other side.
            if (isInstanceMethod)
            {
                if (classNameOfSubroutine == _currentClassName)
                {   //'this' is actually our very own instance, as we're calling
                    //an instance method on ourselves.
                    _codeGenerator.This();
                }
                else
                {   //'this' is actually the variable on which we're making
                    //the call, so we need that pushed.
                    _codeGenerator.VariableRead(firstPart, false);
                }
            }
            ParseExpressionList();
            Match(new Token(TokenType.Symbol, ")"));
            _codeGenerator.Call(classNameOfSubroutine, subroutineName);

            //TODO: Detect the situation where a void subroutine is called
            //inside an expression where a value has to be provided. This is
            //impossible without having the subroutine's declaration in front
            //of us. To alleviate this, we can have all subroutines, even void
            //ones, return some arbitrary value (e.g. 0).
        }

        //expr-list ::= (<expression> (',' <expression>)*)?
        private void ParseExpressionList()
        {
            if (LookAheadToken.Value == ")")
            {
                return;//This is an empty list
            }
            while (true)
            {
                ParseExpression();
                if (LookAheadToken.Value != ",")
                {
                    break;
                }
                Match(new Token(TokenType.Symbol, ","));
            }
        }
    }
}