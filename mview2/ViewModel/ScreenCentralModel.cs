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
        public PlotModel PlotModel { get; set; }

        public ScreenCentralModel()
        {
            PlotModel = new PlotModel { Title = "Example" };
            double x0 = -3.1;
            double x1 = 3.1;
            double y0 = -3;
            double y1 = 3;

            //generate values

            var xx = new double[] { 2, 3, 4, 5, 6};
            var yy = new double[] { 2, 3, 4 };
            var peaksData = new double[,] { { 0, 4, 2,8,-2 }, { 0, -4, 2,6,2 }, { 0, 1, 8,2,0 } };

            var cs = new ContourSeries
            {
                Color = OxyColors.Black,
                LabelBackground = OxyColors.White,
                LabelStep = 1,
                LabelSpacing = 1,
                ColumnCoordinates = yy,
                RowCoordinates = xx,
                ContourLevelStep =1,
          
                Data = peaksData
            };

            cs.CalculateContours();
            PlotModel.Series.Add(cs);
        }
    }
}
