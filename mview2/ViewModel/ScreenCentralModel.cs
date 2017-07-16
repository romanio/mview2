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

        public EclipseProject ECL = new EclipseProject();
        public Engine2D Engine2D = new Engine2D();
        public NameOptions ShowNameOptions;
        public List<string> ListNames { get; set; }
        public List<string> ListKeywords { get; set; }

        public List<string> RestartDates { get; set; }
        public List<string> RestartWellnames { get; set; }

        public void OpenModel(string filename)
        {
            ECL.OpenData(filename);
            ECL.ReadInit();
            UpdateRestartNames();
        }

        public ScreenCentralModel()
        {
            ShowNameOptions = NameOptions.Well;
        }

        public void UpdateRestartNames()
        {
            RestartDates = new List<string>();

            for (int iw = 0; iw < ECL.RESTART.DATE.Count; ++iw)
                RestartDates.Add($"{ECL.RESTART.DATE[iw]:dd.MM.yyyy}    X{ECL.RESTART.REPORT[iw]:0000} ");

            OnPropertyChanged("RestartDates");
        }

        public void UpdateSelectedNames(List<string> selection)
        {
            if (ECL.VECTORS == null) return;

            if (selection.Count == 0) return;

            ListKeywords =
                (from vec in ECL.VECTORS
                 where vec.Name == selection.First() && vec.Type == ShowNameOptions
                 select vec.Data).First().Select(c => c.keyword).ToList();

            OnPropertyChanged("ListKeywords");
        }

        public void UpdateListNames(NameOptions options)
        {
            if (ECL.VECTORS == null) return;

            ShowNameOptions = options;

            ListNames = (
                from item in ECL.VECTORS
                where item.Type == options
                select item.Name).ToList();

            OnPropertyChanged("ListNames");
        }

        void CalcModelLimits()
        {
            Engine2D.SetLimits(
                ECL.EGRID.XORIGIN,
                ECL.EGRID.XENDXAXIS,
                ECL.EGRID.YORIGIN,
                ECL.EGRID.YENDYAXIS);
        }

        void GenerateStructure()
        {
            Engine2D.GenerateStructure(ECL.EGRID);
        }

        public void OnRestartDateSelect(int step)
        {
            ECL.ReadRestart(step);

            //

            RestartWellnames = new List<string>();

            foreach (var item in ECL.RESTART.WELLS)
                RestartWellnames.Add(item.WELLNAME);

            OnPropertyChanged("RestartWellNames");

            //

            ECL.RESTART.ReadRestartGrid("PRESSURE");

            Engine2D.WellData = ECL.RESTART.WELLS;
            Engine2D.SetLimits(0, ECL.RESTART.NX * 50, 0, ECL.RESTART.NY * 50);
            Engine2D.GenerateEasyStructure(ECL.RESTART.NX, ECL.RESTART.NY, ECL.RESTART.NZ, ECL.RESTART.DATA, ECL.INIT.ACTNUM);
        }
    }
}