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
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Series;

namespace mview2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class ScreenCentral : Window
    {
        ScreenCentralModel Model = new ScreenCentralModel();

        public ScreenCentral()
        {
            InitializeComponent();
            this.DataContext = Model;
        }

        EclipseProject ecl = new EclipseProject();

        private void OpenModel(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Eclipse file|*.SMSPEC"
            };

            if (ofd.ShowDialog() == true)
            {
                Model.OpenModel(ofd.FileName);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (boxNameOptions.SelectedIndex == 0) Model.UpdateListNames(NameOptions.Field);
            if (boxNameOptions.SelectedIndex == 1) Model.UpdateListNames(NameOptions.Group);
            if (boxNameOptions.SelectedIndex == 2) Model.UpdateListNames(NameOptions.Well);
            if (boxNameOptions.SelectedIndex == 3) Model.UpdateListNames(NameOptions.Aquifer);
            if (boxNameOptions.SelectedIndex == 4) Model.UpdateListNames(NameOptions.Region);
            if (boxNameOptions.SelectedIndex == 5) Model.UpdateListNames(NameOptions.Other);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<string> selection = new List<string>();
            foreach(string item in e.AddedItems)
            {
                selection.Add(item.ToString());
            }
            Model.UpdateSelectedNames(selection);
        }
    }
}
