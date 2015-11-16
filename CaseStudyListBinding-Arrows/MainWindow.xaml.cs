using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Bindings;
using ArrowDataBinding.Combinators;

namespace CaseStudyListBinding_Arrows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Database ordersDatabase { get; set; }
        private ListArrow<Order, string> arrow { get; set; }
        public ArrowResult arrowResult;

        public MainWindow()
        {
            InitializeComponent();
            InitialiseDatabase();
            InitialiseBinding();
            //ordersDatabase.Orders.Add(new Order("test", new Supplier("bob", "somewhere"), 3, new Customer("Dave", "Cambridge")));
        }

        public void InitialiseDatabase()
        {
            ordersDatabase = new Database();
            arrowResult = new ArrowResult();
        }

        public void InitialiseBinding()
        {
            InitialiseArrow();

            BindingsManager.CreateBinding(
                ordersDatabase.GetBindPoint("Orders"),
                arrow,
                arrowResult.GetBindPoint("Result"));
            InitialiseWPFBinding();


            ordersDatabase.Initialise();
        }

        public void InitialiseArrow()
        {
            arrow = ListArrow.Filter<Order>((Order o) => o.Volume > 1)
                .Map((Order o) => String.Format("Order from {0} in {1} for {2} {3}s from {4}", o.Customer.Name, o.Customer.Location, o.Volume, o.Product, o.Supplier.Name));
        }

        public void InitialiseWPFBinding()
        {
            Binding listBinding = new Binding();
            listBinding.Source = arrowResult;
            listBinding.Path = new PropertyPath("Result");
            BoundListBox.SetBinding(ListBox.ItemsSourceProperty, listBinding);
        }
    }

    public class ArrowResult : Bindable
    {
        [Bindable]
        public List<string> Result { get; set; }

        public ArrowResult()
        {
            Result = new List<string>();
        }
    }
}
