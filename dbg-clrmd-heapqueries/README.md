### Running Heap Queries on Live Processes with CLRMD

In this lab, you will build an application that attaches to a running process, executes a heap query to locate specific interesting objects, dumps out their details, and detaches without any impact on the debugged target. Although this is just a sample, it can be developed into a full-blown monitoring infrastructure that periodically locates and inspects interesting objects in memory without invasively suspending production processes.

#### Task 1

Create a new C# console application.

Download or clone the [dotnetsamples](https://github.com/Microsoft/dotnetsamples) repository, which contains the source code for [CLRMD](https://github.com/Microsoft/dotnetsamples/tree/master/Microsoft.Diagnostics.Runtime/CLRMD/ClrMemDiag) and [CLRMDExt](https://github.com/Microsoft/dotnetsamples/tree/master/Microsoft.Diagnostics.Runtime/CLRMD/ClrMemDiagExt).

> The CLRMDExt library contains some additional helper objects that are very useful for dynamic heap inspection, notably the `ClrObject` class.

Add the ClrMemDiag.csproj and ClrMemDiagExt.csproj projects to your solution. Make sure the solution builds successfully. If you see warnings about using obsolete methods, feel free to ignore them.

#### Task 2

Write code to attach to a process with a specific process id using passive mode:

```C#
DataTarget target = DataTarget.AttachToProcess(pid, 1000, AttachFlag.Passive);
```

You can get the process id as a command-line argument, or use a hard-coded value for now. In any case, make sure to respect the bitness: 32-bit analysis programs can attach to 32-bit targets only.

Repeat the steps from the [CLRMD Triage](../dbg-clrmd-triage/) lab to create a `ClrRuntime` object, and use its `GetHeap()` method to obtain a `ClrHeap` instance.

#### Task 3

Use the `ClrObject` class to perform the following dynamic queries:

* Display all the strings whose length is greater than 200
* Display all the *open* `FileStream` objects and their underlying file name
* Display all the `Exception`-derived objects and their inner exception, if one is present
* (For an ASP.NET application) Display the URL and status code of all `System.Web.HttpContext` objects

Here is a skeleton that might help with your own implementation:

```C#
var query = (from o in heap.EnumerateObjects()
             let t = heap.GetObjectType(o)
             where t.Name == "System.String"
             select new ClrObject(heap, t, o, false)
            );
foreach (dynamic obj in query)
{
	Console.WriteLine(obj.m_stringLength);
}
```
