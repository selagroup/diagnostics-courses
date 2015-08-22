### Dump File Memory Usage Analysis with Visual Studio

There's no better way to illustrate a new Visual Studio diagnostic feature than by applying it to Visual Studio itself. In this lab, you'll see how to use Visual Studio's memory analysis tool to inspect a dump file and display objects and references. This gives you a general idea as to what's going on with your managed memory, and enables rudimentary memory leak analysis.

#### Task 1

Open a command prompt and navigate to the folder where you put the Sysinternals Suite of tools. Specifically, you will need Procdump.exe to capture dump files.

Run a new instance of Visual Studio. Then, capture a full memory dump using the following command in the command prompt window:

```
Procdump -ma devenv C:\temp\devenv.dmp
```

> If you have multiple instances of Visual Studio running, use Task Manager to find the process id of the one you just created, and then replace "devenv" with the process id in the command line above.

#### Task 2

Drag and drop the generated dump file (C:\temp\devenv.dmp) into Visual Studio. Click the **Debug Managed Memory** link on the right-hand side. Wait for symbols to load -- this can be a fairly time-consuming process.

> Note: **Debug Managed Memory** is a Visual Studio 2013 Ultimate and Visual Studio 2015 Enterprise feature. It is not available in the Community and Professional editions.

The table displayed in the **Heap View** tab lists the types on the managed heap, the number of instances of each type, and the amount of memory consumed by it. What are the biggest types? Click one or two of the biggest type to see (on the bottom of the screen) which reference paths lead to these objects.

Find the `Microsoft.VisualStudio.Text.Editor.Implementation.WpfTextView` type and click the button that shows the type's instances. Note that you can inspect the object's contents by hovering other the **Value** field and waiting for the data tip to appear.
