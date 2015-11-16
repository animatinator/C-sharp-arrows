using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;

namespace ArrowPerformanceTests.ListTestFunctions
{
    public class Employer
    {
        public string Name { get; set; }
        public int Size { get; set; }
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Employer Employer { get; set; }
    }

    public abstract class ListTestFunction
    {
        public string Name { get; set; }

        protected ListArrow<Person, Person> arrow;
        protected abstract List<Person> LinqFunction(List<Person> list);
        protected abstract List<Person> Function(List<Person> list);

        private delegate void RunMethod(List<Person> input);
        private List<RunMethod> runMethods;

        public int Iterations { get; set; }
        private static Random rand = new Random();

        private static List<List<Person>> presetLists = new List<List<Person>>
        {
            new List<Person>
            {
                new Person {Name = "Tom", Age = 50, Employer = new Employer {Name = "Sony", Size = 30000}},
                new Person {Name = "Bob", Age = 23, Employer = new Employer {Name = "CiscoSoft", Size = 10}},
                new Person {Name = "Bill", Age = 17, Employer = new Employer {Name = "McDonald's", Size = 3000}}
            },

            new List<Person>
            {
                new Person {Name = "Tommo", Age = 540, Employer = new Employer {Name = "Test Company", Size = 300078790}},
                new Person {Name = "Bobby", Age = 233, Employer = new Employer {Name = "SillySoft", Size = 13420}},
                new Person {Name = "Billy", Age = 217, Employer = new Employer {Name = "Bob", Size = 2343000}}
            },

            new List<Person>
            {
                new Person {Name = "Sean", Age = 14, Employer = new Employer {Name = "Some School", Size = 1000}},
                new Person {Name = "Paul", Age = 19, Employer = new Employer {Name = "Generic University", Size = 10000}},
                new Person {Name = "Tim", Age = 7, Employer = new Employer {Name = "Bob", Size = 1}}
            }
        };


        public ListTestFunction()
        {
            InitialiseArrow();
            InitialiseRunMethods();

            Iterations = 1000000;
            Name = "Test function";
        }

        private void InitialiseRunMethods()
        {
            runMethods = new List<RunMethod>
            {
                RunArrow, RunLinqFunction, RunFunction
            };
        }

        protected abstract void InitialiseArrow();

        public TestResults RunPerformanceTest()
        {
            Console.WriteLine("Running performance test '{0}'", Name);
            TestResults results = new TestResults(Name);

            foreach (RunMethod run in runMethods)
            {
                Console.WriteLine("Running for {0}", run.Method.Name);
                double totalRunTime = 0.0d;

                for (int i = 0; i < 10; i++)
                {
                    Console.Write(" | ");

                    TimeSpan start = Process.GetCurrentProcess().TotalProcessorTime;

                    for (int j = 0; j < Iterations; j++)
                    {
                        run(GenerateList());
                    }

                    TimeSpan end = Process.GetCurrentProcess().TotalProcessorTime;
                    double time = (end - start).TotalMilliseconds;

                    Console.Write(time);
                    totalRunTime += time;
                }

                Console.WriteLine("\nAverage time for {0} executions: {1}", Iterations, totalRunTime / 10);
                results.AddResult(run.Method.Name, totalRunTime / 10);
            }

            return results;
        }

        private List<Person> GenerateList()
        {
            return presetLists[rand.Next(presetLists.Count)];
        }

        private void RunArrow(List<Person> input)
        {
            arrow.Invoke(input);
        }

        private void RunLinqFunction(List<Person> input)
        {
            LinqFunction(input);
        }

        private void RunFunction(List<Person> input)
        {
            Function(input);
        }
    }
}
