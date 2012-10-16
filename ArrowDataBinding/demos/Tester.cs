using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.demos.ArrowTestThing;
using NUnit.Framework;

namespace ArrowDataBinding.demos
{
    [TestFixture]
    class Tester
    {
        public void Run()
        {
            Console.WriteLine("Running tests...");
            Console.WriteLine();
            TestArrowDemo();
        }

        [Test]
        public void TestArrowDemo()
        {
            Console.WriteLine("\t-- Testing arrow demo --\n");

            Arrow<int, int>.func demo1 = x => x * x + 3;

            // Simple arrow test - 'test' is passed through the function 'demo1' to 'result'
            TestSourceObject source = new TestSourceObject();
            source.value = 3;
            BindingDestination<int> result = new BindingDestination<int>(0);
            Arrow<int, int> testArrow = new Arrow<int, int>(demo1);
            Binding<int, int> testBinding = new Binding<int, int>(source, "value", testArrow, result);

            // Change the value of 'test' randomly and assert that 'result' changes accordingly
            Console.WriteLine("Testing single binding...");

            Random random = new Random();

            for (int i = 0; i < 500; i++)
            {
                source.value = random.Next(0, 1000);
                Assert.AreEqual(result, demo1(source.value));
            }

            Console.WriteLine("Tests passed!");
            Console.WriteLine();


            Arrow<int, int>.func2 demo2 = (b, h) => (b * h) / 2;

            // Pair arrow test - 's1' and 's2' are treated as the breadth and height of a triangle,
            // and 'result' is now bound to the area of said triangle
            BindingSource<int> s1 = new BindingSource<int>(4);
            BindingSource<int> s2 = new BindingSource<int>(3);
            Arrow<int, int> dualArrow = new Arrow<int, int>(demo2);
            PairBinding<int, int> pairBinding = new PairBinding<int, int>(s1, s2, dualArrow, result);

            // Change the values of 's1' and 's2' randomly and assert that 'result' changes accordingly
            Console.WriteLine("Testing double binding...");

            random = new Random();

            for (int i = 0; i < 500; i++)
            {
                s1.Value = random.Next(0, 1000);
                s2.Value = random.Next(0, 1000);
                Assert.AreEqual(result, demo2(s1.Value, s2.Value));
            }

            Console.WriteLine("Tests passed!");
            Console.WriteLine();

            Console.WriteLine("All tests passed successfully.\n\n");
        }
    }
}
