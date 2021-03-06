﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ArrowDataBinding.Utils
{
    public class Either<A, B>
    {
        /*
         * To be used for conditional arrows - stores either an object of type A or one of type B;
         * which type is stored is indicated by the Type variable
         */

        private A _a;
        public A ItemA
        {
            get
            {
                return _a;
            }
        }

        private B _b;
        public B ItemB
        {
            get
            {
                return _b;
            }
        }

        public Type type { get; set; }


        public Either(A value)
        {
            this.type = typeof(A);

            this._a = value;
            this._b = default(B);
        }

        public Either(B value)
        {
            this.type = typeof(B);

            this._a = default(A);
            this._b = value;
        }
    }

    public static class TupleOp
    {
        public static Tuple<A, Tuple<B, C>> Assoc<A, B, C>(this Tuple<Tuple<A, B>, C> input)
        {
            return Tuple.Create(input.Item1.Item1, Tuple.Create(input.Item1.Item2, input.Item2));
        }

        public static Tuple<Tuple<A, B>, C> Cossa<A, B, C>(this Tuple<A, Tuple<B, C>> input)
        {
            return Tuple.Create(Tuple.Create(input.Item1, input.Item2.Item1), input.Item2.Item2);
        }

        public static int CountLeaves(Type tupleType)
        {
            /*
             * Treating the tuple as a binary tree structure, return the number of leaves it
             * contains
             */

            if (tupleType.Name == typeof(Tuple<,>).Name)
            {
                return CountLeaves(tupleType.GetGenericArguments()[0])
                    + CountLeaves(tupleType.GetGenericArguments()[1]);
            }
            else
            {
                return 1;
            }
        }

        public static dynamic ConvertType(dynamic tuple, Type requiredType)
        {
            if (tuple.GetType().Name == typeof(Tuple<,>).Name)
            {
                var types = requiredType.GenericTypeArguments;
                var constructor = requiredType.GetConstructor(types);

                dynamic left = ConvertType(tuple.Item1, types[0]);
                dynamic right = ConvertType(tuple.Item2, types[1]);

                return constructor.Invoke(new object[] { left, right });
            }
            else
            {
                return Convert.ChangeType(tuple, requiredType);
            }
        }
    }

    public static class ExtensionMethods
    {
        // Deep clone
        public static T DeepClone<T>(this T a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }

        public static Func<Tuple<A, C>, Tuple<B, D>> Mult<A, B, C, D>(this Func<A, B> f, Func<C, D> g)
        {
            return new Func<Tuple<A, C>, Tuple<B, D>>(x => Tuple.Create(f(x.Item1), g(x.Item2)));
        }
    }
}
