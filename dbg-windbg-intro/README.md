### Exploring WinDbg Commands

In this lab, you will become familiar with basic WinDbg commands and functionality. Unlike most of the labs you will see in this course, the application exhibited in this lab does not have any specific bugs, and was developed for demonstration purposes only.

#### Task 1

Run the DebuggingDemo.exe application from the [bin](bin/) folder.

Run WinDbg from the Debugging Tools for Windows 32-bit folder. (In a typical Windows SDK installation, this folder is C:\Program Files (x86)\Windows Kits\8.1\Debuggers\x86. Usually, you can also find WinDbg on the Start menu.)

Attach to the application’s process using **File** > **Attach to Process**. Note that you can sort the list of processes alphabetically, by the executable name.

When you attach to the process, WinDbg automatically suspends its execution and presents a command prompt. The application is no longer displaying any output. Type `g` into the command prompt or hit F5 to resume execution. Break into the debugger again using the **Debug** > **Break** menu item or the Ctrl + Break key combination.

List the application's threads using the **View** > **Processes and Threads** menu item, and again by using the `~` command.

Switch between the application's threads using the `Processes and Threads` tool window (clicking a thread switches to that thread in the debugger), and then again usin the `~Ns` command (e.g. `~0s`, `~1s`).

Obtain the call stack of each of the application's threads using the **View** > **Call Stack** tool window, and then again using the `kb`, `kv`, `kP`, and `kn` commands. Make sure you understand the output in the command window. Consult the documentation (**Help** > **Contents**) if necessary.

Switch between frames on the stack using the **Call Stack** tool window and using the `.frame N` command (e.g. `.frame 7`). If source code is not displayed, provide the path to the [src](src/) folder to the **File** > **Source File Path** dialog box.

Issue the `~*k` command to list all the application threads' call stacks automatically.

Issue the `~* e .echo -------; kn` command to list all the application threads' call stacks with some custom output.

Display the local variables for each of the application's frames that have private symbols. Do this first by using the **View** > **Locals** menu item, and then using the `dv /v /t` command.

Automatically display the local variables for each of the frames in each of the application's threads using the `~* e !for_each_frame dv /v /t` command.

#### Task 2

The `PrintArrayRepeatedly` method that is executed by one of the application's threads has a local variable of type `int*`. Locate that method and switch to its frame. You can do this manually by checking all the thread stacks for the presence of this method, or by using the `!findstack DebuggingDemo!PrintArrayRepeatedly` command.

Display that local variable using the `dv arr` command, and then display all the array elements using the `dd address Llength` command. For example, if the array is of length 10 and is located at address 0x10203040, use the `dd 0x10203040 L0n10` command.

Use the built-in expression evaluator to find the sum of the array's first three elements. (Issue the `?? arr[0]+arr[1]+arr[2]` command.)

Use the built-in expression evaluator to convert the obtained value to hexadecimal. (Issue the `? 0n<number>` command, for example: `? 0n15` converts 15 to hexadecimal, namely to the value 0xf.)

The `PrintVectorRepeatedly` method that is executed by one of the application's threads has a local variable of type `std::vector<int>`. Locate that method and switch to its frame. (Again, it is advised to use the `!findstack` command for this.)

Run the following loop command in WinDbg to display the vector's contents. This relies on the fact that the pointer to the vector’s first element is in the `_Myfirst` field, and the size of the vector can be found by subtracting `_Myfirst` from `_Mylast`. (Of course, you will have to replace `v` in the following command with the name of the actual vector variable.)

```
.for (r $t0=0; @$t0 < @@c++(v._Mylast - v._Myfirst); r $t0=@$t0+1) { ?? v._Myfirst[@$t0] }
```

> A brief explanation of the above command: the `.for` statement runs a loop. The `r $t0 = 0` command initializes a temporary variable called `$t0` to the value 0. The `r $t0 = @$t0 + 1` command increments `$t0` by 1 with every loop iteration. The loop body displays the `$t0`'s element in the array pointed to by `v._Myfirst`. Finally, the loop's stop condition `$t0 < @@c++(v._Mylast - v._Myfirst)` means that we should keep looping as long as `$t0` is smaller than the number of elements in the vector, calculated as the difference between `v._Mylast` and `v._Myfirst`.

Run the traverse_vector.script file from the [tools](../tools/) folder to display the vector's contents, passing to it the name of the vector variable. For example:

```
$$>a< %COURSEDIR\tools\traverse_vector.script v
```

