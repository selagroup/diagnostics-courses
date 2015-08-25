### About This Repository

This repository contains supporting materials for Sela's diagnostics courses. The instructor will direct you to specific labs as the course progresses. Before you start working on the labs, make sure to review the [prerequisites](prerequisites.md) document. It contains information on required software installations as well as some configuration steps you should perform on your system.

These materials are distributed under the GPL 2.0 license (see [LICENSE](LICENSE)).

### Getting The Materials

You can use the **Download ZIP** button on the GitHub web interface to download a single .zip file with every file in this repository. Alternatively, you can clone the repository by using the GitHub for Windows desktop application, or by running the following command in a command prompt where you have access to the `git.exe` command-line tool:

```
git clone https://github.com/selagroup/diagnostics-courses.git
```

### List of Labs

1. [Analyzing I/O Activity with WPR and WPA](perf-wpr-fileaccesses/)
1. [Analyzing Unmanaged Heap Allocations with XPerf and WPA](perf-xperf-heapalloc/)
1. [Diagnosing High CPU Utilization with PerfView](perf-perfview-cpu/)
1. [Reducing .NET Allocations with PerfView](perf-perfview/netallocs/)
1. [Setting Up Continuous ETW Monitoring with PerfView](perf-perfview-monitoring/)
1. [Diagnosing a .NET Memory Leak with PerfView](perf-perfview-netleak/)
1. [Collecting IntelliTrace Logs with the Stand-Alone Collector](dbg-intellitrace-sc/)
1. [Generating Dump Files Automatically with WER](dbg-dumps-wer/)
1. [Basic Dump Analysis with Visual Studio](dbg-dumps-vs/)
1. [Dump File Memory Usage Analysis with Visual Studio](dbg-dumps-vsmemory/)
1. [Capturing Dump Files with DebugDiag](dbg-dumps-debugdiag/)
1. [Basic Dump Triage with WinDbg](dbg-dumps-windbg/)
1. [Exploring SOS Commands](dbg-sos-intro/)
1. [.NET Memory Leak Analysis with WinDbg and SOS](dbg-sos-leak/)
1. [Advanced Memory Leak Analysis with SOS](dbg-sos-advancedleak/)
1. [Analyzing a .NET Memory Leak with DebugDiag](dbg-sos-debugdiagleak/)
1. [Locks and Wait Analysis with WinDbg and SOS](dbg-sos-locks/)
1. [Automatic Dump Analysis with CLRMD](dbg-clrmd-triage/)
1. [Implementing a Stack Dumper Utility with CLRMD](dbg-clrmd-stackdumper/)
1. [Running Heap Queries on Live Processes with CLRMD](dbg-clrmd-heapqueries/)

Copyright (C) Sela Group, 2015. All rights reserved.