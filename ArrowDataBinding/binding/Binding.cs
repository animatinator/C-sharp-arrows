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

        void Unbind();
        List<BindPoint> GetSources();
        List<BindPoint> GetDestinations();
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

        public virtual void Unbind()
        {
            UnsubscribeFromBindable(source.Object);
        }

        public virtual List<BindPoint> GetSources()
        {
            return new List<BindPoint> { source };
        }

        public virtual List<BindPoint> GetDestinations()
        {
            return new List<BindPoint> { destination };
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

        public void UnsubscribeFromBindable(Bindable bind)
        {
            bind.valueChanged -= NotifyChange;
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

        public override void Unbind()
        {
            base.Unbind();

            UnsubscribeFromBindable(destination.Object);
        }

        public override List<BindPoint> GetSources()
        {
            return base.GetSources().Concat(new List<BindPoint> {destination}).ToList();
        }

        public override List<BindPoint> GetDestinations()
        {
            return base.GetDestinations().Concat(new List<BindPoint> {source}).ToList();
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

        public MultiBinding(List<BindPoint> sources, Arrow<T1, T2> arrow, List<BindPoint> dests)
        {
            this.sources = sources;
            this.destinations = dests;
            this.arrow = arrow;

            SubscribeToSources();
        }

        public virtual void Unbind()
        {
            UnsubscribeFromSources();
        }

        public virtual List<BindPoint> GetSources()
        {
            return sources;
        }

        public virtual List<BindPoint> GetDestinations()
        {
            return destinations;
        }

        protected void SubscribeToSources()
        {
            foreach (BindPoint source in sources)
            {
                SubscribeToBindable(source.Object);
            }
        }

        protected void UnsubscribeFromSources()
        {
            foreach (BindPoint source in sources)
            {
                UnsubscribeFromBindable(source.Object);
            }
        }

        protected void SubscribeToBindable(Bindable bind)
        {
            bind.valueChanged += NotifyChange;
        }

        protected void UnsubscribeFromBindable(Bindable bind)
        {
            bind.valueChanged -= NotifyChange;
        }

        public virtual void NotifyChange(Bindable sourceObj, BindingEventArgs args)
        {
            if (IsSourceVar(sourceObj, args.VarName))
            {
                var arguments = new List<BindPoint>(sources);
                var sourcesTuple = BindingArgumentMarshaller.MarshalArguments(arguments, arrow.a);

                dynamic rawResults = arrow.Invoke(sourcesTuple);
                List<dynamic> results = BindingArgumentMarshaller.UnmarshalArguments(rawResults);

                UpdateDestinations(results);
            }
        }

        private void UpdateDestinations(List<dynamic> values)
        {
            for (int i = 0; i < destinations.Count; i++)
            {
                TrySetVariable(destinations[i], values[i]);
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

        public TwoWayMultiBinding(List<BindPoint> sources, InvertibleArrow<T1, T2> arrow, List<BindPoint> destinations)
            : base(sources, arrow, destinations)
        {
            this.reverseArrow = arrow.Invert();

            SubscribeToSources();
            SubscribeToDestinations();
        }

        public override void Unbind()
        {
            base.Unbind();

            UnsubscribeFromDestinations();
        }

        public override List<BindPoint> GetSources()
        {
            return base.GetSources().Concat(destinations).ToList();
        }

        public override List<BindPoint> GetDestinations()
        {
            return base.GetDestinations().Concat(sources).ToList();
        }

        private void SubscribeToDestinations()
        {
            foreach (BindPoint dest in destinations)
            {
                SubscribeToBindable(dest.Object);
            }
        }

        private void UnsubscribeFromDestinations()
        {
            foreach (BindPoint dest in destinations)
            {
                UnsubscribeFromBindable(dest.Object);
            }
        }

        public override void NotifyChange(Bindable sourceObj, BindingEventArgs args)
        {
            base.NotifyChange(sourceObj, args);  // Will do the forward change if the changed
                                                // object is the binding source
            // Otherwise we check if it's the destination variable, and if so run the arrow in
            // reverse
            if (IsDestinationVar(sourceObj, args.VarName))
            {
                var arguments = new List<BindPoint>(destinations);
                var destinationsTuple = BindingArgumentMarshaller.MarshalArguments(arguments, arrow.b);

                dynamic rawResults = reverseArrow.Invoke(destinationsTuple);
                List<dynamic> results = BindingArgumentMarshaller.UnmarshalArguments(rawResults);

                UpdateSources(results);
            }
        }

        private void UpdateSources(List<dynamic> values)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                TrySetVariable(sources[i], values[i]);
            }
        }
    }
}
