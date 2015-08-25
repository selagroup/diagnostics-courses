### Diagnosing Handle Corruptions and Leaks

In this lab, you will learn how to diagnose Windows handle-related problems, such as handles that are closed prematurely or handles that are not closed, causing a resource leak.

#### Task 1

Run the BatteryMeter.exe application from the [bin\leak](bin/leak/) folder. After a few moments, files begin to pile up in the application's execution directory. Try deleting one of the files while the application is running. You should receive an error that says the file is still in use by BatteryMeter, which means the application is not letting go of the file handle.

It is evident that the application is leaking files, and doesn't really seem to be using them. We now want to determine where these files are being created. Run WinDbg and attach to the application (**File** > **Attach to Process**). Issue the command `!htrace -enable` to turn on handle tracing for the target process.

Let the application run (F5) and create a few more files. Use `!handle 0` to see all file handles in the debugger. Pass some of the newer file handles to the `!htrace` command (e.g. `!htrace 284`). You should see the allocating call stack and determine where the application is creating the files. Hopefully, this will allow you to determine why the files aren't being closed.

Try using `!htrace -diff` directly to see a report that displays only call stacks for handles that have been opened since the previous htrace snapshot (when you ran `!htrace -enable`) and have not been closed yet. This should produce similar call stacks to what you saw in the previous step.

#### Task 2

Run the BatteryMeter.exe application from the [bin\invalid](bin/invalid/) folder. Click the control in the bottom right corner of the application. The application stops responding. Attach WinDbg to the process (**File** > **Attach to Process**) and inspect the application's threads using the `~* e kb` command. The main thread should be waiting for two handles, which you can retrieve from the handle array by looking at its memory:

```
0:000> kb
ChildEBP RetAddr  Args to Child              
00e3f094 76bfea7f 00000002 00e3f24c 00000000 ntdll!NtWaitForMultipleObjects+0xc
00e3f218 75299188 00000000 00e3f24c 00000001 KERNELBASE!WaitForMultipleObjectsEx+0xdc
00e3f234 00d21a66 00000002 00e3f24c 00000001 KERNEL32!WaitForMultipleObjects+0x19
00e3f254 514dbcdf 00e3f6a8 00e3f2fc 00e3fa48 ...
00e3f268 514dbea4 000003ec 0000003d 004efd2e mfc100u!_AfxDispatchCmdMsg+0x58
00e3f284 5150fd2a 000003ec 0000fd2e 00e3f2cc mfc100u!CCmdTarget::OnCmdMsg+0x124
```

In the above call stack, the first parameter to `WaitForMultipleObjects` is the number of handles in the wait array (which is 2), and the second parameter is a pointer to the wait array -- an array of handles. Use the `dd` command to view the handles themselves, as in the following example:

```
dd 00e3f24c L2
```

Inspect both handles by passing them to the `!handle` command. If you receive an error, use `!error <errorcode>` to determine what the error number means.

It seems that the main thread is waiting for an *invalid handle*. If the handle was invalid in the first place, the thread would not be able to start waiting for that object. The only situation in which we would catch a thread waiting for an invalid handle is if the handle was closed after the thread has already started the wait. Therefore, we have to determine who is responsible for closing that handle.

Run the application again from WinDbg (**File** > **Open Executable**). Before clicking the control in the bottom right corner, issue the `!htrace -enable` command in the debugger.

Click the control in the bottom right corner. If you received an "Invalid handle" exception in the debugger, this is a good sign: Windows detected that the application is attempting to call `SetEvent` on an invalid handle. Use the `kb` command to find the handle value and then use `!htrace <handle>` to look for that handle and see who closed it.

Regardless of whether or not you received the "Invalid handle" exception, switch to the main thread and check the second handle that the main thread is waiting on. Use `!htrace <handle>` to look for that handle and see who closed it.

At this point, you know that the main thread is waiting for a handle that was closed by some other thread. The underlying event object is still there (because the main thread is still waiting for it), but there is no way for it to become signaled because the handle has been closed; and hence, there is no way for the main thread to ever stop waiting for it.

#### Task 3

We will now revisit the BatteryMeter application from Task 1 and use Sysinternals Process Explorer and Process Monitor to diagnose the handle leak without using a debugger.

Run the BatteryMeter.exe application from the [bin\leak](bin/leak/) folder.

Run Sysinternals Process Explorer and click **File** > **Show Details for All Processes** to make sure it launches itself as administrator. Find the BatteryMeter process, select it, and click Ctrl+H to display its open handles. Inspect the file handles and note that they are piling up and are not getting closed. If you’d like, try to close one of the handles (right-click it and select **Close Handle**) and then attempt to delete the file it was pointing to; you should be able to delete it successfully.

Run Sysinternals Process Monitor, and open the filter dialog by using **Filter** > **Filter**. Configure a filter for the BatteryMeter.exe process, the `CreateFile` operation, and for paths that end with **.tmp**, so that you don't have to look at thousands of events.

Configure symbols by using **Tools** > **Configure Symbols**: add the %COURSEDIR%\dbg-windbg-handles\bin\leak folder to the symbol path, and the %COURSEDIR%\dbg-windbg-handles\src\leak folder to the source path. Also, make sure to point Process Monitor to the location of dbghelp.dll in the Debugging Tools for Windows folder (and not the one that ships with Windows, in C:\Windows\System32).

Right-click one of the `CreateFile` events and choose **Stack** to see the stack of the thread creation events. This should be the same stack you obtained in Task 1 by using `!htrace`.

Close Process Monitor and the BatteryMeter application.
