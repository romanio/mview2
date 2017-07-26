using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.Windows.Media;
using System.Windows;

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
        public GeometryModel3D Model { get; set; } //GeometryModel3D
        public Point3DCollection Wells { get; set; }
        public IList<SpatialTextItem> WellNamesItems { get; set; }
        public List<string> StaticListNames { get; set; }
        public List<string> DynamicListNames { get; set; }
        public List<string> LogItems { get; set; }
        public NameOptions ShowNameOptions;
        public List<string> ListNames { get; set; }
        public List<string> ListKeywords { get; set; }
        public List<string> RestartDates { get; set; }
        public List<string> RestartWellnames { get; set; }

        public void OpenModel(string filename)
        {
            ECL.OpenData(filename);
            ECL.ReadInitACTNUM();

            StaticListNames = new List<string>
            {
                "PORV"
            };

            for (int it = 0; it < ECL.INIT.NAME.Count; ++it)
                for (int iw = 0; iw < ECL.INIT.NAME[it].Length; ++iw)
                    if (ECL.INIT.NUMBER[it][iw] == ECL.INIT.NACTIV)
                        StaticListNames.Add(ECL.INIT.NAME[it][iw]);

            OnPropertyChanged("StaticListNames");

            DynamicListNames = new List<string>();

            for (int it = 0; it < ECL.RESTART.NAME.Count; ++it)
                for (int iw = 0; iw < ECL.RESTART.NAME[it].Length; ++iw)
                    if (ECL.RESTART.NUMBER[it][iw] == ECL.INIT.NACTIV)
                        DynamicListNames.Add(ECL.RESTART.NAME[it][iw]);

            DynamicListNames = DynamicListNames.Distinct().ToList();

            OnPropertyChanged("DynamicListNames");

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

            if (ECL.RESTART.WELLS != null)
            {
                foreach (var item in ECL.RESTART.WELLS)
                    RestartWellnames.Add(item.WELLNAME);
            }

            OnPropertyChanged("RestartWellNames");


            ECL.RESTART.ReadRestartGrid("OILKR");

            int pos = 0;
            float value = 1;
            int count = 0;
            int index = 0;

            MeshGeometry3D g = new MeshGeometry3D();

            for (int X = 0; X < ECL.INIT.NX; ++X)
                for (int Y = 0; Y < ECL.INIT.NY; ++Y)
                {
                    count = 0;
                    value = 0;

                    for (int Z = 0; Z < ECL.INIT.NZ; ++Z)
                    {
                        index = ECL.INIT.GetActive(X, Y, Z);
                        if (index > 0)
                        {
                            value += ECL.RESTART.GetValue(index - 1);
                            count++;
                        }
                    }

                    if (count > 0) value = (float)value / (float)count;


                    if (count > 0)
                    {
                        g.Positions.Add(new Point3D(X * 10 + 00, Y * 10 + 00, 05));  // 0
                        g.Positions.Add(new Point3D(X * 10 + 10, Y * 10 + 00, 05)); // 1
                        g.Positions.Add(new Point3D(X * 10 + 10, Y * 10 + 10, 05)); // 2
                        g.Positions.Add(new Point3D(X * 10 + 00, Y * 10 + 10, 05)); //3

                        g.TriangleIndices.Add(pos + 0); // 0 - 1 - 2
                        g.TriangleIndices.Add(pos + 1);
                        g.TriangleIndices.Add(pos + 2);

                        g.TriangleIndices.Add(pos + 2); // 2 -  3 - 0
                        g.TriangleIndices.Add(pos + 3);
                        g.TriangleIndices.Add(pos + 0);

                        pos = pos + 4;

                        g.TextureCoordinates.Add(new Point(value, value));
                        g.TextureCoordinates.Add(new Point(value, value));
                        g.TextureCoordinates.Add(new Point(value, value));
                        g.TextureCoordinates.Add(new Point(value, value));
                    }
                }

            /*
            Wells = new Point3DCollection();
            WellNamesItems = new List<SpatialTextItem>();

            foreach (var item in ECL.RESTART.WELLS)
            {
                Wells.Add(new Point3D(item.I * 10 + 5, item.J * 10 + 5, 05));
                Wells.Add(new Point3D(item.I * 10 + 5, item.J * 10 + 5, 15));
                WellNamesItems.Add(new SpatialTextItem
                {
                    Position = new Point3D(item.I * 10 + 5, item.J * 10 + 5, 18),
                    Text = item.WELLNAME,
                    TextDirection = new Vector3D(1, 0, 0),
                    UpDirection = new Vector3D(0, 0, 1)
                });
                }
            */
            LinearGradientBrush b = BrushHelper.CreateRainbowBrush();
            b.StartPoint = new Point(0, 0);
            b.EndPoint = new Point(1, 1);
            b.MappingMode = BrushMappingMode.Absolute;
            Model = new GeometryModel3D(g, MaterialHelper.CreateMaterial(b));

            OnPropertyChanged("Model");
            OnPropertyChanged("Wells");
            OnPropertyChanged("WellNamesItems");
        }

        public void OnStaticPropertySelected(string name)
        {
            ECL.INIT.ReadInitGrid(name);

            int pos = 0;
            float value = 1;
            int count = 0;
            int index = 0;

            MeshGeometry3D g = new MeshGeometry3D();

            for (int X = 0; X < ECL.INIT.NX; ++X)
                for (int Y = 0; Y < ECL.INIT.NY; ++Y)
                {
                    count = 0;
                    value = 0;

                    for (int Z = 0; Z < ECL.INIT.NZ; ++Z)
                    {
                        index = ECL.INIT.GetActive(X, Y, Z);
                        if (index > 0)
                        {
                            value += ECL.INIT.GetValue(index - 1);
                            count++;
                        }
                    }

                    if (count > 0) value = (float)value / (float)count;


                    if (count > 0)
                    {
                        g.Positions.Add(new Point3D(X * 10 + 00, Y * 10 + 00, 05));  // 0
                        g.Positions.Add(new Point3D(X * 10 + 10, Y * 10 + 00, 05)); // 1
                        g.Positions.Add(new Point3D(X * 10 + 10, Y * 10 + 10, 05)); // 2
                        g.Positions.Add(new Point3D(X * 10 + 00, Y * 10 + 10, 05)); //3

                        g.TriangleIndices.Add(pos + 0); // 0 - 1 - 2
                        g.TriangleIndices.Add(pos + 1);
                        g.TriangleIndices.Add(pos + 2);

                        g.TriangleIndices.Add(pos + 2); // 2 -  3 - 0
                        g.TriangleIndices.Add(pos + 3);
                        g.TriangleIndices.Add(pos + 0);

                        pos = pos + 4;

                        g.TextureCoordinates.Add(new Point(value, value));
                        g.TextureCoordinates.Add(new Point(value, value));
                        g.TextureCoordinates.Add(new Point(value, value));
                        g.TextureCoordinates.Add(new Point(value, value));
                    }
                }
            LinearGradientBrush b = BrushHelper.CreateRainbowBrush();
            b.StartPoint = new Point(0, 0);
            b.EndPoint = new Point(1, 1);
            b.MappingMode = BrushMappingMode.Absolute;
            Model = new GeometryModel3D(g, MaterialHelper.CreateMaterial(b));


            OnPropertyChanged("Model");
        }
    }
}