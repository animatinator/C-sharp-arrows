using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowDataBinding.Bindings;
using ArrowDataBinding.Combinators;
using ArrowDataBinding.Arrows;

namespace ListBindingDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Database data = new Database();
            TestOutput test = new TestOutput();
            BindingsManager.CreateBinding(data.GetBindPoint("orders"), ListArrow.Filter((Order x) => x.Supplier.Name == "Microsoft"),
                test.GetBindPoint("Orders"));
            data.Initialise();
            test.Orders.ForEach(x => Console.WriteLine("{0} ordered {1} of the product {2} from {3}", x.Customer.Name, x.Volume, x.Product, x.Supplier.Name));
        }
    }

    public class TestOutput : Bindable
    {
        [Bindable]
        public List<Order> Orders { get; set; }

        public TestOutput()
        {
            Orders = new List<Order>();
        }
    }
}
