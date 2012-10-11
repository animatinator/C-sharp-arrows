using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.demos.ArrowTestThing;

namespace ArrowDataBinding.demos
{
    class Tester
    {
        public void Run()
        {
            Console.WriteLine("Running arrow demo...\n\n");

            // Test the arrow test thing
            ArrowDataBinding.demos.ArrowTestThing.Tester t = new ArrowDataBinding.demos.ArrowTestThing.Tester();
            t.Run();
        }
    }
}
