using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Bindings;

namespace ArrowDataBinding.tests
{
    public class CycleDetectionTests
    {
        public void Run()
        {
            Arrow<int, int> id = new IDArrow<int>();
            A a = new A();
            B b = new B();
            C c = new C();

            BindingHandle aToB = BindingsManager.CreateBinding(
                a.GetBindPoint("Value"),
                id,
                b.GetBindPoint("Value"));

            BindingHandle bToA = BindingsManager.CreateBinding(
                b.GetBindPoint("Value"),
                id,
                a.GetBindPoint("Value"));

            BindingHandle bToC = BindingsManager.CreateBinding(
                b.GetBindPoint("Value"),
                id,
                c.GetBindPoint("Value")
                );

            BindingHandle cToB = BindingsManager.CreateBinding(
                c.GetBindPoint("Value"),
                id,
                b.GetBindPoint("Value")
                );

            try
            {
                BindingHandle cToA = BindingsManager.CreateBinding(
                    c.GetBindPoint("Value"),
                    id,
                    a.GetBindPoint("Value")
                    );
            }
            catch (BindingConflictException)
            {
                Console.WriteLine("Cycle exception was thrown successfully!");
            }
        }
    }

    public class A : Bindable
    {
        [Bindable]
        public int Value { get; set; }

        public A()
        {
            Value = 0;
        }
    }

    public class B : Bindable
    {
        [Bindable]
        public int Value { get; set; }

        public B()
        {
            Value = 0;
        }
    }

    public class C : Bindable
    {
        [Bindable]
        public int Value { get; set; }

        public C()
        {
            Value = 0;
        }
    }
}
