using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;
using ArrowDataBinding.Utils;

namespace ArrowDataBinding.tests
{
    public class TestArrowLaws
    {
        private const int testIterations = 1000;

        private delegate bool Test();

        private static List<Test> standardLawTests = new List<Test>
        {
            TestIdentity, TestDistributivity, TestArrFirstOrderingIrrelevance,
            TestPipingCommutativity, TestFirstPipingSimplification, TestPipingReassociation
        };

        private static List<Test> extraLawTests = new List<Test>
        {
            TestLaw1, TestLaw2, TestLaw3, TestLaw4, TestLaw5, TestLaw6, TestLaw7, TestLaw8,
            TestLaw9
        };

        private static List<List<Test>> tests = new List<List<Test>>
        {
            standardLawTests, extraLawTests
        };


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


        public static bool TestLaw1()
        {
            /*
             * Tests that IDArrow<T>.Combine(Op.Arr(f)) = Op.Arr(f)
             * That is, that the identity arrow combined with another arrow is exactly the same as
             * the other arrow on its own
             */

            Arrow<int, int> arrF = Op.Arr((int x) => (x*x) - 5);
            Arrow<int, int> id = new IDArrow<int>();
            Arrow<int, int> combined = id.Combine(arrF);

            return AssertArrowsGiveSameOutput(arrF, combined);
        }

        public static bool TestLaw2()
        {
            /*
             * Similar to law 1 but checking that Op.Arr(f).Combine(new IDArrow<T>()) = Op.Arr(f)
             */

            Arrow<int, int> arrF = Op.Arr((int x) => (x * x) - 5);
            Arrow<int, int> id = new IDArrow<int>();
            Arrow<int, int> combined = arrF.Combine(id);

            return AssertArrowsGiveSameOutput(arrF, combined);
        }

        public static bool TestLaw3()
        {
            /*
             * Tests that (f.Combine(g)).Combine(h) = f.Combine(g.Combine(h))
             * That is, arrow combination is associative
             */

            Arrow<int, int> arrF = Op.Arr((int x) => (x * x) - 5);
            Arrow<int, int> arrG = Op.Arr((int x) => (x + 5) * 7);
            Arrow<int, int> arrH = Op.Arr((int x) => (x - 50) * x);

            Arrow<int, int> fgH = (arrF.Combine(arrG)).Combine(arrH);
            Arrow<int, int> fGH = arrF.Combine(arrG.Combine(arrH));

            return AssertArrowsGiveSameOutput(fgH, fGH);
        }

        public static bool TestLaw4()
        {
            /*
             * Tests that the arrow of a composed pair of functions is equal to the composition of
             * arrows made from the individual functions, ie:
             * arr (g · f) = arr f >>> arr g
             */

            Func<int, int> f = GenerateFunc();
            Func<int, int> g = GenerateFunc();

            Arrow<int, int> arrowCompose = Op.Arr((int x) => g(f(x)));
            Arrow<int, int> composeArrows = Op.Arr(f).Combine(Op.Arr(g));

            return AssertArrowsGiveSameOutput(arrowCompose, composeArrows);
        }

        public static bool TestLaw5()
        {
            /*
             * first (arr f) = arr (f x id)
             */

            Func<int, int> f = GenerateFunc();
            Arrow<Tuple<int, int>, Tuple<int, int>> firstArr = Op.Arr(f).First<int, int, int>();
            Arrow<Tuple<int, int>, Tuple<int, int>> arrFId = Op.Arr(
                (Tuple<int, int> x) => Tuple.Create(f(x.Item1), x.Item2));

            return AssertPairArrowsGiveSameOutput(firstArr, arrFId);
        }

