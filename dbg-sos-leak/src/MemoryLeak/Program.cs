using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MemoryLeak
{
    public class Schedule
    {
        private byte[] _data = new byte[10000];

        public void FreeData()
        {
            Thread.Sleep(20);
        }
    }

    public class Employee
    {
        private Schedule _schedule = new Schedule();

        // Work is Sleep.  That's what .NET employees do.
        public void Work() { Thread.Sleep(10);  }

        ~Employee()
        {
            _schedule.FreeData();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Employee emp = new Employee();
                emp.Work();
            }
        }
    }
}
