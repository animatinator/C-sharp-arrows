using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArrowDataBinding.demos.project;
using ArrowDataBinding.demos;
using ArrowDataBinding.tests;

namespace ArrowDataBinding
{
    class Program
    {
        static void Main(string[] args)
        {
            BindingArgumentMarshallerTests tests = new BindingArgumentMarshallerTests();
            tests.Run();
        }
    }
}
