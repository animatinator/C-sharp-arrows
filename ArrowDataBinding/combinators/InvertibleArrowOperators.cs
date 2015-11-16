using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowDataBinding.Arrows;

namespace ArrowDataBinding.Combinators
{
    public static partial class Op
    {
        public static InvertibleArrow<A, B>
            Arr<A, B>(this Func<A, B> func, Func<B, A> invFunc)
        {
            /*
             * Basic InvertibleArrow constructor from a Func<A, B> and its inverse Func<B, A>
             */

            return new InvertibleArrow<A, B>(func, invFunc);
        }

        public static InvertibleArrow<A, C>
            Combine<A, B, C>(this InvertibleArrow<A, B> a1, InvertibleArrow<B, C> a2)
        {
            /*
             * Combine two invertible arrows end-to-end
             */

            InvertibleArrow<B, A> a1Reversed = a1.Invert();
            InvertibleArrow<C, B> a2Reversed = a2.Invert();

            InvertibleArrow<A, C> result = new InvertibleArrow<A, C>(
                x => a2.Invoke(a1.Invoke(x)),
                x => a1Reversed.Invoke(a2Reversed.Invoke(x))
                );

            return result;
        }

        public static InvertibleArrow<Tuple<A, C>, Tuple<B, C>>
            First<A, B, C>(this InvertibleArrow<A, B> arr)
        {
            /*
             * Similar to the First function for normal arrows, but obviously performing it on
             * InvertibleArrows instead.
             */

            InvertibleArrow<B, A> reversed = arr.Invert();

            return new InvertibleArrow<Tuple<A, C>, Tuple<B, C>>(
                (Tuple<A, C> x) =>
                    new Tuple<B, C>(
                        arr.Invoke(x.Item1),
                        x.Item2
                        ),
                (Tuple<B, C> x) =>
                    new Tuple<A, C>(
                        reversed.Invoke(x.Item1),
                        x.Item2
                        )
                );
        }

        public static InvertibleArrow<Tuple<A, C>, Tuple<B, C>>
            First<A, B, C>(this InvertibleArrow<A, B> arr, C nullArgument)
        {
            /*
             * Version of First which has an unused parameter passed in to allow it to infer the
             * unknown third type.
             */

            return arr.First<A, B, C>();
        }

        public static InvertibleArrow<Tuple<A, C>, Tuple<B, D>>
            And<A, B, C, D>(this InvertibleArrow<A, B> a1, InvertibleArrow<C, D> a2)
        {
            /*
             * Combines two arrows in parallel leading to an arrow on tuples, similar to the And
             * function on normal arrows
             */

            InvertibleArrow<B, A> a1Inverted = a1.Invert();
            InvertibleArrow<D, C> a2Inverted = a2.Invert();

            return new InvertibleArrow<Tuple<A, C>, Tuple<B, D>>(
                (Tuple<A, C> x) =>
                    {
                        B leftResult = default(B);
                        D rightResult = default(D);

                        Parallel.Invoke(
                            () => leftResult = a1.Invoke(x.Item1),
                            () => rightResult = a2.Invoke(x.Item2));

                        return new Tuple<B, D>(leftResult, rightResult);
                    },
                (Tuple<B, D> x) =>
                    {
                        A leftResult = default(A);
                        C rightResult = default(C);

                        Parallel.Invoke(
                            () => leftResult = a1.Invert().Invoke(x.Item1),
                            () => rightResult = a2.Invert().Invoke(x.Item2));

                        return new Tuple<A, C>(leftResult, rightResult);
                    }
                );
        }

        public static InvertibleArrow<A, Tuple<A, A>> InvertibleSplit<A>()
        {
            /*
             * Returns an invertible arrow which takes an input of type A and returns a Tuple<A, A>
             * which is the input duplicated.
             */

            return Op.Arr((A x) => Tuple.Create(x, x), (Tuple<A, A> t) => t.Item1);
        }

        public static Arrow<A, Tuple<B, B>> Split<A, B>(this InvertibleArrow<A, B> arr)
        {
            /*
             * Takes an invertible arrow from A to B and returns one with its output duplicated
             * into a tuple (overrides the usual version of this for normal Arrows so that Fanout
             * will work for invertible arrows without modification).
             */

            InvertibleArrow<B, Tuple<B, B>> splitArrow = InvertibleSplit<B>();

            return arr.Combine(splitArrow);
        }
    }
}
