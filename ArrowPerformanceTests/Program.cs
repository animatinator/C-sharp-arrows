using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowDataBinding.Arrows;
using ArrowPerformanceTests.TestFunctions;

namespace ArrowPerformanceTests
{
    class Program
    {
        static List<TestFunction> testFunctions = new List<TestFunction>
        {
            new Pythagoras(), new Increment(), new Quadratic(), new Arctan()
        };

        static void Main(string[] args)
        {
            Console.WriteLine("-- Running arrow performance tests --\n");

            foreach (TestFunction test in testFunctions)
            {
                //test.RunPerformanceTest();
            }

            Console.WriteLine();
            Console.WriteLine("-- Running identity arrow chain performance test --\n");

            IdentityArrowChainTest.Run();
        }
    }
}
