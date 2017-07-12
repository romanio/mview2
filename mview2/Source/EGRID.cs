using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
