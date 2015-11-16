using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;

namespace ArrowPerformanceTests
{
    public class IdentityArrowChainTest
    {
        private static int maxLength = 20;
        private static int iterations = 1000000;
        private static double[] results = new double[maxLength];
        private static double[] deviations = new double[maxLength];

        public static void Run()
        {
            for (int currentLength = 1; currentLength <= maxLength; currentLength++)
            {
                Console.WriteLine("Running for an identity arrow chain of length {0}", currentLength);

                var arrow = GetIdentityArrowChain(currentLength);
                double totalRunTime = 0.0;
                List<double> runTimes = new List<double>();

                for (int i = 0; i < 10; i++)
                {
                    Console.Write(" | ");
                    Random rand = new Random();

                    TimeSpan start = Process.GetCurrentProcess().TotalProcessorTime;

                    for (int j = 0; j < iterations; j++)
                    {
                        arrow.Invoke(rand.Next());
                    }

                    TimeSpan end = Process.GetCurrentProcess().TotalProcessorTime;
                    double time = (end - start).TotalMilliseconds;

                    Console.Write(time);
                    runTimes.Add(time);
                    totalRunTime += time;
                }

                Console.WriteLine("\nAverage time for {0} iterations: {1}", iterations, runTimes.Average());
                results[currentLength - 1] = totalRunTime / 10;
                deviations[currentLength - 1] = StandardDeviation(runTimes);
            }

            Console.WriteLine();
            Console.WriteLine("CSV-formatted results:");
            OutputCSV();
        }

        private static void OutputCSV()
        {
            for (int i = 0; i < maxLength; i++)
            {
                Console.WriteLine("{0},{1},{2}", i + 1, results[i], deviations[i]);
            }
        }

        private static Arrow<int, int> GetIdentityArrowChain(int length)
        {
            if (length == 0) return null;
            else
            {
                Arrow<int, int> result = new IDArrow<int>();
                length--;

                while (length > 0)
                {
                    result = result.Combine(new IDArrow<int>());
                    length--;
                }

                return result;
            }
        }

        private static double StandardDeviation(List<double> doubles)
        {
            double average = doubles.Average();
            double sumOfSquaresOfDifferences = doubles.Select(val => (val - average) * (val - average)).Sum();
            double sd = Math.Sqrt(sumOfSquaresOfDifferences / doubles.Count);
            return sd;
        }
    }
}
