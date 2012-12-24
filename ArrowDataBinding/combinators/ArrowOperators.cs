using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ArrowDataBinding.Arrows;

namespace ArrowDataBinding.Combinators
{
    public static partial class Op
    {
        public static Arrow<A, B> Arr<A, B>(this Func<A, B> func)
        {
            /*
             * Basic arrow construction operator from a Func<A, B>
             */

            return new Arrow<A, B>(func);
        }

        public static Arrow<A, C> Combine<A, B, C>(this Arrow<A, B> a1, Arrow<B, C> a2)
        {
            /*
             * Combine arrows end-to-end.
             */

            Arrow<A, C> result = new Arrow<A, C>(
                x => a2.Invoke(a1.Invoke(x))
                );

            return result;
        }

        public static Arrow<Tuple<A, C>, Tuple<B, C>> First<A, B, C>(this Arrow<A, B> arr)
        {
            /*
             * Takes an Arrow<A, B> and returns an arrow from a Tuple<A, C> to a Tuple<B, C> by
             * calling the original arrow function on the first element of the tuple.
             */

            return And(arr, new IDArrow<C>());
        }

        public static Arrow<Tuple<A, C>, Tuple<B, C>> First<A, B, C>(this Arrow<A, B> arr, C nullArgument)
        {
            /*
             * Defers to the standard First<A, B, C> implementation but takes an unused argument
             * of type C so that all three type parameters can be inferred and needn't be written
             * out in full by the programmer.
             */

            return First<A, B, C>(arr);
        }

        public static IArrow First<C>(this IArrow arr)
        {
            /*
             * A slightly messy way of simplifying First using the IArrow interface and determining
             * the types of the input arrow at runtime, so that it doesn't take three type
             * parameters.
             * Horrific code btw.
             */

            Type a = arr.a;
            Type b = arr.b;
            Type c = typeof(C);

            Type inputTupleType = typeof(Tuple<,>).MakeGenericType(a, c);
            Type outputTupleType = typeof(Tuple<,>).MakeGenericType(b, c);
            Type resultType = typeof(Arrow<,>).MakeGenericType(inputTupleType, outputTupleType);

            Type funcType = typeof(Func<,>).MakeGenericType(inputTupleType, outputTupleType);

            Type[] constructorTypes = new Type[1];
            constructorTypes[0] = funcType;
            ConstructorInfo constructor = resultType.GetConstructor(constructorTypes);

            dynamic[] parameters = new dynamic[1];
            parameters[0] = new Func<Tuple<dynamic, dynamic>, Tuple<dynamic, dynamic>>((Tuple<dynamic, dynamic> x) => new Tuple<dynamic, dynamic>(arr.Invoke(x.Item1), x.Item2));

            return (IArrow)constructor.Invoke(parameters);
        }

        public static Arrow<Tuple<C, A>, Tuple<C, B>> Second<A, B, C>(this Arrow<A, B> arr)
        {
            /*
             * Built upon First, but uses the Swap function above in combination with it to return
             * an arrow which runs the input arrow's function on the second argument of the tuple.
             */

            Arrow<Tuple<A, C>, Tuple<B, C>> fstArrow = First(arr, default(C));

            return new SwapArrow<C, A>()  // Swap the tuple
                .Combine(fstArrow)  // Run the function on the first element
                .Combine(new SwapArrow<B, C>());  // Swap the tuple back
        }

        public static Arrow<Tuple<C, A>, Tuple<C, B>> Second<A, B, C>(this Arrow<A, B> arr, C nullArgument)
        {
            /*
             * Similar to First<A, B, C>(arr, nullArgument) in that the nullArgument is used only
             * to determine the third type required so that the programmer needn't write out the
             * type parameters. Simply defers to the usual implementation of Second.
             */

            return Second<A, B, C>(arr);
        }

        public static Arrow<Tuple<A, C>, Tuple<B, D>> And<A, B, C, D>(this Arrow<A, B> a1, Arrow<C, D> a2)
        {
            /*
             * Used to create an arrow which is effectively running two normal arrows side-by-side
             * on an input Tuple.
             */

            // TODO: Make the parallel operator actually parallel?
            return new Arrow<Tuple<A, C>, Tuple<B, D>>(
                (Tuple<A, C> x) =>
                    new Tuple<B, D>(
                        a1.Invoke(x.Item1),
                        a2.Invoke(x.Item2)
                        )
                );
        }

        public static Arrow<A, Tuple<B, C>> Fanout<A, B, C>(this Arrow<A, B> a1, Arrow<A, C> a2)
        {
            /*
             * Applies the input to two arrows in parallel and gives the result as a tuple
             */

            return new Arrow<A, Tuple<B, C>>(
                (A x) =>
                    new Tuple<B, C>(
                        a1.Invoke(x),
                        a2.Invoke(x)
                        )
                );
        }

        public static Arrow<A, Tuple<A, A>> Split<A>()
        {
            /*
             * Returns an arrow which takes an input of type A and returns a Tuple<A, A> which is
             * the input duplicated
             */

            return new Arrow<A, Tuple<A, A>>(
                (A x) =>
                    new Tuple<A, A>(x, x)
                );
        }

        public static Arrow<Tuple<A, B>, C> Unsplit<A, B, C>(Func<A, B, C> op)
        {
            /*
             * Returns an arrow which takes an input of type Tuple<A, B> and applies the provided
             * operator, yielding an output of type C
             * ('Lifts' a binary operator to arrow status)
             */

            return new Arrow<Tuple<A, B>, C>(
                (Tuple<A, B> x) =>
                    op(x.Item1, x.Item2)
                );
        }

        public static Arrow<A, D> LiftA2<A, B, C, D>(Func<B, C, D> op, Arrow<A, B> a1, Arrow<A, C> a2)
        {
            return Split<A>()  // A -> split -> Tuple<A, A>
                .Combine(First(a1, default(A)))  // Tuple<A, A> -> a1 -> Tuple<B, A>
                .Combine(Second(a2, default(B)))  // Tuple<B, A> -> a2 -> Tuple<B, C>
                .Combine(Unsplit(op));  // Tuple<B, C> -> op(B, C) -> D
        }
    }
}
