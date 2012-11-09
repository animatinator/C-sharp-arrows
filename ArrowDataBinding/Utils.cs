using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArrowDataBinding.Utils
{
    class Either<A, B>
    {
        /*
         * To be used for conditional arrows
         */

        public A a { get; set; }
        public B b { get; set; }
        public Type type;

        public Either(A value)
        {
            this.type = typeof(A);
            this.a = value;
        }

        public Either(B value)
        {
            this.type = typeof(B);
            this.b = value;
        }
    }
}
