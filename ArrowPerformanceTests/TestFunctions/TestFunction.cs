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
            foreach (RunMethod run in runMethods)
            {
                Console.WriteLine("Running for {0}", run.Method.Name);
                double totalRunTime = 0.0d;
                Random rand = new Random();

                for (int i = 0; i < Iterations; i++)
                {
                    double time = Environment.TickCount;
                    run(rand.Next());
                    time = Environment.TickCount - time;
                    totalRunTime += time;
                }

                Console.WriteLine("Time for {0} executions: {1}", Iterations, totalRunTime);
            }
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
