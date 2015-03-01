using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    interface IMapRenderer
    {
        void RenderMap(TileEngine.TileMap mapToRender, Point mapOffset, Rectangle screenViewport);

        void Sleep(ulong milliseconds);

        void Setup(int width, int height);

        void Flush();

        void Clear();

        void DrawFrame(int x, int y, int width, int height, bool clear, Color color);

        void PutChar(int x, int y, char c, Color color);

        void PrintStringRect(string msg, int x, int y, int width, int height, libtcodWrapper.LineAlignment alignment, Color color);

        void PrintString(string msg, int x, int y, libtcodWrapper.LineAlignment alignment, Color color);

        void ClearRect(int x, int y, int width, int height);
    }
}
