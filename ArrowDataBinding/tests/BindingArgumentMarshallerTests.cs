using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Bindings;

namespace ArrowDataBinding.tests
{
    public class BindingArgumentMarshallerTests
    {
        public void Run()
        {
            TestClass testObj = new TestClass(4);
            TestClass2 testObj2 = new TestClass2();
            BindingArgumentMarshaller.MarshalArguments(new List<BindPoint>
            {
                new BindPoint(testObj, "Test"),
                new BindPoint(testObj2, "Test")
            });
        }
    }

    public class TestClass : Bindable
    {
        [Bindable]
        public int Test { get; set; }

        public TestClass(int val)
        {
            Test = val;
        }
    }

    public class TestClass2 : Bindable
    {
        [Bindable]
        public string Test { get; set; }

        public TestClass2()
        {
            Test = "Hello";
        }
    }
}
