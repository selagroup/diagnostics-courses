using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using WMICNTLLib;

namespace StrangeLeak
{
    class ExpensiveResource
    {
        private byte[] _data = new byte[100000];

        public ExpensiveResource()
        {
            Console.WriteLine("Expensive resource created.");
        }

        ~ExpensiveResource()
        {
            Console.WriteLine("Expensive resource cleaned up.");
        }

        public void Use()
        {
            Console.Write("Using expensive resource . . .");
            Thread.Sleep(100);
            Console.WriteLine(" DONE");
        }
    }

    class SnapinWrapper
    {
        private WMISnapin _snapin;

        public SnapinWrapper()
        {
            _snapin = new WMISnapin();
        }

        ~SnapinWrapper()
        {
            Marshal.FinalReleaseComObject(_snapin);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Thread thread = new Thread(
                () =>
                    {
                        SnapinWrapper wrapper = new SnapinWrapper();
                        Thread.Sleep(Timeout.Infinite);
                    });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            while (true)
            {
                ExpensiveResource r = new ExpensiveResource();
                r.Use();
            }
        }
    }
}
