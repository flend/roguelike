using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    public enum LineAlignment { Left, Center, Right }

    interface IMapRenderer
    {
        void RenderMap(TileEngine.TileMap mapToRender, Point mapOffset, Rectangle screenViewport);

        void SetSpriteVideoSize(int width, int height);

        void Sleep(ulong milliseconds);

        void Setup(int width, int height);

        void Flush();

        void Clear();

        void DrawTextWidth(string msg, int x, int y, int size, int width, Color foregroundColor, Color backgroundColor);
        void DrawText(string msg, int x, int y, int size, LineAlignment alignment, Color foregroundColor, Color backgroundColor);

        void DrawTextWidth(string msg, int x, int y, int size, int width, Color color);
        void DrawText(string msg, int x, int y, int size, LineAlignment alignment, Color color);
        
        Size TextSize(string msg, int size);

        void DrawUISprite(string id, int x, int y, double scaling, double alpha);
        Size GetUISpriteDimensions(string id);

        void DrawTraumaUISprite(int id, int x, int y, LibtcodColorFlags flags, double scaling, double alpha);
        Size GetTraumaSpriteDimensions(int id);

        void DrawLine(Point p1, Point p2, Color color);
        void DrawRectangle(Rectangle rect, Color color);
    }
}
