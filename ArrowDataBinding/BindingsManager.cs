using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Arrows;

namespace ArrowDataBinding.Bindings
{
    public struct BindingHandle
    {
        private Guid id;

        public BindingHandle()
        {
            id = Guid.NewGuid();
        }

        public override bool Equals(BindingHandle other)
        {
            return id == other.id;
        }

        public static bool operator==(BindingHandle h1, BindingHandle h2)
        {
            return (h1.Equals(h2));
        }
    }


    public class BindingsManager
    {
        public static BindingHandle CreateBinding(object source, string sourceProperty, IArrow arrow, object dest, string destProperty)
        {
            // TODO: BindingsManager
            return new BindingHandle();
        }
    }
}
