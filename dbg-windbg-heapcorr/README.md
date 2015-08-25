### Catching an Unmanaged Heap Corruption

In this lab, you will diagnose a heap corruption in an unmanaged application using Page Heap, a built-in Windows feature designed to find the root cause of impossible memory overruns.

#### Task 1

Run WinDbg, and use **File** > **Open Executable** to run the BatteryMeter.exe application from the [bin](bin/) folder. Don’t forget to press F5 so that the application starts running past the initial breakpoint.

A few seconds later, the application will crash. Locate the exception that occurred and its call stack. Do not inspect the application's source code at this time.

The exact source of the error will depend on your system, but often the location of the crash is somewhere in `ntdll!RtlFreeHeap` or `ntdll!RtlAllocateHeap`. These functions aren't likely to be responsible for the exception -- the heap has become corrupted and these functions are the earliest point where the heap manager was able to detect this. You might also see debugger output such as "Critical error detected c0000374" -- this error code means "A heap has been corrupted". (You can verify this by running the `!error c0000374` command.)

Run Application Verifier (in the Start menu, search for Application Verifier), open the application's executable, and enable the **Heap** tests under the **Basic** category. Disable all other tests and click **Save**. This turns on Page Heap.

> Alternative: Run GFlags.exe (from the Debugging Tools for Windows 32-bit installation directory), go to the **Image** tab, enter BatteryMeter.exe and then hit the TAB key, check the **Enable page heap** checkbox, and click **Apply**.

Run the application again from WinDbg, using the **File** > **Open Executable** menu item. This time, you should get a different call stack, which should be similar to the following:

```
(13cc.15e8): Access violation - code c0000005 (first chance)
First chance exceptions are reported before any exception handling.
This exception may be expected and handled.
eax=011e3b04 ebx=72c52435 ecx=00000043 edx=0648b4fc esi=0766f000 edi=00000064
eip=72c52473 esp=080ffaac ebp=080ffab4 iopl=0         nv up ei pl nz ac pe nc
cs=0023  ss=002b  ds=002b  es=002b  fs=0053  gs=002b             efl=00010216
MSVCR100!wcscpy_s+0x3d:
72c52473 66890c02        mov     word ptr [edx+eax],cx    ds:002b:0766f000=????
0:001> k
ChildEBP RetAddr  
080ffab4 011e1a1e MSVCR100!wcscpy_s+0x3d
080ffacc 011e1730 BatteryMeter!CPUInformation::CPUInformation+0x2e 
080ffaf8 77a4850d BatteryMeter!TemperatureAndBatteryUpdaterThread+0x90 
080ffb04 77d1bf39 KERNEL32!BaseThreadInitThunk+0xe
080ffb48 77d1bf0c ntdll!__RtlUserThreadStart+0x72
080ffb60 00000000 ntdll!_RtlUserThreadStart+0x1b
```

Run the `.exr -1` command to see some details about the access violation, including the specific address that the application was trying to access:

```
0:001> .exr -1
ExceptionAddress: 72c52473 (MSVCR100!wcscpy_s+0x0000003d)
   ExceptionCode: c0000005 (Access violation)
  ExceptionFlags: 00000000
NumberParameters: 2
   Parameter[0]: 00000001
   Parameter[1]: 0766f000
Attempt to write to address 0766f000
```

Note that the access violation occurred when trying to copy bytes around using `wcscpy_s`. Specifically, the write to address 0766f000 has failed (the address will be different in your case, of course). Inspect the memory protection attributes of that memory location using the `!vprot` command:

```
0:001> !vprot 0766f000
BaseAddress:       0766f000
AllocationBase:    07620000
AllocationProtect: 00000001  PAGE_NOACCESS
RegionSize:        000b1000
State:             00002000  MEM_RESERVE
Type:              00020000  MEM_PRIVATE
```

The memory is completely inaccessible ([PAGE_NOACCESS](https://msdn.microsoft.com/en-us/library/windows/desktop/aa366786)), which is typical for buffers that get overwritten and caught by Page Heap. Make sure this is the case by dumping memory before this location and noticing that it's valid memory:

```
0:001> db /c 0n10 0766f000-20 L0n50
0766efe0  c0 c0 c0 c0 c0 c0 c0 c0-c0 c0  ..........
0766efea  c0 c0 c0 c0 c0 c0 c0 c0-c0 c0  ..........
0766eff4  c0 c0 c0 c0 c0 c0 c0 c0-c0 c0  ..........
0766effe  c0 c0 ?? ?? ?? ?? ?? ??-?? ??  ..????????
0766f008  ?? ?? ?? ?? ?? ?? ?? ??-?? ??  ??????????
```

Additionally, because Page Heap is enabled, you can also determine where the buffer has been allocated and make sure that a buffer overrun has occurred. Pass the problematic address to the `!heap -p -a` command, as follows:

```
0:001> !heap -p -a 0766f000
    address 0766f000 found in
    _DPH_HEAP_ROOT @ 791000
    in busy allocation (  DPH_HEAP_BLOCK: UserAddr UserSize - VirtAddr VirtSize)
                                 7f00c30: 766de30  11d0 -     766d000  3000
    546b8a19 verifier!AVrfDebugPageHeapAllocate+0x00000229
    77d9cbab ntdll!RtlDebugAllocateHeap+0x0000002f
    77d485ff ntdll!RtlpAllocateHeap+0x0000009b
    77d155c5 ntdll!RtlAllocateHeap+0x00000176
    72c50269 MSVCR100!malloc+0x0000004b
    5be5b327 mfc100u!operator new+0x00000033
    011e19fd BatteryMeter!CPUInformation::CPUInformation+0x... 
    77a4850d KERNEL32!BaseThreadInitThunk+0x0000000e
    77d1bf39 ntdll!__RtlUserThreadStart+0x00000072
    77d1bf0c ntdll!_RtlUserThreadStart+0x0000001b
```
 
You can now see the allocating stack trace for that buffer, as well as the address and size that were returned to the application. The application is apparently trying to write beyond the end of the buffer.

Finally, verify that the address being accessed is indeed outside the bounds of the allocated heap block by adding the **UserSize** field to the **UserAddr** field. The resulting address is the first invalid byte past the end of the buffer, and it should be the same as the memory address that you got the exception for.

```
0:001> ? 766de30+11d0
Evaluate expression: 124186624 = 0766f000
```

In conclusion, Page Heap has made it possible for you to identify a heap buffer overrun at the precise moment when it occurred. It is now time to inspect the [application's code](src/) and see why it’s trying to access invalid memory past the end of the buffer. (This is the boring part :-))
