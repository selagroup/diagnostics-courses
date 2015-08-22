using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace JackCompiler
{
    internal sealed class CompilerDriver
    {
        static void Main(string[] args)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                DriveCompilerWithCCodeGenerator(args);
                Console.WriteLine("Elapsed: {0}ms", sw.ElapsedMilliseconds);
            }
            catch (CompilationException ex)
            {
                Console.WriteLine("Compilation error at line " + ex.LineNumber + ": " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("General error: " + ex);
            }
        }

        private static void DriveCompilerWithCCodeGenerator(string[] args)
        {
            if (args.Length == 0)
            {
                Usage();
                return;
            }

            string[] sourceFiles = args.Where(a => File.Exists(a)).ToArray();
            if (sourceFiles.Length == 0)
            {
                Usage();
                return;
            }

            ICodeGenerator codeGenerator = new CCodeGenerator();
            var options = new CodeGeneratorOptions();
            var outputTextWriter = new CompilationOutputTextWriter();
            options.Output = outputTextWriter;
            options.Optimize = true;
            codeGenerator.SetOptions(options);
            codeGenerator.EmitEnvironment();
            foreach (string sourceFile in sourceFiles)
            {
                StreamReader source = File.OpenText(sourceFile);
                Tokenizer tokenizer = new Tokenizer(source);
                Parser parser = new Parser(tokenizer, codeGenerator);
                parser.Parse();
                source.Close();
            }
            codeGenerator.EmitBootstrapper();
            string finalText = outputTextWriter.ToString();
            Console.WriteLine("Final text length: {0} characters", finalText.Length);
            File.WriteAllText("out.c", finalText);
        }

        private static void Usage()
        {
            Console.WriteLine();
            Console.WriteLine("JackCompiler: compiles Jack to C");
            Console.WriteLine("By Sasha Goldshtein, http://blog.sashag.net");
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("JackCompiler <source-file> [<source-file> . . .]");
            Console.WriteLine("JackCompiler <wildcard>");
            Console.WriteLine();
        }
    }
}
