### Analyzing a .NET Memory Leak with DebugDiag

In this lab, you will diagnose a memory leak using DebugDiag's analysis engine. You don't have to capture the dump file with DebugDiag to gain access to a fairly sophisticated memory leak analysis engine based on CLRMD.

#### Task 1

Run the SDPApp web application (see instructions in the [DebugDiag Dumps](../dbg-dumps-debugdiag/) lab). Click some speaker links (except for Sasha Goldshtein) multiple times to view the speaker details. The application’s memory should go up every time you refresh a speaker page (this should be visible even in Task Manager as you inspect the iisexpress.exe process). This is the memory leak we are trying to diagnose.

#### Task 2

Run the 32-bit version of Task Manager. On a 32-bit system, it’s located in C:\Windows\System32\taskmgr.exe. On a 64-bit system, it’s located in C:\Windows\SysWOW64\taskmgr.exe. Locate the iisexpress.exe process, right click it, and choose **Create Dump File**. After a few seconds Task Manager will display the full path to the dump file; copy it to the clipboard.

#### Task 3 

From the Start menu, run the **DebugDiag 2.0 Analysis** application. Check the **DotNetMemoryAnalysis** rule and click the **Add Data Files** button on the bottom of the screen. Paste the full path to the dump file and apply the changes. Finally, click **Start Analysis**.

Inspect the generated analysis report to see which objects are taking up lots of memory. Pay special attention to the web cache size, displayed towards the end of the report. Try to figure out what is causing the memory leak, and confirm by looking at the [application's source code](../dbg-dumps-debugdiag/src) and optionally fixing the leak.
