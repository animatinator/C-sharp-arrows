using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Arrows;

namespace ArrowDataBinding.demos.project
{
    class ArrowDemos
    {
        public void Run()
        {
            Console.WriteLine("Demo-ing arrows...\n");
            DemoInvertibleArrows();
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
    }
}
