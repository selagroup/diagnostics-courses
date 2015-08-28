### Locks and Waits Analysis with WinDbg

In this lab, you will experiment with applications that exhibit blocked threads, and diagnose the wait reason and the wait objects. You will also see how to determine which thread owns the lock in case that information can be obtained.

#### Task 1

Run the BatteryMeter.exe application from the [bin](bin/) folder. Click the spin control on the bottom right of the screen. The application stops responding. Attach WinDbg or capture a hang dump.

Use the `~`, `kb`, and any other commands to understand what all of the application's threads are doing (note that if you’re working with a live process, one of the threads will be a debugger thread that is not interesting for our analysis). To inspect handle arrays, use `dd` and `!handle`.

For example, if you encounter a thread with the following call stack, you should focus on the four handles located at the address 0107eef8 to understand what the thread is waiting for. (The first parameter to `WaitForMultipleObjects` is the number of handles in the wait array, and the second parameter is the pointer to an array of handles being waited on.)

```
0:000> kb
ChildEBP RetAddr  Args to Child              
0107ec70 75e1c752 00000004 0107eef8 00000000 ntdll!NtWait...
0107edf4 75006ab5 00000004 00000000 00000001 KERNELBASE!Wait...
0107ee10 011b2eb2 00000004 0107eef8 00000001 kernel32!WaitForMultipleObjects+0x19
...more output truncated...
0:000> dd 0107eef8 L4
0107eef8  00000134 00000138 0000013c 00000140
0:000> !handle 134 f
...output truncated...
```

You will encounter threads that wait for a critical section (they will have `ntdll!RtlEnterCritical` section on their call stack). Use the `!cs -l -o` command to inspect all locked critical sections and their owning stack traces; or use `!cs -o <address>` to view a single critical section and information about its owning thread.

Draw a wait chain of the application's threads and describe the deadlock that occurred. In a wait chain, there are two types of nodes: thread nodes, representing a thread that waits for a synchronization mechanism; and synchronization object nodes, representing a synchronization object that is waited for and owned by some thread. An edge from a thread to a synchronization object means the thread is waiting for that object, and an edge from a synchronization object to a thread means the object is owned by that thread.

> Note: Not all synchronization objects have owners. In fact, only the following Win32 synchronization objects have easily identifiable owners: mutexes and critical sections.

If you encounter a thread that is waiting in a `SendMessage` call, don't give up. Check (on MSDN or a search engine) which parameter of `SendMessage` is the window handle (HWND). The answer: the first parameter.

Use Spy++ from the Visual Studio tools to search for a window with that HWND. In the Processes tab in Spy++ you can find the owner thread for that window and proceed with the wait chain.

At the end of this analysis, you should be equipped to explain exactly why the main application thread is not responding. You should be able to trace through all the application's threads and understand the wait relationships between them.

#### Task 2

Run the ElaborateDeadlock.exe application from the [bin](bin/) folder. Attach WinDbg and inspect the application's threads. Use `~` and `kb` to see the handles the threads are waiting for and pass them to the `!handle` command. For maximum information about a handle (especially mutex handles, that have ownership information), use the `!handle <value> f` command.

At this point, you can draw a wait chain of the application's threads and determine the precise structure of the deadlock. However, doing this manually is quite time consuming. Instead, load the wait chain traversal extension wct_x86.dll from the [tools](tools/) folder using the `.load` command. The `!wct_proc` command will then display the wait chains for all the threads in the current process.
