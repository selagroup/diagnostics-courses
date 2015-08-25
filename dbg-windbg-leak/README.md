### Analyzing an Unmanaged Memory Leak with WinDbg and VMMap

In this lab, you will use WinDbg's `!heap` extension to analyze an unmanaged memory leak. You will also use Sysinternals VMMap's tracing mode to reach similar conclusions, although in a more invasive fashion.

#### Task 1

Run the BatteryMeter.exe application from the [bin\Release](bin/Release/) folder. Verify that the application is leaking memory using Windows Task Manager or any other tool of your choice.

Attach WinDbg (32-bit) and issue the `!heap -s -h 0` command to see all heaps and their sizes.

Let the application run and leak some more. (Remember to press F5 in WinDbg so that the application can continue running.) Use `!heap -s -h 0` to see all heaps again, and see which heap grows.

Close the application. Open an elevated command prompt and navigate to the Debugging Tools for Windows (x86) directory. (This is typically C:\Program Files (x86)\Windows Kits\8.1\Debuggers\x86.)

Issue the following command to enable user-mode stack collection for the BatteryMeter.exe process:

```
gflags -i BatteryMeter.exe +ust
```

Run the application again. Issue the following command to capture a first snapshot of the application's heap allocations:

```
umdh -pn:BatteryMeter.exe -f:C:\temp\dump1.txt
```

Let the application run and leak some more. Issue the following command to capture another snapshot of the application's heap allocations:

```
umdh -pn:BatteryMeter.exe -f:C:\temp\dump2.txt
```

Finally, to obtain a differencing report, issue the following command:

```
umdh C:\temp\dump1.txt C:\temp\dump2.txt -f:C:\temp\report.txt
```

Disable user-mode stack collection using the following command:

```
gflags -i BatteryMeter.exe -ust
```

> Note: User-mode stack collection is an expensive feature. As long as it is turned on, the Windows heap manager will capture an allocation call stack for each heap allocation in the monitored process. That's why it is important to turn this feature off as soon as you don't need it, especially if you are using it in a production environment.

Inspect the report and detect the leak source (the call stacks that are allocating large amounts of memory between the first and the second snapshot are likely responsible for the leak).

Run the application from the [bin\Debug](bin/Debug/) folder and repeat the previous steps -- now you should have better insight as to the source of the leak (inspect the source code to make sure).

#### Task 2

Use Sysinternals VMMap to diagnose the same memory leak. In the VMMap initial window, launch the application from the **Launch and trace...** tab.

Allow the leak to accumulate, and then hit F5 (**View** > **Refresh**). In the middle summary window, click the **Heap** category.

In the bottom details window, click a heap segment that seems large or growing. Click the **Heap Allocations** button at the bottom of the screen to view all allocations in that heap area. For each allocation you can click the **Stack** button to see the allocating call stack.

> Note: You will need to configure symbols for that to work, in the **Options** > **Configure Symbols** menu. Make sure to configure both the symbol path and the path to the dbghelp.dll file that resides in the Debugging Tools for Windows installation directory (and not the one in C:\Windows\System32, which is part of Windows).

Feel free to experiment with the other buttons at the bottom of the screen.
