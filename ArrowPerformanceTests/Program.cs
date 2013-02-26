using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowDataBinding.Arrows;
using ArrowPerformanceTests.TestFunctions;
using ArrowPerformanceTests.ListTestFunctions;

namespace ArrowPerformanceTests
{
    class Program
    {
        static List<TestFunction> simpleTestFunctions = new List<TestFunction>
        {
            new Pythagoras(), new Increment(), new Quadratic(), new Arctan()
        };

        static List<ListTestFunction> listTestFunctions = new List<ListTestFunction>
        {
            new Filter(), new OrderByThenMap(), new Foldl()
        };

        static void Main(string[] args)
        {
            //RunSimpleTestFunctions();
            //Console.WriteLine();
            //RunIdentityArrowPerformanceTest();
            //Console.WriteLine();
            RunListTestFunctions();
        }

        public static void RunSimpleTestFunctions()
        {
            Console.WriteLine("-- Running arrow performance tests --\n");
            List<TestResults> results = new List<TestResults>();

            foreach (TestFunction test in simpleTestFunctions)
            {
                TestResults result = test.RunPerformanceTest();
                results.Add(result);
            }

            Console.WriteLine();
            Console.WriteLine("CSV results:");
            OutputCSV(results);
        }

        public static void OutputCSV(List<TestResults> results)
        {
            Console.Write(",");  // Leave top-left corner square blank
            Console.WriteLine(results[0].resultsList.Keys.Aggregate((i, j) => i + "," + j));
            foreach (TestResults result in results)
            {
                Console.Write(result.Name + ",");
                foreach (double value in result.resultsList.Values)
                {
                    Console.Write(value);
                    Console.Write(",");
                }

                Console.WriteLine();
            }
        }

        public static void RunIdentityArrowPerformanceTest()
        {
            Console.WriteLine("-- Running identity arrow chain performance test --\n");
            IdentityArrowChainTest.Run();
        }

        public static void RunListTestFunctions()
        {
            Console.WriteLine("-- Running list arrow performance tests --\n");
            List<TestResults> results = new List<TestResults>();

            foreach (ListTestFunction test in listTestFunctions)
            {
                TestResults result = test.RunPerformanceTest();
                results.Add(result);
            }

            Console.WriteLine();
            Console.WriteLine("CSV results:");
            OutputCSV(results);
        }
    }
}
