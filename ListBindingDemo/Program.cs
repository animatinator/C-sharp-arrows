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
        private static ListOutput ordersFromMicrosoft;
        private static ListOutput ordersToCambridge;
        private static ListOutput bulkOrders;
        private static IntOutput averageOrdersToScotland;

        static void Main(string[] args)
        {
            Database data = new Database();
            ordersFromMicrosoft = new ListOutput();
            ordersToCambridge = new ListOutput();
            bulkOrders = new ListOutput();
            averageOrdersToScotland = new IntOutput();

            BindingsManager.CreateBinding(
                data.GetBindPoint("orders"),
                ListArrow.Filter((Order x) => x.Supplier.Name == "Microsoft")
                    .OrderBy((Order x, Order y) => x.Customer.Name.CompareTo(y.Customer.Name)),
                ordersFromMicrosoft.GetBindPoint("Orders"));

            BindingsManager.CreateBinding(
                data.GetBindPoint("orders"),
                ListArrow.Filter((Order x) => x.Customer.Location == "Cambridge")
                    .OrderBy((Order x, Order y) => x.Customer.Name.CompareTo(y.Customer.Name)),
                ordersToCambridge.GetBindPoint("Orders"));

            BindingsManager.CreateBinding(
                data.GetBindPoint("orders"),
                ListArrow.Filter((Order x) => x.Volume > 30)
                    .OrderBy((Order x, Order y) => x.Customer.Name.CompareTo(y.Customer.Name)),
                bulkOrders.GetBindPoint("Orders"));

            var averagingArrow = Op.Split<IEnumerable<int>>()
                .Combine(Op.And(
                        ListArrow.Foldl((int x, int y) => x + y, 0),
                        ListArrow.Foldl((int x, int y) => x + 1, 0)))
                    .Unsplit((int total, int count) => total / count);

            BindingsManager.CreateBinding(
                data.GetBindPoint("orders"),
                ListArrow.Filter((Order x) => x.Customer.Location == "Glasgow" || x.Customer.Location == "Aberdeen")
                    .Map((Order x) => x.Volume)
                    .Combine(averagingArrow),
                averageOrdersToScotland.GetBindPoint("Result"));

            data.Initialise();

            PrintOutput("Orders from Microsoft", ordersFromMicrosoft);
            PrintOutput("Orders to Cambridge", ordersToCambridge);
            PrintOutput("Bulk orders", bulkOrders);
            Console.WriteLine("Average orders to Scotland: {0}", averageOrdersToScotland.Result);
            Console.WriteLine();

            IncreaseMoragsOrder(data);
            MoveBrendaToCambridge(data);
            MicrosoftTakeover(data);
        }

        static void IncreaseMoragsOrder(Database data)
        {
            Console.WriteLine("--Morag is now ordering 1000 happy meals--");
            List<Order> orders = data.orders;
            orders
                .First((Order x) => x.Customer.Name == "Morag")
                .Volume = 1000;
            data.orders = orders;

            PrintOutput("Bulk orders", bulkOrders);
        }

        static void MoveBrendaToCambridge(Database data)
        {
            Console.WriteLine("--Brenda has moved to Cambridge--");
            List<Order> orders = data.orders;
            orders
                .First((Order x) => x.Customer.Name == "Brenda")
                .Customer.Location = "Cambridge";
            data.orders = orders;

            PrintOutput("Orders to Cambridge", ordersToCambridge);
        }

        static void MicrosoftTakeover(Database data)
        {
            Console.WriteLine("--Microsft have bought McDonald's--");
            List<Order> orders = data.orders;
            orders
                .Where((Order x) => x.Supplier.Name == "McDonald's").ToList()
                .ForEach((Order x) => x.Supplier.Name = "Microsoft");
            data.orders = orders;

            PrintOutput("Orders from Microsoft", ordersFromMicrosoft);
        }

        static void PrintOutput(string title, ListOutput output)
        {
            Console.WriteLine(title+":");
            output.Orders.ForEach(x => Console.WriteLine("{0} ordered {1} of the product {2} from {3}", x.Customer.Name, x.Volume, x.Product, x.Supplier.Name));
            Console.WriteLine();
        }
    }

    public class ListOutput : Bindable
    {
        [Bindable]
        public List<Order> Orders { get; set; }

        public ListOutput()
        {
            Orders = new List<Order>();
        }
    }

    public class IntOutput : Bindable
    {
        [Bindable]
        public int Result { get; set; }
    }
}
