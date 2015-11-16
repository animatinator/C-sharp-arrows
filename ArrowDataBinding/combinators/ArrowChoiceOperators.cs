using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Utils;

namespace ArrowDataBinding.Combinators
{
    public static partial class Op
    {
        public static ArrowChoice<A, C, B, C> LeftChoice<A, B, C>(Arrow<A, B> arr)
        {
            return new ArrowChoice<A, C, B, C>(x => arr.Invoke(x), (C x) => x);
        }

        public static ArrowChoice<C, A, C, B> RightChoice<A, B, C>(Arrow<A, B> arr)
        {
            return new ArrowChoice<C, A, C, B>((C x) => x, x => arr.Invoke(x));
        }

        public static ArrowChoice<A, B, C, D> Either<A, B, C, D>(Arrow<A, C> ar1, Arrow<B, D> ar2)
        {
            return new ArrowChoice<A, B, C, D>(x => ar1.Invoke(x), x => ar2.Invoke(x));
        }

        public static Arrow<Either<A, B>, E> FanInChoice<A, B, C, D, E>(this ArrowChoice<A, B, C, D> choice, Arrow<C, E> ar1, Arrow<D, E> ar2)
        {
            return new Arrow<Either<A, B>, E>(x =>
            {
                dynamic choiceArrowResult = choice.Invoke(x);
                if (choiceArrowResult.type == typeof(C))
                {
                    return ar1.Invoke(choiceArrowResult);
                }
                else
                {
                    return ar2.Invoke(choiceArrowResult);
                }
            });
        }
    }
}
