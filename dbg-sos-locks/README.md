### Locks and Wait Analysis with WinDbg and SOS

In this lab, you will diagnose synchronization-related issues using WinDbg, SOS, and SOSEX. Visual Studio offers no particular help with viewing synchronization objects and handles, so even if you are debugging in the development environment, you'll need WinDbg for complete mastery of locks and waits.

#### Task 1

Run the FileExplorer.exe application from the [bin](bin/) folder. Expand the directory tree on the left and click some directories until you reach one that has files in it (the files are displayed on the right). Double-click one of these files. A Notepad window opens. You can close Notepad and go back to the application, but unfortunately it stops responding.

> Throughout the rest of this lab, we will use WinDbg for live debugging (attaching to a process), but these steps are perfectly applicable to a dump file as well.

Run WinDbg (make sure to use the same bitness as the target process) and attach to the process using **File** > **Attach to Process** and selecting FileExplorer.exe from the list.

> You can use Task Manager to determine the bitness of a particular process.

Load SOS into the debugging session using the `.loadby sos mscorwks` command. Switch to the main thread using the `~0s` command and inspect its call stack using the `!CLRStack` command. It seems to be stuck in the `FileExplorer.MainForm.listBox1_DoubleClick` method, but it’s not clear what it is waiting for.

Issue the `!Threads` command to get a general picture of which threads are running in this application.

Load the SOSEX extension using the `.load %COURSEDIR%\tools\sosex_32\sosex.dll` command (if the target is a 32-bit process) or `.load %COURSEDIR%\tools\sosex_64\sosex.dll` (if the target is a 64-bit process). Note that WinDbg doesn't expand environment variables, so you will need to replace **%COURSEDIR%** with the actual value.

Run the `!mwaits` command to see which synchronization objects your threads are waiting for. You will notice that thread 0 is waiting for a sync block.

> A sync block, or monitor, is the synchronization mechanism behind C#'s `lock` keyword. The underlying BCL methods are `Monitor.Enter` and `Monitor.Exit`. Although it is an implementation detail, monitors are implemented using Win32 event objects.

To determine who owns the sync block, issue the `!mlocks` command. You can now build a wait chain – you know that thread 0 is waiting for a lock held by some other thread, and so forth.

In your wait chain, there should be two kinds of nodes: Thread nodes and SyncObject nodes. There should be a link between a Thread node and the SyncObject node it waits on, and there should be a link between a SyncObject node and the Thread node that owns this object. After completing the wait chain, you should see a cycle, indicating a deadlock.

To streamline the analysis, issue the `!dlk` command. SOSEX will report that it found a deadlock and explain which threads are involved in it and how.

#### Task 2

Run the SimpleWriteApp.exe application from the [bin](bin/) folder. It does not terminate, whereas it should under normal conditions. Attach WinDbg to the application. Note that this is a 32-bit application, so you should use the 32-bit version of WinDbg regardless of your operating system bitness.

Load SOS (using the `.loadby sos clr` command) and use the standard SOS commands (`!Threads`, `!CLRStack -a`) to make a first attempt at hang diagnostics.

There are two threads that seem to be using an instance of the `MyReaderWriterLock` class. Are both threads using the same instance? Pay attention to the following remark from the [class' XML documentation](src/SimpleWriteApp/Program.cs#L11): "If more than one writer attempts to acquire the lock at the same time, the behavior is undefined."

Inspect the `MyReaderWriterLock` instances used by the two threads with the `!do` command. Inspect their internal implementation state (the semaphore) and its handle. For example, use the `!handle 100 f` command to view handle information for handle 100.

Consult the application's [source code](src/SimpleWriteApp) and finalize your understanding of the deadlock's cause. Suggest how the `MyReaderWriterLock` implementation can be improved to prevent this deadlock.
