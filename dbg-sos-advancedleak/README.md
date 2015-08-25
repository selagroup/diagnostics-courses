### Advanced Memory Leak Analysis with SOS

In this lab, you will build upon your experience with SOS memory leak analysis and continue to explore additional memory leaks.

#### Task 1

Run the StrangeLeak.exe application from the [bin](bin/) folder. Establish that the application is leaking managed memory.

Attach WinDbg to the application and examine its memory usage. As always, load SOS using the `.loadby sos clr` command and then inspect the heap with the `!dumpheap -stat` and `!gcroot` commands. (Consult the [SOS Leak Lab](dbg-sos-leak/) for more detailed instructions.)

Ascertain that the finalizer thread is responsible for blocking the application’s memory cleanup. Switch to the finalizer thread and inspect its complete call stack by loading SOSEX (`.load %COURSEDIR\tools\sosex_32\sosex.dll`) and running the `!mk` command.

You should now be confident that the finalizer thread is stuck trying to release a COM object (`Marshal.FinalReleaseComObject`), which resulted in a switch to the STA thread that owns that object (`MTAThreadDispatchCrossApartmentCall` or `GetToSTA` functions appear on the stack, depending on the operating system version).

To determine which thread is the owner of the STA object to which your thread is attempting to switch, inspect the unmanaged call stack of the thread using the `kbn` command. Locate the `ole32!CRpcChannelBuffer::SendReceive2` frame and make note of its first parameter (the first value that appears under the **Args to Child** heading).

On Windows 7 and earlier operating systems, use the following commands to print out the process ID and thread ID of the STA thread (replace *firstparam* with the value of the parameter from the previous step):

```
r $t0 = poi(firstparam+0n24)
.printf "Process id = %x, thread id = %x\n", dwo(@$t0+0n8), dwo(@$t0+0n12)
```

On Windows 8 and later operating systems, private symbols for combase.dll are available, so we don't have to guess -- we need to inspect the OXIDEntry that belongs to the RPC channel (replace *number* with the number of the `CRpcChannelBuffer::SendReceiver2` frame):

```
.frame number
.printf "Process id = %x, thread id = %x\n", @@(this->_pOXIDEntry->_dwPid), @@(this->_pOXIDEntry->_dwTid)
```

Fortunately, this STA thread is in our process (this is not a cross-process COM call). Therefore, we can inspect its current stack using the `~~[tid] e !clrstack` command (replace *tid* with the thread id obtained in the previous steps). What is this thread doing? Is it blocked?

At this point, you can conclude that there is a memory leak because the finalizer thread needs to perform a cross-apartment COM call to another thread, which isn't able to service the request because it is blocked.

#### Task 2

> Performing this lab requires familiarity with WCF (Windows Communication Foundation) and its concurrency model.

Run the MemoryExhaustingService.exe application from the [bin](bin/) folder. Monitor its memory usage with Performance Monitor and make sure there is a memory leak.

Perform memory leak analysis as in the previous part and try to determine the cause of the memory leak. You might find it useful to inspect the various threads created by the application (their number also seems to accumulate as time goes by).

> Tip: The Visual Studio Parallel Stacks window can be very helpful in situations when you have a large number of threads and you want to see at a glance what they are doing. In this case, you will notice many threads have a large portion of their call stack in common, indicating that they are queueing up in some central point.

For a detailed walkthrough of this scenario, consult [this blog post](http://blogs.microsoft.co.il/blogs/sasha/archive/2010/08/19/the-case-of-the-rogue-heartbeat-timer.aspx).
