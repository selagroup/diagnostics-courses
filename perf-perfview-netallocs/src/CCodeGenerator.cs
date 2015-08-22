using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace JackCompiler
{
    internal sealed class CCodeGenerator : CodeGenerator
    {
        private string _currentClassName;
        private List<Symbol> _fields;
        private LabelStack _beginWhileLabels;
        private LabelStack _endWhileLabels;
        private LabelStack _elseLabels;
        private LabelStack _endIfLabels;
        private bool _emittedStructDeclaration;

        private void ReInitialize()
        {
            _currentClassName = null;
            _fields = new List<Symbol>();
            _beginWhileLabels = new LabelStack("BEGINWHILE_");
            _endWhileLabels = new LabelStack("ENDWHILE_");
            _elseLabels = new LabelStack("ELSE_");
            _endIfLabels = new LabelStack("ENDIF_");
            _emittedStructDeclaration = false;
        }
        private string FormatStaticName(string staticName)
        {
            return String.Format("{0}__{1}", _currentClassName, staticName);
        }
        private string FormatSubroutineName(string subroutineName, string className = null)
        {
            return String.Format("{0}__{1}", className ?? _currentClassName, subroutineName);
        }
        private void EmitLocalsAndParameters(Subroutine subroutine)
        {
            foreach (Symbol localVar in subroutine.Locals)
            {
                Output.WriteLine("  __WORD {0};", localVar.Name);
            }
            foreach (Symbol param in subroutine.Parameters)
            {
                Output.WriteLine("  __WORD {0};", param.Name);
            }
        }
        private void ExtractParameters(Subroutine subroutine)
        {
            foreach (Symbol param in subroutine.Parameters.Reverse())
            {
                Output.WriteLine("  {0} = __POP();", param.Name);
            }
        }
        private int GetClassSizeInWords()
        {
            return _fields.Count;
        }
        private void EmitStructDeclarationIfNotEmitted()
        {
            if (!_emittedStructDeclaration)
            {
                _emittedStructDeclaration = true;
                Output.WriteLine("typedef struct __{0} {{", _currentClassName);
                foreach (Symbol field in _fields)
                {
                    Output.WriteLine("  __WORD {0};", field.Name);
                }
                //NOTE: C requires that every struct declaration have at least
                //one member. If there are no declared members, we introduce a
                //dummy member.
                if (_fields.Count == 0)
                {
                    Output.WriteLine("  __WORD __DUMMY;");
                }
                Output.WriteLine("}} {0};", _currentClassName);
            }
        }

        public CCodeGenerator()
        {
            ReInitialize();
        }

        public override void EmitEnvironment()
        {
            //Includes and definitions necessary for the support environment:
            Output.WriteLine("#include <stdlib.h>");
            Output.WriteLine("#include <stdio.h>");
            Output.WriteLine("typedef int __WORD;");
            Output.WriteLine("static __WORD __SCRATCH1;");
            Output.WriteLine("static __WORD __SCRATCH2;");
            Output.WriteLine("#define __ALLOC(n) malloc(n)");
            Output.WriteLine("#define __FREE(p) free(p)");
            
            //The evaluation stack, limited in size. Note that __PUSH and __POP
            //are macros and not functions to make sure that they are inlined
            //(I couldn't force the C compiler to inline them always).
            Output.WriteLine("#define STACK_SIZE 1000");
            Output.WriteLine("static int __SP = 0;");
            Output.WriteLine("static __WORD __STACK[STACK_SIZE];");
            Output.WriteLine("#define __PUSH(w) __STACK[__SP++] = w ");
            Output.WriteLine("#define __POP() (__STACK[--__SP]) ");

            //Memory allocation and deallocation:
            Output.WriteLine("static __WORD Array__new() {");
            Output.WriteLine("  return (__WORD)__ALLOC(__POP()*sizeof(__WORD));");
            Output.WriteLine("}");
            Output.WriteLine("__WORD Memory__free() {");
            Output.WriteLine("  __FREE((void*)__POP());");
            Output.WriteLine("  return 0;");
            Output.WriteLine("}");

            //Input and output:
            Output.WriteLine("static __WORD System__print() {");
            Output.WriteLine("  printf(\"%s\", (char*)__POP());");
            Output.WriteLine("  return 0;");
            Output.WriteLine("}");
            Output.WriteLine("static __WORD System__println() {");
            Output.WriteLine("  printf(\"\\n\");");
            Output.WriteLine("  return 0;");
            Output.WriteLine("}");
            Output.WriteLine("static __WORD System__printInt() {");
            Output.WriteLine("  printf(\"%d\", (int)__POP());");
            Output.WriteLine("  return 0;");
            Output.WriteLine("}");
            Output.WriteLine("static __WORD System__readInt() {");
            Output.WriteLine("  int i;");
            Output.WriteLine("  scanf_s(\"%d\", &i);");
            Output.WriteLine("  return i;");
            Output.WriteLine("}");
        }

        public override void EmitBootstrapper()
        {
            Output.WriteLine("int main() {");
            Output.WriteLine("  Main__main();");
            Output.WriteLine("  return 0;");
            Output.WriteLine("}");
        }

        public override void StaticDeclaration(Symbol variable)
        {
            Output.WriteLine(
                "static __WORD {0};",
                FormatStaticName(variable.Name));
        }

        public override void FieldDeclaration(Symbol variable)
        {
            _fields.Add(variable);
        }

        public override void ConstructorDeclaration(Subroutine subroutine)
        {
            EmitStructDeclarationIfNotEmitted();
            Output.WriteLine("__WORD {0} () {{", FormatSubroutineName(subroutine.Name));
            EmitLocalsAndParameters(subroutine);
            Output.WriteLine("  __WORD THIS;");
            //A constructor must allocate memory for its class and
            //assign the result to the THIS local variable.
            Output.WriteLine("  THIS = (__WORD) __ALLOC({0}*sizeof(__WORD));", GetClassSizeInWords());
            ExtractParameters(subroutine);
        }

        public override void FunctionDeclaration(Subroutine subroutine)
        {
            EmitStructDeclarationIfNotEmitted();
            Output.WriteLine("__WORD {0} () {{", FormatSubroutineName(subroutine.Name));
            EmitLocalsAndParameters(subroutine);
            ExtractParameters(subroutine);
        }

        public override void MethodDeclaration(Subroutine subroutine)
        {
            EmitStructDeclarationIfNotEmitted();
            Output.WriteLine("__WORD {0} () {{", FormatSubroutineName(subroutine.Name));
            EmitLocalsAndParameters(subroutine);
            //By convention, 'this' is the first parameter of the subroutine
            //call, so it's the last thing on the stack.
            Output.WriteLine("  __WORD THIS;");
            Output.WriteLine("  THIS = __POP();");
            ExtractParameters(subroutine);
        }

        public override void EndSubroutine()
        {
            Output.WriteLine("}");
        }

        public override void Return()
        {
            Output.WriteLine("  return __POP();");
        }

        public override void BeginWhile()
        {
            Output.WriteLine("{0}:", _beginWhileLabels.Push());
        }

        public override void WhileCondition()
        {
            //We have the result of evaluating the condition on the stack.
            //If the result was 0, we should jump to the end label; otherwise,
            //we continue with the loop body from here.

            Output.WriteLine("  if (__POP() == 0) goto {0};", _endWhileLabels.Push());
        }

        public override void EndWhile()
        {
            Output.WriteLine("  goto {0};", _beginWhileLabels.Pop());
            Output.WriteLine("{0}:", _endWhileLabels.Pop());
        }

        public override void BeginIf()
        {
            //We have the result of evaluating the condition on the stack.
            //If the result was 0, we should jump to the else label; otherwise,
            //we continue with the if statement body from here.

            Output.WriteLine("  if (__POP() == 0) goto {0};", _elseLabels.Push());
        }

        public override void PossibleElse()
        {
            Output.WriteLine("  goto {0};", _endIfLabels.Push());
            Output.WriteLine("{0}:", _elseLabels.Pop());
        }

        public override void EndIf()
        {
            Output.WriteLine("{0}:", _endIfLabels.Pop());
        }

        public override void Assignment(Token varName, bool withArrayIndex)
        {
            //This is slightly tricky. We need to have the address of the
            //variable to which we're doing the assignment on the stack,
            //and then everything is really easy. If it's an array, we need
            //the address of the array element to which we're doing the
            //assignment (its index is on the stack following the LHS value).

            if (MethodSymTable.HasSymbol(varName.Value))
            {
                Symbol symbol = MethodSymTable.GetSymbol(varName.Value);

                Contract.Assert(symbol.Kind == SymbolKind.Local ||
                                symbol.Kind == SymbolKind.Parameter);

                Output.WriteLine("  __PUSH((__WORD)&{0});", symbol.Name);
            }
            else
            {
                Contract.Assert(ClassSymTable.HasSymbol(varName.Value));
                Symbol symbol = ClassSymTable.GetSymbol(varName.Value);

                if (symbol.Kind == SymbolKind.Static)
                {
                    Output.WriteLine("  __PUSH((__WORD)&{0});", FormatStaticName(symbol.Name));
                }
                else if (symbol.Kind == SymbolKind.Field)
                {
                    Output.WriteLine("  __PUSH((__WORD)&((({0}*)THIS)->{1}));", _currentClassName, symbol.Name);
                }
            }

            //If it's an array, obtain the address of the right element by
            //adding the index which is on the stack. We need a scratch
            //location because issuing two __POP() calls in the same statement
            //does not guarantee left-to-right evaluation.
            if (withArrayIndex)
            {
                //The array address is now on the stack, but we really need the
                //address of the first element. Hence the dereference:
                Output.WriteLine("  __SCRATCH2 = *(__WORD*)__POP();");
                //This is the RHS value that we ought to put in the array element:
                Output.WriteLine("  __SCRATCH1 = __POP();");
                //Finally, the top of the stack contains the value of the array
                //indexing expression, i.e. the element index:
                Output.WriteLine("  * ( ((__WORD*)__SCRATCH2) + __POP() ) = __SCRATCH1;");
            }
            else
            {
                Output.WriteLine("  __SCRATCH1 = __POP();"); //This is the LHS
                Output.WriteLine("  * ((__WORD*)__SCRATCH1) = __POP();");
            }
        }

        public override void Add()
        {
            Output.WriteLine("  __SCRATCH1 = __POP();");
            Output.WriteLine("  __PUSH(__SCRATCH1 + __POP());");
        }

        public override void Sub()
        {
            Output.WriteLine("  __SCRATCH1 = __POP();");
            Output.WriteLine("  __PUSH(__POP() - __SCRATCH1);");
        }

        public override void Mul()
        {
            Output.WriteLine("  __SCRATCH1 = __POP();");
            Output.WriteLine("  __PUSH(__POP() * __SCRATCH1);");
        }

        public override void Div()
        {
            Output.WriteLine("  __SCRATCH1 = __POP();");
            Output.WriteLine("  __PUSH(__POP() / __SCRATCH1);");
        }

        public override void Mod()
        {
            Output.WriteLine("  __SCRATCH1 = __POP();");
            Output.WriteLine("  __PUSH(__POP() % __SCRATCH1);");            
        }

        public override void And()
        {
            Output.WriteLine("  __SCRATCH1 = __POP();");
            Output.WriteLine("  __PUSH(__POP() & __SCRATCH1);");
        }

        public override void Or()
        {
            Output.WriteLine("  __SCRATCH1 = __POP();");
            Output.WriteLine("  __PUSH(__POP() | __SCRATCH1);");
        }

        public override void Less()
        {
            Output.WriteLine("  __SCRATCH1 = __POP();");
            Output.WriteLine("  __PUSH(__POP() < __SCRATCH1 ? -1 : 0);");
        }

        public override void Greater()
        {
            Output.WriteLine("  __SCRATCH1 = __POP();");
            Output.WriteLine("  __PUSH(__POP() > __SCRATCH1 ? -1 : 0);");
        }

        public override void Equal()
        {
            Output.WriteLine("  __SCRATCH1 = __POP();");
            Output.WriteLine("  __PUSH(__POP() == __SCRATCH1 ? -1 : 0);");
        }

        public override void LessOrEqual()
        {
            Output.WriteLine("  __SCRATCH1 = __POP();");
            Output.WriteLine("  __PUSH(__POP() <= __SCRATCH1 ? -1 : 0);");
        }

        public override void GreaterOrEqual()
        {
            Output.WriteLine("  __SCRATCH1 = __POP();");
            Output.WriteLine("  __PUSH(__POP() >= __SCRATCH1 ? -1 : 0);");
        }

        public override void NotEqual()
        {
            Output.WriteLine("  __SCRATCH1 = __POP();");
            Output.WriteLine("  __PUSH(__POP() != __SCRATCH1 ? -1 : 0);");
        }

        public override void IntConst(int value)
        {
            Output.WriteLine("  __PUSH({0});", value);
        }

        public override void StrConst(string value)
        {
            Output.WriteLine("  __PUSH((__WORD)\"{0}\");", value);
        }

        public override void True()
        {
            IntConst(-1);
        }

        public override void False()
        {
            IntConst(0);
        }

        public override void Null()
        {
            IntConst(0);
        }

        public override void This()
        {
            Output.WriteLine("  __PUSH(THIS);");
        }

        public override void Negate()
        {
            Output.WriteLine("  __PUSH(-__POP());");
        }

        public override void Not()
        {
            Output.WriteLine("  __PUSH(~__POP());");
        }

        public override void VariableRead(Token varName, bool withArrayIndex)
        {
            //Put the value of the variable on the top of the stack. If it's
            //an array, the value is the address of the array's first element.
            if (MethodSymTable.HasSymbol(varName.Value))
            {
                Symbol symbol = MethodSymTable.GetSymbol(varName.Value);

                Contract.Assert(symbol.Kind == SymbolKind.Local ||
                                symbol.Kind == SymbolKind.Parameter);
                
                Output.WriteLine("  __PUSH({0});", symbol.Name);
            }
            else
            {
                Contract.Assert(ClassSymTable.HasSymbol(varName.Value));
                Symbol symbol = ClassSymTable.GetSymbol(varName.Value);

                if (symbol.Kind == SymbolKind.Static)
                {
                    Output.WriteLine("  __PUSH({0});", FormatStaticName(symbol.Name));
                }
                else if (symbol.Kind == SymbolKind.Field)
                {
                    Output.WriteLine("  __PUSH((({0}*)THIS)->{1});", _currentClassName, symbol.Name);
                }
            }
            
            //If it's an array, dereference it using []. We need a scratch
            //location because issuing two __POP() calls in the same statement
            //does not guarantee left-to-right evaluation.
            if (withArrayIndex)
            {
                Output.WriteLine("  __SCRATCH1 = __POP();");
                Output.WriteLine("  __PUSH( ((__WORD*)__SCRATCH1)[ __POP() ] );");
            }
        }

        public override void Call(string className, string subroutineName)
        {
            Output.WriteLine("  __PUSH( {0}() );", FormatSubroutineName(subroutineName, className));
        }

        public override void DiscardReturnValueFromLastCall()
        {
            //The return value is stored on the stack *by the caller*, so if
            //we don't need it, we have to pop it:
            Output.WriteLine("  __POP();");
        }

        public override void BeginClass(string className)
        {
            _currentClassName = className;
        }

        public override void EndClass()
        {
            ReInitialize();
        }
    }
}