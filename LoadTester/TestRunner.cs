using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoadTester
{
    /// <summary>
    /// Runs load tests.
    /// </summary>
    /// <remarks>
    /// This is a test runner that repeatedly runs a set of tests.
    /// It can run many tests in parallel (<see cref="ParallelTests"/>), and it can be configured to repeat tests at a limited rate (<see cref="TargetTestsPerSecond"/>).
    /// Use <see cref="AddTest"/> to add test functions, then run <see cref="Start"/> to start running the tests.
    /// </remarks>
    public class TestRunner
    {
        /// <summary>
        /// The list of tests that will be run. Tests will be drawn randomly from this list.
        /// </summary>
        /// <remarks>
        /// Each test is a function that takes no parameters and returns true iff the test succeeded.
        /// </remarks>
        public List<Func<bool>> Tests { get; protected set; } = new List<Func<bool>>();

        /// <summary>
        /// Adds a test to the list of <see cref="Tests"/>.
        /// </summary>
        /// <remarks>
        /// Each test is a function that takes no parameters and returns true iff the test succeeded.
        /// </remarks>
        public void AddTest(Func<bool> test) { Tests.Add(test); }

        /// <summary>
        /// Starts running tests. Starts new threads and returns immedeately.
        /// </summary>
        /// <remarks>
        /// After calling this method, there will be <see cref="ParallelTests"/> many worker
        /// threads that repeatedly run random tests from the list of <see cref="Tests"/>.
        /// Call <see cref="Stop"/> if you want to stop running tests.
        /// Will throw an InvalidOperationException if tests are already running.
        /// </remarks>
        public void Start()
        {
            if (Running)
                throw new InvalidOperationException("Tests are already running.");

            StartTime = DateTime.Now;
            Running = true;
            _TotalTestsExecuted = 0;
            _TotalErrors = 0;

            for (int i = 0; i < ParallelTests; i++)
            {
                var thread = new Thread(RunTests);
                WorkerThreads.Add(thread);
                thread.Start();
            }
        }

        /// <summary>
        /// Stops the test execution.
        /// </summary>
        /// <param name="GracefulTimeout">
        /// The timespan in milliseconds to wait for each worker thread to finish the current test.
        /// If the worker thread has not finished its current test after this duration, it will be aborted.
        /// Default is 5000 ms.
        /// </param>
        public void Stop(int GracefulTimeout=5000)
        {
            foreach (var thread in WorkerThreads)
            {
                var stopped = thread.Join(GracefulTimeout);
                if (!stopped)
                    thread.Abort();
            }

            WorkerThreads.Clear();

            Running = false;
            StartTime = DateTime.MinValue;
        }

        /// <summary>
        /// The timestamp when <see cref="Start"/> was called, or DateTime.MinValue when no tests are running.
        /// </summary>
        public DateTime StartTime { get; protected set; }

        bool _Running = false;
        /// <summary>
        /// True iff tests are currently running.
        /// </summary>
        public bool Running
        {
            get { lock (settingsLock) return _Running; }
            protected set { lock (settingsLock) _Running = value; }
        }

        double _TargetTestsPerSecond = -1;

        /// <summary>
        /// The desired maximum rate of tests per second.
        /// </summary>
        /// <remarks>
        /// This is the global value of tests per second, not per worker thread.
        /// The worker threads will try to add sleep operations if tests are executed faster than this.
        /// If this value is negative, the rate of tests per second is unlimited.
        /// This value can safely be changed while tests are running.
        /// Each worker thread will adapt the change at its next test execution.
        /// </remarks>
        public double TargetTestsPerSecond
        {
            get { lock (settingsLock) return _TargetTestsPerSecond; }
            set { lock (settingsLock) _TargetTestsPerSecond = value; }
        }

        int _ParallelTests = 20;

        /// <summary>
        /// The number of worker threads that will run the tests.
        /// </summary>
        /// <remarks>
        /// When calling <see cref="Start"/>, this number of worker threads will be created.
        /// This value can safely be changed during the execution of tests, but has no effect
        /// until the test execution is restarted (by calling <see cref="Stop"/> and <see cref="Start"/>).
        /// </remarks>
        public int ParallelTests
        {
            get { lock (settingsLock) return _ParallelTests; }
            set { lock (settingsLock) _ParallelTests = value; }
        }

        long _TotalTestsExecuted;

        /// <summary>
        /// The total number of tests that have been executed since the last call of <see cref="Start"/>.
        /// </summary>
        public long TotalTestsExecuted
        {
            get { return Interlocked.Read(ref _TotalTestsExecuted); }
        }

        long _TotalErrors;

        /// <summary>
        /// The total number of tests that have failed since the last call of <see cref="Start"/>.
        /// </summary>
        public long TotalErrors
        {
            get { return Interlocked.Read(ref _TotalErrors); }
        }

        /// <summary>
        /// The time that has passed since the last call of <see cref="Start"/>, in seconds.
        /// </summary>
        public double TotalSeconds { get { return (DateTime.Now - StartTime).TotalSeconds; } }

        /// <summary>
        /// Lock object for settings that are read by the worker threads.
        /// </summary>
        protected object settingsLock = new object();

        /// <summary>
        /// List of active worker threads.
        /// </summary>
        protected List<Thread> WorkerThreads = new List<Thread>();

        static Random rand = new Random();

        /// <summary>
        /// Returns the next test. Used by RunTests.
        /// </summary>
        /// <returns></returns>
        protected Func<bool> GetNextTest()
        {
            return Tests[rand.Next(Tests.Count)];
        }

        /// <summary>
        /// Worker thread main method. Runs tests in an infinite loop.
        /// </summary>
        protected void RunTests()
        {
            Func<bool> test = null;

            while (Running)
            {
                var testStart = DateTime.Now;
                var targetSecondsPerTest = 1.0 / ((double)TargetTestsPerSecond / WorkerThreads.Count);

                test = GetNextTest();

                var success = test();

                Interlocked.Increment(ref _TotalTestsExecuted);
                if (!success)
                    Interlocked.Increment(ref _TotalErrors);

                var testDuration = (DateTime.Now - testStart).TotalSeconds;

                if (testDuration < targetSecondsPerTest)
                    Thread.Sleep((int)Math.Round((1000 * Math.Max(targetSecondsPerTest - testDuration, 0))));
            }
        }
    }
}