        public static bool TestLaw6()
        {
            /*
             * Tests that the First operator distributes over arrow composition
             * first (f >>> g) = first f >>> first g
             */

            Arrow<int, int> f = Op.Arr(GenerateFunc());
            Arrow<int, int> g = Op.Arr(GenerateFunc());

            Arrow<Tuple<int, int>, Tuple<int, int>> firstOutside = f.Combine(g).First<int, int, int>();
            Arrow<Tuple<int, int>, Tuple<int, int>> firstDistributed = f.First<int, int, int>()
                .Combine(g.First<int, int, int>());

            return AssertPairArrowsGiveSameOutput(firstOutside, firstDistributed);
        }

        public static bool TestLaw7()
        {
            /*
             * Equivalent to TestPipingCommutativity:
             * first f >>> arr (id × g) = arr (id × g) >>> first f
             */

            return TestPipingCommutativity();
        }

        public static bool TestLaw8()
        {
            /*
             * Tests that a split arrow commutes with the 'fst' function (which returns the first
             * element of a tuple), that is:
             * first f >>> arr fst = arr fst >>> f
             */

            Arrow<int, int> f = Op.Arr(GenerateFunc());
            Arrow<Tuple<int, int>, int> fstArrow = Op.Arr((Tuple<int, int> x) => x.Item1);
            Arrow<Tuple<int, int>, int> firstFArrFst = f.First<int, int, int>().Combine(fstArrow);
            Arrow<Tuple<int, int>, int> arrFstF = fstArrow.Combine(f);

            return AssertPairToSingleArrowsGiveSameOutput(firstFArrFst, arrFstF);
        }

        public static bool TestLaw9()
        {
            /*
             * Equivalent to TestPipingReassociation:
             * first (first f) >>> arr assoc = arr assoc >>> first f
             */

            return TestPipingReassociation();
        }


        public static bool TestIdentity()
        {
            /*
             * Tests that the IDArrow<T> class preserves identity for any type
             */

            // TODO: Make TestIdentity cleaner

            IEnumerable<Type> types = GetBuiltInTypes();

            bool passed = true;

            foreach (Type type in types)
            {
                Type typeIdentity = typeof(IDArrow<>).MakeGenericType(type);
                object identityArrow = Activator.CreateInstance(typeIdentity);
                MethodInfo invokeMethod = typeIdentity.GetMethod("Invoke", new Type[] {type});

                object parameter = Activator.CreateInstance(type);

                object result = invokeMethod.Invoke(identityArrow, new object[]{parameter});

                if (!result.Equals(parameter)) passed = false;
            }

            return passed;
        }

        public static bool TestDistributivity()
        {
            /*
             * Tests that the two fundamental operators, Arr and First, distribute correctly
             * over function and arrow composition.
             */

            return (TestArrOperatorDistributivity() && TestFirstOperatorDistributivity());
        }

        public static bool TestArrOperatorDistributivity()
        {
            /*
             * Tests that the Arr operator distributes over function composition.
             * That is, g(f(x)) = (arr f) >>> (arr g)
             * We test this by comparing a lambda combination of two functions with their arrow
             * combination.
             */

            Func<int, int> f = GenerateFunc();
            Func<int, int> g = GenerateFunc();
            Func<int, int> fg = (x => g(f(x)));
            Arrow<int, int> arr = Op.Arr(f).Combine(Op.Arr(g));

            return AssertArrowEqualsFunc(arr, fg);
        }

        public static bool TestFirstOperatorDistributivity()
        {
            /*
             * Tests that the First operator distributes over function competition, ie:
             * first (f >>> g) = first f >>> first g
             * This test is done using two arrows on pairs, f and g
             */

            Arrow<int, int> f = Op.Arr(GenerateFunc());
            Arrow<int, int> g = Op.Arr(GenerateFunc());

            Arrow<Tuple<int, int>, Tuple<int, int>> firstFG =
                Op.First<int, int, int>(f.Combine(g));

            Arrow<Tuple<int, int>, Tuple<int, int>> firstFfirstG =
                Op.First<int, int, int>(f).Combine(Op.First<int, int, int>(g));

            return AssertPairArrowsGiveSameOutput(firstFG, firstFfirstG);
        }

