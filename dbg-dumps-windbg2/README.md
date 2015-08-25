### Basic Unmanaged Dump Triage with WinDbg

In this lab, you will use WinDbg to perform basic triage of unmanaged crash dumps. You will also see how to determine which C++ exception caused a crash, even if you don't have full symbols for the component that threw the exception.

#### Task 1

Run the FileExplorer.exe application from the [bin](bin/) folder. Use the directory browser control to choose any directory you like. Click the *Populate* button. The application crashes.

Capture a crash dump of the application using the tools shown in class. You may use whatever tool you like, provided that at the end you have a dump file.

One way to do this: use a command prompt to navigate to the directory where you installed the Sysinternals Suite (specifically, the Procdump.exe utility), and run the following command:

```
Procdump -ma -e -w FileExplorer.exe C:\temp\fe.dmp
```

At this point, you can run FileExplorer.exe and reproduce the crash. Procdump should capture a dump file automatically when the application crashes.

Run the 32-bit version of WinDbg, and use the **File** > **Symbol File Path** dialog to point the debugger to the [bin](bin/) directory, where the FileExplorer.pdb file resides. The symbol path should be a combination of the Microsoft symbol server and the local path. For example:

```
C:\Course\dbg-dumps-windbg2\bin;srv*C:\symbols*http://msdl.microsoft.com/download/symbols
```

Next, use **File** > **Source File Path** to point the debugger to the [src\FileExplorer](src/FileExplorer/) folder. Finally, open the dump file using **File** > **Open Crash Dump**.

Issue the `k` command to view the call stack of the offending thread. You can also use the **View** > **Call Stack window** and click the frames to switch between them and view the source code.

> Note: If the preceding command displays a trimmed call stack with only one frame, you have to perform a manual stack walk. You can run the following command, which usually works in this case:
>
> ```
> .for (r $t0 = @ebp; poi(@$t0) != 0; r $t0 = poi(@$t0)) { dps @$t0+4 L1 }
> ```

Alternatively, consult the instructor for assistance or repeat the exercise to obtain another crash dump.

> If the `k` command was able to decipher the call stack, try letting WinDbg triage the problem automatically by running the `!analyze -v` command.

#### Task 2

Run the NumberCruncher.exe application from the [bin](bin/) folder. The application crashes. Capture a crash dump of the application using the tools shown in class. You may use whatever tool you like.

Open the dump file in Visual Studio using **File** > **Open File**. Configure symbols if necessary using the **Set Symbols Path** link. Click the **Debug with Native** button. If asked for source file locations, point the debugger to the [src\NumberCruncher](src/NumberCruncher/) folder.

Inspect the current line, the call stack, the locals window and try to diagnose the root cause of the crash. It might be useful to inspect other threads in addition to the current one.

#### Task 3

Run WinDbg and choose **File** > **Open Executable**. Point the debugger to the [bin\ILikeToThrow.exe](bin/ILikeToThrow.exe) file.

Enter the `g` command to let the application start running (the debugger stops at the initial loader breakpoint when you use **File** > **Open Executable**). The application crashes.

Issue the `k` command to inspect the call stack. You can see that the application has thrown a C++ exception - the `MSVCR100!_CxxThrowException` function is responsible for this.

To obtain exception information, issue the `.exr -1` command. The exception object is the `Parameter[1]` field, but you don’t necessarily know the exception type.

```
0:000> .exr -1
ExceptionAddress: 76174b32 (KERNELBASE!RaiseException+0x0000006c)
   ExceptionCode: e06d7363 (C++ EH exception)
  ExceptionFlags: 00000001
NumberParameters: 3
   Parameter[0]: 19930520
   Parameter[1]: 010dfec0
   Parameter[2]: 008f2398
```

To obtain the exception type, pass the value of `Parameter[2]` to the following command (it deferences a few internal Visual C++ structures and obtains the exception class name in mangled form):

```
0:000> da poi(poi(poi(008f2398+0n12)+4)+4)+8
008f3040  ".?AVmy_elaborate_exception@@"
```

Now that you know the exception class name, pass the exception object to the `dt` command to inspect its properties:

```
0:000> dt 010dfec0 my_elaborate_exception
ILikeToThrow!my_elaborate_exception
   +0x000 __VFN_table : 0x008f2120 
   +0x004 _Mywhat          : (null) 
   +0x008 _Mydofree        : 0
   +0x00c _error_message   : 0x011e54a8  "error: 1244 4dc"
   +0x010 _error_code      : 0n1244
```
