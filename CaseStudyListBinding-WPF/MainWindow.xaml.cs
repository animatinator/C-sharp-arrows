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
using System.Collections.ObjectModel;
using System.Globalization;

namespace CaseStudyListBinding_WPF
{
    public partial class MainWindow : Window
    {
        public Database ordersDatabase { get; set; }

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
        }

        public void InitialiseBinding()
        {
            Binding listBinding = new Binding();
            listBinding.Source = ordersDatabase;
            listBinding.Path = new PropertyPath("Orders");
            listBinding.Converter = new ListFilterConverter();
            BoundListBox.SetBinding(ListBox.ItemsSourceProperty, listBinding);
        }
    }

    public class ListFilterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<Order> list = (ObservableCollection<Order>)value;
            List<string> resultsList = new List<string>();

            foreach (Order order in list)
            {
                if (order.Volume > 1)
                {
                    resultsList.Add(String.Format("Order from {0} in {1} for {2} '{3}' from {4}", order.Customer.Name, order.Customer.Location, order.Volume, order.Product, order.Supplier.Name));
                }
            }

            return resultsList;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
