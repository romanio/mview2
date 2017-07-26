using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace mview2
{
    public partial class ScreenCentral : MetroWindow
    {
        ScreenCentralModel Model = new ScreenCentralModel();

        public ScreenCentral()
        {
            this.DataContext = Model;
            InitializeComponent();

            view1.PanGesture2 = null;
            view1.PanGesture.MouseAction = MouseAction.RightClick;
            view1.PanGesture.Modifiers = ModifierKeys.None;
            view1.RotateGesture.MouseAction = MouseAction.MiddleClick;
        }

        private void OpenModel(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog() { Filter = "Eclipse file|*.SMSPEC" };
            if (fileDialog.ShowDialog() == true)
            {
                Model.OpenModel(fileDialog.FileName);
            }
        }

        private void OnOpenModel(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog() { Filter = "Eclipse file|*.SMSPEC" };
            if (fileDialog.ShowDialog() == true)
            {
                Model.OpenModel(fileDialog.FileName);
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

        private void OnStaticListSelected(object sender, RoutedEventArgs e)
        {
            string name = ((TreeViewItem)e.OriginalSource).Header.ToString();
            if (name != "STATIC")
                Model.OnStaticPropertySelected(name);
        }

        private void OnViewAll(object sender, RoutedEventArgs e)
        {
            view1.ZoomExtents();
        }
    }
}