using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
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

            return arr.Second<A, B, C>();
        }

        public static Arrow<Tuple<A, C>, Tuple<B, D>> And<A, B, C, D>(this Arrow<A, B> a1, Arrow<C, D> a2)
        {
            /*
             * Used to create an arrow which is effectively running two normal arrows side-by-side
             * on an input Tuple.
             */

            return new Arrow<Tuple<A, C>, Tuple<B, D>>(
                (Tuple<A, C> x) =>
                    {
                        B leftResult = default(B);
                        D rightResult = default(D);

                        Parallel.Invoke(
                            () => leftResult = a1.Invoke(x.Item1),
                            () => rightResult = a2.Invoke(x.Item2));

                        return new Tuple<B, D>(leftResult, rightResult);
                    }
                );
        }

        public static Arrow<A, Tuple<C, D>> Fanout<A, B, C, D>(this Arrow<A, B> input, Arrow<B, C> a1, Arrow<B, D> a2)
        {
            /*
             * Applies the input to two arrows in parallel and gives the result as a tuple
             */

            // TODOL Check Fanout works for invertible arrows
            return input.Split().Combine(a1.And(a2));
        }

        public static Arrow<A, Tuple<A, A>> Split<A>()
        {
            /*
             * Returns an arrow which takes an input of type A and returns a Tuple<A, A> which is
             * the input duplicated.
             */

            return Op.Arr((A x) => Tuple.Create(x, x));
        }

        public static Arrow<A, Tuple<B, B>> Split<A, B>(this Arrow<A, B> arr)
        {
            /*
             * Takes an arrow from A to B and returns one with its output duplicated into a tuple
             */

            Arrow<B, Tuple<B, B>> splitArrow = Split<B>();

            return arr.Combine(splitArrow);
        }

        public static Arrow<Tuple<A, B>, C> Unsplit<A, B, C>(Func<A, B, C> op)
        {
            /*
             * Returns an arrow which takes an input of type Tuple<A, B> and applies the provided
             * operator, yielding an output of type C
             * ('Lifts' a binary operator to arrow status)
             * Can't be written for invertible arrows as the operation will naturally lose
             * information which can't be recovered in the other direction.
             */

            return new Arrow<Tuple<A, B>, C>(
                (Tuple<A, B> x) =>
                    op(x.Item1, x.Item2)
                );
        }

        public static Arrow<A, D> Unsplit<A, B, C, D>(this Arrow<A, Tuple<B, C>> arr, Func<B, C, D> op)
        {
            /*
             * Extension method version of Unsplit
             */

            Arrow<Tuple<B, C>, D> unsplitArrow = Unsplit(op);

            return arr.Combine(unsplitArrow);
        }

        public static Arrow<A, D> LiftA2<A, B, C, D>(Func<B, C, D> op, Arrow<A, B> a1, Arrow<A, C> a2)
        {
            /*
             * Applies two arrows in parallel on the same input and then uses the supplied binary
             * operator to give a single output from the two results.
             */

            return Split<A>().Combine(a1.And(a2)).Combine(Unsplit(op));
        }
    }
}
