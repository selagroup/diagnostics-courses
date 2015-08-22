### Collecting IntelliTrace Logs with the Stand-Alone Collector

In this lab, you will use the IntelliTrace stand-alone collector (IntelliTraceSC.exe) to obtain a trace of an application's execution and subsequently analyze it in Visual Studio.

#### Task 1

We are working with a WPF application that causes an exception on the user's machine but doesn't display detailed information that helps diagnose the exception. Try to run it (SDPApp.WPF.exe) from the [bin](bin/) folder and note that it's displaying an extremely unhelpful error message.

Open a command prompt and navigate to the directory where you expanded the IntelliTrace Stand-Alone Collector. Run the following command to launch and trace the application's execution:

```
IntelliTraceSC.exe /logfile:C:\temp\log.itrace /collectionplan:collection_plan.ASP.NET.trace.xml launch %COURSEDIR%\dbg-intellitrace-sc\bin\SDPApp.WPF.exe
```

When the application exits, an .itrace file is finalized in the C:\temp folder.

#### Task 2

Double-click the .itrace file to open it in Visual Studio and navigate to the  `KeyNotFoundException` event in the **IntelliTrace Events** view. That's the root cause of the exception, which is then rethrown multiple times. From the exception's location and call stack you can determine that a certain speaker's name was not found in the dictionary of speakers.

> If prompted for source code, point Visual Studio to the [src](src/) folder.

To figure out which speaker it was, use the buttons in the left margin to go up to the line that assigns the `speakerName` variable. In the **Locals** window you can see the result of the JSON `TryConvert` call, which returns "TBD" -- so we have a session with no speaker assigned! To fix this issue, we simply need to ignore such sessions. Feel free to do so by modifying the application's [source code](src/).
