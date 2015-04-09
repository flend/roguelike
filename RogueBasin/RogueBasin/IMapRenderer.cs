﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    enum LineAlignment { Left, Center, Right }

    interface IMapRenderer
    {
        void RenderMap(TileEngine.TileMap mapToRender, Point mapOffset, Rectangle screenViewport);

        void SetSpriteVideoSize(int width, int height);

        void Sleep(ulong milliseconds);

        void Setup(int width, int height);

        void Flush();

        void Clear();

        void DrawFramePixel(int x, int y, int width, int height, bool clear, Color color);

        void PrintStringRect(string msg, int x, int y, int width, int height, LineAlignment alignment, Color color);

        void PrintString(string msg, int x, int y, Color color);

        void DrawText(string msg, int x, int y, Color color);
        void DrawTextWidth(string msg, int x, int y, int width, Color color);
        void DrawText(string msg, int x, int y, LineAlignment alignment, Color color);
        void DrawSmallText(string msg, int x, int y, LineAlignment alignment, Color color);
        void DrawLargeText(string msg, int x, int y, LineAlignment alignment, Color color);

        void ClearRect(int x, int y, int width, int height);

        void DrawUISprite(string id, int x, int y, double scaling);

        void DrawTileSprite(string id, int x, int y, double scaling);


        void DrawTraumaSprite(int id, int x, int y);

        void DrawTraumaUISprite(int id, int x, int y);

        Size GetUISpriteDimensions(string id);

        Size GetTraumaSpriteDimensions(int id);


    }
}
