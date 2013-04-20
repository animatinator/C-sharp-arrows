using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Arrows;

namespace ArrowDataBinding.Bindings
{
    public static class ExtensionMethods
    {
        public static ArrowValueConverter GetValueConverter(this IArrow arrow)
        {
            return new ArrowValueConverter(arrow);
        }

        // TODO: Need InvertibleArrowValueConverter implemented
        //public static InvertibleArrowValueConverter GetValueConverter(IInvertibleArrow arrow)
        //{
        //    return new InvertibleArrowValueConverter(arrow);
        //}
    }
}
