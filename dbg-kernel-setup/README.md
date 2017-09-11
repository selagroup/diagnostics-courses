### Setting Up Kernel Debugging

In this lab, you will experiment with setting up local kernel debugging for inspection and analysis of the current system, as well as setting up kernel debugging of a guest virtual machine on your host system.

#### Task 1

To perform local kernel debugging, you can use WinDbg's built-in support (**File** > **Kernel Debug** > **Local**), but it usually requires administrator privileges, and will require booting the system in **/DEBUG** mode, which may have adverse effect on stability.

Therefore, we recommend that you use the Sysinternals LiveKD utility. Launch it as follows:

```
LiveKD -w
```

When the WinDbg instance opens, you are working against a snapshot of kernel space taken at the time when you launched LiveKD.

> Note: LiveKD needs the Dbghelp.dll version that ships with Debugging Tools for Windows, so you might find it easier to copy LiveKD.exe to the Debugging Tools for Windows folder and launch it from there.

#### Task 2

Use the `!process 0 0` command to locate a process, and then `!process <address>` to view more details about the process.

Choose one of the threads in the process, and pass its address to the `dt` command, as follows:

```
dt nt!_KTHREAD <address>
```

The information will be different between OS versions and between 32-bit and 64-bit systems. In fact, you might want to experiment with different OS versions.

Next, look for a field that tells you whether the thread is a GUI thread:

* On some OS versions, there is a single-bit field called `GuiThread`.
* On some OS versions, there is a field called `ServiceTable` which points to the thread’s system service table. If this field points to `KeServiceDescriptorTable`, this is not a GUI thread. If this field points to `KeServiceDescriptorTableShadow`, this is a GUI thread.

Repeat this experiment until you have found at least one GUI thread and at least one non-GUI thread.

#### Task 3

Walk the system’s process list yourself (instead of using `!process 0 0`). Start with the `nt!KiProcessListHead` variable, which is a pointer to an `nt!_LIST_ENTRY` structure and traverse the link nodes. Use the `dt nt!_KPROCESS` command to see the structure of a process control block, and use the `dt nt!_EPROCESS` command to see the structure of the executive process block.

In the `_KPROCESS` structure, locate the `ProcessListEntry` field (on Windows 7 x64, it is located at offset 0xE0). This field contains the forward and backward links in the process list. Start with the value to which `nt!KiProcessListHead`’s `Flink` field points, and subtract the offset of the `ProcessListEntry` field from it. For example:

```
dt nt!_EPROCESS 0xfffffa80`0719bc10-0xe0
dt nt!_KPROCESS 0xfffffa80`0719bc10-0xe0
```

Continue in a similar fashion to enumerate several processes by using the `ProcessListEntry.Flink` field in each `_KPROCESS` you inspect. Use the `UniqueProcessId` field of the `_EPROCESS` structure to make sure that you are looking at the correct addresses.
