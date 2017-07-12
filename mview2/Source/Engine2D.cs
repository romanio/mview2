using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Windows.Forms;

namespace mview2
{
    public class Engine2D
    {
        public void Paint()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.Texture2D);

            // Масштабирование и перенос области отображения

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Scale(scale, scale, 1);
            GL.Translate(shX + shXEnd - shXStart, -shY + shYEnd - shYStart, 0); // Сдвиг за счет мышки
            GL.Translate((-xmin - 0.5 * (xmax - xmin)), (-ymin - 0.5 * (ymax - ymin)), 0); // Центрирование

            // Отрисовка ячеек

            GL.PolygonOffset(+1, +1);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.DrawElements(PrimitiveType.Quads, ElementCount, DrawElementsType.UnsignedInt, 0);

            GL.PolygonOffset(0, 0);
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.Color3(Color.Black);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.DrawElements(PrimitiveType.Quads, ElementCount, DrawElementsType.UnsignedInt, 0);

            // Рамка

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Begin(PrimitiveType.Lines);
            GL.LineWidth(4);
            GL.Color3(Color.Black);
            GL.Vertex3(xmin, ymin, 0);
            GL.Vertex3(xmin, ymax, 0);
            GL.Vertex3(xmin, ymax, 0);
            GL.Vertex3(xmax, ymax, 0);
            GL.Vertex3(xmax, ymax, 0);
            GL.Vertex3(xmax, ymin, 0);
            GL.Vertex3(xmax, ymin, 0);
            GL.Vertex3(xmin, ymin, 0);
            GL.End();
        }

        public int width, height; // Параметры окна вывода

        float xmin = 0;
        float xmax = 100;
        float ymin = 0;
        float ymax = 100;

        public void SetLimits(float _xmin, float _xmax, float _ymin, float _ymax)
        {
            xmin = 0;
            xmax = (_xmax - _xmin);
            ymin = 0;
            ymax = (_ymax - _ymin);

            // Calc optimal scale rate

            float xscale = 0.95f * width / Math.Abs(xmax - xmin);
            float yscale = 0.95f * height / Math.Abs(ymax - ymin);

            scale = xscale;
            if (yscale < xscale) scale = yscale;

            shX = 0;
            shY = 0;
        }

        public void Resize(int _width, int _height)
        {
            width = _width;
            height = _height;

            // При изменении размера окна, необходимо определить новые координаты OpenGL
            // и задать новые размеры текстуры, для вывода текста

            GL.ClearColor(Color.White);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            // Центр координат устанавливается в центре окна. Можно было и сместить центр координат в левый верхний угол
            // При использовании центральных точек отсчета, запись несколько тяжеловата

            GL.Ortho(-0.5 * width, +0.5 * width, +0.5 * height, -0.5 * height, -1, +1);
            GL.Viewport(0, 0, width, height);

            /*
            if (render != null) render.Dispose(); // Удаляем старый рендер текста
            render = new TextRender(Width, Height); // И объявляем новый
            */

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        // Переменные управления мышью

        float scale = 1; // Масштабирующий коэффициент
        bool IsMouseShifting = false; // В процессе сдвига
        float shXStart, shYStart;
        float shXEnd, shYEnd;
        float shX, shY;

        public void MouseMove(MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:
                    if (!IsMouseShifting)
                    {
                        shXStart = e.X / scale;
                        shYStart = e.Y / scale;
                    }
                    shXEnd = e.X / scale;
                    shYEnd = e.Y / scale;

                    IsMouseShifting = true;
                    break;
                default:
                    if (IsMouseShifting == true)
                    {
                        IsMouseShifting = false;
                        shX += shXEnd - shXStart;
                        shY += -shYEnd + shYStart;
                        shXEnd = 0;
                        shXStart = 0;
                        shYEnd = 0;
                        shYStart = 0;
                    }
                    IsMouseShifting = false;
                    break;
            }
        }

        public void MouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0) scale *= 1.05f;
            if (e.Delta < 0) scale *= 0.95f;
        }

        public void MouseClick(MouseEventArgs e)
        {
        }

        int vboID;
        int eboID;

        public void Load()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);

            vboID = GL.GenBuffer();
            eboID = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboID);
        }

        public void Unload()
        {
            GL.DeleteBuffer(vboID);
            GL.DeleteBuffer(eboID);
        }

        EGRID egrid;
        int ElementCount;

        public void GenerateStructure(EGRID _egrid)
        {
            egrid = _egrid;
            IntPtr VertexIntPtr;
            IntPtr ElementIntPtr;
            float value;
            int index = 0;
            Color color;
            Cell CELL;

            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)(egrid.NX * egrid.NY * sizeof(float) * 3 * 4 + egrid.NX * egrid.NY * sizeof(byte) * 4 * 3),
                IntPtr.Zero, BufferUsageHint.StaticDraw);

            VertexIntPtr = GL.MapBuffer(
                BufferTarget.ArrayBuffer, BufferAccess.ReadWrite);

            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                (IntPtr)(egrid.NX * egrid.NY * sizeof(int) * 4), IntPtr.Zero, BufferUsageHint.StaticDraw);

            ElementIntPtr = GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.ReadWrite);

            GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
            GL.ColorPointer(3, ColorPointerType.UnsignedByte, 0, egrid.NX * egrid.NY * sizeof(float) * 3 * 4);

            unsafe
            {
                float* vertexMemory = (float*)VertexIntPtr;
                int* indexMemory = (int*)ElementIntPtr;
                byte* colorMemory = (byte*)(VertexIntPtr + egrid.NX * egrid.NY * sizeof(float) * 3 * 4);

                for (int X = 0; X < egrid.NX; ++X)
                    for (int Y = 0; Y < egrid.NY; ++Y)
                    {
                        value = GetCellIndex(X, Y, 0);
                        value = 1;

                        if (value > 0)
                        {
                            CELL = GetCell(X, Y, 0);
                            color = Color.Red;

                            //color = colorizer.ColorByValue(value);

                            indexMemory[index] = index;
                            vertexMemory[index * 3 + 0] = CELL.TNW.X;
                            vertexMemory[index * 3 + 1] = CELL.TNW.Y;
                            vertexMemory[index * 3 + 2] = 0.1f;

                            colorMemory[index * 3 + 0] = color.R;
                            colorMemory[index * 3 + 1] = color.G;
                            colorMemory[index * 3 + 2] = color.B;

                            index++;

                            indexMemory[index] = index;
                            vertexMemory[index * 3 + 0] = CELL.TNE.X;
                            vertexMemory[index * 3 + 1] = CELL.TNE.Y;
                            vertexMemory[index * 3 + 2] = 0.1f;

                            colorMemory[index * 3 + 0] = color.R;
                            colorMemory[index * 3 + 1] = color.G;
                            colorMemory[index * 3 + 2] = color.B;

                            index++;

                            indexMemory[index] = index;
                            vertexMemory[index * 3 + 0] = CELL.TSE.X;
                            vertexMemory[index * 3 + 1] = CELL.TSE.Y;
                            vertexMemory[index * 3 + 2] = 0.1f;

                            colorMemory[index * 3 + 0] = color.R;
                            colorMemory[index * 3 + 1] = color.G;
                            colorMemory[index * 3 + 2] = color.B;

                            index++;

                            indexMemory[index] = index;
                            vertexMemory[index * 3 + 0] = CELL.TSW.X;
                            vertexMemory[index * 3 + 1] = CELL.TSW.Y;
                            vertexMemory[index * 3 + 2] = 0.1f;

                            colorMemory[index * 3 + 0] = color.R;
                            colorMemory[index * 3 + 1] = color.G;
                            colorMemory[index * 3 + 2] = color.B;

                            index++;
                        }
                    }
            }

            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);

            ElementCount = index;

        }


        public int GetCellIndex(int column, int row, int layer)
        {
            return egrid.ACTNUM[column + egrid.NX * row + layer * egrid.NX * egrid.NY];
        }

        public struct Cell
        {
            public Vector3 TNW;
            public Vector3 TNE;
            public Vector3 TSW;
            public Vector3 TSE;
            public Vector3 BNW;
            public Vector3 BNE;
            public Vector3 BSW;
            public Vector3 BSE;
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

            CELL.TNW.Z = egrid.ZCORN[(ulong)(Z * egrid.NX * egrid.NY * 8 + Y * egrid.NX * 4 + 2 * X + 0)];
            CELL.TNE.Z = egrid.ZCORN[(ulong)(Z * egrid.NX * egrid.NY * 8 + Y * egrid.NX * 4 + 2 * X + 1)];
            CELL.TSW.Z = egrid.ZCORN[(ulong)(Z * egrid.NX * egrid.NY * 8 + Y * egrid.NX * 4 + 2 * X + egrid.NX * 2 + 0)];
            CELL.TSE.Z = egrid.ZCORN[(ulong)(Z * egrid.NX * egrid.NY * 8 + Y * egrid.NX * 4 + 2 * X + egrid.NX * 2 + 1)];

            CELL.BNW.Z = egrid.ZCORN[(ulong)(Z * egrid.NX * egrid.NY * 8 + Y * egrid.NX * 4 + egrid.NX * egrid.NY * 4 + 2 * X + 0)];
            CELL.BNE.Z = egrid.ZCORN[(ulong)(Z * egrid.NX * egrid.NY * 8 + Y * egrid.NX * 4 + egrid.NX * egrid.NY * 4 + 2 * X + 1)];
            CELL.BSW.Z = egrid.ZCORN[(ulong)(Z * egrid.NX * egrid.NY * 8 + Y * egrid.NX * 4 + egrid.NX * egrid.NY * 4 + 2 * X + egrid.NX * 2 + 0)];
            CELL.BSE.Z = egrid.ZCORN[(ulong)(Z * egrid.NX * egrid.NY * 8 + Y * egrid.NX * 4 + egrid.NX * egrid.NY * 4 + 2 * X + egrid.NX * 2 + 1)];

            // Направляющая линия от TNW до BNW

            Vector3 TOP, BTM;

            TOP.X = egrid.COORD[(X + (egrid.NX + 1) * Y) * 6 + 0];
            TOP.Y = egrid.COORD[(X + (egrid.NX + 1) * Y) * 6 + 1];
            TOP.Z = egrid.COORD[(X + (egrid.NX + 1) * Y) * 6 + 2];

            BTM.X = egrid.COORD[(X + (egrid.NX + 1) * Y) * 6 + 3 + 0];
            BTM.Y = egrid.COORD[(X + (egrid.NX + 1) * Y) * 6 + 3 + 1];
            BTM.Z = egrid.COORD[(X + (egrid.NX + 1) * Y) * 6 + 3 + 2];

            float FRAC = 0;

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

            TOP.X = egrid.COORD[((X + 1) + (egrid.NX + 1) * Y) * 6 + 0];
            TOP.Y = egrid.COORD[((X + 1) + (egrid.NX + 1) * Y) * 6 + 1];
            TOP.Z = egrid.COORD[((X + 1) + (egrid.NX + 1) * Y) * 6 + 2];

            BTM.X = egrid.COORD[((X + 1) + (egrid.NX + 1) * Y) * 6 + 3 + 0];
            BTM.Y = egrid.COORD[((X + 1) + (egrid.NX + 1) * Y) * 6 + 3 + 1];
            BTM.Z = egrid.COORD[((X + 1) + (egrid.NX + 1) * Y) * 6 + 3 + 2];

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

            TOP.X = egrid.COORD[((X + 1) + (egrid.NX + 1) * (Y + 1)) * 6 + 0];
            TOP.Y = egrid.COORD[((X + 1) + (egrid.NX + 1) * (Y + 1)) * 6 + 1];
            TOP.Z = egrid.COORD[((X + 1) + (egrid.NX + 1) * (Y + 1)) * 6 + 2];

            BTM.X = egrid.COORD[((X + 1) + (egrid.NX + 1) * (Y + 1)) * 6 + 3 + 0];
            BTM.Y = egrid.COORD[((X + 1) + (egrid.NX + 1) * (Y + 1)) * 6 + 3 + 1];
            BTM.Z = egrid.COORD[((X + 1) + (egrid.NX + 1) * (Y + 1)) * 6 + 3 + 2];

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

            TOP.X = egrid.COORD[(X + (egrid.NX + 1) * (Y + 1)) * 6 + 0];
            TOP.Y = egrid.COORD[(X + (egrid.NX + 1) * (Y + 1)) * 6 + 1];
            TOP.Z = egrid.COORD[(X + (egrid.NX + 1) * (Y + 1)) * 6 + 2];

            BTM.X = egrid.COORD[(X + (egrid.NX + 1) * (Y + 1)) * 6 + 3 + 0];
            BTM.Y = egrid.COORD[(X + (egrid.NX + 1) * (Y + 1)) * 6 + 3 + 1];
            BTM.Z = egrid.COORD[(X + (egrid.NX + 1) * (Y + 1)) * 6 + 3 + 2];

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
