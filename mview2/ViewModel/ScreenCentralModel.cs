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
            System.Diagnostics.Debug.WriteLine(property);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public EclipseProject ecl = new EclipseProject();
        public Engine2D Engine2D = new Engine2D();
        public NameOptions ShowNameOptions;
        public List<string> ListNames { get; set; }
        public List<string> ListKeywords { get; set; }
        public List<string> RestartDates { get; set; }

        public void OpenModel(string filename)
        {
            ecl.OpenData(filename);
            UpdateRestartNames();
        }

        public ScreenCentralModel()
        {
            ShowNameOptions = NameOptions.Well;

            // Пустое начальное значение в ComboBox даты рестарта
            RestartDates = new List<string>();
            RestartDates.Add($"09.10.1980    X0000 ");
            OnPropertyChanged("RestartDates");
        }

        public void UpdateRestartNames()
        {
            RestartDates.Clear();
            for (int iw = 0; iw < ecl.RESTART.DATE.Count; ++iw)
                RestartDates.Add($"{ecl.RESTART.DATE[iw]:dd.MM.yyyy}    X{ecl.RESTART.TIME[iw]:0000} ");
            OnPropertyChanged("RestartDates");
        }


        public void UpdateSelectedNames(List<string> selection)
        {
            if (ecl.VECTORS == null) return;

            if (selection.Count == 0) return;

            ListKeywords =
                (from vec in ecl.VECTORS
                 where vec.Name == selection.First() && vec.Type == ShowNameOptions
                 select vec.Data).First().Select(c => c.keyword).ToList();

            OnPropertyChanged("ListKeywords");
        }

        public void UpdateListNames(NameOptions options)
        {
            if (ecl.VECTORS == null) return;

            ShowNameOptions = options;

            ListNames = (
                from item in ecl.VECTORS
                where item.Type == options
                select item.Name).ToList();

            OnPropertyChanged("ListNames");
        }

        void CalcModelLimits()
        {
            Engine2D.SetLimits(
                ecl.EGRID.XORIGIN,
                ecl.EGRID.XENDXAXIS,
                ecl.EGRID.YORIGIN,
                ecl.EGRID.YENDYAXIS);
        }

        void GenerateStructure()
        {
            Engine2D.GenerateStructure(ecl.EGRID);
        }
    }
}