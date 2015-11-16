using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;
using ArrowDataBinding.tests;
using ArrowDataBinding.Utils;

namespace ArrowDataBinding.tests
{
    public class TestInvertibleArrowLaws : TestSuite
    {
        private static List<Test> compositionLaws = new List<Test>
        {
            TestCompositionAssociativity, TestCompositionDistributivity, TestCompositionWithIdentity
        };

        private static List<Test> pairLaws = new List<Test>
        {
            TestFirstDistributivity, TestFirstCommutativity
        };

        private static List<Test> inversionLaws = new List<Test>
        {
            TestDoubleInversion, TestInversionDistributivity, TestInversionCorrectness,
            TestFirstInversion
        };


        public TestInvertibleArrowLaws()
        {
            tests = new List<List<Test>>
            {
                compositionLaws, pairLaws, inversionLaws
            };
        }

        public static bool TestCompositionAssociativity()
        {
            var arrf = new Func<int, int>(x => x * 9 - 3).Arr((int y) => (y + 3) / 9);
            var arrg = new Func<int, int>(x => x + 1).Arr((int y) => y - 1);
            var arrh = new Func<int, int>(x => x * 5).Arr((int y) => y / 5);

            var f_gh = arrf.Combine(arrg.Combine(arrh));
            var fg_h = (arrf.Combine(arrg)).Combine(arrh);

            return ArrowTestUtils.AssertInvertibleArrowsGiveSameOutput(f_gh, fg_h);
        }

        public static bool TestCompositionDistributivity()
        {
            var f1 = new Func<int, int>(x => x * 2);
            var f2 = new Func<int, int>(x => x - 1);
            var g1 = new Func<int, int>(x => x / 2);
            var g2 = new Func<int, int>(x => x + 1);

            var undistributed = f1.Arr(g1).Combine(f2.Arr(g2));
            var distributed = new Func<int, int>(x => f2(f1(x)))
                                .Arr(new Func<int, int>(y => g1(g2(y))));

            return ArrowTestUtils.AssertInvertibleArrowsGiveSameOutput(undistributed, distributed);
        }

        public static bool TestCompositionWithIdentity()
        {
            var id = new InvertibleIDArrow<int>();
            var f = Op.Arr((int x) => x * 7 - 1,
                            (int x) => (x + 1) / 7);

            var fID = f.Combine(id);
            var idF = id.Combine(f);

            return ArrowTestUtils.AssertInvertibleArrowsGiveSameOutput(fID, idF);
        }


        public static bool TestFirstDistributivity()
        {
            var f = new Func<int, int>(x => x * 3 - 2);
            var g = new Func<int, int>(y => (y + 2) / 3);
            var id = new Func<int, int>(x => x);

            var unDistributed = f.Arr(g).First(default(int));

            var distributed = (f.Mult(id)).Arr(g.Mult(id));

            return ArrowTestUtils.AssertPairInvertibleArrowsGiveSameOutput(distributed, unDistributed);
        }

        public static bool TestFirstCommutativity()
        {
            var f = new Func<int, int>(x => x + 1);
            var g = new Func<int, int>(y => y - 1);
            var id = new Func<int, int>(x => x);

            var firstArrow = Op.Arr((int x) => x * 7, (int y) => y / 7).First(default(int));
            var fgArrow = id.Mult(f).Arr(id.Mult(g));

            var firstArrowFirst = firstArrow.Combine(fgArrow);
            var firstArrowLast = fgArrow.Combine(firstArrow);

            return ArrowTestUtils.AssertPairInvertibleArrowsGiveSameOutput(firstArrowFirst, firstArrowLast);
        }


        public static bool TestDoubleInversion()
        {
            var arr1 = Op.Arr((int x) => x * 7, (int y) => y / 7);
            var arr2 = arr1.Invert().Invert();

            return ArrowTestUtils.AssertArrowsGiveSameOutput(arr1, arr2);
        }

        public static bool TestInversionDistributivity()
        {
            var arr1 = Op.Arr((int x) => x * 9 - 5, (int y) => (y + 5) / 9);
            var arr2 = Op.Arr((int x) => x * 10 - 4, (int y) => (y + 4) / 10);

            var undistributed = (arr1.Combine(arr2)).Invert();
            var distributed = (arr2.Invert()).Combine(arr1.Invert());

            return ArrowTestUtils.AssertInvertibleArrowsGiveSameOutput(undistributed, distributed);
        }

        public static bool TestInversionCorrectness()
        {
            var f = new Func<int, int>(x => x * 3);
            var g = new Func<int, int>(y => y / 3);

            var inverted = f.Arr(g).Invert();
            var actuallyInverse = g.Arr(f);

            return ArrowTestUtils.AssertInvertibleArrowsGiveSameOutput(inverted, actuallyInverse);
        }

        public static bool TestFirstInversion()
        {
            var arrow = Op.Arr((int x) => x + 54, (int y) => y - 54);

            var invFirst = arrow.First(default(int)).Invert();
            var firstInv = arrow.Invert().First(default(int));

            return ArrowTestUtils.AssertPairInvertibleArrowsGiveSameOutput(invFirst, firstInv);
        }

        /*
         * TODO: Add test for inv (left f) = left (inv f)? Not sure how 'left' fits in here
         */
    }
}
