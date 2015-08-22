using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackDumper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Stack dumper v0.1");
            
            int processId = GetProcessId(args);
            VerifyProcessExists(processId);
            DumpStacks(processId);
        }

        private static int GetProcessId(string[] args)
        {
            int processId = -1;
            if (args.Length != 1 || !int.TryParse(args[0], out processId))
            {
                Console.WriteLine("Usage: <process id>");
                Environment.Exit(1);
            }
            return processId;
        }

        private static void VerifyProcessExists(int processId)
        {
            if (Process.GetProcessById(processId) == null)
            {
                Console.WriteLine("The process {0} does not exist", processId);
                Environment.Exit(1);
            }
        }

        private static void DumpStacks(int processId)
        {
            // TODO
        }
    }
}
