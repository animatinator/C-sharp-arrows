using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;
using System.Diagnostics;

namespace ArrowDataBinding.tests
{
    public class TestArrowLaws
    {
        public const int testIterations = 1000;


        public void Run()
        {
            if (TestIdentity() && TestDistributivity() && TestArrFirstOrderingIrrelevance()
                && TestPipingCommutativity() && TestFirstPipingSimplification()
                && TestPipingReassociation())
            {
                Console.WriteLine("Success!");
            }
            else
            {
                Console.WriteLine("Tests failed.");
            }
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

        // TODO: The rest of the laws from the 'Arrow Calculus: Functional Pearl' paper

        public static bool TestIdentity()
        {
            /*
             * Tests that the IDArrow<T> class preserves identity for any type
             */

            // TODO: TestIdentity
            // Maybe test with loads of types, randomly selected? Tricky.

            return false;
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
            //TODO: TestPipingCommutativity
            return false;
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

            return false;
        }

        public static bool TestPipingReassociation()
        {
            // TODO: TestPipingReassociation
            return false;
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
    }
}
