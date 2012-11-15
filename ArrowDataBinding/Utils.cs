using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArrowDataBinding.Utils
{
    public class Either<A, B>
    {
        /*
         * To be used for conditional arrows - stores either an object of type A or one of type B;
         * which type is stored is indicated by the Type variable
         */

        private A _a;
        public A ItemA
        {
            get
            {
                return _a;
            }
        }

        private B _b;
        public B ItemB
        {
            get
            {
                return _b;
            }
        }

        public Type type { get; set; }


        public Either(A value)
        {
            this.type = typeof(A);

            this._a = value;
            this._b = default(B);
        }

        public Either(B value)
        {
            this.type = typeof(B);

            this._a = default(A);
            this._b = value;
        }
    }

    static class TupleOp
    {
        public static Tuple<A, Tuple<B, C>> Assoc<A, B, C>(Tuple<Tuple<A, B>, C> input)
        {
            return Tuple.Create(input.Item1.Item1, Tuple.Create(input.Item1.Item2, input.Item2));
        }

        public static Tuple<Tuple<A, B>, C> Cossa<A, B, C>(Tuple<A, Tuple<B, C>> input)
        {
            return Tuple.Create(Tuple.Create(input.Item1, input.Item2.Item1), input.Item2.Item2);
        }
    }
}
