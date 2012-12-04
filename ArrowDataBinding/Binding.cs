using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Arrows;

namespace ArrowDataBinding.Bindings
{
    public interface IBinding
    {
        // Just being used to refer to bindings in a typeless way for now
        // For instance, the binding handle struct uses this as it needs to take a parameter
        // but doesn't actually care what that parameter is, so it might as well be an untyped
        // binding.
        // TODO: Should the IBinding interface actually provide anything useful?
    }

    public class Binding<T1, T2> : IBinding
    {
        private Bindable source;
        private IArrow arrow;
        private Bindable destination;
        // TODO: Binding class
    }
}
