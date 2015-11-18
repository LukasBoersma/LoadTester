# LoadTester

LoadTester is a framework for load tests in .NET. It runs a given set of tests repeatedly and measures their performance.

## License and contributing

This software is distributed under the terms of the MIT license. You can use it for your own projects for free under the conditions specified in LICENSE.txt.

If you have questions, feel free to contact me. Visit [lukas-boersma.com](https://lukas-boersma.com) for my contact details.

If you think you found a bug, you can open an Issue on Github. If you make changes to this library, I would be happy about a pull request.

## Usage

The basic procedure is:

1. Create a `TestRunner`
2. Call `AddTest()` to define the tests
3. Start execution with `Start()`.

Here is a simple C# example that measures the performance of .NET's `Array.CopyTo` method:

````csharp
// Create the test runner. Configure it to run lots of tests in parallel.
var tester = new TestRunner
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

# API Reference

LoadTester consists of a single class, `TestRunner`:

<a id="LoadTester.TestRunner"></a>
## class LoadTester.TestRunner

Runs load tests.

This is a test runner that repeatedly runs a set of tests.
It can run many tests in parallel ( [ParallelTests](#LoadTester.TestRunner.ParallelTests) ), and it can be configured to repeat tests at a limited rate ( [TargetTestsPerSecond](#LoadTester.TestRunner.TargetTestsPerSecond) ).
Use [AddTest](#LoadTester.TestRunner.AddTest(System.Func{System.Boolean})) to add test functions, then run [Start](#LoadTester.TestRunner.Start) to start running the tests.

**Methods**

<a id="LoadTester.TestRunner.AddTest(System.Func`1[[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]])"></a>

* *void* **AddTest** *(Func&lt;bool&gt; test)*  

<a id="LoadTester.TestRunner.Start"></a>

* *void* **Start** *()*  
  Starts running tests. Starts new threads and returns immedeately.  
  After calling this method, there will be [ParallelTests](#LoadTester.TestRunner.ParallelTests) many worker
threads that repeatedly run random tests from the list of [Tests](#LoadTester.TestRunner.Tests) .
Call [Stop](#LoadTester.TestRunner.Stop(System.Int32)) if you want to stop running tests.
Will throw an InvalidOperationException if tests are already running.

<a id="LoadTester.TestRunner.Stop(System.Int32)"></a>

* *void* **Stop** *([int GracefulTimeout])*  
  Stops the test execution.  


**Properties and Fields**

<a id="LoadTester.TestRunner.Tests"></a>

* *List&lt;Func&lt;bool&gt;&gt;* **Tests**  
  The list of tests that will be run. Tests will be drawn randomly from this list.  
  Each test is a function that takes no parameters and returns true iff the test succeeded.


<a id="LoadTester.TestRunner.StartTime"></a>

* *DateTime* **StartTime**  
  The timestamp when [Start](#LoadTester.TestRunner.Start) was called, or DateTime.MinValue when no tests are running.  


<a id="LoadTester.TestRunner.Running"></a>

* *bool* **Running**  
  True iff tests are currently running.  


<a id="LoadTester.TestRunner.TargetTestsPerSecond"></a>

* *double* **TargetTestsPerSecond**  
  The desired maximum rate of tests per second.  
  This is the global value of tests per second, not per worker thread.
The worker threads will try to add sleep operations if tests are executed faster than this.
If this value is negative, the rate of tests per second is unlimited.
This value can safely be changed while tests are running.
Each worker thread will adapt the change at its next test execution.


<a id="LoadTester.TestRunner.ParallelTests"></a>

* *int* **ParallelTests**  
  The number of worker threads that will run the tests.  
  When calling [Start](#LoadTester.TestRunner.Start) , this number of worker threads will be created.
This value can safely be changed during the execution of tests, but has no effect
until the test execution is restarted (by calling [Stop](#LoadTester.TestRunner.Stop(System.Int32)) and [Start](#LoadTester.TestRunner.Start) ).


<a id="LoadTester.TestRunner.TotalTestsExecuted"></a>

* *long* **TotalTestsExecuted**  
  The total number of tests that have been executed since the last call of [Start](#LoadTester.TestRunner.Start) .  


<a id="LoadTester.TestRunner.TotalErrors"></a>

* *long* **TotalErrors**  
  The total number of tests that have failed since the last call of [Start](#LoadTester.TestRunner.Start) .  


<a id="LoadTester.TestRunner.TotalSeconds"></a>

* *double* **TotalSeconds**  
  The time that has passed since the last call of [Start](#LoadTester.TestRunner.Start) , in seconds.  

