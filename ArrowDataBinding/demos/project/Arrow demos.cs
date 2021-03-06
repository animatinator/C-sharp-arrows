﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Combinators;
using ArrowDataBinding.Arrows;

namespace ArrowDataBinding.demos.project
{
    class ArrowDemos
    {
        public void Run()
        {
            Console.WriteLine("Demo-ing arrows...\n");
            //DemoInvertibleArrows();
            DemoListArrows();
        }

        public static void DemoInvertibleArrows()
        {
            Console.WriteLine("\t-- Invertible arrows demo --\n");
            
            InvertibleArrow<int, int> arrow = Op.ArrowFunc((int x) => x * 2 + 5).Arr((int y) => (y - 5) / 2);
            InvertibleArrow<int, string> arrow2 = Op.ArrowFunc((int x) => x.ToString()).Arr((string y) => Convert.ToInt32(y));
            InvertibleArrow<int, string> arrow3 = arrow.Combine(arrow2);
            string result = arrow3.Invoke(3);
            Console.WriteLine("3 into the arrow gives {0}", result);
            Console.WriteLine("{0} into the inverted arrow gives {1}", result, arrow3.Invert().Invoke(result));
        }

        public static void DemoListArrows()
        {
            List<String> cities = new List<String> {
                "London",
                "Edinburgh",
                "Newcastle",
                "Manchester",
                "Glasgow",
                "Cambridge"
            };

            ListArrow<String, String> sorter = ListArrow.Map((String x) => Tuple.Create(x, x.Length))
                .Combine(ListArrow.OrderBy<Tuple<String, int>>((s1, s2) => s1.Item2 - s2.Item2)
                .Combine(ListArrow.Map((Tuple<String, int> x) => x.Item1)))
                .Combine(ListArrow.Filter((String x) => x.IndexOf('E') != 0));

            var result = sorter.Invoke(cities);
            foreach (var s in result)
            {
                Console.WriteLine(s);
            }

            Console.WriteLine();

            ListArrow<String, String> cityArrow =
                ListArrow.Map((String city) => Tuple.Create(city, city.Length))
                .Filter((Tuple<String, int> cityTuple) => cityTuple.Item2 > 7)
                .Map((Tuple<String, int> cityTuple) => cityTuple.Item1)
                .Filter((String city) => city != "Manchester")
                .OrderBy((String c1, String c2) => c1.CompareTo(c2));

            result = cityArrow.Invoke(cities);
            foreach (var s in result)
            {
                Console.WriteLine(s);
            }
        }

        public static void blah()
        {
            Arrow<int, string> test = Op.Arr((int x) => x + 1).Combine(Op.Arr((int x) => x.ToString()));
            Arrow<int, string> test1 = Op.Combine(Op.Arr((int x) => x + 1), Op.Arr((int x) => x.ToString()));
        }

        public static void blahblah()
        {
            Arrow<int, double> pythagoras = Op.LiftA2((int x, int y) => x + y,
                    Op.Arr((int x) => x * x),
                    Op.Arr((int y) => y * y))
                .Combine(Op.Arr((int x) => Math.Sqrt(x)));
        }
    }
}
