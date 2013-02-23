using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;

namespace ArrowPerformanceTests.TestFunctions
{
    public abstract class TestFunction
    {
        public string Name { get; set; }

        protected Arrow<int, int> arrow;
        protected Func<int, int> func;
        protected abstract int Function(int input);

        private delegate void RunMethod(int input);
        private List<RunMethod> runMethods;

        public int Iterations { get; set; }


        public TestFunction()
        {
            InitialiseArrow();
            InitialiseFunc();
            InitialiseRunMethods();

            Iterations = 1000000;
            Name = "Test function";
        }

        private void InitialiseRunMethods()
        {
            runMethods = new List<RunMethod>
            {
                RunArrow, RunFunc, RunFunction
            };
        }

        protected abstract void InitialiseArrow();
        protected abstract void InitialiseFunc();

        public void RunPerformanceTest()
        {
            Console.WriteLine("Running performance test '{0}'", Name);

            foreach (RunMethod run in runMethods)
            {
                Console.WriteLine("Running for {0}", run.Method.Name);
                double totalRunTime = 0.0d;

                for (int i = 0; i < 10; i++)
                {
                    Console.Write("| ");
                    Random rand = new Random();

                    for (int j = 0; j < Iterations; j++)
                    {
                        double time = Environment.TickCount;
                        run(rand.Next());
                        time = Environment.TickCount - time;
                        totalRunTime += time;
                    }
                }

                Console.WriteLine("\nAverage time for {0} executions: {1}", Iterations, totalRunTime / 10);
            }

            Console.WriteLine();
        }

        private void RunArrow(int input)
        {
            arrow.Invoke(input);
        }

        private void RunFunc(int input)
        {
            func(input);
        }

        private void RunFunction(int input)
        {
            Function(input);
        }
    }
}
