using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
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
        protected BindPoint source;
        protected  Arrow<T1, T2> arrow;
        protected BindPoint destination;

        //public Binding(Bindable source, string sourceVar, Bindable dest, string destVar)
        public Binding(BindPoint source, Arrow<T1, T2> arrow, BindPoint dest)
        {
            this.source = source;
            this.destination = dest;
            this.arrow = arrow;

            SubscribeToBindable(source.Object);
        }

        public void SubscribeToBindable(Bindable bind)
        {
            bind.valueChanged += NotifyChange;
        }

        public void NotifyChange(object sourceObj, BindingEventArgs args)
        {
            if (IsSourceVar(sourceObj, args.VarName))
            {
                T1 sourceValue = source.Object.GetVariable<T1>(source.Var);
                T2 newValue = arrow.Invoke(sourceValue);
                // TODO: This bit should catch a locked variable exception; should probably be pulled out into a separate method
                destination.Object.LockAndSetVariable(destination.Var, newValue);
            }
        }


        public bool IsSourceVar(object obj, string varName)
        {
            return (obj.Equals(source.Object) && varName == source.Var);
        }

        public bool IsDestinationVar(object obj, string varName)
        {
            return (obj.Equals(destination.Object) && varName == destination.Var);
        }
    }

    public class TwoWayBinding<T1, T2> : Binding<T1, T2>
    {
        public TwoWayBinding(BindPoint source, InvertibleArrow<T1, T2> arrow, BindPoint destination)
            : base(source, arrow, destination)
        {
            // TODO: TwoWayBinding class
        }
    }
}
