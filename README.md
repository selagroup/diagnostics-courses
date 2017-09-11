### About This Repository

This repository contains supporting materials for Sela's diagnostics courses. The instructor will direct you to specific labs as the course progresses. Before you start working on the labs, make sure to review the [prerequisites](prerequisites.md) document. It contains information on required software installations as well as some configuration steps you should perform on your system.

These materials are distributed under the GPL 2.0 license (see [LICENSE](LICENSE)).

### Getting The Materials

You can use the **Download ZIP** button on the GitHub web interface to download a single .zip file with every file in this repository. Alternatively, you can clone the repository by using the GitHub for Windows desktop application, or by running the following command in a command prompt where you have access to the `git.exe` command-line tool:

```
git clone https://github.com/selagroup/diagnostics-courses.git
```

### List of Labs

> Note: For each lab below, [C++] indicates it's a lab that would mostly interest C++ developers; [C#] indicates that it's a lab that would mostly interest C# developers; and [kernel] indicates that it's a lab that would mostly interest driver developers. Labs not marked with either tag can be interesting for both audiences. In any case, if you are building a managed application that has some unmanaged components, you should consider working through *all* the labs.

1. [Analyzing I/O Activity with WPR and WPA](perf-wpr-fileaccesses/)
1. [Analyzing Unmanaged Heap Allocations with XPerf and WPA](perf-xperf-heapalloc/) [C++]
1. [Diagnosing High CPU Utilization with PerfView](perf-perfview-cpu/)
1. [Profiling CPU Work by Sampling with XPerf](perf-xperf-cpu/)
1. [Profiling CPU Work by Sampling with VSPerfCmd](perf-vsperf-cpu/)
1. [Analyzing Startup Performance with PerfView](perf-perfview-startup/)
1. [Reducing .NET Allocations with PerfView](perf-perfview-netallocs/) [C#]
1. [Setting Up Continuous ETW Monitoring with PerfView](perf-perfview-monitoring/)
1. [Diagnosing a .NET Memory Leak with PerfView](perf-perfview-netleak/) [C#]
1. [Inspecting GC Segments and Fragmentation with VMMap](perf-gc-segments) [C#]
1. [Collecting IntelliTrace Logs with the Stand-Alone Collector](dbg-intellitrace-sc/) [C#]
1. [Generating Dump Files Automatically with WER](dbg-dumps-wer/)
1. [Basic Dump Analysis with Visual Studio](dbg-dumps-vs/)
1. [Dump File Memory Usage Analysis with Visual Studio](dbg-dumps-vsmemory/) [C#]
1. [Capturing Dump Files with DebugDiag](dbg-dumps-debugdiag/)
1. [Basic Managed Dump Triage with WinDbg](dbg-dumps-windbg/) [C#]
1. [Basic Unmanaged Dump Triage with Windbg](dbg-dumps-windbg2/) [C++]
1. [Exploring WinDbg Commands](dbg-windbg-intro/) [C++]
1. [Exploring SOS Commands](dbg-sos-intro/) [C#]
1. [.NET Memory Leak Analysis with WinDbg and SOS](dbg-sos-leak/) [C#]
1. [Advanced Memory Leak Analysis with SOS](dbg-sos-advancedleak/) [C#]
1. [Analyzing a .NET Memory Leak with DebugDiag](dbg-sos-debugdiagleak/) [C#]
1. [Locks and Wait Analysis with WinDbg and SOS](dbg-sos-locks/) [C#]
1. [Locks and Waits Analysis with WinDbg](dbg-windbg-locks/) [C++]
1. [Catching an Unmanaged Heap Corruption](dbg-windbg-heapcorr/) [C++]
1. [Catching a VTable Corruption](dbg-windbg-vtablecorr/) [C++]
1. [Analyzing Stack Corruptions](dbg-windbg-stackcorr/) [C++]
1. [Analyzing an Unmanaged Memory Leak with WinDbg and VMMap](dbg-windbg-leak/) [C++]
1. [Diagnosing Handle Corruptions and Leaks](dbg-windbg-handles/)
1. [Automatic Dump Analysis with CLRMD](dbg-clrmd-triage/)
1. [Implementing a Stack Dumper Utility with CLRMD](dbg-clrmd-stackdumper/) [C#]
1. [Running Heap Queries on Live Processes with CLRMD](dbg-clrmd-heapqueries/) [C#]
1. [Analyzing UI Delays with the Visual Studio Concurrency Visualizer](perf-concvis-ui/)
1. [Command-Line Performance Analysis with `etrace`](perf-etrace-intro/)
1. [Programmatic ETW Recording with TraceEvent](perf-traceevent/) [C#]
1. [Micro-Benchmarking with BenchmarkDotNet](perf-benchmarkdotnet/) [C#]
1. [Viewing Top-Level GC Behavior with Performance Counters](perf-perfcounters-gc/) [C#]

Copyright (C) Sela Group, 2007-2016. All rights reserved.