        public static bool TestArrFirstOrderingIrrelevance()
        {
            /*
             * Tests that the First and Arr operators, used in conjunction, will have the same
             * effect regardless of ordering. That is:
             * arr (first f) = first (arr f)
             */

            Func<int, int> f = GenerateFunc();
            Arrow<Tuple<int, int>, Tuple<int, int>> arrFirst = Op.Arr(
                (Tuple<int, int> x) =>
                    Tuple.Create(f(x.Item1), x.Item2)
                );
            Arrow<Tuple<int, int>, Tuple<int, int>> firstArr = Op.First<int, int, int>(Op.Arr(f));

            return AssertPairArrowsGiveSameOutput(arrFirst, firstArr);
        }

        public static bool TestPipingCommutativity()
        {
            /*
             * If an identity is merged with a second function to form an arrow, attaching it to a
             * piped function must be commutative. In code:
             * arr (id *** g) >>> first f = first f >>> arr (id *** g)
             * This is tested by constructing the two arrows and checking their outputs match.
             */

            Arrow<int, int> f = Op.Arr(GenerateFunc());
            Arrow<int, int> g = Op.Arr(GenerateFunc());

            Arrow<Tuple<int, int>, Tuple<int, int>> mergeFirst = new IDArrow<int>().And(g).Combine(f.First<int, int, int>());
            Arrow<Tuple<int, int>, Tuple<int, int>> firstMerge = f.First<int, int, int>().Combine(new IDArrow<int>().And(g));

            return AssertPairArrowsGiveSameOutput(mergeFirst, firstMerge);
        }

        public static bool TestFirstPipingSimplification()
        {
            /*
             * Tests that piping a function before type simplification is equivalent to simplifying
             * type before connecting to the unpiped function.
             * That is, first f >>> arr ((s,t) -> s) = arr ((s,t) -> s) >>> f
             */

            Arrow<int, int> f = Op.Arr(GenerateFunc());
            Arrow<Tuple<int, int>, int> firstFArr = Op.First<int, int, int>(f).Combine(Op.Arr((Tuple<int, int> x) => x.Item1));
            Arrow<Tuple<int, int>, int> arrF = Op.Arr((Tuple<int, int> x) => x.Item1).Combine(f);

            return AssertPairToSingleArrowsGiveSameOutput(firstFArr, arrF);
        }

        public static bool TestPipingReassociation()
        {
            // TODO: Finish TestPipingReassociation
            // TODO: Make the code in TestPipingReassociation less awful

            Arrow<int, int> f = Op.Arr(GenerateFunc());
            AssocArrow<int, int, int> assoc = new AssocArrow<int, int, int>();

            Arrow<Tuple<Tuple<int, int>, int>, Tuple<int, Tuple<int, int>>> firstFirstArr =
                f.First<int, int, int>().First<Tuple<int, int>, Tuple<int, int>, int>()
                .Combine(assoc);
            Arrow<Tuple<Tuple<int, int>, int>, Tuple<int, Tuple<int, int>>> arrFirst =
                assoc.Combine(f.First<int, int, Tuple<int, int>>());

            return AssertReassociationArrowsGiveSameOutput(firstFirstArr, arrFirst);
        }


        public static Func<int, int> GenerateFunc()
        {
            List<Func<int, int>> functions = new List<Func<int, int>> {
                (x => (x * 5) - 4),
                (x => (x + 3) * 9),
                (x => (x - 78) * x),
                (x => (x - 1) * x)
            };

            Random rand = new Random();

            return functions[rand.Next(functions.Count)];
        }

        public static Func<Tuple<int, int>, Tuple<int, int>> GenerateTupleFunc()
        {
            List<Func<Tuple<int, int>, Tuple<int, int>>> functions = new List<Func<Tuple<int, int>, Tuple<int, int>>> {
                (x => Tuple.Create(x.Item1 - 3, x.Item2 * 4)),
                (x => Tuple.Create(x.Item1 + x.Item2, x.Item2 * 4 - 3*x.Item1)),
                (x => Tuple.Create(x.Item1 * x.Item2, x.Item2 - 79)),
                (x => Tuple.Create(x.Item1 - 3 * x.Item2, x.Item1 * 3)),
            };

            Random rand = new Random();

            return functions[rand.Next(functions.Count)];
        }

