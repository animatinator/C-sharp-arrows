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

            var tup = BindingArgumentMarshaller.MarshalArguments(new List<BindPoint>
            {
                new BindPoint(testObj, "Test"),
                new BindPoint(testObj2, "Test"),
                new BindPoint(testObj2, "Test")
            }, Tuple.Create(Tuple.Create(3, 4), 5).GetType());

            Console.WriteLine(tup);

            List<dynamic> list = BindingArgumentMarshaller.UnmarshalArguments(Tuple.Create(Tuple.Create(4, Tuple.Create("cheese", "peas")), Tuple.Create("Hi", 5)));
            Console.WriteLine(list);
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
