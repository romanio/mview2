using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;

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
        public MeshGeometry3D Model { get; set; }

        public NameOptions ShowNameOptions;
        public List<string> ListNames { get; set; }
        public List<string> ListKeywords { get; set; }
        public List<string> RestartDates { get; set; }
        public List<string> RestartWellnames { get; set; }

        public void OpenModel(string filename)
        {
            ECL.OpenData(filename);
            GenerateModel();
            //ECL.ReadInit();
           // UpdateRestartNames();
        }

        public void GenerateModel()
        {
            int compress = 0;
            MeshBuilder builder = new MeshBuilder();

            /*
            for (int X = 0; X < ECL.EGRID.NX; ++X)
                for (int Y = 0; Y < ECL.EGRID.NY; ++Y)
                    for (int Z = 0; Z < ECL.EGRID.NZ; ++Z)
                    {
                        if (ECL.EGRID.GetActive(X, Y, Z) > 0)
                        {
                            // Проверим окружение, есть ли соседние активные ячейки
                            if (ECL.EGRID.CheckEasyNeighbour(X, Y, Z))
                            {
                                builder.AddBox(new Vector3(X * 10, Y * 10, Z * 10), 10, 10, 10);
                            }
                            else
                                compress++;
                            //var CELL = ECL.EGRID.GetCell(X, Y, 0);
                            //builder.AddQuad(CELL.TNW, CELL.TNE, CELL.TSE, CELL.TSW);
                        }
                    }
            */

            builder.AddBox(new Vector3(0, 0, 0), 10, 10, 10, BoxFaces.All);
            Model = builder.ToMeshGeometry3D();
            //Model = new GeometryModel3D(builder.ToMesh(), MaterialHelper.CreateMaterial(Brushes.DarkRed));
            //System.Diagnostics.Debug.WriteLine(compress);

            
            OnPropertyChanged("Model");
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
        }
    }
}