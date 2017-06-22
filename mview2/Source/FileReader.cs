﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace mview2
{
    public struct Header
    {
        public string keyword;
        public string type;
        public int count;
    }

    public class FileReader
    {
        FileStream fs = null;
        BinaryReader br = null;
        public Header header;

        public long Length = 0;
        public long Position = 0;

        public void OpenBinaryFile(string filename)
        {
            fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            br = new BinaryReader(fs, System.Text.Encoding.GetEncoding(1251));
            Length = fs.Length;
            Position = 0;
        }

        public void CloseBinaryFile()
        {
            br.Close();
            fs.Close();
            Length = 0;
            Position = 0;
        }

        public void ReadHeader()
        {
            // Перед блоком данных всегда следует заголовок,
            // в котором указывается имя параметра, тип и количество элементов

            ReadBytes(4);
            header.keyword = ReadString(8).Trim();
            header.count = ReadInt32();
            header.type = ReadString(4);
            ReadBytes(4);
        }

        public void SkipEclipseData()
        {
            // Если блок данных не представляет интереса, можно его пропустить.
            // В зависимости от типа данных, расчет количества пропускаемых байт меняется:
            //
            // "CHAR" поделен на строки по 8 символов, в одном блоке 105 строк;
            // "INTE, LOGI, REAL" это 4 байтные элементы, в одном блоке 1000 элементов;
            // "DOUB" это 8 байтные элементы, в одной блоке также 1000 элементов.
            //
            // В конце и перед началом каждого блока записано по 4 байта. Это тяжелое наследние FORTRAN.
            // Последний блок может быть не целым, чтобы понять сколько байт содержится в последнем блоке,
            // расчитывается количество целых блоков "block" и количество байт, оставшихся в последнем блоке "mod".

            if (header.type == "CHAR")
            {
                int block = header.count / 105;
                int mod = header.count - block * 105;
                if (block > 0)
                    ReadBytes((2 * 4 + 8 * 105) * block);
                if (mod > 0)
                    ReadBytes(2 * 4 + 8 * mod);
            }

            if (header.type == "INTE" || header.type == "LOGI" || header.type == "REAL")
            {
                int block = header.count / 1000;
                int mod = header.count - block * 1000;
                if (block > 0)
                    ReadBytes((2 * 4 + 4 * 1000) * block);
                if (mod > 0)
                    ReadBytes(2 * 4 + 4 * mod);
            }

            if (header.type == "DOUB")
            {
                int block = header.count / 1000;
                int mod = header.count - block * 1000;
                if (block > 0)
                    ReadBytes((2 * 4 + 8 * 1000) * block);
                if (mod > 0)
                    ReadBytes(2 * 4 + 8 * mod);
            }
        }

        // Пропускает заданное количество байт от текущей позиции в файле
        public void ReadBytes(int count)
        {
            Position += count;
            fs.Seek(count, SeekOrigin.Current);
        }

        public string ReadString(int count)
        {
            Position += count;
            return new string(br.ReadChars(count));
        }

        public string[] ReadStringList()
        {
            string[] list = new string[header.count];
            int index = 0;
            int block = header.count / 105;
            int mod = header.count - block * 105;
            while (block > 0)
            {
                ReadBytes(4);
                for (int iw = 0; iw < 105; ++iw)
                    list[index++] = ReadString(8).Trim();
                ReadBytes(4);
                block--;
            }
            if (mod > 0)
            {
                ReadBytes(4);
                while (mod > 0)
                {
                    list[index++] = ReadString(8).Trim();
                    mod--;
                }
                ReadBytes(4);
            }
            return list;
        }

        // Функции чтения целых чисел
        public Int32 ReadInt32()
        {
            Position += 4;
            return SwapInt32(br.ReadInt32());
        }

        unsafe public float[] ReadFloatList(int count)
        {
            // Процедура аналогична ReadIntList
            float[] list = new float[header.count];

            int index = 0;
            int bindex = 0;
            int block = count / 1000;
            int mod = count - block * 1000;

            int buflen = 0;
            if (block > 0)
                buflen = (2 * 4 + 4000) * block;
            if (mod > 0)
                buflen += 2 * 4 + 4 * mod;

            byte[] nums = br.ReadBytes(buflen);
            Position += buflen;
            int local;

            while (block > 0)
            {
                bindex += 4;

                for (int iw = 0; iw < 1000; ++iw)
                {
                    local = (nums[bindex + 3]) | (nums[bindex + 2] << 8) | (nums[bindex + 1] << 0x10) | (nums[bindex] << 0x18);
                    list[index++] = *(float*)(&local);
                    bindex += 4;
                }
                bindex += 4;
                block--;
            }

            if (mod > 0)
            {
                bindex += 4;
                while (mod > 0)
                {
                    local = (nums[bindex + 3]) | (nums[bindex + 2] << 8) | (nums[bindex + 1] << 0x10) | (nums[bindex] << 0x18);
                    list[index++] = *(float*)(&local);
                    bindex += 4;
                    mod--;
                }
                bindex += 4;
            }
            return list;
        }

        public int[] ReadIntList()
        {
            // Считывания массива целых чисел. Вариант кажется мне самым быстрым
            // Сначала расчитывается требуемый буфер байт, исходя из количества целых блоков "block"
            // и количества байт в последнем, нецелом блоке "mod" и затем происходит конвертирование из Big Endian
            // Ускорение достигается за счет одновременного считывания из бинарного файла в буфер.

            int[] list = new int[header.count];
            int index = 0;
            int bindex = 0;
            int block = header.count / 1000;
            int mod = header.count - block * 1000;

            int buflen = 0;
            if (block > 0)
                buflen = (2 * 4 + 4000) * block;
            if (mod > 0)
                buflen += 2 * 4 + 4 * mod;

            byte[] nums = br.ReadBytes(buflen); // Одновременное считывание всего массива данных
            Position += buflen;

            while (block > 0)
            {
                bindex += 4;
                for (int iw = 0; iw < 1000; ++iw) // Конвертирование целых блоков
                {
                    list[index++] = (nums[bindex + 3]) | (nums[bindex + 2] << 8) | (nums[bindex + 1] << 0x10) | (nums[bindex] << 0x18);
                    bindex += 4;
                }
                bindex += 4;
                block--;
            }

            if (mod > 0) // И конвертация остатка
            {
                bindex += 4;
                while (mod > 0)
                {
                    list[index++] = (nums[bindex + 3]) | (nums[bindex + 2] << 8) | (nums[bindex + 1] << 0x10) | (nums[bindex] << 0x18);
                    bindex += 4;
                    mod--;
                }
                bindex += 4;
            }
            return list;
        }

        Int64 ReadInt64()
        {
            Position += 8;
            return SwapInt64(br.ReadInt64());
        }
        // Функции Swap меняют байты местам, чтобы перейти от системыBig Endian,
        // которая не используется на процессорах x86 и x64, то есть сейчас не используется нигде.

        Int16 SwapInt16(short value)
        {
            return (short)(((value & 0xff) << 8) | ((value >> 8) & 0xff));
        }

        Int32 SwapInt32(int value)
        {
            return (int)(((SwapInt16((short)value) & 0xffff) << 0x10) | (SwapInt16((short)(value >> 0x10)) & 0xffff));
        }

        Int64 SwapInt64(long value)
        {
            return (Int64)(((SwapInt32((int)value) & 0xffffffffL) << 0x20) | (SwapInt32((int)(value >> 0x20)) & 0xffffffffL));
        }

        // Функции чтения чисел с плавающей запятой.
        // Порядок перевода от Big Endian тот же что и для целых чисел, легче всего
        // считать целое число и затем разименовать в плавающее.

        unsafe public float ReadFloat()
        {
            Int32 num = ReadInt32();
            return *(float*)&num;
        }

        unsafe public double ReadDouble()
        {
            Int64 num = ReadInt64();
            return *(double*)&num;
        }
    }
}
