using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using OxyPlot;
using OxyPlot.Series;

namespace mview2
{
    class ScreenCentralModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        EclipseProject ecl = new EclipseProject();
        public List<string> ListNames { get; set; }
        public PlotModel PlotModel { get; set; }

        public void OpenModel(string filename)
        {
            ecl.OpenData(filename);
        }

        public void UpdateSelectedNames(List<string> selection)
        {

        }

        public void UpdateListNames(NameOptions options)
        {
            if (ecl.VECTORS == null) return;

            ListNames = (
                from item in ecl.VECTORS
                where item.Type == options
                select item.Name).ToList();

            OnPropertyChanged("ListNames");
        }

        public ScreenCentralModel()
        {
            PlotModel = new PlotModel { Title = "WOPR" };

            var cs = new LineSeries
            {
                Color = OxyColors.Blue
            };
            
            PlotModel.Series.Add(cs);
        }
    }
}
