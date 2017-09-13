### Structure Deserialization with Unsafe Code

In this lab, you will implement a micro-benchmark for the different ways to read a structure from memory.

#### Task 1

Create a new console application in Visual Studio, and add the BenchmarkDotNet NuGet package. Follow similar steps to the ones described in the [micro-benchmarking lab](../perf-benchmarkdotnet/) and implement a benchmark that compares the following approaches (if you don't have time for all of them, implement in the order of appearance here). The structure you should be deserializing is up to you, here's one simple option:

```C#
public struct Packet
{
    public int   Field1;
    public short Field2;
    public short Field3;
    public long  Field4;
    public long  Field5;
}
```

1. `fixed` with `Marshal.PtrToStructure`
1. Non-generic method that supports only `Packet` structures, and reads them using `fixed`
1. `BinaryReader`-based method that uses reflection to understand the type's fields
1. Code generation-based approach that generates a new non-generic method for each type

#### Task 2

To understand how the non-generic method is so fast, inspect its machine code (assembly instructions). You can attach a debugger while it is running, or use the Disassembly Diagnoser. Are there still any inefficiencies that can be removed to further improve on this version?
