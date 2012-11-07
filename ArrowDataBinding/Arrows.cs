using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArrowDataBinding.Arrows
{
    public abstract class IArrow
    {
        protected Type _a;
        public Type a
        {
            get
            {
                return this._a;
            }
        }

        protected Type _b;
        public Type b
        {
            get
            {
                return this._b;
            }
        }

        public abstract dynamic Invoke<T>(T var);
    }

    public class Arrow<A, B> : IArrow
    {
        private Func<A, B> func;

        public Arrow(Func<A, B> expr)
        {
            this.func = expr;
            this._a = typeof(A);
            this._b = typeof(B);
        }


        public B Invoke(A input)
        {
            return func(input);
        }

        public override dynamic Invoke<T>(T input)
        {
            if (typeof(T) == typeof(A))
            {
                return func((A)Convert.ChangeType(input, typeof(A)));
            }
            else
            {
                throw new Exception("Invalid type supplied to 'Arrow.Invoke'!");
            }
        }


        // TODO: Stop putting random test functions in the arrow class
        public static void blah()
        {
            Arrow<int, string> test = Op.Arr((int x) => x + 1).Combine(Op.Arr((int x) => x.ToString()));
            Arrow<int, string> test1 = Op.Combine(Op.Arr((int x) => x + 1), Op.Arr((int x) => x.ToString()));
        }

        public static void blahblah()
        {
            Arrow<int, double> pythagoras = Op.LiftA2((int x, int y) => x + y,
                    Op.Arr((int x) => x * x),
                    Op.Arr((int y) => y * y))
                .Combine(Op.Arr((int x) => Math.Sqrt(x)));
        }
    }


    public static class Op
    {
        // TODO: Identity arrow. See the note above the Swap function - basically, would be cool
        // if one could define an identity arrow without needing to specify what concrete type it
        // should work on.


        public static Arrow<A, B> Arr<A, B>(Func<A, B> func)
        {
            /*
             * Basic arrow construction operator from a Func<A, B>
             */

            return new Arrow<A, B>(func);
        }

        public static Arrow<A, C> Combine<A, B, C>(this Arrow<A, B> a1, Arrow<B, C> a2)
        {
            /*
             * Combine arrows end-to-end (defined in the Arrow class itself for now)
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

            return new Arrow<Tuple<A, C>, Tuple<B, C>>(
                (Tuple<A, C> x) =>
                    new Tuple<B, C>(
                        arr.Invoke(x.Item1),
                        x.Item2
                        )
                );
        }

        // TODO: In the following code, a function is used to generate an arrow whose concrete
        // types don't matter and are decided on usage. Other examples of this sort of thing
        // exist - for instance, an identity arrow which simply returns the input needn't know the
        // type of that input. Could there be a way of defining arrows like these without the hack
        // of writing a function to return a concretely typed arrow?
        public static Arrow<Tuple<A, B>, Tuple<B, A>> Swap<A, B>()
        {
            /*
             * Returns an arrow which takes as input a Tuple<A, B> and returns a Tuple<B, A> which
             * is the input with its values swapped.
             * Used by Second.
             */

            return new Arrow<Tuple<A, B>, Tuple<B, A>>(
                (Tuple<A, B> x) =>
                    new Tuple<B, A> (
                        x.Item2,
                        x.Item1
                        )
                );
        }

        public static Arrow<Tuple<C, A>, Tuple<C, B>> Second<A, B, C>(this Arrow<A, B> arr)
        {
            /*
             * Built upon First, but uses the Swap function above in combination with it to return
             * an arrow which runs the input arrow's function on the second argument of the tuple.
             */

            Arrow<Tuple<A, C>, Tuple<B, C>> fstArrow = First<A, B, C>(arr);

            return Swap<C, A>()  // Swap the tuple
                .Combine(fstArrow)  // Run the function on the first element
                .Combine(Swap<B, C>());  // Swap the tuple back
        }

        public static Arrow<Tuple<A, C>, Tuple<B, D>> And<A, B, C, D>(this Arrow<A, B> a1, Arrow<C, D> a2)
        {
            /*
             * Used to create an arrow which is effectively running two normal arrows side-by-side
             * on an input Tuple.
             */

            // TODO: Make this parallel?
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
            return new Arrow<A, Tuple<A, A>>(
                (A x) =>
                    new Tuple<A, A>(x, x)
                );
        }

        public static Arrow<Tuple<A, B>, C> Unsplit<A, B, C>(Func<A, B, C> op)
        {
            return new Arrow<Tuple<A, B>, C>(
                (Tuple<A, B> x) =>
                    op(x.Item1, x.Item2)
                );
        }

        public static Arrow<A, D> LiftA2<A, B, C, D>(Func<B, C, D> op, Arrow<A, B> a1, Arrow<A, C> a2)
        {
            return Split<A>().Combine(First<A, B, A>(a1)).Combine(Second<A, C, B>(a2)).Combine(Unsplit(op));
        }
    }
}
