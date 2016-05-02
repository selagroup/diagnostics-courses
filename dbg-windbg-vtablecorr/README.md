### Catching a VTable Corruption

In this lab, you will diagnose a vtable corruption caused by a race condition and pinpoint where it happens by using hardware memory breakpoints.

#### Task 1

Run BatteryMeter.exe from the [bin](bin/) directory. It crashes almost immediately.

Run WinDbg (X86) and use **File** > **Open Executable** to launch BatteryMeter.exe. Make sure to configure the symbol path and the source file path to point to the [bin](bin/) and [src](src/) directories, and then hit `g` to let the application crash. This time, take a look at the crashing call stack. It looks very odd:

```
0:002> k
 # ChildEBP RetAddr  
00 02d3fb60 70d34f2b ucrtbase!abort+0x4b
01 02d3fb68 01291806 VCRUNTIME140!_purecall+0x1b [f:\dd\vctools\crt\vcruntime\src\misc\purevirt.cpp @ 29]
02 02d3fb74 75bc7c04 BatteryMeter!VendorRetrieverThread+0x16 [c:\temp\batterymeter\batterymeterdlg.cpp @ 54]
03 02d3fb88 77a3ad6f KERNEL32!BaseThreadInitThunk+0x24
04 02d3fbd0 77a3ad3a ntdll!__RtlUserThreadStart+0x2f
05 02d3fbe0 00000000 ntdll!_RtlUserThreadStart+0x1b
```

If you look at the `VendorRetrieverThread` function, it certainly doesn't seem like it's calling a CRT function called `_purecall`. Indeed, the `_purecall` function indicates that the impossible has happened, and you've successfully invoked a pure virtual function, at runtime!

#### Task 2

OK, it's time for a more thorough analysis. There's an object of type `ACPIVendorData`, and we're trying to call a virtual method on it, called `GetVendorID`. Let's figure out exactly what's wrong with the vtable when the object is created, and when the object is first used. Restart the application under the debugger (you can use the `.restart` command if you haven't quit the debugging session yet), and place a breakpoint in the `VendorRetrieverThread` function, when the object is first created:

```
bp `BatteryMeterDlg.cpp:64`
```

Hit F5 to get to the breakpoint, and step to the next line. Inspect the `pData` object using `dx pData`. Note that the pointer looks valid. Take a look at the vtable by running `?? pData._Mypair._Myval2`. The vtable pointer should be pointing to a global variable that you can find a symbol for:

```
0:001> ?? pData._Mypair._Myval2
class ACPIVendorData * 0x00e26a78
   +0x000 __VFN_table : 0x00c94d1c 
0:001> ln 0x00c94d1c 
Browse module
Set bu breakpoint

(00c94d1c)   BatteryMeter!ACPIVendorData::`vftable'   |  (00c94d24)   BatteryMeter!messageMap
Exact matches:
```

All right, we have an `ACPIVendorData` vtable, which is just fine. Does it contain a `_purecall` by any chance?

```
0:001> dps 0x00c94d1c L2
00c94d1c  00c920f0 BatteryMeter!ACPIVendorData::GetVendorID [c:\temp\batterymeter\vendordata.cpp @ 13]
00c94d20  00c91bd0 BatteryMeter!ACPIVendorData::`scalar deleting destructor'
```

This looks valid -- the first entry in the vtable is the `ACPIVendorData::GetVendorID` function, which is the non-pure, concrete implementation of `VendorData::GetVendorID`. All right, let's see how it gets corrupted. Let the application continue by hitting F5, and wait for the exception to occur. Switch to the `VendorRetrieverThread` frame and inspect the vtable again by looking at the `pData` object. This time, it looks different!

```
0:002> ?? pData
class ACPIVendorData * 0x00e26a78
   +0x000 __VFN_table : 0x00c94d10 
0:002> dps 0x00c94d10 L2
00c94d10  00c93220 BatteryMeter!purecall
00c94d14  00c91c40 BatteryMeter!VendorData::`scalar deleting destructor'
```

All right, so someone is corrupting our vtable, making it point to some other address. Let's figure out who by setting a hardware breakpoint. Restart the application one final time under the debugger using the `.restart` command, and set a breakpoint at BatteryMeterDlg.cpp line 65, right after initializing `pData`. Hit F5; when the breakpoint is hit, set up a hardware memory access breakpoint on the vtable field of `pData` by using the following command:

```
ba w4 @@c++(pData._Mypair._Myval2)
```

Hit F5 and wait for the breakpoint to be hit. Voila! We are inside the destructor of `VendorData` -- how can this be? Take a look at the call stack: it seems that the `pData` unique pointer is being destroyed before the `VendorRetrieverThread` has a chance to access the object. But oddly, the object doesn't die right away -- there's a delay in the destructor that keeps it alive for at least five seconds. How come the vtable becomes invalid before the destructor had a chance to return?

The answer to this final question is that when the base class destructor runs, one of the first things it does is restore the object's vtable to that of the base class, so that any virtual function calls made by the destructor are satisfied by the base class. In our case, the destructor doesn't make any virtual function calls, but there is *another* thread that uses the object while it is in this zombie state; and that other thread calls a pure virtual function by using the base class' vtable. 

