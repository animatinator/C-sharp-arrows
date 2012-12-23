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
using ArrowDataBinding.Combinators;

namespace SimpleFormDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Arrow<int, Tuple<int, int>> circle = null;
        private int x = 10;
        private int y = 10;

        public MainWindow()
        {
            InitializeComponent();
            InitialiseArrows();
        }

        public void InitialiseArrows()
        {
            InitialiseCircleArrow();
        }

        public void InitialiseCircleArrow()
        {
            Arrow<int, Tuple<int, int>> timeDup = Op.Split<int>();
            Arrow<int, double> sin = Op.Arr((int x) => Math.Sin((double)x / 10));
            Arrow<int, double> cos = Op.Arr((int y) => Math.Cos((double)y / 10));
            Arrow<double, int> doubleToInt = Op.Arr((double x) => (int)x);
            Arrow<Tuple<double, double>, Tuple<int, int>> adapter = doubleToInt.And(doubleToInt);
            Arrow<int, Tuple<int, int>> circle = timeDup.Combine(sin.And(cos)).Combine(adapter);
        }
    }
}
