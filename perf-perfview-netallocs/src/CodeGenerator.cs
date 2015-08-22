using System.Diagnostics.Contracts;
using System.IO;

namespace JackCompiler
{
    internal sealed class CodeGeneratorOptions
    {
        public bool Optimize { get; set; }
        public TextWriter Output { get; set; }
    }

    internal interface ICodeGenerator
    {
        //NOTE: The subroutine declaration methods are responsible for allocating
        //storage for local variables, among other things. The CG isn't invoked
        //for each local variable separately.

        void SetOptions(CodeGeneratorOptions options);
        void InitSymbolTables(SymbolTable classSymTable, SymbolTable methodSymTable);
        void EmitEnvironment();
        void EmitBootstrapper();

        void StaticDeclaration(Symbol variable);
        void FieldDeclaration(Symbol variable);
        void ConstructorDeclaration(Subroutine subroutine);
        void FunctionDeclaration(Subroutine subroutine);
        void MethodDeclaration(Subroutine subroutine);
        void EndSubroutine();
        void Return();
        void BeginWhile();
        void WhileCondition();
        void EndWhile();
        void BeginIf();
        void PossibleElse();
        void EndIf();
        void Assignment(Token varName, bool withArrayIndex);
        void Add();
        void Sub();
        void Mul();
        void Div();
        void Mod();
        void And();
        void Or();
        void Less();
        void Greater();
        void Equal();
        void LessOrEqual();
        void GreaterOrEqual();
        void NotEqual();
        void IntConst(int value);
        void StrConst(string value);
        void True();
        void False();
        void Null();
        void This();
        void Negate();
        void Not();
        void VariableRead(Token varName, bool withArrayIndex);
        void Call(string className, string subroutineName);
        void DiscardReturnValueFromLastCall();
        void BeginClass(string className);
        void EndClass();
    }

    internal abstract class CodeGenerator : ICodeGenerator
    {
        protected CodeGeneratorOptions Options { get; private set; }
        protected TextWriter Output { get { return Options.Output; } }
        protected SymbolTable ClassSymTable { get; private set; }
        protected SymbolTable MethodSymTable { get; private set; }

        public void SetOptions(CodeGeneratorOptions options)
        {
            Options = options;
        }
        public void InitSymbolTables(SymbolTable classSymTable, SymbolTable methodSymTable)
        {
            ClassSymTable = classSymTable;
            MethodSymTable = methodSymTable;
        }

        public abstract void EmitEnvironment();
        public abstract void EmitBootstrapper();

        public abstract void StaticDeclaration(Symbol variable);
        public abstract void FieldDeclaration(Symbol variable);
        public abstract void ConstructorDeclaration(Subroutine subroutine);
        public abstract void FunctionDeclaration(Subroutine subroutine);
        public abstract void MethodDeclaration(Subroutine subroutine);
        public abstract void EndSubroutine();
        public abstract void Return();
        public abstract void BeginWhile();
        public abstract void WhileCondition();
        public abstract void EndWhile();
        public abstract void BeginIf();
        public abstract void PossibleElse();
        public abstract void EndIf();
        public abstract void Assignment(Token varName, bool withArrayIndex);
        public abstract void Add();
        public abstract void Sub();
        public abstract void Mul();
        public abstract void Div();
        public abstract void Mod();
        public abstract void And();
        public abstract void Or();
        public abstract void Less();
        public abstract void Greater();
        public abstract void Equal();
        public abstract void LessOrEqual();
        public abstract void GreaterOrEqual();
        public abstract void NotEqual();
        public abstract void IntConst(int value);
        public abstract void StrConst(string value);
        public abstract void True();
        public abstract void False();
        public abstract void Null();
        public abstract void This();
        public abstract void Negate();
        public abstract void Not();
        public abstract void VariableRead(Token varName, bool withArrayIndex);
        public abstract void Call(string className, string subroutineName);
        public abstract void DiscardReturnValueFromLastCall();
        public abstract void BeginClass(string className);
        public abstract void EndClass();
    }
}