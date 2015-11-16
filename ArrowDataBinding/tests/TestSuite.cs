using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArrowDataBinding.tests
{
    public class TestSuite
    {
        protected delegate bool Test();
        protected static List<List<Test>> tests;

        public TestSuite()
        {
            // ...
        }

        public void Run()
        {
            int passCount = 0;
            int total = 0;

            List<string> failedTestNames = new List<string>();
            string currentTestName;

            foreach (List<Test> testSet in tests)
            {
                foreach (Test testRun in testSet)
                {
                    currentTestName = testRun.Method.ToString();
                    Console.WriteLine("Running test: {0}", currentTestName);

                    if (testRun())
                    {
                        Console.WriteLine("Test passed.");
                        passCount++;
                    }
                    else
                    {
                        Console.WriteLine("Test failed.");
                        failedTestNames.Add(currentTestName);
                    }

                    total++;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Test summary:");
            Console.WriteLine("Ran {0} tests, {1} passed and {2} failed.", total, passCount, total - passCount);

            if (passCount < total)
            {
                Console.WriteLine("Failed tests:");

                foreach (string failedTest in failedTestNames)
                {
                    Console.WriteLine("\t-{0}", failedTest);
                }
            }

            Console.WriteLine();
        }
    }
}
