### .NET Memory Leak Analysis with WinDbg and SOS

In this lab, you will diagnose managed memory leaks with WinDbg, SOS, and SOSEX. In many cases, memory leak diagnostics is easier with a dedicated memory profiler (such as JetBrains dotMemory or SciTech .NET Memory Profiler), or even with [PerfView heap snapshots](../perf-perfview-netleak). Still, it's important to be familiar with the basic techniques for analyzing memory leaks with free, non-invasive tools.

#### Task 1

Launch the FileExplorer.exe application from the [bin](bin/) folder. It takes a little while to launch because it enumerates all the folders in your Program Files directory.

Run Sysinternals Process Explorer (procexp.exe) and double-click the FileExplorer.exe process. On the **.NET Performance** tab, note the values of the **# Bytes in all Heaps** performance counter. This is the application's managed memory usage.

Navigate through some of the folders displayed in File Explorer's folder tree. Note that the application's memory usage increases. It will go down occasionally as garbage collections occur, but the overall pattern is that the more folders you navigate, the higher the memory usage climbs.

#### Task 2

Run WinDbg (the 32-bit version) and attach to the FileExplorer.exe process. Load SOS using the `.loadby sos clr` command.

To get an initial reading of the application's memory usage, run the `!dumpheap -stat` command. You'll notice that most of the application's memory is consumed by strings (`System.String`).

Run the `g` command to let the application continue running. Keep navigating between folder in the UI so that the managed memory usage increases even further, and then break into the debugger (using the **Debug** > **Break** menu item, or the Ctrl+Break shortcut on your keyboard) and run `!dumpheap -stat` again. 

Although it is obvious that strings are responsible for most of the memory leak, it is really hard to chase every single string and figure out where it is coming from and why it is not being reclaimed. Instead, we will now try to identify higher-level application objects that are also accumulating.

#### Task 3

The `MainForm+FileInformation` objects seem to be accumulating as well. It would make sense to assume that for each file displayed in the File Explorer UI, a `FileInformation` object will be created. However, as soon as you navigate away from their folder, these objects should probably be destroyed. Having hundreds, or even thousands, of these objects is suspicious.

List all objects of type `MainForm+FileInformation` by executing the following command:

```
!dumpheap -type FileExplorer.MainForm+FileInformation
```

Now, pick a few object addresses (the first column in the output) and pass them to the `!objsize` command. This will tell you whether these objects are responsible for keeping a large graph of additional objects in memory. It turns out that a typical `FileInformation` object is keeping multiple kilobytes of additional memory alive. We now turn to figuring out what is keeping these objects in memory.

#### Task 4

Pick a few `FileInformation` objects and pass their addresses to the `!gcroot` command. This command should tell you why these objects are being retained -- in other words, why the GC isn't able to let them go.

You will probably see some objects with a reference chain that looks as follows:

```
0:004> !gcroot 0674efa4 
HandleTable:
    00a513ec (pinned handle)
    -> 036d34c8 System.Object[]
    -> 08f799c0 System.EventHandler
    -> 0681019c System.Object[]
    -> 06751688 System.EventHandler
    -> 0674efa4 FileExplorer.MainForm+FileInformation

Found 1 unique roots (run '!GCRoot -all' to see all roots).
```

And some other objects (especially the ones closer to the end of the `!dumpheap` output) will have two reference chains:

