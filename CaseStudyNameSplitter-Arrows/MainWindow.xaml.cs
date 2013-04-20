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
using System.Globalization;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Bindings;
using ArrowDataBinding.Combinators;

namespace CaseStudyNameSplitter_Arrows
{
    public class NameData : Bindable
    {
        [Bindable]
        public string Name { get; set; }

        public NameData(string name)
        {
            Name = name;
        }
    }

    public class NameSplit : Bindable
    {
        [Bindable]
        public string Forename { get; set; }

        [Bindable]
        public string Surname { get; set; }
    }

    public partial class MainWindow : Window
    {
        private Arrow<string, Tuple<string, string>> nameArrow;
        private NameData name;
        private NameSplit splitName;

        public MainWindow()
        {
            InitializeComponent();
            InitialiseArrow();
            name = new NameData("Deita Bindings");
            splitName = new NameSplit();
            InitialiseBindings();
            InitialiseName();
        }

        public void InitialiseName()
        {
            name.Name = "Deita Bindings";
        }

        public void InitialiseArrow()
        {
            nameArrow = Op.Arr((string x) => Tuple.Create(
                                                    x.Split()[0],
                                                    x.Split()[1]));
        }

        public void InitialiseBindings()
        {
            BindingsManager.CreateBinding(
                BindingsManager.Sources(name.GetBindPoint("Name")),
                nameArrow,
                BindingsManager.Destinations(splitName.GetBindPoint("Forename"), splitName.GetBindPoint("Surname")));

            InitialiseNameBinding(ForenameBox, "Forename");
            InitialiseNameBinding(SurnameBox, "Surname");
        }

        public void InitialiseNameBinding(TextBox textBox, string property)
        {
            Binding bind = new Binding();
            bind.Source = splitName;
            bind.Path = new PropertyPath(property);
            textBox.SetBinding(TextBox.TextProperty, bind);
        }

        private void NameChangeButton_Click(object sender, RoutedEventArgs e)
        {
            name.Name = "Mister Newname";
        }
    }
}
