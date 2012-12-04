using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Arrows;

namespace ArrowDataBinding.Combinators
{
    public static partial class Op
    {
        public static Func<A, B> ArrowFunc<A, B>(Func<A, B> f)
        {
            /*
             * The extension method for creating invertible arrows is created on Funcs, so to use
             * it the programmer must do:
             * new Func<A, B>(lambda stuff).Arr(other lambda stuff)
             * This is a bit messy-looking, so this function is here to allow the compiler to infer
             * the type parameters. The syntax now looks like:
             * ArrowFunc(lambda stuff).Arr(other lambda stuff)
             */

            return f;
        }
    }
}