> Note that WinDbg doesn't expand environment variables, so you will have to replace %COURSEDIR% with the actual directory where you extracted the course materials.

#### Task 3

Detach from the application using the `qd` command. Note that it continues running (printing numbers to the console). Attach to the application again using the **File** > **Attach to Process** menu item (or the F6 keyboard shortcut). Quit debugging (thus quitting the application as well) using the `q` command.

Run the application from within the debugger by using the **File** > **Open Executable** command. When launched, the application stops at the initial *loader breakpoint*. It will not start running until you hit F5 (or issue the `g` command). This may be useful for setting up breakpoints and otherwise modifying execution state before you start the application under the debugger. Another common scenario is when you need to set up a breakpoint to see why a particular DLL is being loaded. The best opportunity to do so is during the initial breakpoint. Try the following command to set up a breakpoint when kernel32.dll is being loaded, and then hit F5 to continue the application and wait for the breakpoint to be hit:

```
sxe ld kernel32
```

Quit the debugger and the debugged application.

#### Task 4

Open the HeuristicsWhenOptimized.dmp file from the [bin](bin/) folder. This is a dump of a 64-bit application, so use the 64-bit version of WinDbg.

Switch to thread 1 using the `~1s` command and inspect the stack (`kb`). The thread is stuck in `WaitForSingleObjectEx`, but the `kb` command is not able to display the function’s parameters because the code is optimized and because in the x64 calling convention, parameters are (usually) not passed on the stack.

Load the CMKD extension from the [tools](tools/) folder using the `.load %COURSEDIR%\tools\cmkd_x64.dll` command. As always, replace %COURSEDIR% with the actual value on your system.

Issue the `!stack -p -t` command to inspect the parameter values. Try to understand the tracking information which explains how CMKD was able to deduce the parameter values. For example:

```
02 00000084ef12f8d0 00007ff6992c109b KERNEL32!WaitForMultipleObjects+f 
	Parameter[0] = 0000000000000001 : rcx setup in parent frame by lea instruction @ 00007ff6992c1086 from mem @ 0000000000000001 
	Parameter[1] = 00000084ef12f940 : rdx setup in parent frame by lea instruction @ 00007ff6992c1081 from mem @ 00000084ef12f940 
	Parameter[2] = 0000000000000001 : r8  setup in parent frame by movb instruction @ 00007ff6992c1076 from immediate data 
	Parameter[3] = 000000000000ea60 : r9  setup in parent frame by movb instruction @ 00007ff6992c108a from immediate data
```

#### Task 5

Switch to thread 0 using the `~0s` command and issue the `k` command to inspect the call stack. You are interested in the values passed to the `WaitForMultipleObjects` function, but a command like `kb` would not display the right information.

Recall that under the x64 calling convention, the four first integer (or pointer) parameters are passed in the RCX, RDX, R8, and R9 registers. Switch to the `WaitForMultipleObjects` frame (on most systems, `.frame 2` would do it) and display the register values using the `r` command. Note that the relevant registers appear to have been overwritten by subsequent function calls.

Disassemble the code around the return address for `WaitForMultipleObjects`. Typically, the `uf HeuristicsWhenOptimized!wmain` command would be easiest approach. Look for the vicinity of the call to `_imp_WaitForMultipleObjects`. You should see the following assembly instructions that initialize the four parameters passed to `WaitForMultipleObjects`:

```
mov  r8d,1
mov  qword ptr [rsp+30h],rcx
lea  rdx,[rsp+30h]
lea  ecx,[r8+1]
mov  r9d,0EA60h
mov  qword ptr [rsp+38h],rax
call qword ptr [...!_imp_WaitForMultipleObjects (...)]
```

The R8, R9, and RCX values are readily available. Namely, R8 is initialized to 1 using the `mov r8d, 1` instruction; R9 is initialized to 2 using the `lea ecx, [r8+1]` instruction; and R9 is initialized to 0xEA60 using the `mov r9, 0xea60` instruction.

The RDX register is initialized with the stack address RSP+30. Issue the `k` command again and note the **Child-SP** column value for the `wmain` frame. Issue the command `dq <child-SP-from-previous-step>+30 L2` command to identify the two handles passed to the `WaitForMultipleObjects` function.

> Note: The steps illustrated in this exercise are specific for this situation. There is no general recipe that can help in obtaining the values of parameters or local variables that have been stored in registers and subsequently overwritten by other function calls. The CMKD extension is powerful but not omnipotent -- there is nothing that can trump careful manual analysis.
