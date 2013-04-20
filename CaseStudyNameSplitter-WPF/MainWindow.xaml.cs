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
using System.ComponentModel;
using System.Globalization;

namespace CaseStudyNameSplitter_WPF
{
    public class NameData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private String fullName;
        public String Name
        {
            get
            {
                return fullName;
            }
            set
            {
                fullName = value;
                RaisePropertyChanged("Name");
            }
        }

        public NameData(string name)
        {
            Name = name;
        }

        public void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public class ForenameValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value).Split(' ')[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SurnameValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value).Split(' ')[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainWindow : Window
    {
        private NameData name;

        public MainWindow()
        {
            InitializeComponent();
            InitialisePerson();
            InitialiseBindings();
        }

        public void InitialisePerson()
        {
            name = new NameData("Deita Bindings");
        }

        public void InitialiseBindings()
        {
            InitialiseNameBinding(ForenameBox, new ForenameValueConverter());
            InitialiseNameBinding(SurnameBox, new SurnameValueConverter());
        }

        public void InitialiseNameBinding(TextBox textBox, IValueConverter converter)
        {
            Binding bind = new Binding();
            bind.Source = name;
            bind.Path = new PropertyPath("Name");
            bind.Converter = converter;
            textBox.SetBinding(TextBox.TextProperty, bind);
        }

        private void NameChangeButton_Click(object sender, RoutedEventArgs e)
        {
            name.Name = "Mister Newname";
        }
    }
}
