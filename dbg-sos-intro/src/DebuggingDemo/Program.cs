using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DebuggingDemo
{
    class Program
    {
        static void PrintNumbersRepeatedly()
        {
            List<int> numbers = new List<int>();
            for (int i = 0; i < 10; ++i)
                numbers.Add(i);
            while (true)
            {
                foreach (int n in numbers)
                    Console.Write(n + " ");
                Thread.Sleep(1000);
            }
        }

        static void Main(string[] args)
        {
            args = new[] { "Hello", "Goodbye" };

            ThreadPool.QueueUserWorkItem(dummy => PrintNumbersRepeatedly());

            Console.WriteLine("Press ENTER to quit.");
            Console.ReadLine();
        }
    }
}
