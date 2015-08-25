### Exploring SOS Commands

In this lab, you will become familiar with basic WinDbg and SOS commands. Unlike most of the labs you will see in this course, the application exhibited in this lab does not have any specific bugs, and was developed for demonstration purposes only.

#### Task 1

Run the DebuggingDemo.exe application from the [bin](bin/) folder.

Run WinDbg from the Debugging Tools for Windows 32-bit folder. (In a typical Windows SDK installation, this folder is C:\Program Files (x86)\Windows Kits\8.1\Debuggers\x86.)

Attach to the application's process using **File** > **Attach to Process**. Note that you can sort the list of processes alphabetically, by the executable name.

When you attach to the process, WinDbg automatically suspends its execution and presents a command prompt. Note that the application is no longer displaying any output. Type `g` into the command prompt or hit F5 to resume execution. Break into the debugger again using the **Debug** > **Break** menu item or the Ctrl + Break key combination.

#### Task 2

List the application's threads using the **View** > **Processes and Threads** menu item, and again by using the `~` command.

Switch between the application's threads using the **Processes and Threads** tool window (clicking a thread switches to that thread in the debugger), and then again usin the `~Ns` command (e.g. `~0s`, `~1s`).

Obtain the call stack of each of the application's threads using the **View** > **Call Stack** tool window, and then again using the `k` command. Managed functions will not be displayed when you do this, because WinDbg doesn't natively understand how to translate memory addresses to managed method names.

> Note: Even though this is a managed application, some of the threads are unmanaged. For example, there are likely to be a CLR debugger helper thread and a Win32 debugger helper thread in the process.

#### Task 3

Load the SOS.dll debugging extension into your debugging session using the following command:

```
.loadby sos clr
```

Switch to the main thread using the `~0s` command and issue the `!CLRStack` command to inspect the thread's managed call stack. Compare this output with the output of the `k` command. Specifically, note that the instruction pointers (IP) displayed by the `!CLRStack` command correspond to some of the return addresses displayed by the `k` command.

Issue the `~* e !clrstack` command to list all the application threads’ managed call stacks automatically. For some threads, this will display an error because the thread is not a managed thread (so it doesn’t have a managed call stack).

Issue the `~* e ".echo -------; !clrstack"` command to list all the application threads’ managed call stacks with some additional custom output.

Issue the `!Threads` command to view a list of all the managed threads in the application. Note the first three columns – the debugger thread id, the managed thread id, and the OS (Windows) thread id. Also note the **Exception** column, which tells you that two of the threads are thread pool threads. If there is a managed exception on one of the threads, you will see the managed exception details.

While still in thread 0, issue the `!CLRStack -a` command to display all parameters and local variables for every frame. Locate the args parameter of the Program.Main method. The output will be similar to the following:

```
...
0096f368 741f5f76 System.Console.ReadLine()
0096f370 00be0118 DebuggingDemo.Program.Main(System.String[])
    PARAMETERS:
        args (0x0096f380) = 0x028c260c
    LOCALS:
        0x0096f37c = 0x028c260c
0096f50c 748f2952 [GCFrame: 0096f50c]
```

The value of the `args` argument (which, of course, changes with each run) is the address of the string array on the managed heap. Pass this address to the `!do` command to verify that it is an array of strings: `!do 028c260c` (replace the address with the address from your own debugging session).

Now that you're convinced it’s an array of strings, pass the address to the `!dumparray` command to display the array contents. For example:

```
0:000> !dumparray 0x028c260c
Name:        System.String[]
MethodTable: 73beab98
EEClass:     738abb80
Size:        24(0x18) bytes
Array:       Rank 1, Number of elements 2, Type CLASS
Element Methodtable: 73c3acc0
[0] 028c25a0
[1] 028c25b8
```

Next, inspect the array in memory to make sure you can see the same thing as the `!dumparray` command displays. Pass the array address to the `dd 028c260c L5` command. The `Lnnn` parameter tells the `dd` command to display a certain number of elements. The first three memory locations in an array object are the type object pointer, the array length, and the type of array elements; these are followed by the two array elements.

