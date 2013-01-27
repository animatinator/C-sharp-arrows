using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ArrowDataBinding.Arrows;

namespace ArrowDataBinding.Bindings
{
    [Serializable]
    public class BindingTypeError : Exception
    {
        public BindingTypeError(Type t1, Type t2, Type arrowType)
            : base(String.Format("Type error: trying to create a binding from type {0} to type {1}"
                                +" with an arrow of type {2}", t1, t2, arrowType))
        { }
    }


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

            TypeCheck();
            SubscribeToBindable(source.Object);
        }

        public void TypeCheck()
        {
            Type t1 = source.Object.GetVariable<T1>(source.Var).GetType();
            Type t2 = destination.Object.GetVariable<T2>(destination.Var).GetType();

            if (typeof(T1).IsAssignableFrom(t1) && t2.IsAssignableFrom(typeof(T2)))
            {
                // All's good
            }
            else
            {
                //TODO: T2 comparison doesn't work
                //throw new BindingTypeError(typeof(T1), typeof(T1), arrow.GetType());
            }
        }

        public void SubscribeToBindable(Bindable bind)
        {
            bind.valueChanged += NotifyChange;
        }

        public virtual void NotifyChange(object sourceObj, BindingEventArgs args)
        {
            if (IsSourceVar(sourceObj, args.VarName))
            {
                T1 sourceValue = source.Object.GetVariable<T1>(source.Var);
                T2 newValue = arrow.Invoke(sourceValue);
                TrySetVariable(destination, newValue);
            }
        }


        public void TrySetVariable<T>(BindPoint bindPoint, T newValue)
        {
            try
            {
                bindPoint.Object.LockAndSetVariable(bindPoint.Var, newValue);
            }
            catch (VariableLockedException)
            {
                // Nothing to do here
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
        protected Arrow<T2, T1> reverseArrow;

        public TwoWayBinding(BindPoint source, InvertibleArrow<T1, T2> arrow, BindPoint destination)
            : base(source, arrow, destination)
        {
            this.reverseArrow = arrow.Invert();

            SubscribeToBindable(destination.Object);
        }

        public override void NotifyChange(object sourceObj, BindingEventArgs args)
        {
            base.NotifyChange(sourceObj, args);  // Will do the forward change if the changed
                                                // object is the binding source
            // Otherwise we check if it's the destination variable, and if so run the arrow in
            // reverse
            if (IsDestinationVar(sourceObj, args.VarName))
            {
                T2 sourceValue = destination.Object.GetVariable<T2>(destination.Var);
                T1 newValue = reverseArrow.Invoke(sourceValue);
                TrySetVariable(source, newValue);
            }
        }
    }
}
