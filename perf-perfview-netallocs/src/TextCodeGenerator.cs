using System;

namespace JackCompiler
{
    internal sealed class TextCodeGenerator : CodeGenerator
    {
        public override void EmitEnvironment()
        {
        }

        public override void EmitBootstrapper()
        {
        }

        public override void StaticDeclaration(Symbol variable)
        {
            Output.WriteLine(
                "STATIC {0} {1}.{2}",
                variable.Type, variable.DeclaringClassName, variable.Name);
        }

        public override void FieldDeclaration(Symbol variable)
        {
            Output.WriteLine(
                "FIELD {0} {1}.{2}",
                variable.Type, variable.DeclaringClassName, variable.Name);
        }

        public override void ConstructorDeclaration(Subroutine subroutine)
        {
            Output.WriteLine(
                "CONSTRUCTOR {0}.{1}",
                subroutine.ClassName, subroutine.Name);
        }

        public override void FunctionDeclaration(Subroutine subroutine)
        {
            Output.WriteLine(
                "FUNCTION {0}.{1}",
                subroutine.ClassName, subroutine.Name);
        }

        public override void MethodDeclaration(Subroutine subroutine)
        {
            Output.WriteLine(
                "METHOD {0}.{1}",
                subroutine.ClassName, subroutine.Name);
        }

        public override void EndSubroutine()
        {
            Output.WriteLine("END SUBROUTINE");
        }

        public override void Return()
        {
            Output.WriteLine("RETURN");
        }

        public override void BeginWhile()
        {
            Output.WriteLine("START WHILE");
        }

        public override void WhileCondition()
        {
            Output.WriteLine("WHILE CONDITION");
        }

        public override void EndWhile()
        {
            Output.WriteLine("END WHILE");
        }

        public override void BeginIf()
        {
            Output.WriteLine("BEGIN IF");
        }

        public override void PossibleElse()
        {
            Output.WriteLine("POSSIBLE ELSE");
        }

        public override void EndIf()
        {
            Output.WriteLine("END IF");
        }

        public override void Assignment(Token varName, bool withArrayIndex)
        {
            Output.WriteLine(
                "ASSIGNMENT TO {0}{1}",
                varName.Value,
                withArrayIndex ? "[...]" : "");
        }

        public override void Add()
        {
            Output.WriteLine("+");
        }

        public override void Sub()
        {
            Output.WriteLine("-");
        }

        public override void Mul()
        {
            Output.WriteLine("*");
        }

        public override void Div()
        {
            Output.WriteLine("/");
        }

        public override void Mod()
        {
            Output.WriteLine("%");
        }

        public override void And()
        {
            Output.WriteLine("&");
        }

        public override void Or()
        {
            Output.WriteLine("|");
        }

        public override void Less()
        {
            Output.WriteLine("<");
        }

        public override void Greater()
        {
            Output.WriteLine(">");
        }

        public override void Equal()
        {
            Output.WriteLine("=");
        }

        public override void LessOrEqual()
        {
            Output.WriteLine("<=");
        }

        public override void GreaterOrEqual()
        {
            Output.WriteLine(">=");
        }

        public override void NotEqual()
        {
            Output.WriteLine("!=");
        }

        public override void IntConst(int value)
        {
            Output.WriteLine(value);
        }

        public override void StrConst(string value)
        {
            Output.WriteLine("\"" + value + "\"");
        }

        public override void True()
        {
            Output.WriteLine("TRUE");
        }

        public override void False()
        {
            Output.WriteLine("FALSE");
        }

        public override void Null()
        {
            Output.WriteLine("NULL");
        }

        public override void This()
        {
            Output.WriteLine("THIS");
        }

        public override void Negate()
        {
            Output.WriteLine("-");
        }

        public override void Not()
        {
            Output.WriteLine("!");
        }

        public override void VariableRead(Token varName, bool withArrayIndex)
        {
            Output.WriteLine(
                "READ FROM {0}{1}",
                varName.Value, withArrayIndex ? "[...]" : "");
        }

        public override void Call(string className, string subroutineName)
        {
            Output.WriteLine("CALLING {0}.{1}", className, subroutineName);
        }

        public override void DiscardReturnValueFromLastCall()
        {
            Output.WriteLine("DISCARDED RETURN VALUE");
        }

        public override void BeginClass(string className)
        {
            Output.WriteLine("BEGIN CLASS " + className);
        }

        public override void EndClass()
        {
            Output.WriteLine("END CLASS");
        }
    }
}