using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    public enum LineAlignment { Left, Center, Right }

    interface IScreenRenderer
    {
        void Sleep(ulong milliseconds);

        void Setup(int width, int height);

        void Flush();

        void Clear();

        void DrawTextWidth(string msg, int x, int y, int size, int width, Color foregroundColor, Color backgroundColor);
        void DrawText(string msg, int x, int y, int size, LineAlignment alignment, Color foregroundColor, Color backgroundColor);

        void DrawTextWidth(string msg, int x, int y, int size, int width, Color color);
        void DrawText(string msg, int x, int y, int size, LineAlignment alignment, Color color);
        
        Size TextSize(string msg, int size);

        void DrawScaledSprite(string filePath, int x, int y, double scaling = 1.0, double alpha = 1.0, bool isAnimated = false, int frameNo = 0);
        Size GetSpriteDimensions(string id);

        void DrawTraumaSprite(int id, int x, int y, LibtcodColorFlags flags, double scaling, double alpha);
        Size GetTraumaSpriteDimensions(int id);

        void DrawLine(Point p1, Point p2, Color color);
        void DrawRectangle(Rectangle rect, Color color);
    }
}
