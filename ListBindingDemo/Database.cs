using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Bindings;
using ArrowDataBinding.Combinators;

namespace ListBindingDemo
{
    public class Order
    {
        public string Product { get; set; }
        public Supplier Supplier { get; set; }
        public int Volume { get; set; }
        public Customer Customer { get; set; }

        public Order(string product, Supplier supplier, int volume, Customer customer)
        {
            this.Product = product;
            this.Supplier = supplier;
            this.Volume = volume;
            this.Customer = customer;
        }
    }

    public class Customer
    {
        public string Name { get; set; }
        public string Location { get; set; }

        public Customer(string name, string location)
        {
            this.Name = name;
            this.Location = location;
        }
    }

    public class Supplier
    {
        public string Name { get; set; }
        public string Location { get; set; }

        public Supplier(string name, string location)
        {
            this.Name = name;
            this.Location = location;
        }
    }


    public class Database : Bindable
    {
        [Bindable]
        public dynamic orders { get; set; }

        public Database()
        {
            Initialise();
        }

        public void Initialise()
        {
            Supplier ms = new Supplier("Microsoft", "Redmond");
            Supplier google = new Supplier("Google", "Mountain View");
            Supplier mcDonalds = new Supplier("McDonald's", "Everywhere");
            Customer jack = new Customer("Jack", "Cambridge");
            Customer david = new Customer("David", "Cambridge");
            Customer james = new Customer("James", "Cambridge");
            Customer brenda = new Customer("Brenda", "Aberdeen");
            Customer morag = new Customer("Morag", "Aberdeen");
            Customer will = new Customer("William Wallace", "Glasgow");

            orders = new List<Order> {
                new Order("VS2012", ms, 57, jack),
                new Order("Happy meal", mcDonalds, 1, morag),
                new Order("Google car", google, 3, james),
                new Order("MS Word", ms, 1, david),
                new Order("Nexus 7", google, 1, brenda),
                new Order("Haggis", mcDonalds, 50, will)
            };
        }
    }
}
