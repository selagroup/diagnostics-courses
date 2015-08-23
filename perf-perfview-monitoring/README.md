### Setting Up Continuous ETW Monitoring with PerfView

In this lab you will experiment with continuous ETW monitoring and analysis with PerfView, and also review how to diagnose IIS requests in PerfView reports.

#### Task 1

Open the [SDPApp.sln](src/SDPApp.sln) solution in Visual Studio. Build and run the web application and make sure it displays a list of conference speakers. You might need to manually restore NuGet packages for the solution. Build the application and publish it to your local IIS instance.

> Do not use IIS Express for this lab. If you do not have IIS installed, you can install it from the Windows **Programs and Features** control panel applet (or the **Features** pane in the Windows Server Manager on server systems).

Refresh the home page a few times. You'll notice that most refresh operations return instantaneously (the speakers' info is served from an in-memory cache), but occasionally the refresh takes a few seconds to complete. This is the kind of issue that is hard to diagnose with a profiler -- and besides, who's going to let you attach an invasive profiler to a production service? Instead, we're going to use continuous ETW collection. We will record ETW events to an in-memory buffer that discards old events if the buffer runs out of space. As soon as we hit the performance issue, we will stop the recording and review the contents of the buffer.

> Note: In this lab we are going to stop the recording manually. However, PerfView also has a set of automatic triggers that you could use to stop the recording. For example, PerfView can stop collection automatically when:
>
> * A performance counter exceeds a certain threshold
> * A specific exception occurs
> * A specific event log message is written
> * An IIS request takes longer than a certain number of seconds to complete

#### Task 2

Open a command prompt and navigate to the directory where you placed PerfView.exe. Run the following command that will start a PerfView recording into a 512MB memory buffer:

```
PerfView start -LogFile:C:\Temp\PerfView.log -CircularMB:512 -ThreadTime -AcceptEULA -DataFile:C:\Temp\PerfView.etl -NoView
```

> PerfView has a certain performance impact, but it should be negligible because it’s not writing the logs to disk -- only to a memory buffer. With that said, it *will* cost you 512MB physical memory. Because 512MB is a fairly small buffer, it might only be enough for a minute or so. This is why it’s very important to stop the recording at the right moment.

Go back to the browser and start refreshing the page. As soon as you hit one of these pesky delays, run the following command that will stop the PerfView recording and flush the buffer to a file:

```
PerfView stop -LogFile:C:\Temp\PerfView.log
```

After a minute or so, the log file will stabilize and end with a line similar to "Generating NGEN Pdbs took 34.4 seconds". This is your cue that you can move to the next step, to actually analyze the results.

#### Task 3

Go back to the command prompt and run `PerfView C:\Temp\PerfView.etl.zip`. The PerfView UI will open and the status bar will provide the progress of the open operation. After a few dozen seconds you should be able to see the various reports aggregated under the PerfView.etl file.

![Screenshot of PerfView window with open report list](figure1.png)

Double-click the **Asp.Net Stats** report to review some general information on web request performance in the application. On the bottom of the screen you can see statistics for the request URLs that were present in the trace. You can immediately see that the root URL (`/`) has a maximum response time of over 5,000ms.

Back in the main PerfView window, double-click the **ASP.NET Thread Time (with Tasks) Stacks** report. Pick the w3wp process in the process picker. Under **Requests** in the tree, start expanding the expensive requests until you see the call stack that was responsible for the long delay. At this point, you can review the application's [source code](src/) to see the exact problematic line of code.
