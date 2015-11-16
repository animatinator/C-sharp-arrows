using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;

namespace ArrowPerformanceTests.TestFunctions
{
    class Quadratic : TestFunction
    {
        public Quadratic()
        {
            Iterations = 10000000;
            Name = "Quadratic";
        }

        protected override void InitialiseArrow()
        {
            arrow = Op.Arr((int x) => (x * x) + (2 * x) - 5);
        }

        protected override void InitialiseFunc()
        {
            func = (int x) => (x * x) + (2 * x) - 5;
        }

        protected override int Function(int input)
        {
            return (input * input) + (2 * input) + 5;
        }
    }
}
