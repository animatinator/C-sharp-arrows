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
    public class TestArrowLaws : TestSuite
    {
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


        public TestArrowLaws()
        {
            tests = new List<List<Test>>
            {
                standardLawTests, extraLawTests
            };
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

            return ArrowTestUtils.AssertArrowsGiveSameOutput(arrF, combined);
        }

        public static bool TestLaw2()
        {
            /*
             * Similar to law 1 but checking that Op.Arr(f).Combine(new IDArrow<T>()) = Op.Arr(f)
             */

            Arrow<int, int> arrF = Op.Arr((int x) => (x * x) - 5);
            Arrow<int, int> id = new IDArrow<int>();
            Arrow<int, int> combined = arrF.Combine(id);

            return ArrowTestUtils.AssertArrowsGiveSameOutput(arrF, combined);
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

            return ArrowTestUtils.AssertArrowsGiveSameOutput(fgH, fGH);
        }

        public static bool TestLaw4()
        {
            /*
             * Tests that the arrow of a composed pair of functions is equal to the composition of
             * arrows made from the individual functions, ie:
             * arr (g · f) = arr f >>> arr g
             */

            Func<int, int> f = ArrowTestUtils.GenerateFunc();
            Func<int, int> g = ArrowTestUtils.GenerateFunc();

            Arrow<int, int> arrowCompose = Op.Arr((int x) => g(f(x)));
            Arrow<int, int> composeArrows = Op.Arr(f).Combine(Op.Arr(g));

            return ArrowTestUtils.AssertArrowsGiveSameOutput(arrowCompose, composeArrows);
        }

        public static bool TestLaw5()
        {
            /*
             * first (arr f) = arr (f x id)
             */

            Func<int, int> f = ArrowTestUtils.GenerateFunc();
            Arrow<Tuple<int, int>, Tuple<int, int>> firstArr = Op.Arr(f).First(default(int));
            Arrow<Tuple<int, int>, Tuple<int, int>> arrFId = Op.Arr(
                (Tuple<int, int> x) => Tuple.Create(f(x.Item1), x.Item2));

            return ArrowTestUtils.AssertPairArrowsGiveSameOutput(firstArr, arrFId);
        }

        public static bool TestLaw6()
        {
            /*
             * Tests that the First operator distributes over arrow composition
             * first (f >>> g) = first f >>> first g
             */

            Arrow<int, int> f = Op.Arr(ArrowTestUtils.GenerateFunc());
            Arrow<int, int> g = Op.Arr(ArrowTestUtils.GenerateFunc());

            Arrow<Tuple<int, int>, Tuple<int, int>> firstOutside = f.Combine(g).First(default(int));
            Arrow<Tuple<int, int>, Tuple<int, int>> firstDistributed = f.First(default(int))
                .Combine(g.First(default(int)));

            return ArrowTestUtils.AssertPairArrowsGiveSameOutput(firstOutside, firstDistributed);
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

            Arrow<int, int> f = Op.Arr(ArrowTestUtils.GenerateFunc());
            Arrow<Tuple<int, int>, int> fstArrow = Op.Arr((Tuple<int, int> x) => x.Item1);
            Arrow<Tuple<int, int>, int> firstFArrFst = f.First(default(int)).Combine(fstArrow);
            Arrow<Tuple<int, int>, int> arrFstF = fstArrow.Combine(f);

            return ArrowTestUtils.AssertPairToSingleArrowsGiveSameOutput(firstFArrFst, arrFstF);
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

            IEnumerable<Type> types = ArrowTestUtils.GetBuiltInTypes();

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

            Func<int, int> f = ArrowTestUtils.GenerateFunc();
            Func<int, int> g = ArrowTestUtils.GenerateFunc();
            Func<int, int> fg = (x => g(f(x)));
            Arrow<int, int> arr = Op.Arr(f).Combine(Op.Arr(g));

            return ArrowTestUtils.AssertArrowEqualsFunc(arr, fg);
        }

        public static bool TestFirstOperatorDistributivity()
        {
            /*
             * Tests that the First operator distributes over function competition, ie:
             * first (f >>> g) = first f >>> first g
             * This test is done using two arrows on pairs, f and g
             */

            Arrow<int, int> f = Op.Arr(ArrowTestUtils.GenerateFunc());
            Arrow<int, int> g = Op.Arr(ArrowTestUtils.GenerateFunc());

            Arrow<Tuple<int, int>, Tuple<int, int>> firstFG =
                Op.First(f.Combine(g), default(int));

            Arrow<Tuple<int, int>, Tuple<int, int>> firstFfirstG =
                Op.First(f, default(int)).Combine(Op.First(g, default(int)));

            return ArrowTestUtils.AssertPairArrowsGiveSameOutput(firstFG, firstFfirstG);
        }

        public static bool TestArrFirstOrderingIrrelevance()
        {
            /*
             * Tests that the First and Arr operators, used in conjunction, will have the same
             * effect regardless of ordering. That is:
             * arr (first f) = first (arr f)
             */

            Func<int, int> f = ArrowTestUtils.GenerateFunc();
            Arrow<Tuple<int, int>, Tuple<int, int>> arrFirst = Op.Arr(
                (Tuple<int, int> x) =>
                    Tuple.Create(f(x.Item1), x.Item2)
                );
            Arrow<Tuple<int, int>, Tuple<int, int>> firstArr = Op.First(Op.Arr(f), default(int));

            return ArrowTestUtils.AssertPairArrowsGiveSameOutput(arrFirst, firstArr);
        }

        public static bool TestPipingCommutativity()
        {
            /*
             * If an identity is merged with a second function to form an arrow, attaching it to a
             * piped function must be commutative. In code:
             * arr (id *** g) >>> first f = first f >>> arr (id *** g)
             * This is tested by constructing the two arrows and checking their outputs match.
             */

            Arrow<int, int> f = Op.Arr(ArrowTestUtils.GenerateFunc());
            Arrow<int, int> g = Op.Arr(ArrowTestUtils.GenerateFunc());

            Arrow<Tuple<int, int>, Tuple<int, int>> mergeFirst = new IDArrow<int>().And(g).Combine(f.First(default(int)));
            Arrow<Tuple<int, int>, Tuple<int, int>> firstMerge = f.First(default(int)).Combine(new IDArrow<int>().And(g));

            return ArrowTestUtils.AssertPairArrowsGiveSameOutput(mergeFirst, firstMerge);
        }

        public static bool TestFirstPipingSimplification()
        {
            /*
             * Tests that piping a function before type simplification is equivalent to simplifying
             * type before connecting to the unpiped function.
             * That is, first f >>> arr ((s,t) -> s) = arr ((s,t) -> s) >>> f
             */

            Arrow<int, int> f = Op.Arr(ArrowTestUtils.GenerateFunc());
            Arrow<Tuple<int, int>, int> firstFArr = Op.First<int, int, int>(f).Combine(Op.Arr((Tuple<int, int> x) => x.Item1));
            Arrow<Tuple<int, int>, int> arrF = Op.Arr((Tuple<int, int> x) => x.Item1).Combine(f);

            return ArrowTestUtils.AssertPairToSingleArrowsGiveSameOutput(firstFArr, arrF);
        }

        public static bool TestPipingReassociation()
        {
            /*
             * Tests the following thing:
             * first (first f) >>> arr assoc = arr assoc >>> first f
             * The code itself is probably more expressive than any explanation I could come up
             * with.
             */

            Arrow<int, int> f = Op.Arr(ArrowTestUtils.GenerateFunc());
            AssocArrow<int, int, int> assoc = new AssocArrow<int, int, int>();

            Arrow<Tuple<Tuple<int, int>, int>, Tuple<int, Tuple<int, int>>> firstFirstArr =
                f.First(default(int)).First(default(int))
                .Combine(assoc);
            Arrow<Tuple<Tuple<int, int>, int>, Tuple<int, Tuple<int, int>>> arrFirst =
                assoc.Combine(f.First(default(Tuple<int, int>)));

            return ArrowTestUtils.AssertReassociationArrowsGiveSameOutput(firstFirstArr, arrFirst);
        }
    }
}
