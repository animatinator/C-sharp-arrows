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
            // Get the three generic types
            Type t1 = a1.a;
            Type t2 = a1.b;
            Type t3 = a2.b;

            // Create types for the combined function and the two source functions
            Type funcType = typeof(Func<,>).MakeGenericType(t1, t3);
            Type a1FuncType = typeof(Func<,>).MakeGenericType(t1, t2);
            Type a2FuncType = typeof(Func<,>).MakeGenericType(t2, t3);

            // Get the types of the two arrows
            Type a1Type = a1.GetType();
            Type a2Type = a2.GetType();

            // Get an arrow constructor, genericised
            Type[] arrowConstructorTypes = new Type[1];
            arrowConstructorTypes[0] = funcType;
            ConstructorInfo arrowConstructor = typeof(SimpleArrow<,>).MakeGenericType(t1, t3).GetConstructor(arrowConstructorTypes);

            // Get the two arrows' lambda functions (type safety is cast asunder from here on...)
            dynamic ar1 = a1;
            dynamic ar2 = a2;
            dynamic l1 = ar1.function;
            dynamic l2 = ar2.function;

            // Get a genericised version of the CombineLambdas function
            Type[] types2 = new Type[3];
            types2[0] = t1;
            types2[1] = t2;
            types2[2] = t3;
            MethodInfo combinator = typeof(LambdaCombinator).GetMethod("CombineLambdas");
            combinator = combinator.MakeGenericMethod(types2);

            // Invoke the genericised CombineLambdas function on the extracted lambda functions
            dynamic[] combineArgs = new dynamic[2];
            combineArgs[0] = l1;
            combineArgs[1] = l2;
            dynamic combinedFunc = combinator.Invoke(null, combineArgs);

            // Invoke the genericised arrow constructor on the result of the CombineLambdas function
            dynamic[] parameters = new dynamic[1];
            parameters[0] = combinedFunc;
            dynamic result = arrowConstructor.Invoke(parameters);

            return result;
        }
    }
}
