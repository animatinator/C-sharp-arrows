using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace ArrowDataBinding.demos
{
    public interface ISimpleArrow
    {
        Type a { get; }
        Type b { get; }
    }

    public class SimpleArrow<T1, T2> : ISimpleArrow
    {
        private Type _a, _b;

        public Type a
        {
            get
            {
                return _a;
            }
            protected set
            {
                _a = value;
            }
        }
        public Type b
        {
            get
            {
                return _b;
            }
            protected set
            {
                _b = value;
            }
        }


        public Func<T1, T2> function { get; set; }
        public SimpleArrow(Func<T1, T2> function)
        {
            this.function = function;
            this.a = typeof(T1);
            this.b = typeof(T2);
        }

        public static SimpleArrow<T1, T2> arr(Func<T1, T2> function)
        {
            return null;
        }
    }

    public class LambdaCombinator
    {
        public static Func<T1, T3> CombineLambdas<T1, T2, T3>(Func<T1, T2> l1, Func<T2, T3> l2)
        {
            Func<T1, T3> result = x => l2(l1(x));
            //Expression<Func<T1, T2>> e1 = l1;
            return result;
        }

        public static ISimpleArrow Comb(ISimpleArrow a1, ISimpleArrow a2)
        {
            Type t1 = a1.a;
            Type t2 = a1.b;
            Type t3 = a2.b;

            // Create an appropriate type
            Type funcType = typeof(Func<,>).MakeGenericType(t1, t3);
            Type a1FuncType = typeof(Func<,>).MakeGenericType(t1, t2);
            Type a2FuncType = typeof(Func<,>).MakeGenericType(t2, t3);

            Type a1Type = a1.GetType();
            Type a2Type = a2.GetType();

            Type[] types = new Type[1];
            types[0] = funcType;

            // Get the constructor, genericised
            ConstructorInfo arrowConstructor = typeof(SimpleArrow<,>).MakeGenericType().GetConstructor(types);
            //dynamic result = arrowConstructor.Invoke(x => x);  Something like this now

            return null;
        }
    }
}
