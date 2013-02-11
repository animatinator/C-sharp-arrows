using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ArrowDataBinding.Bindings
{
    public class BindingArgumentMarshaller
    {
        /*
         * Utility class which provides functions for marshalling and unmarshalling values going
         * into and coming out of arrows.
         * This is used by the BindingsManager class which has a list of BindPoint objects and
         * needs to pass them into arrows which take tree-structured tuples and then turn the tuple
         * result back into a list of values.
         */

        static Type[] tupleTypes = new Type[] {
            typeof(Tuple<>),  // This will be indexed with the number of arguments the tuple should have
                              // Zero is therefore unused and can be left as this
            typeof(Tuple<>),
            typeof(Tuple<,>),
            typeof(Tuple<,,>),
            typeof(Tuple<,,,>),
            typeof(Tuple<,,,,>),
            typeof(Tuple<,,,,,>),
            typeof(Tuple<,,,,,,>),
            typeof(Tuple<,,,,,,,>),
        };

        public static List<dynamic> UnmarshalArguments<T1, T2>(Tuple<T1, T2> input)
        {
            /*
             * Converts a tree-structured tuple into a list of the values it contains
             */

            List<dynamic> results = new List<dynamic>();

            results.AddRange(ProcessNodeUnmarshalling(input.Item1));
            results.AddRange(ProcessNodeUnmarshalling(input.Item2));

            return results;
        }

        public static List<dynamic> UnmarshalArguments<T>(T input)
        {
            return new List<dynamic> { input };
        }

        public static List<dynamic> ProcessNodeUnmarshalling(dynamic node)
        {
            /*
             * Takes a node and returns a list of elements contained beneath it
             * (Those belonging to child elements if the node is a tuple, and the node itself if
             * it is a leaf)
             */

            List<dynamic> results = new List<dynamic>();

            if (node.GetType().Name == typeof(Tuple<,>).Name)
            {
                results.AddRange(UnmarshalArguments(node));
            }
            else
            {
                results.Add(node);
            }

            return results;
        }

        public static dynamic FlatMarshalArguments(List<BindPoint> elements)
        {
            /*
             * A precursor to MarshalElements, kept in case flat marshalling is needed.
             * This marshals a list into a tuple of the same size (and is therefore limited to
             * seven elements at most)
             */

            int count = elements.Count;
            Type rawTupleType = tupleTypes[count];
            Type[] types = GetTypes(elements);
            Type tupleType = rawTupleType.MakeGenericType(types);
            var constructor = tupleType.GetConstructor(types);

            object[] args = new object[count];
            for (int i = 0; i < count; i++)
            {
                args[i] = GetValue(elements[i]);
            }

            var result = constructor.Invoke(args);

            //BEGIN SILLY TEST CODE WHICH MIGHT BE HANDY IN A BIT
            Type testType = tupleTypes[count].MakeGenericType(types);
            dynamic thingy = Convert.ChangeType(result, testType);
            var test = UnmarshalArguments(thingy);
            //END TEST CODE

            return result;
        }

        public static dynamic MarshalArguments(List<BindPoint> elements, Type tupleType)
        {
            /*
             * Takes a list of BindPoint objects and returns a tree-structured tuple containing
             * their values. The 'tupleSample' argument is used to supply a tuple with the desired
             * structure so the function knows what shape of tuple to create.
             */

            //TODO: MarshalArguments needs a way of ensuring the tuple is correctly sized for the number of elements

            // If the tupleSample is a tuple and not a leaf, recurse into its children
            if (tupleType.Name == typeof(Tuple<,>).Name)
            {
                return Tuple.Create(MarshalArguments(elements, tupleType.GetGenericArguments()[0]),
                    MarshalArguments(elements, tupleType.GetGenericArguments()[1]));
            }
            else  // If the tupleSample is a leaf, return one of the elements and remove it from the list
            {
                dynamic nextValue = GetValue(elements[0]);
                elements.RemoveAt(0);
                return nextValue;
            }
        }

        private static Type[] GetTypes(List<BindPoint> elements)
        {
            /*
             * Gets a list of the types represented by a list of BindPoint objects
             */

            int count = elements.Count;
            Type[] types = new Type[count];

            for (int i = 0; i < count; i++)
            {
                types[i] = GetValue(elements[i]).GetType();
            }

            return types;
        }

        private static dynamic GetValue(BindPoint point)
        {
            /*
             * Gets the value associated with a BindPoint
             * (This greatly simplifies the mess of calling Object.GetVariable<dynamic> everywhere)
             */

            return point.Object.GetVariable<dynamic>(point.Var);
        }
    }
}
