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
        protected  IArrow arrow;
        protected BindPoint destination;

        //public Binding(Bindable source, string sourceVar, Bindable dest, string destVar)
        public Binding(BindPoint source, IArrow arrow, BindPoint dest)
        {
            this.source = source;
            this.destination = dest;

            SubscribeToBindable(source.Object);
        }

        public void SubscribeToBindable(Bindable bind)
        {
            bind.valueChanged += NotifyChange;
        }

        public void NotifyChange(object sourceObj, BindingEventArgs args)
        {
            // TODO: Finish Binding.NotifyChange() and neaten considerably

            if (sourceObj.Equals(this.source) && args.VarName == source.Var)
            {
                // First check whether the target variable is locked
                if (destination.Object.VariableLocked(destination.Var)) return;

                Type t1 = typeof(T1);
                PropertyInfo info = source.GetType().GetProperty(source.Var);
                T1 thingy = (T1)info.GetValue(destination, null);

                // Lock the variable (to avoid a recursive update once it has been set) and update
                destination.Object.LockVariable(destination.Var);
                destination.Object.SetVariable<T2>(destination.Var, (T2)source.GetType().GetProperty(source.Var).GetValue(source, null));
                destination.Object.UnlockVariable(destination.Var);
            }
        }
    }

    public class TwoWayBinding<T1, T2> : Binding<T1, T2>
    {
        public TwoWayBinding(BindPoint source, IInvertibleArrow arrow, BindPoint destination)
            : base(source, arrow, destination)
        {
            // TODO: TwoWayBinding class
        }
    }
}
