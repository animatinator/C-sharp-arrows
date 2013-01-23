using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Arrows;

namespace ArrowDataBinding.Combinators
{
    public static partial class Op
    {
        public static ListArrow<A, C> Combine<A, B, C>(this ListArrow<A, B> a1, ListArrow<B, C> a2)
        {
            /*
             * Combine arrows end-to-end. This is an overloaded version of the normal Arrow.Combine
             * method used to prevent ListArrows being prematurely converted into arrows between
             * IEnumerables.
             */

            ListArrow<A, C> result = new ListArrow<A, C>(
                x => a2.Invoke(a1.Invoke(x))
                );

            return result;
        }

        public static ListArrow<A, B> Filter<A, B>(this ListArrow<A, B> listArrow, Func<B, bool> predicate)
        {
            /*
             * Append a FilterArrow to the end of the supplied ListArrow using the predicate passed
             * in.
             */

            FilterArrow<B> filterArrow = ListArrow.Filter(predicate);

            return listArrow.Combine(filterArrow);
        }

        public static ListArrow<A, C> Map<A, B, C>(this ListArrow<A, B> listArrow, Func<B, C> transformation)
        {
            /*
             * Append a MapArrow to the end of the supplied ListArrow using the transformation Func
             * passed in.
             */

            MapArrow<B, C> mapArrow = ListArrow.Map(transformation);

            return listArrow.Combine(mapArrow);
        }

        public static ListArrow<A, B> OrderBy<A, B>(this ListArrow<A, B> listArrow, Func<B, B, int> comparer)
        {
            OrderByArrow<B> orderByArrow = ListArrow.OrderBy(comparer);

            return listArrow.Combine(orderByArrow);
        }
    }
}
