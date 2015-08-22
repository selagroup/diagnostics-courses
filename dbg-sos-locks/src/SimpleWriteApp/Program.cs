using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SimpleWriteApp
{
    /// <summary>
    /// A reader-writer lock implementation designed to support a limited number of concurrent
    /// readers, and a single writer. If more than one writer attempts to acquire the lock at
    /// the same time, the behavior is undefined.
    /// </summary>
    class MySmartReaderWriterLock
    {
        private readonly Semaphore _sem;
        private readonly int _maxReaders;

        public MySmartReaderWriterLock(int maxReaders)
        {
            _maxReaders = maxReaders;
            _sem = new Semaphore(_maxReaders, _maxReaders, "RWLSemaphore_" + maxReaders);
        }

        public void LockForReading()
        {
            _sem.WaitOne();
        }

        public void UnlockForReading()
        {
            _sem.Release();
        }

        public void LockForWriting()
        {
            for (int i = 0; i < _maxReaders; ++i)
            {
                Thread.Sleep(1);
                _sem.WaitOne();
            }
        }

        public void UnlockForWriting()
        {
            _sem.Release(_maxReaders);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                MySmartReaderWriterLock rwl1 = new MySmartReaderWriterLock(10);
                Console.WriteLine("Work item 1 locking for writing...");
                rwl1.LockForWriting();
                Console.WriteLine("Work item 1 locked for writing");
                rwl1.UnlockForWriting();
                Console.WriteLine("Work item 1 done");
            });
            ThreadPool.QueueUserWorkItem(_ =>
            {
                MySmartReaderWriterLock rwl2 = new MySmartReaderWriterLock(10);
                Console.WriteLine("Work item 2 locking for writing...");
                rwl2.LockForWriting();
                Console.WriteLine("Work item 2 locked for writing");
                rwl2.UnlockForWriting();
                Console.WriteLine("Work item 2 done");
            });
            Console.ReadLine();
        }
    }
}
