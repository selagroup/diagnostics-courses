### Command-Line Performance Analysis with `etrace`

In this lab, you will run some ad-hoc performane queries on a system using [`etrace`](https://github.com/goldshtn/etrace). Before you begin, you will need to download or clone the etrace repository, and compile it using Visual Studio 2015 or later.

#### Task 1

Open an elevated command prompt and navigate to the output directory where you compiled etrace.

Run the following command to monitor process start events:

```
etrace --kernel Process --event Process/Start
```

Run one or two processes and note them showing up in the trace. Hit Ctrl+C to stop recording at any time.

Next, run the following command to monitor GC allocation events:

```
etrace --clr GC --event GC/AllocationTick
```

Run Visual Studio or open a solution in an existing Visual Studio instance to see the GC events occur in real-time. You can also experiment with some additional command-line options (run `etrace --help` to see a full list), for example to display only certain fields from each event, or to filter the output only to events that have a particular property (e.g., `ImageFileName=notepad`).

#### Task 2

Use etrace to answer the following questions about the live system and/or about one of the .etl files you recorded for the previous labs:

* Which files are being opened?
* Which DLLs are being loaded into the devenv process as you open a new solution in Visual Studio?
* Which ASP.NET requests are being handled by IIS? (You will need a recording of an IIS process with ASP.NET, or a live ASP.NET application running on the system. The events you care about belong to the AspNet provider.)
* Which .NET object types are being allocated often by WPA when you open a trace recording? (WPA is a managed application.)
* How many thread creation events occurred during a particular interval (on the live system or in an .etl file)?

> You can use `etrace --help` to review the general command-line options. Most of the events you will need are kernel events or CLR events, which you can list using `etrace --list CLR` and `etrace --list Kernel`.
