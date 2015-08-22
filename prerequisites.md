### Before You Begin

Before you start working on the labs, make sure you performed all the configuration tasks described in this document.

#### Required Software

You will need the following software to work on these hands-on labs:

* Windows 8 or newer 64-bit operating system
* [Windows 10 SDK](https://dev.windows.com/en-us/downloads/windows-10-sdk)
* [Visual Studio 2015](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx)
* [PerfView 1.7](http://www.microsoft.com/en-us/download/details.aspx?id=28567)
* [DebugDiag 2.0](http://www.microsoft.com/en-us/download/details.aspx?id=42933)
* [Sysinternals Suite](https://technet.microsoft.com/en-us/sysinternals/bb842062.aspx)
* [IntelliTrace Stand-Alone Collector](https://www.microsoft.com/en-us/download/details.aspx?id=44909)

#### Installing IntelliTrace Stand-Alone Collector

After downloading the stand-alone collector, run the downloaded executable to extract the IntelliTraceCollection.cab file. Open a command prompt window, navigate to the directory where you put the .cab file, and run the following command to extract it (note the trailing dot -- it is required):

```
expand /f:* IntelliTraceCollection.cab .
```

#### Symbol Path

Configure the `_NT_SYMBOL_PATH` environment variable to point to the Microsoft symbol servers. You can use the command-line `SETX` command, or search the Windows start menu for "Environment variables". This is the value you should use:

```
srv*C:\symbols*http://msdl.microsoft.com/download/symbols
```

#### Course Directory

For convenience, set the `COURSEDIR` environment variable to the root of this repository. For example, if you cloned (or downloaded) this repository to C:\Sela, run the following command from a Command Prompt window:

```
setx COURSEDIR C:\Sela
```