        public static IEnumerable<Type> GetBuiltInTypes()
        {
            /*
             * Returns a list of types to iterate through
             */

            return typeof(int).Assembly
                              .GetTypes()
                              .Where(t => t.IsPrimitive);
        }


        public static bool AssertArrowsGiveSameOutput<A, B>(Arrow<A, B> arr1, Arrow<A, B> arr2)
        {
            Debug.Assert(arr1 != null);
            Debug.Assert(arr2 != null);

            Random rand = new Random();
            bool passed = true;

            for (int i = 0; i < testIterations; i++)
            {
                int input = rand.Next();
                if (arr1.Invoke(input) != arr2.Invoke(input))
                {
                    passed = false;
                }
            }

            return passed;
        }

        public static bool AssertArrowEqualsFunc(Arrow<int, int> arrow, Func<int, int> func)
        {
            Debug.Assert(arrow != null);
            Debug.Assert(func != null);

            Random rand = new Random();
            bool passed = true;

            for (int i = 0; i < testIterations; i++)
            {
                int input = rand.Next();
                if (arrow.Invoke(input) != func.Invoke(input))
                {
                    passed = false;
                }
            }

            return passed;
        }

        public static bool AssertPairArrowsGiveSameOutput(Arrow<Tuple<int, int>, Tuple<int, int>> arr1,
            Arrow<Tuple<int, int>, Tuple<int, int>> arr2)
        {
            Debug.Assert(arr1 != null);
            Debug.Assert(arr2 != null);

            Random rand = new Random();
            bool passed = true;

            for (int i = 0; i < testIterations; i++)
            {
                int inputA = rand.Next();
                int inputB = rand.Next();

                Tuple<int, int> pairResult1 = arr1.Invoke(Tuple.Create(inputA, inputB));
                Tuple<int, int> pairResult2 = arr2.Invoke(Tuple.Create(inputA, inputB));

                if (!pairResult1.Equals(pairResult2))
                {
                    passed = false;
                }
            }

            return passed;
        }

        public static bool AssertPairToSingleArrowsGiveSameOutput(Arrow<Tuple<int, int>, int> arr1,
            Arrow<Tuple<int, int>, int> arr2)
        {
            Debug.Assert(arr1 != null);
            Debug.Assert(arr2 != null);

            Random rand = new Random();
            bool passed = true;

            for (int i = 0; i < testIterations; i++)
            {
                int inputA = rand.Next();
                int inputB = rand.Next();

                int result1 = arr1.Invoke(Tuple.Create(inputA, inputB));
                int result2 = arr2.Invoke(Tuple.Create(inputA, inputB));

                if (result1 != result2)
                {
                    passed = false;
                }
            }

            return passed;
        }

        public static bool AssertReassociationArrowsGiveSameOutput(
            Arrow<Tuple<Tuple<int, int>, int>, Tuple<int, Tuple<int, int>>> arr1,
            Arrow<Tuple<Tuple<int, int>, int>, Tuple<int, Tuple<int, int>>> arr2)
        {
            Debug.Assert(arr1 != null);
            Debug.Assert(arr2 != null);

            Random rand = new Random();
            bool passed = true;

            for (int i = 0; i < testIterations; i++)
            {
                int inputA = rand.Next();
                int inputB = rand.Next();
                Tuple<int, int> inputTuple = Tuple.Create(inputA, inputB);
                int inputInt = rand.Next();

                Tuple<int, Tuple<int, int>> result1 = arr1.Invoke(Tuple.Create(inputTuple, inputInt));
                Tuple<int, Tuple<int, int>> result2 = arr2.Invoke(Tuple.Create(inputTuple, inputInt));

                if (!result1.Equals(result2))
                {
                    passed = false;
                }
            }

            return passed;
        }
    }
}
