using System.Collections.Generic;
using System;

namespace mview2
{
    public class INSPEC
    {
        public List<string[]> NAME = new List<string[]>(); // Имя массива
        public List<string[]> TYPE = new List<string[]>(); // Тип массива
        public List<int[]> NUMBER = new List<int[]>(); // Размер векторов
        public List<int[]> POINTER = new List<int[]>(); // Lower part of the address
        public List<int[]> POINTERB = new List<int[]>(); // Upper part of the address
        public List<float[]> ARRAYMAX = new List<float[]>(); // Maximum values
        public List<float[]> ARRAYMIN = new List<float[]>(); // Minimum values
        public List<string[]> UNITS = new List<string[]>(); // Единица измерения
        public string FILENAME = null;

        public INSPEC(string filename)
        {
            FileReader br = new FileReader();
            br.OpenBinaryFile(filename);

            if (br.Length > 0)
            {
                while (br.Position < br.Length - 24)
                {
                    br.ReadHeader();

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
        public int[] ACTNUM = null;

        public int GetActive(int X, int Y, int Z)
        {
            return ACTNUM[X + NX * Y + Z * NX * NY];
        }

        public void ReadACTNUM(string filename)
        {
            FILENAME = filename;
            FileReader br = new FileReader();
            br.OpenBinaryFile(filename);

            if (br.Length > 0)
            {
                while (br.Position < br.Length - 24)
                {
                    br.ReadHeader();

                    if (br.header.keyword == "INTEHEAD")
                    {
                        int[] INTH = br.ReadIntList();
                        NX = INTH[8];
                        NY = INTH[9];
                        NZ = INTH[10];
                        NACTIV = INTH[11];
                        IPHS = INTH[14];
                        continue;
                    }

                    if (br.header.keyword == "PORV")
                    {
                        var PORV = br.ReadFloatList(br.header.count);

                        ACTNUM = new int[NX * NY * NZ];

                        // Для сжатого формата хранения данных
                        // требуется провести индексирования массива активных ячеек
                        // ACTNUM сам по себе это "1" для активной ячейки и "0" для не активной

                        int index = 1;
                        for (int iw = 0; iw < PORV.Length; ++iw)
                            if (PORV[iw] > 0)
                                ACTNUM[iw] = index++;

                        break;
                    }
                    br.SkipEclipseData();
                }
            }
            br.CloseBinaryFile();
        }

        public float[] DATA = null;

        public void ReadInitGrid(string property)
        {
            FileReader br = new FileReader();

            Action<string> SetPosition = (name) =>
            {
                int INIT_STEP = 0;
                int index = -1;

                for (int iw = 0; iw < NAME.Count; ++iw)
                {
                    index = Array.IndexOf(NAME[iw], name);
                    if (index > -1)
                    {
                        INIT_STEP = iw;
                        break;
                    }
                }

                long pointer = POINTER[INIT_STEP][index];
                long pointerb = POINTERB[INIT_STEP][index];
                br.SetPosition(pointerb * 2147483648 + pointer);
            };

            br.OpenBinaryFile(FILENAME);
            SetPosition(property);
            br.ReadHeader();
            DATA = br.ReadFloatList(br.header.count);
            //
            br.CloseBinaryFile();
        }
        public float GetValue(int index)
        {
            return DATA[index];
        }
    }
}