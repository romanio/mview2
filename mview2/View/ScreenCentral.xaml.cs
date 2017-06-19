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

namespace mview2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class ScreenCentral : Window
    {
        public ScreenCentral()
        {
            InitializeComponent();
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
                ecl.OpenData(ofd.FileName);
            }
        }
    }
}
