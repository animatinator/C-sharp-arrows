using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Bindings.Graph;
using ArrowDataBinding.Utils;

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
        private static BindingGraph bindGraph = new BindingGraph();

        public static BindingHandle CreateBinding<A, B>(Bindable source, string sourceProperty, Arrow<A, B> arrow, Bindable destination, string destinationProperty)
        {
            BindPoint sourcePoint = new BindPoint(source, sourceProperty);
            BindPoint destinationPoint = new BindPoint(destination, destinationProperty);

            return CreateBinding(sourcePoint, arrow, destinationPoint);
        }

        public static BindingHandle CreateBinding<A, B>(BindPoint source, Arrow<A, B> arrow, BindPoint destination)
        {
            if (LinkWouldCauseCycle(source, destination)) throw new BindingCycleException();
            // TODO: Check is a binding should overwrite an existing one

            Binding<A, B> result;

            if (arrow is InvertibleArrow<A, B>)
            {
                result = new TwoWayBinding<A, B>(source, (InvertibleArrow<A, B>)arrow, destination);
                UpdateBindingGraphBothWays(source, destination);
            }
            else
            {
                result = new Binding<A, B>(source, arrow, destination);
                UpdateBindingGraph(source, destination);
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

            // TODO: Cycle checking for multibinding

            MultiBinding<A, B> result;

            if (arrow is InvertibleArrow<A, B>)
            {
                result = new TwoWayMultiBinding<A, B>(sources.ToList(), (InvertibleArrow<A, B>)arrow, destinations.ToList());
                // TODO: Update binding graph for multibindings
            }
            else
            {
                result = new MultiBinding<A,B>(sources.ToList(), arrow, destinations.ToList());
                // As above
            }

            BindingHandle handle = new BindingHandle(result);
            bindings.Add(handle, result);

            return handle;
        }

        public static void UpdateBindingGraph(BindPoint source, BindPoint destination)
        {
            bindGraph.Add(source);
            bindGraph.Add(destination);
            bindGraph.Bind(source, destination);
        }

        public static void UpdateBindingGraphBothWays(BindPoint a, BindPoint b)
        {
            UpdateBindingGraph(a, b);
            UpdateBindingGraph(b, a);
        }

        public static bool LinkWouldCauseCycle(BindPoint a, BindPoint b)
        {
            BindingGraph tempGraph = bindGraph.Copy();
            tempGraph.Add(a);
            tempGraph.Add(b);
            tempGraph.Bind(a, b);
            return tempGraph.HasCycle();
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


    public static class BindingsManagerExtensionMethods
    {
        public static BindPoint GetBindPoint(this Bindable obj, string varName)
        {
            /*
             * Helper function for quickly creating BindPoint objects
             */

            return new BindPoint(obj, varName);
        }
    }
}
