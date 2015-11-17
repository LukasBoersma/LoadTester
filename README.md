# LoadTester
LoadTester is a framework for load tests in .NET. It runs a given set of tests repeatedly and measure their performance.

# Usage

The API is simple. Create a `LoadTester` , call `AddTest()` and start execution with `Start()`.

Here is a simple example that measures the performance of .NET's `Array.CopyTo` method:

````csharp
// Create the load tester. Configure it to run lots of tests in parallel.
var tester = new LoadTester
{
    ParallelTests = 128
};

// Get a chunk of data
var data = MakeData();

// Test how fast we can copy that data
tester.AddTest(() => {
    var dataCopy = new byte[data.Length];
    data.CopyTo(dataCopy, 0);

    // This test can not fail, so always report success.
    return true;
});

// Run the test
tester.Start();

// Print status information every second
while(true)
{
    Thread.Sleep(1000);
    Console.WriteLine(String.Format(
        "Requests: {0} ({1:0.00}/s), Errors: {2} ({3:0.00}%), ",
        tester.TotalTestsExecuted,
        tester.TotalTestsExecuted / tester.TotalSeconds,
        tester.TotalErrors,
        100.0 * (double)tester.TotalErrors / tester.TotalTestsExecuted
    ));
}
````

## License and contributing

This software is distributed under the terms of the MIT license. You can use it for your own projects for free under the conditions specified in LICENSE.txt.

If you have questions, feel free to contact me. Visit [lukas-boersma.com](https://lukas-boersma.com) for my contact details.

If you think you found a bug, you can open an Issue on Github. If you make changes to this library, I would be happy about a pull request.

