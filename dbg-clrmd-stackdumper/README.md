### Implementing a Stack Dumper Utility with CLRMD

In this lab, you will implement a simple utility that connects to a running process and prints out the stacks of all its threads. This can be very useful for production monitoring, checking on the state of a specific component, or even logging system activity over time.

#### Task 1

Open the [StackDumper.sln](src/StackDumper.sln) solution in Visual Studio. It already has a reference to the ClrMD library, and some code to parse the first command-line argument and determine if it is a valid process id. All you need to do is implement the `DumpStacks` method, which should use the ClrMD API to attach to the process, retrieve all CLR threads, and print out their stacks.

These are some of the API methods you'll need:

* `DataTarget.AttachToProcess()`
* `DataTarget.ClrVersions[0].TryDownloadDac()` provides the path to mscordacwks.dll
* `DataTarget.CreateRuntime()` takes the path to mscordacwks.dll
* `ClrRuntime.Threads` property
* `ClrThread.StackTrace` property

#### Task 2

When passed a method name as the second command-line argument, StackDumper will print out only threads that have that method on their call stack. This makes it easy to identify waiting threads, WCF threads, ASP.NET threads, and so on.

#### Task 3 (Optional)

Grab all thread stacks and aggregate them into a tree, such that common stack sections are shared across multiple threads.

For example, if you have three threads with the following stacks:

```
Thread 1
Foo
Bar
Main

Thread 2
Luck
Buck
Baz
Bar
ThreadStart

Thread 3
Muck
Duck
Baz
Bar
ThreadStart

Thread 4
Duck
Baz
Bar
ThreadStart
```

You can visualize these four call stacks as a tree, which can make them easier to comprehend at a glance:

```
+ Main (1 thread)
  | Bar
  | Foo
+ ThreadStart (2 threads)
  | Bar
  | Baz
    + Buck (1 thread)
    + Duck (2 threads)
      + Luck (1 thread)
      + Muck (1 thread)
```
