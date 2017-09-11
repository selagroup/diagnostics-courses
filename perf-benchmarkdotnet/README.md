### Micro-Benchmarking with BenchmarkDotNet

In this lab, you will use the [BenchmarkDotNet](http://benchmarkdotnet.org/) .NET benchmarking library to develop and run simple micro-benchmarks. Micro-benchmarking is a key part of low-level performance optimization, and can help quickly estimate the costs of various alternatives for a certain algorithm or task.

#### Task 1

Create a new console application in Visual Studio, and install the BenchmarkDotNet NuGet package. Take a quick look at the [Getting Started](http://benchmarkdotnet.org/Guides/GettingStarted.htm) guide to see what a simple benchmark looks like, and how to run it from your `Main` method.

> Note: BenchmarkDotNet is a cross-platform project that supports .NET Core and runs on Linux, macOS, and Windows. In this lab, we will be using it on Desktop .NET Framework, but you can definitely experiment with other runtimes.

#### Task 2

In this benchmark, you will compare three .NET collections in terms of iterating through a large number of elements (which are already in the collection). Specifically, you will compare `List<int>`, `LinkedList<int>`, and `int[]` when it comes to traversing and adding all the collection's elements together.

Begin by writing a public class that will contain your benchmark methods. Add a method decorated with `[GlobalSetup]` that will perform the collection initialization: insert 1,000,000 integers into a `List<int>`, `LinkedList<int>`, and `int[]`, which should be stored as members on your benchmark class.

Next, implement three benchmark methods. They should look like the following:

```C#
[Benchmark]
public int SumList()
{
    int sum = 0;
    for (int i = 0; i < _list.Count; ++i)
        sum += _list[i];
    return sum;
}
```

Finally, in your `Main` method, add code to run the benchmarks and report the results. Answer the following questions:

* Which collection was fastest?
* Which was slowest?
* Was there a considerable difference in standard deviation, which might point to noisiness?
* What are the reasons that explain the performance differences between the collections?

#### Bonus

If you have time, pick one of the following additional tasks (or some other task you think of!) and develop a benchmark for it:

* You need to parse very simple, flat JSON documents that contain only a single list of strings. Compare the performance of JSON.NET to parse the JSON document, as opposed to custom C# code that you'll write to do the parsing for the specific case.

* You need to read a large text file and look for anything resembling a phone number (e.g. +18005551212). Compare the performance of using a regular expression to do the parsing, as opposed to custom C# code that you'll write for the specific case.

* You need to read a large XML document that contains root/order/address/@city attribute values you're interested in. Compare the performance of using `XmlDocument` to using `XmlReader` to do the parsing.
