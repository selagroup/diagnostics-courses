### Automatic Dump Analysis with CLRMD

In this lab, you will build a simple application that traverses a directory full of crash dumps and performs automatic analysis that detects the exception and call stack of the faulting thread. This can serve as a foundation for a complete diagnostic solution that uses this information to automatically assign bugs, email developers, or aggregate information on failure types encountered at specific client sites.

#### Task 1

Create a new C# console application in Visual Studio. Add the **Microsoft.Diagnostics.Runtime** NuGet package. Make sure the application builds successfully.

> Note: Due to the fact that the unmanaged mscordacwks.dll library needs to be loaded into the analysis process, you will need a 32-bit (x86) build of this application to analyze 32-bit dump files and a 64-bit (x64) build to analyze 64-bit dump files.

#### Task 2

Accept a directory name as a command-line argument and traverse all the .dmp files in that directory (use `Directory.GetFiles` or a similar API). For each dump file, use the following code to create a CLRMD `ClrRuntime` object that can be used for inspecting threads:

```C#
DataTarget target = DataTarget.LoadCrashDump(filename, CrashDumpReader.ClrMD);
string dacLocation = target.ClrVersions[0].TryDownloadDac();
ClrRuntime runtime = target.CreateRuntime(dacLocation);
```

#### Task 3

Use the `ClrRuntime.Threads` property to enumerate all the managed threads, and identify the `ClrThread` whose `CurrentException` property is not null. This is likely the thread that caused the application to crash.

> There are some situations when the `CurrentException` property will be set even though there is no current exception being thrown on the thread. You can ignore them for now.

After detecting the `ClrThread` object that has an active exception, print out the exception type and exception message, and the thread's call stack (use `ClrThread.StackTrace`).

#### Task 4

Test your application by placing a few dump files in a directory and running it with the name of that directory as a command-line argument. You can use the dump files generated in the [WER Dumps](../dbg-dumps-wer/) lab.
