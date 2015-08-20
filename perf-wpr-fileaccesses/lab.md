### Analyzing I/O Activity with WPR and WPA

In this lab, you will use Windows Performance Recorder and Windows Performance Analyzer to collect and analyze file I/O activity on your system. Determining which files are accessed while an application is running can be important for many kinds of production-time investigations:

* Understanding slow startup caused by many file accesses
* Troubleshooting failed accesses when the application doesn’t make it clear which file is involved
* Auditing the system for suspicious file activity

#### Task 1

Run Windows Performance Recorder (use the Start menu to look it up). Make sure the **File I/O activity** checkbox under **Resource Analysis** is checked. You can select additional checkboxes as well if you’d like to collect additional data. When you’re ready, click **Start**.

Open a command prompt (cmd.exe) and run the following command:

```
dir C:\ /s
```

After a few seconds (or when the dir command completes), click **Save**, pick a location for the .etl file, and click **Save** again. It might take a couple of minutes to generate the final .etl file.

#### Task 2

If you haven’t closed Windows Performance Recorder yet, click **Open in WPA**. Otherwise, navigate to the directory where you saved the .etl file and double-click it. This will open Windows Performance Analyzer (WPA).

In the Graph Explorer, expand the **Storage** category and then expand the **File I/O** category. Drag the **Duration by Process, Thread, Type** graph to the main analysis pane on the right. In the table on the bottom of the screen, locate the cmd.exe process, right-click it, and choose **Filter to Selection**. Explore the table and the grouping options – you’re looking at all the file accesses performed by the cmd.exe process.

Right-click the table heading and remove the **Process**, **Thread**, **Event Type**, and **Event Sub Type** columns. Instead, drag the **File Name** column to the left of the gold bar. You should now see the files that were accessed the most by the cmd.exe process. Try sorting by duration or size.

#### Task 3

While still in Windows Performance Analyzer, click **Trace > Load Symbols**. This is a time-consuming operation, especially if you’re on a slow WiFi connection. You might want to grab a cup of coffee at this point.

In the table you were working on, remove the **File Name** column and add the **Stack** column instead. Make sure it’s on the left of the gold bar. Expand the stack tree to see interesting function names inside the cmd.exe process, such as `cmd.exe!WalkTree` and `cmd.exe!PrintPatterns`.

#### Task 4 (Optional)

If you’d like to see WPA’s support for managed call stacks as well, you can repeat this experiment with a managed application. Try recording a trace while opening a solution in Visual Studio.
