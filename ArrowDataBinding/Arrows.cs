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


    // A few general utility arrows

    public class IDArrow<A> : Arrow<A, A>
    {
        /*
         * An identity arrow
         */
        public IDArrow() : base(x => x) { }
    }

    public class SwapArrow<A, B> : Arrow<Tuple<A, B>, Tuple<B, A>>
    {
        /*
         * An arrow on pairs which returns a tuple which is the input with its components swapped
         */
        public SwapArrow()
            : base(
                (Tuple<A, B> x) =>
                    new Tuple<B, A>(
                        x.Item2,
                        x.Item1
                    )
                ) { }
    }

    public class AssocArrow<A, B, C> : Arrow<Tuple<Tuple<A, B>, C>, Tuple<A, Tuple<B, C>>>
    {
        /*
         * An arrow form of the Assoc function in Utils - saves the faff of creating one whenever
         * it's used
         */
        public AssocArrow()
            : base(
            (Tuple<Tuple<A, B>, C> x) =>
                TupleOp.Assoc(x)
            ) { }
    }

    public class CossaArrow<A, B, C> : Arrow<Tuple<A, Tuple<B, C>>, Tuple<Tuple<A, B>, C>>
    {
        /*
         * An arrow form of the Cossa function in Utils - saves the faff of creating one whenever
         * it's used
         */
        public CossaArrow()
            : base(
            (Tuple<A, Tuple<B, C>> x) =>
                TupleOp.Cossa(x)
            ) { }
    }
}
