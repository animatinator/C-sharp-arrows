using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;

namespace ArrowPerformanceTests.TestFunctions
{
    public class Increment : TestFunction
    {
        public Increment()
        {
            Iterations = 10000000;
            Name = "Increment";
        }

        protected override void InitialiseArrow()
        {
            arrow = Op.Arr((int x) => x + 1);
        }

        protected override void InitialiseFunc()
        {
            func = (int x) => x + 1;
        }

        protected override int Function(int input)
        {
            return input + 1;
        }
    }
}
