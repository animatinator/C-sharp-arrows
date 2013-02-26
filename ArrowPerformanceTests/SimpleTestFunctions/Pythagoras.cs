using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;

namespace ArrowPerformanceTests.TestFunctions
{
    class Pythagoras : TestFunction
    {
        public Pythagoras()
        {
            Name = "Pythagoras";
            Iterations = 3000000;
        }

        protected override void InitialiseArrow()
        {
            var square = Op.Arr((int x) => x * x);
            Func<int, int, int> add = ((int x, int y) => x + y);

            //arrow = Op.LiftA2(add, square, square).Combine(Op.Arr((int x) => (int)Math.Sqrt(x)));
            //arrow = Op.Split<int>().Combine(square.And(square)).Unsplit(add).Combine(Op.Arr((int x) => (int)Math.Sqrt(x)));
            arrow = Op.Arr((int x) => Tuple.Create(x * x, x * x)).Unsplit(add).Combine(Op.Arr((int x) => (int)Math.Sqrt(x)));
        }

        protected override void InitialiseFunc()
        {
            func = new Func<int, int>(x => (int)Math.Sqrt(x * x * 2));
        }

        protected override int Function(int input)
        {
            return (int)Math.Sqrt(input * input * 2);
        }
    }
}
