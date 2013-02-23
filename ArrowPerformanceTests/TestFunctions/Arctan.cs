using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;

namespace ArrowPerformanceTests.TestFunctions
{
    class Arctan : TestFunction
    {
        public Arctan()
        {
            Iterations = 10000000;
            Name = "Arctan";
        }

        protected override void InitialiseArrow()
        {
            arrow = Op.Arr((int x) => (int)(Math.Atan(x)*(180.0 / Math.PI)));
        }

        protected override void InitialiseFunc()
        {
            func = (int x) => (int)(Math.Atan(x) * (180.0 / Math.PI));
        }

        protected override int Function(int input)
        {
            return (int)(Math.Atan(input) * (180.0 / Math.PI));
        }
    }
}
