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

        void DrawFramePixel(int x, int y, int width, int height, bool clear, Color color);

        void DrawTextWidth(string msg, int x, int y, int size, int width, Color foregroundColor, Color backgroundColor);
        void DrawText(string msg, int x, int y, int size, LineAlignment alignment, Color foregroundColor, Color backgroundColor);

        void DrawTextWidth(string msg, int x, int y, int size, int width, Color color);
        void DrawText(string msg, int x, int y, int size, LineAlignment alignment, Color color);
        
        Size TextSize(string msg, int size);

        void ClearRect(int x, int y, int width, int height);

        void DrawUISprite(string id, int x, int y, double scaling);

        Size GetUISpriteDimensions(string id);

        void DrawTraumaSprite(int id, int x, int y);
        void DrawTraumaUISprite(int id, int x, int y);
        Size GetTraumaSpriteDimensions(int id);
    }
}
