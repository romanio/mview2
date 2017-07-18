using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace mview2
{
    public class BigArray<T>
    {
        internal const int BLOCK_SIZE = 524288;
        internal const int BLOCK_SIZE_LOG2 = 24;

        T[][] _elements;
        ulong _length;

        public BigArray(ulong size)
        {
            int numBlocks = (int)(size / BLOCK_SIZE);
            if ((ulong)(numBlocks * BLOCK_SIZE) < size)
                numBlocks++;

            _length = size;
            _elements = new T[numBlocks][];

            for (int iw = 0; iw < (numBlocks - 1); iw++)
                _elements[iw] = new T[BLOCK_SIZE];

            _elements[numBlocks - 1] = new T[size - (ulong)((numBlocks - 1) * BLOCK_SIZE)];
            numBlocks++;
        }

        public ulong Length
        {
            get
            {
                return _length;
            }
        }
        public T this[ulong index]
        {
            get
            {
                int blockNum = (int)(index >> BLOCK_SIZE_LOG2);
                int indexInBlock = (int)(index & (BLOCK_SIZE - 1));
                return _elements[blockNum][indexInBlock];
            }
            set
            {
                int blockNum = (int)(index >> BLOCK_SIZE_LOG2);
                int indexInBlock = (int)(index & (BLOCK_SIZE - 1));
                _elements[blockNum][indexInBlock] = value;
            }
        }
    }

    public struct Cell
    {
        public Point3D TNW;
        public Point3D TNE;
        public Point3D TSW;
        public Point3D TSE;
        public Point3D BNW;
        public Point3D BNE;
        public Point3D BSW;
        public Point3D BSE;
    }

    public class EGRID
    {
        public int[] FILEHEAD = null;
        public int[] GRIDHEAD = null;
        public int GRIDTYPE;
        public int DUALPORO;
        public int FORMATDATA;
        public string MAPUNITS;
        public float XORIGIN;
        public float YORIGIN;
        public float XENDYAXIS;
        public float YENDYAXIS;
        public float XENDXAXIS;
        public float YENDXAXIS;
        public int NX;
        public int NY;
        public int NZ;
        public float[] COORD = null;
        public BigArray<float> ZCORN = null;
        public int[] ACTNUM = null;
        public int NACTIV; // Количество активных ячеек

        public EGRID(string filename)
        {
            FileReader br = new FileReader();
            br.OpenBinaryFile(filename);

            System.Diagnostics.Debug.WriteLine("EGRID");

            while (br.Position < br.Length - 24)
            {
                br.ReadHeader();
                if (br.header.keyword == "FILEHEAD")
                {
                    FILEHEAD = br.ReadIntList();
                    GRIDTYPE = FILEHEAD[4];
                    DUALPORO = FILEHEAD[5];
                    FORMATDATA = FILEHEAD[6];
                    continue;
                }

                if (br.header.keyword == "MAPUNITS")
                {
                    br.ReadBytes(4);
                    MAPUNITS = br.ReadString(8);
                    br.ReadBytes(4);
                    continue;
                }

                if (br.header.keyword == "MAPAXES")
                {
                    br.ReadBytes(4);
                    XENDYAXIS = br.ReadFloat();
                    YENDYAXIS = br.ReadFloat();
                    XORIGIN = br.ReadFloat();
                    YORIGIN = br.ReadFloat();
                    XENDXAXIS = br.ReadFloat();
                    YENDXAXIS = br.ReadFloat();
                    br.ReadBytes(4);
                    continue;
                }

                if (br.header.keyword == "GRIDHEAD")
                {
                    GRIDHEAD = br.ReadIntList();
                    NX = GRIDHEAD[1];
                    NY = GRIDHEAD[2];
                    NZ = GRIDHEAD[3];
                    continue;
                }

                if (br.header.keyword == "COORD")
                {
                    COORD = br.ReadFloatList(6 * (NY + 1) * (NX + 1));
                    continue;
                }

                if (br.header.keyword == "ZCORN")
                {
                    ZCORN = br.ReadBigList((ulong)(8 * NX * NY * NZ));
                    continue;
                }

                if (br.header.keyword == "ACTNUM")
                {
                    ACTNUM = br.ReadIntList();

                    // Для сжатого формата хранения данных
                    // требуется провести индексирования массива активных ячеек
                    // ACTNUM сам по себе это "1" для активной ячейки и "0" для не активной

                    int index = 1;
                    for (int iw = 0; iw < ACTNUM.Length; ++iw)
                        if (ACTNUM[iw] > 0) ACTNUM[iw] = index++;

                    NACTIV = index - 1;

                    // Теперь ACTNUM хранит ещё и индекс ячейки INDEX, который есть просто линейный порядковый номер
                    // увеличенный на "1", так как надо сохранить нулевые значения в неактивных ячейках
                    continue;
                }

                System.Diagnostics.Debug.WriteLine(br.header.keyword);
                br.SkipEclipseData();
            }
            br.CloseBinaryFile();
        }

        public bool CheckEasyNeighbour(int X, int Y, int Z)
        {
            if (X > 0)
                if (GetActive(X - 1, Y, Z) == 0) return false;
                    
            if (Y > 0)
                if (GetActive(X , Y - 1, Z) == 0) return false;

            if (Z > 0)
                if (GetActive(X, Y, Z - 1) == 0) return false;

            if (X < (NX - 1))
                if (GetActive(X + 1, Y, Z) == 0) return false;

            if (Y < (NY - 1))
                if (GetActive(X, Y + 1, Z) == 0) return false;

            if (Z < (NZ - 1))
                if (GetActive(X, Y, Z + 1) == 0) return false;

            return true;
        }

        public int GetActive(int X, int Y, int Z)
        {
            return ACTNUM[X + NX * Y + Z * NX * NY];
        }

        public Cell GetCell(int X, int Y, int Z)
        {
            // Формат именования вершин в кубе.
            // На первом месте либо T (top, верхняя грань), либо B (bottom, нижняя грань)
            // далее N (north, северная, условный верх) либо S (south, южная, условный низ) грань 
            // и завершается  W( west, западная, условное лево) либо E (east, восточное, условное право).
            //Таким образом, трехбуквенным кодом обозначаются восемь вершин одной ячейки.
            // Это распространенный подход.

            Cell CELL = new Cell();

            // Отметки глубин

            CELL.TNW.Z = ZCORN[(ulong)(Z * NX * NY * 8 + Y * NX * 4 + 2 * X + 0)];
            CELL.TNE.Z = ZCORN[(ulong)(Z * NX * NY * 8 + Y * NX * 4 + 2 * X + 1)];
            CELL.TSW.Z = ZCORN[(ulong)(Z * NX * NY * 8 + Y * NX * 4 + 2 * X + NX * 2 + 0)];
            CELL.TSE.Z = ZCORN[(ulong)(Z * NX * NY * 8 + Y * NX * 4 + 2 * X + NX * 2 + 1)];

            CELL.BNW.Z = ZCORN[(ulong)(Z * NX * NY * 8 + Y * NX * 4 + NX * NY * 4 + 2 * X + 0)];
            CELL.BNE.Z = ZCORN[(ulong)(Z * NX * NY * 8 + Y * NX * 4 + NX * NY * 4 + 2 * X + 1)];
            CELL.BSW.Z = ZCORN[(ulong)(Z * NX * NY * 8 + Y * NX * 4 + NX * NY * 4 + 2 * X + NX * 2 + 0)];
            CELL.BSE.Z = ZCORN[(ulong)(Z * NX * NY * 8 + Y * NX * 4 + NX * NY * 4 + 2 * X + NX * 2 + 1)];

            // Направляющая линия от TNW до BNW

            Point3D TOP = new Point3D();
            Point3D BTM = new Point3D();

            TOP.X = COORD[(X + (NX + 1) * Y) * 6 + 0];
            TOP.Y = COORD[(X + (NX + 1) * Y) * 6 + 1];
            TOP.Z = COORD[(X + (NX + 1) * Y) * 6 + 2];

            BTM.X = COORD[(X + (NX + 1) * Y) * 6 + 3 + 0];
            BTM.Y = COORD[(X + (NX + 1) * Y) * 6 + 3 + 1];
            BTM.Z = COORD[(X + (NX + 1) * Y) * 6 + 3 + 2];

            double FRAC = 0;

            if (BTM.Z == TOP.Z) // нет наклона направляющей линии, значит координаты равны
            {
                CELL.TNW.X = TOP.X;
                CELL.TNW.Y = TOP.Y;
                CELL.BNW.X = BTM.X;
                CELL.BNW.Y = BTM.Y;
            }
            else
            {
                FRAC = (CELL.TNW.Z - TOP.Z) / (BTM.Z - TOP.Z);
                CELL.TNW.X = TOP.X + FRAC * (BTM.X - TOP.X);
                CELL.TNW.Y = TOP.Y + FRAC * (BTM.Y - TOP.Y);

                FRAC = (CELL.BNW.Z - TOP.Z) / (BTM.Z - TOP.Z);
                CELL.BNW.X = TOP.X + FRAC * (BTM.X - TOP.X);
                CELL.BNW.Y = TOP.Y + FRAC * (BTM.Y - TOP.Y);
            }

            // Направляющая линия от TNE до BNE

            TOP.X = COORD[((X + 1) + (NX + 1) * Y) * 6 + 0];
            TOP.Y = COORD[((X + 1) + (NX + 1) * Y) * 6 + 1];
            TOP.Z = COORD[((X + 1) + (NX + 1) * Y) * 6 + 2];

            BTM.X = COORD[((X + 1) + (NX + 1) * Y) * 6 + 3 + 0];
            BTM.Y = COORD[((X + 1) + (NX + 1) * Y) * 6 + 3 + 1];
            BTM.Z = COORD[((X + 1) + (NX + 1) * Y) * 6 + 3 + 2];

            if (BTM.Z == TOP.Z) // нет наклона направляющей линии, значит координаты равны
            {
                CELL.TNE.X = TOP.X;
                CELL.TNE.Y = TOP.Y;
                CELL.BNE.X = BTM.X;
                CELL.BNE.Y = BTM.Y;
            }
            else
            {
                FRAC = (CELL.TNE.Z - TOP.Z) / (BTM.Z - TOP.Z);
                CELL.TNE.X = TOP.X + FRAC * (BTM.X - TOP.X);
                CELL.TNE.Y = TOP.Y + FRAC * (BTM.Y - TOP.Y);

                FRAC = (CELL.BNE.Z - TOP.Z) / (BTM.Z - TOP.Z);
                CELL.BNE.X = TOP.X + FRAC * (BTM.X - TOP.X);
                CELL.BNE.Y = TOP.Y + FRAC * (BTM.Y - TOP.Y);
            }

            // Направляющая линия от TSE до BSE

            TOP.X = COORD[((X + 1) + (NX + 1) * (Y + 1)) * 6 + 0];
            TOP.Y = COORD[((X + 1) + (NX + 1) * (Y + 1)) * 6 + 1];
            TOP.Z = COORD[((X + 1) + (NX + 1) * (Y + 1)) * 6 + 2];

            BTM.X = COORD[((X + 1) + (NX + 1) * (Y + 1)) * 6 + 3 + 0];
            BTM.Y = COORD[((X + 1) + (NX + 1) * (Y + 1)) * 6 + 3 + 1];
            BTM.Z = COORD[((X + 1) + (NX + 1) * (Y + 1)) * 6 + 3 + 2];

            if (BTM.Z == TOP.Z) // нет наклона направляющей линии, значит координаты равны
            {
                CELL.TSE.X = TOP.X;
                CELL.TSE.Y = TOP.Y;
                CELL.BSE.X = BTM.X;
                CELL.BSE.Y = BTM.Y;
            }
            else
            {
                FRAC = (CELL.TSE.Z - TOP.Z) / (BTM.Z - TOP.Z);
                CELL.TSE.X = TOP.X + FRAC * (BTM.X - TOP.X);
                CELL.TSE.Y = TOP.Y + FRAC * (BTM.Y - TOP.Y);

                FRAC = (CELL.BSE.Z - TOP.Z) / (BTM.Z - TOP.Z);
                CELL.BSE.X = TOP.X + FRAC * (BTM.X - TOP.X);
                CELL.BSE.Y = TOP.Y + FRAC * (BTM.Y - TOP.Y);
            }

            // Направляющая линия от TSW до BSW

            TOP.X = COORD[(X + (NX + 1) * (Y + 1)) * 6 + 0];
            TOP.Y = COORD[(X + (NX + 1) * (Y + 1)) * 6 + 1];
            TOP.Z = COORD[(X + (NX + 1) * (Y + 1)) * 6 + 2];

            BTM.X = COORD[(X + (NX + 1) * (Y + 1)) * 6 + 3 + 0];
            BTM.Y = COORD[(X + (NX + 1) * (Y + 1)) * 6 + 3 + 1];
            BTM.Z = COORD[(X + (NX + 1) * (Y + 1)) * 6 + 3 + 2];

            if (BTM.Z == TOP.Z) // нет наклона направляющей линии, значит координаты равны
            {
                CELL.TSW.X = TOP.X;
                CELL.TSW.Y = TOP.Y;
                CELL.BSW.X = BTM.X;
                CELL.BSW.Y = BTM.Y;
            }
            else
            {
                FRAC = (CELL.TSW.Z - TOP.Z) / (BTM.Z - TOP.Z);
                CELL.TSW.X = TOP.X + FRAC * (BTM.X - TOP.X);
                CELL.TSW.Y = TOP.Y + FRAC * (BTM.Y - TOP.Y);

                FRAC = (CELL.BSW.Z - TOP.Z) / (BTM.Z - TOP.Z);
                CELL.BSW.X = TOP.X + FRAC * (BTM.X - TOP.X);
                CELL.BSW.Y = TOP.Y + FRAC * (BTM.Y - TOP.Y);
            }

            return CELL;
        }
    }

}
