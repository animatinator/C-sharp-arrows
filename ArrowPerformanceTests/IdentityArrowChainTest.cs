using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;

namespace ArrowPerformanceTests
{
    public class IdentityArrowChainTest
    {
        private static int maxLength = 15;
        private static int iterations = 1000000;

        public static void Run()
        {
            for (int currentLength = 1; currentLength <= maxLength; currentLength++)
            {
                Console.WriteLine("Running for an identity arrow chain of length {0}", currentLength);

                var arrow = GetIdentityArrowChain(currentLength);
                double totalRunTime = 0.0;

                for (int i = 0; i < 10; i++)
                {
                    Console.Write("| ");
                    Random rand = new Random();

                    for (int j = 0; j < iterations; j++)
                    {
                        double time = Environment.TickCount;
                        arrow.Invoke(rand.Next());
                        time = Environment.TickCount - time;
                        totalRunTime += time;
                    }
                }

                Console.WriteLine("Average time for {0} iterations: {1}", iterations, totalRunTime / 10);
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
    }
}
