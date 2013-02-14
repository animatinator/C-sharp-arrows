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
    public class BindingConflictException : Exception
    {
        public BindingConflictException()
            : base("The binding you have attempted to add would cause a destination to be " +
                "reachable from a source by two paths, potentially leading to a conflict and " +
                "unpredictable behaviour.")
        { }
    }

    [Serializable]
    public class BindingEndpointCountException : Exception
    {
        public BindingEndpointCountException()
            : base("The number of sources or destinations is incorrect for the arrow you are " +
                "trying to bind with.")
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
        // TODO: Sodding unbind function ya nyaff
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
            /*
             * Creates a one-to-one binding from a source, an arrow and a destination. Infers
             * whether it should be bidirectional from the type of the supplied arrow.
             */

            if (LinkWouldCauseConflict(source, destination)) throw new BindingConflictException();

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
             * Creates a many-to-many binding from a list of sources, an arrow (on tuples) and a
             * list of destinations. Infers whether it should be bidirectional from the type of the
             * supplied arrow.
             * 
             * NOTE: Syntax for this constructor is simplified by the helper functions below,
             * leading to:
             * 
             *     CreateBinding(Sources(BindPoint(obj, var), BindPoint(obj', var')), arrow,
             *         Destinations(BindPoint(obj'', var'')))
             */

            if (MultiLinkWouldCauseConflict(sources, destinations)) throw new BindingConflictException();
            if (!ArgumentCountsCorrectForArrow(sources.Count(), destinations.Count(), arrow)) throw new BindingEndpointCountException();

            MultiBinding<A, B> result;

            if (arrow is InvertibleArrow<A, B>)
            {
                result = new TwoWayMultiBinding<A, B>(sources.ToList(), (InvertibleArrow<A, B>)arrow, destinations.ToList());
                UpdateBindingGraphBothWays(sources, destinations);
            }
            else
            {
                result = new MultiBinding<A,B>(sources.ToList(), arrow, destinations.ToList());
                UpdateBindingGraph(sources, destinations);
            }

            BindingHandle handle = new BindingHandle(result);
            bindings.Add(handle, result);

            return handle;
        }

        public static void Unbind(BindingHandle handle)
        {
            IBinding binding = bindings[handle];
            binding.Unbind();
            bindings.Remove(handle);
            // TODO: Remove from graph
        }

        public static void UpdateBindingGraph(BindPoint source, BindPoint destination)
        {
            bindGraph.AddMany(source, destination);
            bindGraph.Bind(source, destination);
        }

        public static void UpdateBindingGraph(BindPoint[] sources, BindPoint[] destinations)
        {
            bindGraph.AddMany(sources);
            bindGraph.AddMany(destinations);
            bindGraph.MultiBind(sources, destinations);
        }

        public static void UpdateBindingGraphBothWays(BindPoint a, BindPoint b)
        {
            UpdateBindingGraph(a, b);
            UpdateBindingGraph(b, a);
        }

        public static void UpdateBindingGraphBothWays(BindPoint[] sources, BindPoint[] destinations)
        {
            UpdateBindingGraph(sources, destinations);
            UpdateBindingGraph(destinations, sources);
        }

        public static bool LinkWouldCauseConflict(BindPoint a, BindPoint b)
        {
            BindingGraph tempGraph = bindGraph.Copy();
            tempGraph.Add(a);
            tempGraph.Add(b);
            tempGraph.Bind(a, b);
            return tempGraph.HasConflict();
        }

        public static bool MultiLinkWouldCauseConflict(BindPoint[] sources, BindPoint[] destinations)
        {
            BindingGraph tempGraph = bindGraph.Copy();
            tempGraph.AddMany(sources);
            tempGraph.AddMany(destinations);
            tempGraph.MultiBind(sources, destinations);
            return tempGraph.HasConflict();
        }

        public static bool ArgumentCountsCorrectForArrow(int sourceCount, int destinationCount, IArrow arrow)
        {
            /*
             * Checks the numbers of arguments being provided for the given arrow match the tuple
             * sizes expected
             */

            Type inputTupleType = arrow.a;
            Type outputTupleType = arrow.b;
            return (TupleOp.CountLeaves(inputTupleType) == sourceCount
                && TupleOp.CountLeaves(outputTupleType) == destinationCount);
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
