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
            if (TestLaw1() && TestLaw2())
            {
                Console.WriteLine("Success!");
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

            return AssertSameOutputGiven(arrF, combined);
        }

        public static bool TestLaw2()
        {
            /*
             * Similar to law 1 but checking that Op.Arr(f).Combine(new IDArrow<T>()) = Op.Arr(f)
             */
            Arrow<int, int> arrF = Op.Arr((int x) => (x * x) - 5);
            Arrow<int, int> id = new IDArrow<int>();
            Arrow<int, int> combined = arrF.Combine(id);

            return AssertSameOutputGiven(arrF, combined);
        }


        public static bool AssertSameOutputGiven<A, B>(Arrow<A, B> arr1, Arrow<A, B> arr2)
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
    }
}
