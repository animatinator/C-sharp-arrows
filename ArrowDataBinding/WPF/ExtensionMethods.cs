using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Arrows;

namespace ArrowDataBinding.Bindings
{
    public static class ExtensionMethods
    {
        public static ArrowValueConverter<A, B> GetValueConverter<A, B>(Arrow<A, B> arrow)
        {
            return new ArrowValueConverter<A, B>(arrow);
        }

        public static InvertibleArrowValueConverter<A, B> GetValueConverter<A, B>(InvertibleArrow<A, B> arrow)
        {
            return new InvertibleArrowValueConverter<A, B>(arrow);
        }
    }
}