Next, execute a debugger command that will display each of the two strings in the args array. Replace *address* in the following command with the address of the array obtained in the previous steps:

```
.foreach /pS 4 (s {dd /c 5 address L5}) {!do -nofields s}
```

In the preceding command, the `/c` switch tells the `dd` command to display five columns on every line, which means the output fits on one line. Then, the `/pS` switch tells the `.foreach` command to skip the first four tokens (which are the array address and its first three DWORDs). This means that the loop executes for each of the actual string references in the array.

#### Task 4

Load the SOSEX debugging extension using the following command:

```
.load %COURSEDIR%\tools\sosex_32\sosex
```

> Note: The `.load` command doesn't expand environment variables. You will need to replace %COURSEDIR% in the command above with the actual value of that environment variable.

Next, switch to the first thread pool thread in the `!Threads` output that you have seen previously, and issue the `!mk` command.

Locate the `PrintNumbersRepeatedly` frame and click the frame number (the first link on that line). This switches the current frame to that method. Next, issue the `!mdv` command to display the frame's local variables.

Pass the address of the `numbers` local variable to the `!do` command to see what kind of object it is. As you see, it's a `List<int>` object, and it has a field of type `System.Int32[]` called `_items`. The field’s offset is 4.

Display the value of the field using the following command, where you should replace *address* with the address of the `List<int>` object obtained in the previous step.

```
!dumparray poi(address+4)
```

> Note: The `poi` operator retrieves the pointer-sized value pointed to by its operand. It's similar to the `*` (dereference) operator in C.

Now that you know the address of the array, assign the address of the first element to a temporary register in the debugger, so we can experiment with evaluating some expressions. In this case, the address of the first element is at offset 8 from the beginning of the array object. Therefore, the following command should work, if you replace *address* with the address of the array object (not the `List<int>` object):

```
r? $t0 = (int*)(0xaddress+8)
```

Use the built-in expression evaluator to find the sum of the array's first three elements: issue the `?? @$t0[0]+@$t0[1]+@$t0[2]` command.

Use the built-in expression evaluator to convert the obtained value to hexadecimal. (Issue the `? 0n<number>` command, for example: `? 0n15` converts 15 to hexadecimal, namely to the value 0xf.)

Run the following loop in the debugger to display the array contents. The length of the array is stored just prior to the first element, so it will be available at `@$t0[-1]`.

```
.for (r $t1 = 0; @$t1 < @@c++(@$t0[-1]); r $t1 = @$t1+1) { ?? @$t0[@$t1] }
```

> Note: In the command above, the `@@c++` operator switches from the default MASM evaluation engine to the more sophisticated C++ evaluation engine. The C++ evaluation engine knows that `$t0` has the type `int*`, and therefore it can be accessed using the subscript operator (`[]`).

You might notice that some zero elements are printed -- the array is in fact of length 16 even though only 10 elements are being used. This is how the `List<T>` class manages its internal storage.

#### Task 5

Quit debugging (thus quitting the application as well) using the `q` command.

Run the application from within the debugger by using the **File** > **Open Executable** command. When launched, the application stops at the initial loader breakpoint. It will not start running until you hit F5 (or issue the `g` command). This may be useful for setting up breakpoints and otherwise modifying execution state before you start the application under the debugger. For example, run the following command to stop the debugger when the CLR is loaded:

```
sxe ld clr
```

Now, hit F5 (or `g`) to resume execution and wait for the breakpoint to hit. Next, you can load SOS (which requires that the CLR is already loaded) using the usual `.loadby sos clr` command. Finally, you can set up a breakpoint in the application's `Main` method:

```
!bpmd DebuggingDemo DebuggingDemo.Program.Main
```

Finally, hit F5 (or `g`) and wait for the breakpoint to be hit. At this point, you can quit the debugger and the debugged application.

> Note: The above process could be automated somewhat -- you could have the debugger automatically stop when the CLR is loaded and set up the breakpoint in the `Main` method using the following command:
>
> ```
> sxe ld clr ".loadby sos clr; !bpmd DebuggingDemo DebuggingDemo.Program.Main; gc"
> ```
