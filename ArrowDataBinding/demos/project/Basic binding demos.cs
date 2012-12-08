using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.Bindings;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;

namespace ArrowDataBinding.demos.project
{
    public class Source : Bindable
    {
        [Bindable]
        public int source { get; set; }

        [Bindable]
        public int multiplies { get; set; }
    }

    public class Destination : Bindable
    {
        [Bindable]
        public int result { get; set; }
    }


    public class BasicBindingDemos
    {
        public void Run()
        {
            Console.WriteLine("Running basic binding demos...");
            RunSimpleDemo();
        }

        public static void RunSimpleDemo()
        {
            Source source = new Source();
            Destination dest = new Destination();
            Arrow<int, int> arr = new IDArrow<int>();
            BindingsManager.CreateBinding(BindingsManager.BindPoint(source, "source"), arr, BindingsManager.BindPoint(dest, "result"));

            bool passed = true;
            Random rand = new Random();

            for (int i = 0; i < 100; i++)
            {
                int next = rand.Next();
                source.source = next;
                if (dest.result != source.source) passed = false;
            }

            if (passed) Console.WriteLine("Works!");
            else Console.WriteLine("Doesn't work D:");
        }
    }
}
