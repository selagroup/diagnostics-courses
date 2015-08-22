### Basic Dump Analysis with Visual Studio

In this lab, you will open a dump file and perform basic exception analysis in Visual Studio.

#### Task 1

Open Visual Studio and drag the dump file generated in the [previous lab](../dbg-dumps-wer/) onto its surface. Visual Studio should open a Minidump File Summary window with some basic information about the dump.

Click the **Set Symbol Paths** link on the top right of the window, and add the full path to the [%COURSEDIR%\dbg-dumps-wer\bin](../dbg-dumps-wer/bin/) folder, which is where the application's .pdb files (debugging symbols) reside. Visual Studio will then use this path to locate the application’s symbols. When ready, click **Debug with Managed Only**. If prompted for the location of the source code, provide the path to the [%COURSEDIR%\dbg-dumps-wer\src\FileExplorer](../dbg-dumps-wer/src/FileExplorer/) folder.

At this point, you should see the exact exception that occurred in the application. You can review the call stack, local variables, other threads, and the application’s source code until you are confident why the exception occurred and how to fix the issue.
