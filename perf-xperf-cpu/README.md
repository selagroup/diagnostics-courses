### Profiling CPU Work by Sampling with XPerf

In this lab, you will analyze the CPU performance of an application by using the non-intrusive ETW sampling profiling method.

#### Task 1

Open an elevated command prompt and navigate to the Windows Performance Toolkit installation directory (under the Windows SDK installation directory, which by default is in C:\Program Files (x86)\Windows Kits\8.1).

Run the following command to start capturing detailed profiling information:

```
xperf -on PROC_THREAD+LOADER+PROFILE -stackwalk Profile
```

3Run the PrimeNumberCalculation.exe application from the [bin](bin/) folder. Hit ENTER as necessary for the application to complete.

Run the following command to stop capturing the profiling information:

```
xperf -d C:\results.etl
```

Double-click the created ETL file (C:\results.etl) to open it in Windows Performance Analyzer. Click **Trace** > **Load Symbols** and wait for symbol resolution to complete. (This may take a long time when you launch it on a fresh system.)

On the left pane, click the **System Activity** expander, and then drag and drop the **Stack Events by Name** graph to the main pane on the right. If the graph is spiky, zoom in on the section that has many samples per time unit.

Click the small button on the top right whose tooltip is **Graph and Table**. In the table below, right-click the column headers to select the **Stack** column and remove the **Event Name** column. Make sure the **Stack** and **Process** columns are to the left of the gold bar (drag the column headers if necessary).

Expand the **PrimeNumberCalculation.exe** process in the table and then navigate down the stack tree to see where the application has spent a significant amount of CPU time.
