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

            BindingHandle bind = BindingsManager.CreateBinding(
                a.GetBindPoint("Value"),
                id,
                b.GetBindPoint("Value"));

            BindingHandle bind1 = BindingsManager.CreateBinding(
                b.GetBindPoint("Value"),
                id,
                a.GetBindPoint("Value"));
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
