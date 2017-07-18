using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private void OpenModel(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog() { Filter = "Eclipse file|*.SMSPEC" };
            if (fileDialog.ShowDialog() == true)
            {
                Model.OpenModel(fileDialog.FileName);
                view1.ZoomExtents();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (boxNameOptions.SelectedIndex)
            {
                case 0:
                    Model.UpdateListNames(NameOptions.Field);
                    break;
                case 1:
                    Model.UpdateListNames(NameOptions.Group);
                    break;
                case 2:
                    Model.UpdateListNames(NameOptions.Well);
                    break;
                case 3:
                    Model.UpdateListNames(NameOptions.Aquifer);
                    break;
                case 4:
                    Model.UpdateListNames(NameOptions.Region);
                    break;
                case 5:
                    Model.UpdateListNames(NameOptions.Other);
                    break;
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = new List<string>();
            foreach (string item in ((System.Windows.Controls.ListBox)sender).SelectedItems)
            {
                selection.Add(item.ToString());
            }
            Model.UpdateSelectedNames(selection);
        }

        private void OnRestartDateSelect(object sender, SelectionChangedEventArgs e)
        {
            Model.OnRestartDateSelect(((System.Windows.Controls.ComboBox)sender).SelectedIndex);
        }

    }
}
