using System.Collections.Generic;

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

        public float[] PORV = null;
        public int[] ACTNUM = null;

        public void ReadInit(string filename)
        {
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
                        PORV = br.ReadFloatList(br.header.count);

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

                    System.Diagnostics.Debug.WriteLine(br.header.keyword);
                    br.SkipEclipseData();
                }
            }
            br.CloseBinaryFile();
        }
    }
}
