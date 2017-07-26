using System;
using System.Collections.Generic;

namespace mview2
{
    public class WELLDATA
    {
        public string WELLNAME;
        public int I;
        public int J;
        public int K;
        public int COMPLNUM;
        public int GROUPNUM;
        public int WELLTYPE;
        public int WELLSTATUS;
        public float WOPRH;
        public float WWPRH;
        public float WGPRH;
        public float WLPRH;
        public float REF_DEPTH;
        public float WEFA;
        public float BHPH;
        public double WOPR;
        public double WWPR;
        public double WLPR;
        public double WBHP;
        public double WWCT;
        public double WOPT;
        public double WWPT;
        public double WWIT;
        public double WPI;
        public double WOPTH;
        public double WWPTH;
        public double WWITH;
    }
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
        public string FILENAME = null;

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
        public int NX, NY, NZ; // Размер по X, Y, Z
        public int NACTIV; // Количество активных ячеек
        public int IPHS; // Индикатор фазы
        public int NWELLS; // Количество скважин
        public int NCWMAX; // Максимальное количество перфораций на одну скважину
        public int NIWELZ; // Количество элементов данных в IWEL (int значения)
        public int NSWELZ; // Количество элементов данных в SWEL (float значения)
        public int NXWELZ; // Количество элементов данных в XWEL (double значения)
        public int NZWELZ; // Количество элементов данных в ZWEL (string значения)
        public int NICONZ; // Количество элементов данных в ICON (int значения)
        public int NSCONZ; // Количество элементов данных в SCON (float значения)
        public int NXCONZ; // Количество элементов данных в XCON (double значения)

        // Разворачивание в человеческий вид содержимое рестарт файла

        public List<WELLDATA> WELLS;
        public int RESTART_STEP;
        public float[] DATA = null;

        public void ReadRestart(string filename, int step)
        {
            FILENAME = filename;
            RESTART_STEP = step;

            FileReader br = new FileReader();

            Action<string> SetPosition = (name) =>
            {
                int index = Array.IndexOf(NAME[step], name);
                long pointer = POINTER[step][index];
                long pointerb = POINTERB[step][index];
                br.SetPosition(pointerb * 2147483648 + pointer);
            };


            WELLS = new List<WELLDATA>();
            br.OpenBinaryFile(filename);
            SetPosition("INTEHEAD");
            br.ReadHeader();
            int[] INTH = br.ReadIntList();
            NX = INTH[8];
            NY = INTH[9];
            NZ = INTH[10];
            NACTIV = INTH[11];
            IPHS = INTH[14];
            NWELLS = INTH[16];
            NCWMAX = INTH[17];
            NIWELZ = INTH[24];
            NSWELZ = INTH[25];
            NXWELZ = INTH[26];
            NZWELZ = INTH[27];
            NICONZ = INTH[32];
            NSCONZ = INTH[33];
            NXCONZ = INTH[34];

            SetPosition("IWEL");
            br.ReadHeader();
            int[] IWEL = br.ReadIntList();

            for (int iw = 0; iw < NWELLS; ++iw)
            {
                WELLS.Add(new WELLDATA
                {
                    I = IWEL[iw * NIWELZ + 0],
                    J = IWEL[iw * NIWELZ + 1],
                    K = IWEL[iw * NIWELZ + 2],
                    COMPLNUM = IWEL[iw * NIWELZ + 4],
                    GROUPNUM = IWEL[iw * NIWELZ + 5],
                    WELLTYPE = IWEL[iw * NIWELZ + 6],
                    WELLSTATUS = IWEL[iw * NIWELZ + 10]
                });
            }

            SetPosition("SWEL");
            br.ReadHeader();

            float[] SWEL = br.ReadFloatList(br.header.count);

            for (int iw = 0; iw < NWELLS; ++iw)
            {
                WELLS[iw].WOPRH = SWEL[0];
                WELLS[iw].WWPRH = SWEL[1];
                WELLS[iw].WGPRH = SWEL[2];
                WELLS[iw].WLPRH = SWEL[3];
                WELLS[iw].REF_DEPTH = SWEL[9];
                WELLS[iw].WEFA = SWEL[24];
                WELLS[iw].BHPH = SWEL[68];
            }

            SetPosition("XWEL");
            br.ReadHeader();

            double[] XWEL = br.ReadDoubleList();

            for (int iw = 0; iw < NWELLS; ++iw)
            {
                WELLS[iw].WOPR = XWEL[0];
                WELLS[iw].WWPR = XWEL[1];
                WELLS[iw].WLPR = XWEL[3];
                WELLS[iw].WBHP = XWEL[6];
                WELLS[iw].WWCT = XWEL[7];
                WELLS[iw].WOPT = XWEL[18];
                WELLS[iw].WWPT = XWEL[19];
                WELLS[iw].WWIT = XWEL[23];
                WELLS[iw].WPI = XWEL[55];
                WELLS[iw].WOPTH = XWEL[75];
                WELLS[iw].WWPTH = XWEL[76];
                WELLS[iw].WWITH = XWEL[81];
                WELLS[iw].WOPTH = XWEL[75];
            }

            SetPosition("ZWEL");
            br.ReadHeader();

            string[] ZWEL = br.ReadStringList();

            for (int iw = 0; iw < NWELLS; ++iw)
            {
                WELLS[iw].WELLNAME = ZWEL[iw * NZWELZ + 0];
            }
            br.CloseBinaryFile();
        }

        public float GetValue(int index)
        {
            return DATA[index];
        }

        public void ReadRestartGrid(string property)
        {
            FileReader br = new FileReader();

            Action<string> SetPosition = (name) =>
            {
                int index = Array.IndexOf(NAME[RESTART_STEP], name);
                long pointer = POINTER[RESTART_STEP][index];
                long pointerb = POINTERB[RESTART_STEP][index];
                br.SetPosition(pointerb * 2147483648 + pointer);
            };

            br.OpenBinaryFile(FILENAME);
            SetPosition(property);
            br.ReadHeader();
            DATA = br.ReadFloatList(br.header.count);
            //
            br.CloseBinaryFile();
        }
    }
}