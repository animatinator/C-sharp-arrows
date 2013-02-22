using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;

namespace ArrowDataBinding.tests
{
    public class ArrowTestUtils
    {
        private const int testIterations = 1000;

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

        public static bool AssertInvertibleArrowsGiveSameOutput<A, B>(InvertibleArrow<A, B> arr1, InvertibleArrow<A, B> arr2)
        {
            return AssertArrowsGiveSameOutput(arr1, arr2) && AssertArrowsGiveSameOutput(arr1.Invert(), arr2.Invert());
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

        public static bool AssertPairInvertibleArrowsGiveSameOutput(
                            InvertibleArrow<Tuple<int, int>, Tuple<int, int>> ar1,
                            InvertibleArrow<Tuple<int, int>, Tuple<int, int>> ar2)
        {
            return AssertPairArrowsGiveSameOutput(ar1, ar2)
                && AssertPairArrowsGiveSameOutput(ar1.Invert(), ar2.Invert());
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
