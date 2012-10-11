using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ArrowTestThing
{
    interface IArrow
    {
        /*
         * This could be used to allow arrow combinators to combine an arrow with another arrow of
         * unknown type (currently used in the signature of the composition function)
         */
    }

    class Arrow<T1, T2> : IArrow
    {
        /*
         * Arrow class - holds a function from T1 to T2 and can be called to execute said function.
         * Currently a mess as I was trying it with a pair of inputs as well; will ultimately take
         * one input (which could be a pair) and produce one output rather than having a simple
         * function and a pair function.
         * Also, currently has no way of combining it with other arrows - standard arrow functions
         * will be added later. Is essentially just a lambda term in a box for now.
         */

        public delegate T2 func(T1 input);  // The function on one input's type
        public delegate T2 func2(T1 in1, T1 in2);  // The type of the function on two
        private func theFunc;
        private func2 thePairFunc;

        public Arrow(func theFunction)
        {
            theFunc = theFunction;
        }

        public Arrow(func2 thePairFunction)
        {
            thePairFunc = thePairFunction;
        }

        public T2 Execute(T1 input)
        {
            return theFunc(input);
        }

        public T2 ExecutePair(T1 in1, T1 in2)
        {
            return thePairFunc(in1, in2);
        }

        public static IArrow operator+(Arrow<T1, T2> first, IArrow second)
        {
            // Compose arrows - not inplemented yet
            Console.WriteLine("Combining arrows!");
            return null;
        }
    }

    class BindingSource<T>
    {
        /*
         * The source variable for a binding.
         * The actual value is accessed through the Value property.
         */

        private T _val;
        public T Value
        {
            get
            {
                return _val;
            }

            set  // Set is overridden so that it also notifies the attached Binding
            {  // This way the Binding can update automatically whenever the value is changed
                _val = value;

                if (BindingObject != null)
                {
                    BindingObject.NotifyChange();
                }
            }
        }

        // The Binding object to which this source is bound
        public IBinding<T> BindingObject
        {
            get;
            set;
        }

        public BindingSource(T val)
        {
            Value = val;
        }

        public BindingSource(BindingSource<T> other)
        {
            Value = other.Value;
        }
    }

    class BindingDestination<T>
    {
        /*
         * The destination variable for a binding. As with sources, the value is accessed through
         * the Value property. Implicit conversions are implemented for BindingDestination->T and
         * vice versa so that a BindingDestination<T> object can be used in assignments as though
         * a T object.
         */

        public T Value { get; set; }

        public BindingDestination(T val)
        {
            Value = val;
        }

        public static implicit operator BindingDestination<T>(T val)
        {
            return new BindingDestination<T>(val);
        }

        public static implicit operator T(BindingDestination<T> bd)
        {
            return bd.Value;
        }
    }

    interface IBinding<T>
    {
        /*
         * An interface for the Binding class to inherit, used by binding sources - sources
         * only know their own type and not the type of the value at the other end of the
         * binding, and so this interface is used to allow sources to refer to Binding<T1, T2>
         * objects without knowing T2.
         */

        void NotifyChange();
    }

    class Binding<T1, T2> : IBinding<T1>
    {
        /*
         * Stores a binding consisting of a BindingSource, a BindingDestination and an Arrow to
         * carry through the data.
         */

        protected BindingSource<T1> source;
        protected Arrow<T1, T2> arrow;
        protected BindingDestination<T2> destination;

        public Binding(BindingSource<T1> theSource, Arrow<T1, T2> theArrow, BindingDestination<T2> destination)
        {
            source = theSource;
            source.BindingObject = this;
            arrow = theArrow;
            this.destination = destination;
        }

        public virtual void NotifyChange()
        {
            destination.Value = arrow.Execute(source.Value);
        }
    }

    class PairBinding<T1, T2> : Binding<T1, T2>  // Should be made generic with Binding; see comment in class Arrow
    {
        protected BindingSource<T1> source2;

        public PairBinding(BindingSource<T1> s1, BindingSource<T1> s2, Arrow<T1, T2> theArrow, BindingDestination<T2> destination) :
            base(s1, theArrow, destination)
        {
            this.source2 = s2;
            s2.BindingObject = this;
        }

        public override void NotifyChange()
        {
            destination.Value = arrow.ExecutePair(source.Value, source2.Value);
        }
    }

    class Tester
    {
        static void Main(string[] args)
        {
            Tester t = new Tester();
            t.Run();
        }

        public void Run()
        {
            // Simple arrow test - 'test' is passed through the function x * x + 3 to 'result'
            BindingSource<int> test = new BindingSource<int>(3);
            BindingDestination<int> result = new BindingDestination<int>(0);
            Arrow<int, int> testArrow = new Arrow<int, int>(x => x * x + 3);
            Binding<int, int> testBinding = new Binding<int, int>(test, testArrow, result);

            // Change the value of 'test' and print the value of 'result' to demonstrate it changing
            test.Value = 4;
            Console.WriteLine("Result is {0}", (int)result);
            test.Value = 1;
            Console.WriteLine("Result is {0}", (int)result);
            Console.WriteLine();


            // Pair arrow test - 's1' and 's2' are treated as the breadth and height of a triangle,
            // and 'result' is now bound to the area of said triangle
            BindingSource<int> s1 = new BindingSource<int>(4);
            BindingSource<int> s2 = new BindingSource<int>(3);
            Arrow<int, int> dualArrow = new Arrow<int, int>((b, h) => (b*h) / 2);
            PairBinding<int, int> pairBinding = new PairBinding<int, int>(s1, s2, dualArrow, result);

            s1.Value = 3;
            Console.WriteLine("Result is {0}", (int)result);
            s2.Value = 8;
            Console.WriteLine("Result is {0}", (int)result);

            IArrow newArrow = testArrow + testArrow;  // Possible syntax for composing arrows
        }
    }
}
