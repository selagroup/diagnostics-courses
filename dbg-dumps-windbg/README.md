### Basic Managed Dump Triage with WinDbg

In this lab, you will perform basic dump analysis with WinDbg. This is an important skill to have, because you might find yourself in a production environment where you don't have access to Visual Studio, but will still need to get a quick reading into why your application crashed or misbehaved.

#### Task 1

Run WinDbg from the Windows SDK installation directory. You need the 32-bit version of WinDbg, which will reside in the following folder for a typical Windows SDK 8.1 installation:

```
C:\Program Files (x86)\Windows Kits\8.1\Debuggers\x86\windbg.exe
```

For a Windows SDK 10 installation, it will typically be in the following folder:

```
C:\Program Files (x86)\Windows Kits\10\Debuggers\x86\windbg.exe
```

Click **File** > **Symbol File Path** and make sure the path consists of two parts, separated by a semicolon:

```
srv*C:\symbols*http://msdl.microsoft.com/download/symbols;%COURSEDIR%\dbg-dumps-wer\bin
```

Drag the dump file generated in the [WER Dumps lab](../dbg-dumps-wer/) onto the WinDbg surface. In the command window on the bottom, issue the following command:

```
!analyze -v
```

The debugger should then print the details and the call stack of the exception that occurred in the application, among a bunch of additional details.

#### Task 2

To see source-level information (source file name and line number) and additional data on parameters and local variables, issue the following commands:

```
.load %COURSEDIR%\tools\sosex_32\sosex.dll
!mk -a
```

> Note: WinDbg doesn't expand environment variables. You will need to replace %COURSEDIR% in the above command with the actual value of the environment variable.

Feel free to experiment with the hyperlinks: clicking a hyperlink next to a parameter or local variable will provide some additional information about that object, similar to hovering over a variable in Visual Studio.
