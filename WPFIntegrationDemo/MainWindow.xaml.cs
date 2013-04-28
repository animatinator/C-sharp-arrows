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
using System.Threading;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Bindings;
using ArrowDataBinding.Combinators;

namespace WPFIntegrationDemo
{
    public partial class MainWindow : Window
    {
        private Arrow<int, string> arrow;
        private Time time;

        public MainWindow()
        {
            InitializeComponent();
            InitialiseTime();
            InitialiseArrow();
            InitialiseBinding();
        }

        public void InitialiseTime()
        {
            time = new Time();
        }

        public void InitialiseArrow()
        {
            var sinArrow = Op.Arr((int x) => 50 + (int)(50.0*Math.Sin(x / 30.0)));
            var textSizeArrow = Op.Arr((int length) => new String('|', length));
            arrow = sinArrow.Combine(textSizeArrow);
        }

        public void InitialiseBinding()
        {
            Binding binding = new Binding();
            binding.Source = time;
            binding.Mode = BindingMode.OneWay;
            binding.Path = new PropertyPath("CurrentTime");
            binding.Converter = new ArrowValueConverter(arrow);
            TextBox.SetBinding(TextBlock.TextProperty, binding);
        }
    }

    public class Time : Bindable
    {
        [Bindable]
        public int CurrentTime { get; set; }

        private Timer timer;

        public Time()
        {
            CurrentTime = 0;
            timer = new Timer(state => CurrentTime++, null, 25, 25);
        }
    }
}
