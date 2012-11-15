using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Utils;

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


    public interface IInvertibleArrow
    {
        //IInvertibleArrow Invert();  // TODO: Don't know how to implement this; covariant return types aren't allowed
        dynamic Invoke<T>(T var);
    }


    public class Arrow<A, B> : IArrow
    {
        protected Func<A, B> func;

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
    }


    public class ArrowChoice<A, B, C, D> : Arrow<Either<A, B>, Either<C, D>>
    {
        /*
         * An arrow which takes an input of either type and returns an output of the associated
         * type.
         * TODO: Finish ArrowChoice and get it working
         */

        public ArrowChoice(Func<A, C> func1, Func<B, D> func2) : base(default(Func<Either<A, B>, Either<C, D>>))
        {
            func = new Func<Either<A, B>, Either<C, D>>(x =>
                {
                    if (x.type == typeof(A))
                    {
                        return new Either<C, D>(func1(x.ItemA));
                    }
                    else
                    {
                        return new Either<C, D>(func2(x.ItemB));
                    }
                }
            );
        }

        public C Invoke(A input)
        {
            Either<C, D> result = func(new Either<A, B>(input));
            return default(C);
        }

        public D Invoke(B input)
        {
            Either<C, D> result = func(new Either<A, B>(input));
            return default(D);
        }
    }


    public class InvertibleArrow<A, B> : Arrow<A, B>, IInvertibleArrow
    {
        protected Func<B, A> inverseFunc;

        public InvertibleArrow(Func<A, B> func, Func<B, A> inverseFunc)
            : base(func)
        {
            this.inverseFunc = inverseFunc;
        }

        public InvertibleArrow<B, A> Invert()
        {
            return new InvertibleArrow<B, A>(inverseFunc, func);
        }
    }


    public static class Op
    {
        // TODO: Identity arrow. See the note above the Swap function - basically, would be cool
        // if one could define an identity arrow without needing to specify what concrete type it
        // should work on.


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
        
        public static Arrow<A, B> Arr<A, B>(this Func<A, B> func)
        {
            /*
             * Basic arrow construction operator from a Func<A, B>
             */

            return new Arrow<A, B>(func);
        }

        public static InvertibleArrow<A, B> Arr<A, B>(this Func<A, B> func, Func<B, A> invFunc)
        {
            return new InvertibleArrow<A, B>(func, invFunc);
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

        public static InvertibleArrow<A, C> Combine<A, B, C>(this InvertibleArrow<A, B> a1, InvertibleArrow<B, C> a2)
        {
            InvertibleArrow<A, C> result = new InvertibleArrow<A, C>(
                x => a2.Invoke(a1.Invoke(x)),
                x => a1.Invert().Invoke(a2.Invert().Invoke(x))  // TODO: Make InvertibleArrow combinator more efficient
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

        public static InvertibleArrow<Tuple<A, C>, Tuple<B, C>> First<A, B, C>(this InvertibleArrow<A, B> arr)
        {
            return new InvertibleArrow<Tuple<A, C>, Tuple<B, C>>(
                (Tuple<A, C> x) =>
                    new Tuple<B, C>(
                        arr.Invoke(x.Item1),
                        x.Item2
                        ),
                (Tuple<B, C> x) =>
                    new Tuple<A, C>(
                        arr.Invert().Invoke(x.Item1),
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

            // TODO: Make a version of first/second which only takes one type parameter
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
                .Combine(First<A, B, A>(a1))  // Tuple<A, A> -> a1 -> Tuple<B, A>
                .Combine(Second<A, C, B>(a2))  // Tuple<B, A> -> a2 -> Tuple<B, C>
                .Combine(Unsplit(op));  // Tuple<B, C> -> op(B, C) -> D
        }


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

        public static ArrowChoice<A, B, C, C> FanInChoice<A, B, C>(Arrow<A, C> ar1, Arrow<B, C> ar2)
        {
            //TODO: How to make this so it can plug arrowchoices into normal arrows?
            return null;
        }
    }
}
