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
using ArrowDataBinding.Combinators;
using ArrowDataBinding.Bindings;

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

        public int X
        {
            get
            {
                return x;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitialiseArrows();
            InitialiseDataContext();
        }

        public void InitialiseArrows()
        {
            InitialiseCircleArrow((int)(Width / 2), (int)(Height / 2), (int)(Width / 2), (int)(Height / 2));
        }

        public void InitialiseCircleArrow(int xRadius, int yRadius, int cX, int cY)
        {
            Arrow<int, Tuple<int, int>> timeDup = Op.Split<int>();

            Arrow<int, double> sin = Op.Arr((int x) => cX + xRadius * Math.Sin((double)x / 20));
            Arrow<int, double> cos = Op.Arr((int y) => cY + yRadius * Math.Cos((double)y / 20));
            Arrow<Tuple<int, int>, Tuple<double, double>> sinCos = sin.And(cos);

            Arrow<double, int> doubleToInt = Op.Arr((double x) => (int)x);
            Arrow<Tuple<double, double>, Tuple<int, int>> adapter = doubleToInt.And(doubleToInt);

            circle = timeDup.Combine(sinCos).Combine(adapter);
        }

        public void InitialiseDataContext()
        {
            this.DataContext = new CoordsProvider(circle);
        }
    }

    public class CoordsProvider : Bindable
    {
        private Arrow<int, Tuple<int, int>> coordsGenerator;
        private int time;
        private Timer timeIncrementTimer;

        [Bindable]
        public int X { get; set; }

        [Bindable]
        public int Y { get; set; }


        public CoordsProvider(Arrow<int, Tuple<int, int>> generator)
        {
            this.time = 0;
            this.X = 30;
            this.Y = 50;
            this.coordsGenerator = generator;
            this.timeIncrementTimer = new Timer(Update, this, 40L, 40L);
        }

        private void AdvanceTime()
        {
            time++;
        }

        private void Update(object state)
        {
            AdvanceTime();
            UpdateBinding();
        }

        private void UpdateBinding()
        {
            Tuple<int, int> newCoords = coordsGenerator.Invoke(time);
            X = newCoords.Item1; Y = newCoords.Item2;
        }
    }
}
