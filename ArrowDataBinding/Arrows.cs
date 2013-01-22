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


    // List processing arrows
    public class ListArrow<A, B> : Arrow<IEnumerable<A>, IEnumerable<B>>
    {
        /*
         * A base class for list arrows to allow them to be referred to and combined under a common
         * type (rather than having to either give combinations of list arrows a dynamic type or
         * type Arrow<IEnumerable<A>, IEnumerable<B>> - the list arrow programmer shouldn't need
         * to care about that).
         */
        public ListArrow(Func<IEnumerable<A>, IEnumerable<B>> listFunc)
            : base(listFunc)
        { }
    }

    public static class ListArrow
    {
        /*
         * Holds handy functions for creating list arrows without specifying type parameters.
         */
        public static FilterArrow<A> Filter<A>(Func<A, bool> predicate)
        {
            return new FilterArrow<A>(predicate);
        }

        public static MapArrow<A, B> Map<A, B>(Func<A, B> transformation)
        {
            return new MapArrow<A, B>(transformation);
        }

        // TODO: Can't do this for OrderByArrow as the below doesn't work - apparently lambdas
        // can't be dynamic or something?
        public static OrderByArrow<A> OrderBy<A>(dynamic comparison)
        {
            // Have to use a dynamic argument because it is a delegate type which depends on the
            // type parameter passed in - not possible as far as I know.
            return new OrderByArrow<A>(comparison);
        }
    }

    public class FilterArrow<A> : ListArrow<A, A>
    {
        /*
         * A utility arrow for quickly filtering an IEnumerable based on a predicate
         */
        public FilterArrow(Func<A, bool> predicate)
            : base((IEnumerable<A> list) => list.Where(predicate))
        { }
    }

    public class MapArrow<A, B> : ListArrow<A, B>
    {
        /*
         * Allows one IEnumerable<A> to be mapped to another with type B by way of a user-supplied
         * transformation func
         */
        public MapArrow(Func<A, B> transformation)
            : base((IEnumerable<A> list) => list.Select(transformation))
        { }
    }

    public class OrderByArrow<A> : ListArrow<A, A>
    {
        /*
         * Utility arrow for ordering an IEnumerable using a function passed in by the user.
         * Makes use of the FuncComparer class below.
         */
        public delegate int comparer(A a, A b);

        public OrderByArrow(comparer comparerFunc)
            : base((IEnumerable<A> list) =>
                list.OrderBy(
                    (x => x),
                    new FuncComparer<A>((Tuple<A, A> pair) =>
                        comparerFunc(pair.Item1, pair.Item2))).ToList())
        { }
    }

    public class FuncComparer<A> : IComparer<A>
    {
        /*
         * A utility comparer which allows an IComparer to be set up using a Func from a tuple to
         * an int. Originally written for the OrderByArrow.
         */
        private Func<Tuple<A, A>, int> comparerFunc;

        public FuncComparer(Func<Tuple<A, A>, int> comparerFunc)
        {
            this.comparerFunc = comparerFunc;
        }

        public int Compare(A x, A y)
        {
            return comparerFunc(Tuple.Create(x, y));
        }
    }
}
