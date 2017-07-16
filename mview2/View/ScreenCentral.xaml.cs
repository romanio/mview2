using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OpenTK;


namespace mview2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class ScreenCentral : Window
    {
        ScreenCentralModel Model = new ScreenCentralModel();

        private GLControl glControl;

        public ScreenCentral()
        {
            InitializeComponent();

            glControl = new GLControl();
            glControl.MouseMove += GlControl_MouseMove;
            glControl.MouseClick += GlControl_MouseClick;
            glControl.Paint += GlControl_Paint;
            glControl.MouseWheel += GlControl_MouseWheel;
            glControl.Resize += GlControl_Resize;
            glControl.Load += GlControl_Load;
            HostGL.Child = this.glControl;
            this.DataContext = Model;
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            Model.Engine2D.Load();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Model.Engine2D.Unload();
        }
        private void GlControl_Resize(object sender, EventArgs e)
        {
            Model.Engine2D.Resize(glControl.Width, glControl.Height);
            glControl.SwapBuffers();
        }

        private void GlControl_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Model.Engine2D.MouseWheel(e);
            Model.Engine2D.Paint();
            glControl.SwapBuffers();
        }

        private void GlControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Model.Engine2D.Paint();
            glControl.SwapBuffers();
        }

        private void GlControl_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Model.Engine2D.MouseClick(e);
            Model.Engine2D.Paint();
            glControl.SwapBuffers();
        }

        private void GlControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Model.Engine2D.MouseMove(e);
            Model.Engine2D.Paint();
            glControl.SwapBuffers();
        }

        private void OpenModel(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog() { Filter = "Eclipse file|*.SMSPEC" };
            if (fileDialog.ShowDialog() == true)
            {
                Model.OpenModel(fileDialog.FileName);
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
