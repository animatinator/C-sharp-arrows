using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ArrowDataBinding.Bindings
{
    class BindingArgumentMarshaller
    {
        static Type[] tupleTypes = new Type[] {
            typeof(Tuple<>),
            typeof(Tuple<>),
            typeof(Tuple<,>),
            typeof(Tuple<,,>),
            typeof(Tuple<,,,>),
            typeof(Tuple<,,,,>),
            typeof(Tuple<,,,,,>),
            typeof(Tuple<,,,,,,>),
            typeof(Tuple<,,,,,,,>),
        };

        public static void DFS<T1, T2>(Tuple<T1, T2> input)
        {
            dynamic left = input.Item1;
            dynamic right = input.Item2;

            if (left.GetType().Name == typeof(Tuple<,>).Name)
            {
                DFS(left);
            }
            else
            {
                Console.WriteLine(left);
            }

            if (right.GetType().Name == typeof(Tuple<,>).Name)
            {
                DFS(right);
            }
            else
            {
                Console.WriteLine(right);
            }
        }

        public static dynamic MarshalArguments(List<BindPoint> elements)
        {
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
            Test(thingy);
            //END TEST CODE

            return result;
        }

        private static void Test<T1, T2>(Tuple<T1, T2> input)
        {
            Console.WriteLine(typeof(T1));
        }

        private static Type[] GetTypes(List<BindPoint> elements)
        {
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
            return point.Object.GetVariable<dynamic>(point.Var);
        }
    }
}