```
0:004> !gcroot 08ef8a48 
Thread f6c:
    0096f2c8 641ce246 System.Windows.Forms.Application+ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr, Int32, Int32)
        ebp+50: 0096f2f4
            ->  03226b7c System.Windows.Forms.Application+ComponentManager
            ->  03226bdc System.Collections.Hashtable
            ->  03226c10 System.Collections.Hashtable+bucket[]
            ->  03226bcc System.Windows.Forms.Application+ComponentManager+ComponentHashtableEntry
            ->  026d5264 System.Windows.Forms.Application+ThreadContext
            ->  026eae34 System.Windows.Forms.ApplicationContext
            ->  026d36fc FileExplorer.MainForm
            ->  026d4ac8 System.Windows.Forms.PropertyStore
            ->  032265c4 System.Windows.Forms.PropertyStore+ObjectEntry[]
            ->  026eac3c System.Windows.Forms.Form+ControlCollection
            ->  026eac54 System.Collections.ArrayList
            ->  026eac6c System.Object[]
            ->  026e90fc System.Windows.Forms.SplitContainer
            ->  026e9270 System.Windows.Forms.PropertyStore
            ->  03226640 System.Windows.Forms.PropertyStore+ObjectEntry[]
            ->  026e9718 System.Windows.Forms.SplitContainer+SplitContainerTypedControlCollection
            ->  026e9758 System.Collections.ArrayList
            ->  026e9770 System.Object[]
            ->  026e954c System.Windows.Forms.SplitterPanel
            ->  026e9604 System.Windows.Forms.PropertyStore
            ->  026eda7c System.Windows.Forms.PropertyStore+ObjectEntry[]
            ->  026ea6ac System.Windows.Forms.Control+ControlCollection
            ->  026ea6c0 System.Collections.ArrayList
            ->  026ea6d8 System.Object[]
            ->  026ea994 System.Windows.Forms.ListBox
            ->  03228574 System.Windows.Forms.ListBox+ObjectCollection
            ->  03228584 System.Windows.Forms.ListBox+ItemArray
            ->  0d04d4dc System.Windows.Forms.ListBox+ItemArray+Entry[]
            ->  08f799e0 System.Windows.Forms.ListBox+ItemArray+Entry
            ->  08ef8a48 FileExplorer.MainForm+FileInformation

HandleTable:
    00a513ec (pinned handle)
    -> 036d34c8 System.Object[]
    -> 08f799c0 System.EventHandler
    -> 0681019c System.Object[]
    -> 08f799a0 System.EventHandler
    -> 08ef8a48 FileExplorer.MainForm+FileInformation

Found 2 unique roots (run '!GCRoot -all' to see all roots).
```

What this output means is that some `FileInformation` objects are retained because they are held by a `ListBox`'s item collection, and the `ListBox` is part of the main form which is currently displayed on-screen. But *most* `FileInformation` objects are not retained by the `ListBox`; they are retained only by an `EventHandler` that we don't have a lot of details about. Note that all the chains have the same `EventHandler` as the second link -- in the output above, it's the `EventHandler` at address 08f799c0.

Take a look at that `EventHandler` next by passing its address to the `!do` command. Note the `_invocationCount` field -- this is the number of delegates registered to the event.

#### Task 5

It looks like `FileInformation` objects are being retained by some event. There are hundreds, or even thousands, of these objects registered to the event. Take a look at the [application's source code](src/FileExplorer/) to figure out which event is responsible for the leak, and fix the code so that the leak goes away.

Finally, run the application again and use Process Explorer (and WinDbg if necessary) to confirm that the memory leak is gone, no matter how many folders you navigate.

#### Task 6

Run the MemoryLeak.exe application from the [bin](bin/) folder. Let it run for a few seconds, and then attach WinDbg (mind the bitness) and execute the following commands:

```
.loadby sos mscorwks
!dumpheap -stat
```

Most of the memory is probably consumed by arrays of bytes. Let the application continue running (`g`), break into the debugger after a few seconds, and repeat the analysis to see what the memory leak looks like.

Use the following command to view all instances of `byte[]` that are present on your GC heap:

```
!dumpheap -type System.Byte[]
```

Use the `!gcroot` command to view the GC root chain for several byte array instances. Analyze at least ten samples from various addresses and attempt to reach a conclusion as to why the instances of `byte[]` are not released.

> You can automate this (rather tedious) process by running a `.foreach` loop in WinDbg. Try it:
>
> ```
> .foreach (obj { !dumpheap -type System.Byte[] -short }) { !gcroot obj }
> ```
>
> This command can be rather time-consuming, so don't be afraid to hit Ctrl+Break to stop it in the middle.

* What is stopping the byte array instances from being collected?
* What should be the next step to diagnose the root cause of the memory leak?

Use the `!FinalizeQueue` command to inspect the finalization queue. Determine how many object instances are currently waiting for finalization. Next, use the `!Threads` command to determine which thread is the finalizer thread. The finalizer thread has the string "(Finalizer)" in the **Exception** column:

```
   ID	OSID	ThreadOBJ	...		APT	Exception
0  ...					            MTA	
2  ...					            MTA	(Finalizer)
```

Switch to the finalizer thread using the `~2s` command (substitute the actual thread number in your debugger session if necessary). Issue the `!CLRStack command to see the call stack of the finalizer thread. What is the finalizer thread doing?

It seems now that application creates objects at a faster rate than the finalizer thread is able to run their finalizers, because the finalizer thread seems to be asleep. Take a look at the [source code](src/MemoryLeak/) to ascertain.
