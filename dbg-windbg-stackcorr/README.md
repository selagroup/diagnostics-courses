### Analyzing Stack Corruptions

In this lab, you will learn the fundamentals of stack reconstruction techniques that can be used when the stack is corrupted and standard debugger commands do not produce useful output.

#### Task 1

Open the Dump.dmp crash dump from the [bin](bin/) folder in WinDbg and attempt to diagnose the cause of the crash. Make sure to inspect all the threads. Also look at the register values using the `r` command – specifically EBP, EIP, ESP.

Read about the `dps` command in the documentation. It's designed to display a range of memory addresses with additional symbolic information if it can resolve a symbol for the address in question. Make sure to add the [bin](bin/) folder to the WinDbg symbol path (use **File** > **Symbol File Path**).

Pass the value of the ESP register to the `dps` command. (You can do this directly: `dps esp`.)

> Note: The `dps` command accepts a range of memory addresses, so if it doesn't display enough data, use a larger range, e.g.: `dps esp L200`

To perform a manual stack walk, you need to locate a valid EBP value (note that the EBP register in the dump is corrupted with 0, and so is EIP). EBP values are typically stored immediately before the return address. Locate a return address on the stack and try to use the memory location preceding it as an EBP anchor. For example:

```
0:000> dps 002af1a8 
002af1a8  00000000
002af1ac  002af120
002af1b0  00000000
002af1b4  014cfe90
002af1b8  002af0fc
002af1bc  742fd594 uxtheme!StreamInit+0x36
002af1c0  002af180
002af1c4  01850815
002af1c8  0000029e
```

Suppose you think that `uxtheme!StreamInit+0x36` is a return address on the stack. Then the value 002af0fc might be a saved EBP. Use that value to trace back through the stack to the previous EBP, and hence previous return address. Remember, however, that not everything that looks like a symbol on the stack is actually a return address.

To streamline the analysis, you can use the kb = ... command. You need to give it a guessed EBP, ESP, EIP triple, for example:

```
kb = 002af0fc 002af0fc 742fd594
```

You already know how to look for a saved EBP. The ESP associated with that EBP is hard to tell, but you can pass the same value (during the function prologue, ESP and EBP have the same value). The EIP associated with that EBP is the return address immediately following the saved EBP.

You might also want to edit the EBP, ESP, and EIP values directly. This will make it possible for you to issue other debugger commands, such as switch to frame (`.frame`) and will help locating the problematic source code. To do so, after you’re convinced that the `kb = ...` output is satisfactory, issue the following command with the new EBP, ESP, and EIP values:

```
r ebp = 002af0fc; r esp = 002af0fc; r eip = 742fd594
```

Finally, in some situations the debugger will not display a full call stack even if you provide good values for EBP, ESP, and EIP. In that case, you can attempt to automate the process of walking the EBP chain yourself. The following command will walk the EBP chain from an initial guess of EBP = 002af0fc:

```
.for (r $t0=002af0fc; poi(@$t0)!=0; r $t0=poi(@$t0)) { dps @$t0+4 L1 }
```

What is the likely culprit? After detecting the function name, look at the [source code](src/).
