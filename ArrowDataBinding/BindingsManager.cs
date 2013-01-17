using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using ArrowDataBinding.Arrows;

namespace ArrowDataBinding.Bindings
{
    [Serializable]
    public class BindingCycleException : Exception
    {
        public BindingCycleException()
            : base("The binding you have attempted to add would lead to a binding cycle which would " +
            "cause unpredictable behaviour.")
        { }
    }

    public struct BindingHandle
    {
        private Guid id;

        public BindingHandle(IBinding source)  // Struct constructors can't be parameterless
        {
            id = Guid.NewGuid();
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override bool Equals(Object o)
        {
            if (o is BindingHandle)
            {
                BindingHandle otherBinding = (BindingHandle)o;
                return Equals(id, otherBinding.id);
            }
            
            else return false;
        }

        public static bool operator==(BindingHandle h1, BindingHandle h2)
        {
            return (h1.Equals(h2));
        }

        public static bool operator !=(BindingHandle h1, BindingHandle h2)
        {
            return !(h1 == h2);
        }
    }


    public class BindingsManager
    {
        private static Dictionary<BindingHandle, IBinding> bindings = new Dictionary<BindingHandle,IBinding>();

        public static BindingHandle CreateBinding<A, B>(Bindable source, string sourceProperty, Arrow<A, B> arrow, Bindable destination, string destinationProperty)
        {
            BindPoint sourcePoint = new BindPoint(source, sourceProperty);
            BindPoint destinationPoint = new BindPoint(destination, destinationProperty);

            return CreateBinding(sourcePoint, arrow, destinationPoint);
        }

        public static BindingHandle CreateBinding<A, B>(BindPoint source, Arrow<A, B> arrow, BindPoint destination)
        {
            if (LinkWouldCauseCycle(source, destination)) throw new BindingCycleException();

            Binding<A, B> result;

            if (arrow is InvertibleArrow<A, B>)
            {
                result = new TwoWayBinding<A, B>(source, (InvertibleArrow<A, B>)arrow, destination);
            }
            else
            {
                result = new Binding<A, B>(source, arrow, destination);
            }

            BindingHandle handle = new BindingHandle(result);
            bindings.Add(handle, result);

            return handle;
        }

        public static BindingHandle CreateBinding<A, B>(BindPoint[] sources, Arrow<A, B> arrow, BindPoint[] destinations)
        {
            /*
             * Blah... (Description goes here)
             * 
             * NOTE: Syntax for this constructor is simplified by the helper functions below,
             * leading to:
             * 
             *     CreateBinding(Sources(BindPoint(obj, var), BindPoint(obj', var')), arrow,
             *         Destinations(BindPoint(obj'', var'')))
             */

            // TODO: multibinding constructor
            return new BindingHandle();
        }

        public static bool LinkWouldCauseCycle(BindPoint a, BindPoint b)
        {
            // TODO: Cycle checking code
            return false;
        }


        public static BindPoint BindPoint(Bindable obj, string varName)
        {
            /*
             * Helper function for quickly creating BindPoint objects
             */

            return new BindPoint(obj, varName);
        }

        public static BindPoint[] BindPoints(params BindPoint[] parameters)
        {
            /*
             * Helper function for quickly creating an array of BindPoints
             */

            return parameters;
        }

        public static BindPoint[] Sources(params BindPoint[] sources)
        {
            /*
             * Makes binding-creation syntax a bit cleaner
             */

            return BindPoints(sources);
        }

        public static BindPoint[] Destinations(params BindPoint[] dests)
        {
            /*
             * Similar to Sources
             */

            return BindPoints(dests);
        }
    }
}
