using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LoadTester;

namespace LoadTester.Demo
{
    public class Program
    {
        static void Main(string[] args)
        {
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
        }

        static byte[] MakeData()
        {
            // Return a 1 MB byte array
            const int dataSize = 1024 * 1024;
            byte[] data = new byte[dataSize];
            for (int i = 0; i < dataSize; i++)
            {
                // Fill in some numbers
                data[i] = (byte)(i % 256);
            }
            return data;
        }
    }
}
