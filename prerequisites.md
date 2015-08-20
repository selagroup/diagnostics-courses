You will need the following software to work on these hands-on labs:

* Windows 8 or newer 64-bit operating system
* [Windows 10 SDK](https://dev.windows.com/en-us/downloads/windows-10-sdk)
* [Visual Studio 2015](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx)
* [PerfView 1.7](http://www.microsoft.com/en-us/download/details.aspx?id=28567)

Also, make sure to configure the `_NT_SYMBOL_PATH` environment variable to point to the Microsoft symbol servers. You can use the command-line `SETX` command, or search the Windows start menu for "Environment variables". This is the value you should use:

```
srv*C:\symbols*http://msdl.microsoft.com/download/symbols
```
