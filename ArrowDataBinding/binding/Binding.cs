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
    }

    public class Binding<T1, T2> : IBinding
    {
        protected BindPoint source;
        protected Arrow<T1, T2> arrow;
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

    public class MultiBinding<T1, T2> : IBinding
    {
        protected List<BindPoint> sources;
        protected Arrow<T1, T2> arrow;
        protected List<BindPoint> destinations;

        //public Binding(Bindable source, string sourceVar, Bindable dest, string destVar)
        public MultiBinding(List<BindPoint> sources, Arrow<T1, T2> arrow, List<BindPoint> dests)
        {
            this.sources = sources;
            this.destinations = dests;
            this.arrow = arrow;

            SubscribeToSources();
        }

        // TODO: Typecheck bindings
        //public void TypeCheck()
        //{
        //    Type t1 = source.Object.GetVariable<T1>(source.Var).GetType();
        //    Type t2 = destination.Object.GetVariable<T2>(destination.Var).GetType();

        //    if (typeof(T1).IsAssignableFrom(t1) && t2.IsAssignableFrom(typeof(T2)))
        //    {
        //        // All's good
        //    }
        //    else
        //    {
        //        //TODO: T2 comparison doesn't work
        //        //throw new BindingTypeError(typeof(T1), typeof(T1), arrow.GetType());
        //    }
        //}

        public void SubscribeToSources()
        {
            foreach (BindPoint source in sources)
            {
                SubscribeToBindable(source.Object);
            }
        }

        public void SubscribeToBindable(Bindable bind)
        {
            bind.valueChanged += NotifyChange;
        }

        public virtual void NotifyChange(Bindable sourceObj, BindingEventArgs args)
        {
            if (IsSourceVar(sourceObj, args.VarName))
            {
                // TODO: Need to marshall source values into a tuple and pass them in
                //T1 sourceValue = source.Object.GetVariable<T1>(source.Var);
                //T2 newValue = arrow.Invoke(sourceValue);
                // TODO: Now need to unmarshall them and put them in the destinations
                //TrySetVariable(destination, newValue);
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


        public bool IsSourceVar(Bindable obj, string varName)
        {
            return BindPointInList(new BindPoint((Bindable)obj, varName), sources);
        }

        public bool IsDestinationVar(Bindable obj, string varName)
        {
            return BindPointInList(new BindPoint((Bindable)obj, varName), destinations);
        }

        private bool BindPointInList(BindPoint bindPoint, List<BindPoint> list)
        {
            bool isInList = false;

            foreach (BindPoint point in list)
            {
                isInList = isInList || (bindPoint.Object.Equals(point.Object) && bindPoint.Var == point.Var);
            }

            return isInList;
        }
    }

    public class TwoWayMultiBinding<T1, T2> : MultiBinding<T1, T2>
    {
        protected Arrow<T2, T1> reverseArrow;

        public TwoWayMultiBinding(BindPoint source, InvertibleArrow<T1, T2> arrow, BindPoint destination)
            : base(new List<BindPoint> {source}, arrow, new List<BindPoint> {destination})
        {
            this.reverseArrow = arrow.Invert();

            SubscribeToBindable(destination.Object);
        }

        public override void NotifyChange(Bindable sourceObj, BindingEventArgs args)
        {
            base.NotifyChange(sourceObj, args);  // Will do the forward change if the changed
                                                // object is the binding source
            // Otherwise we check if it's the destination variable, and if so run the arrow in
            // reverse
            if (IsDestinationVar(sourceObj, args.VarName))
            {
                //T2 sourceValue = destination.Object.GetVariable<T2>(destination.Var);
                //T1 newValue = reverseArrow.Invoke(sourceValue);
                //TrySetVariable(source, newValue);
            }
        }
    }
}
