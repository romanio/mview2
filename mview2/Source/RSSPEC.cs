using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mview2
{
    public class RSSPEC
    {
        public List<double> TIME = new List<double>(); // Количество дней с начала расчёта
        public List<int> REPORT = new List<int>();
        public List<int> TYPE_RESTART = new List<int>();
        public List<DateTime> DATE = new List<DateTime>();
        public List<string[]> NAME = new List<string[]>(); // Имя массива
        public List<string[]> TYPE = new List<string[]>(); // Тип массива
        public List<int[]> NUMBER = new List<int[]>(); // Размер векторов
        public List<int[]> POINTER = new List<int[]>(); // Lower part of the address
        public List<int[]> POINTERB = new List<int[]>(); // Upper part of the address
        public List<float[]> ARRAYMAX = new List<float[]>(); // Maximum values
        public List<float[]> ARRAYMIN = new List<float[]>(); // Minimum values
        public List<string[]> UNITS = new List<string[]>(); // Единица измерения

        public RSSPEC(string filename)
        {
            FileReader br = new FileReader();
            br.OpenBinaryFile(filename);

            if (br.Length > 0)
            {
                while (br.Position < br.Length - 24)
                {
                    br.ReadHeader();

                    if (br.header.keyword == "TIME")
                    {
                        br.ReadBytes(4);
                        TIME.Add(br.ReadDouble());
                        br.ReadBytes(4);
                        continue;
                    }

                    if (br.header.keyword == "ITIME")
                    {
                        int[] ITIME = br.ReadIntList();
                        REPORT.Add(ITIME[0]);
                        TYPE_RESTART.Add(ITIME[5]);
                        if (ITIME.Length > 10)
                            DATE.Add(new DateTime(ITIME[3], ITIME[2], ITIME[1], ITIME[10], ITIME[11], (int)(ITIME[12] * 1e6)));
                        else
                            DATE.Add(new DateTime(ITIME[3], ITIME[2], ITIME[1]));

                        continue;
                    }

                    if (br.header.keyword == "NAME")
                    {
                        NAME.Add(br.ReadStringList());
                        continue;
                    }

                    if (br.header.keyword == "TYPE")
                    {
                        TYPE.Add(br.ReadStringList());
                        continue;
                    }

                    if (br.header.keyword == "UNITS")
                    {
                        UNITS.Add(br.ReadStringList());
                        continue;
                    }

                    if (br.header.keyword == "NUMBER")
                    {
                        NUMBER.Add(br.ReadIntList());
                        continue;
                    }

                    if (br.header.keyword == "ARRAYMAX")
                    {
                        ARRAYMAX.Add(br.ReadFloatList(br.header.count));
                        continue;
                    }

                    if (br.header.keyword == "ARRAYMIN")
                    {
                        ARRAYMIN.Add(br.ReadFloatList(br.header.count));
                        continue;
                    }

                    if (br.header.keyword == "POINTER")
                    {
                        POINTER.Add(br.ReadIntList());
                        continue;
                    }

                    if (br.header.keyword == "POINTERB")
                    {
                        POINTERB.Add(br.ReadIntList());
                        continue;
                    }

                    br.SkipEclipseData();
                }
                br.CloseBinaryFile();
            }
        }
    }
}
