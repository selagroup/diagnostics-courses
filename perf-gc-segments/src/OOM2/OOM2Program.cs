using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OOM2
{
    class OOM2Program
    {
        static void Main(string[] args)
        {
            const int BlockSize = 1500*1048576;
            const int SegmentSize = 16*1048576;
            const int PageSize = 4096;

            IntPtr pHugeBlock = VirtualAlloc(
                IntPtr.Zero,
                new UIntPtr(BlockSize),
                AllocationType.RESERVE,
                MemoryProtection.READWRITE);
            Console.WriteLine(pHugeBlock);
            //Hope that no one else snatches this block:
            VirtualFree(pHugeBlock, UIntPtr.Zero, 0x8000 /*MEM_RELEASE*/);

            for (int address = (int)pHugeBlock; address < (int)pHugeBlock + BlockSize; address += SegmentSize)
            {
                IntPtr temp = VirtualAlloc(
                    new IntPtr(address),
                    new UIntPtr(PageSize),
                    AllocationType.COMMIT|AllocationType.RESERVE,
                    MemoryProtection.READWRITE);
                Console.WriteLine(temp);
            }

            List<byte[]> list = new List<byte[]>();
            while (true)
            {
                try
                {
                    list.Add(new byte[50000]);
                }
                catch (OutOfMemoryException)
                {
                    Console.WriteLine("OOM occurred");
                    Console.ReadLine();
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize,
           uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize,
           AllocationType flAllocationType, MemoryProtection flProtect);

        [Flags()]
        public enum AllocationType : uint
        {
            COMMIT = 0x1000,
            RESERVE = 0x2000,
            RESET = 0x80000,
            LARGE_PAGES = 0x20000000,
            PHYSICAL = 0x400000,
            TOP_DOWN = 0x100000,
            WRITE_WATCH = 0x200000
        }

        [Flags()]
        public enum MemoryProtection : uint
        {
            EXECUTE = 0x10,
            EXECUTE_READ = 0x20,
            EXECUTE_READWRITE = 0x40,
            EXECUTE_WRITECOPY = 0x80,
            NOACCESS = 0x01,
            READONLY = 0x02,
            READWRITE = 0x04,
            WRITECOPY = 0x08,
            GUARD_Modifierflag = 0x100,
            NOCACHE_Modifierflag = 0x200,
            WRITECOMBINE_Modifierflag = 0x400
        }
    }
}
