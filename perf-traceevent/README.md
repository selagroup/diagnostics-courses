### Programmatic ETW Recording with TraceEvent

In this lab, you will experiment with the [TraceEvent](https://github.com/Microsoft/perfview/tree/master/src/TraceEvent) library, which helps write managed code that controls ETW sessions, receives events in real-time, and parses ETL recordings. TraceEvent is the library that makes PerfView possible.

#### Task 1

Create a new console application in Visual Studio, and add the Microsoft.Diagnostics.Tracing.TraceEvent NuGet package and Microsoft.Diagnostics.Tracing.TraceEvent.Samples NuGet package. The first one is the library itself, and the second one installs a number of samples into the current project.

Explore the samples. Specifically, try to figure out how to:

* Create a new ETW session to record events to a file.
* Create a new ETW session and parse the incoming events in real-time.
* Open an existing ETL recording and parse the events contained in it.

#### Task 2

Create a new console application in Visual Studio, and add only the Microsoft.Diagnostics.Tracing.TraceEvent NuGet package (don't add the samples package this time). Build a simple console-based tool that monitors the CLR `GCAllocationTick` events, and produces a summary every 5 seconds with the top allocated types in all the .NET processes on the system. The output can look like the following:

```
> SampleAllocs 5 3
Sampling GCAllocationTick events across the system, Ctrl+C to quit.
Summary every 5 seconds, printing top 3 types.

[10:05:12]
Process                Type                       Allocated Bytes
-----------------------------------------------------------------
devenv.exe             System.Byte[]                     19877110
devenv.exe             System.String                      8421026
devenv.exe             System.Int32[]                      110800
w3wp.exe               System.Byte[]                    506112870
w3wp.exe               System.Xml.XmlDocument              876200
w3wp.exe               System.Object                       964388

[10:05:17]
Process                Type                       Allocated Bytes
-----------------------------------------------------------------
devenv.exe             System.Byte[]                     19877110
devenv.exe             System.String                      8421026
devenv.exe             System.Int32[]                      110800
w3wp.exe               System.Byte[]                    506112870
w3wp.exe               System.Xml.XmlDocument              876200
w3wp.exe               System.Object                       964388

^C
Quitting.
```

As a bonus, it would be a nice touch to allow filtering specific processes, specific types, or displaying the allocation size in megabytes. If you find the end result useful, you might consider polishing it into a complete tool and publishing it on GitHub!
