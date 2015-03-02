﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SdlDotNet.Graphics;
using SdlDotNet.Core;


namespace RogueBasin
{
    class SurfaceCacheEntry
    {
        public int Id { get; set; }
        public Color ForegroundColor { get; set; }
        public Color BackgroundColor { get; set; }

        public override bool Equals(object other)
        {
            var otherFoo = other as SurfaceCacheEntry;
            if (otherFoo == null)
                return false;
            return Id == otherFoo.Id && ForegroundColor == otherFoo.ForegroundColor && BackgroundColor == otherFoo.BackgroundColor;
        }

        public override int GetHashCode()
        {
            return 17 * Id.GetHashCode() + 17 * ForegroundColor.GetHashCode() + BackgroundColor.GetHashCode();
        }
    }

    /// <summary>
    /// Renders a multi-layer tile map onto the screen
    /// </summary>
    class MapRendererSDLDotNet : IMapRenderer
    {
        SdlDotNet.Graphics.Font font;
        Dictionary<SurfaceCacheEntry, Surface> surfaceCache = new Dictionary<SurfaceCacheEntry,Surface>();

        private Color transparentColor = Color.FromArgb(255, 0, 255);

        private Surface videoSurface;
        private Surface spriteSheet;
        private int videoWidth = 960;
        private int videoHeight = 720;
        private int spritesPerRow = 16;
        private int spriteSheetWidth = 16;
        private int spriteSheetHeight = 16;
        private int spriteVideoWidth = 16;
        private int spriteVideoHeight = 16;

        /// <summary>
        /// Render the map, with TL in map at mapOffset. Screenviewport is the screen viewport in tile dimensions (for now)
        /// </summary>
        /// <param name="mapToRender"></param>
        /// <param name="mapOffset"></param>
        /// <param name="screenViewport"></param>
        public void RenderMap(TileEngine.TileMap mapToRender, Point mapOffset, Rectangle screenViewport)
        {

            if (mapOffset.x >= mapToRender.Columns || mapOffset.y >= mapToRender.Rows)
            {
                throw new Exception("Point outside map " + mapOffset);
            }

            //Calculate visible area of map
            int maxColumn = mapOffset.x + screenViewport.Width - 1;
            int maxRow = mapOffset.y + screenViewport.Height - 1;

            if (maxColumn >= mapToRender.Columns)
                maxColumn = mapToRender.Columns - 1;
            if (maxRow >= mapToRender.Rows)
                maxRow = mapToRender.Rows - 1;

            //Render layers in order
            foreach (TileEngine.TileLayer layer in mapToRender.Layer)
            {
                for (int y = mapOffset.y; y <= maxRow; y++)
                {
                    for (int x = mapOffset.x; x <= maxColumn; x++)
                    {
                        TileEngine.TileCell thisCell = layer.Rows[y].Columns[x];

                        if (thisCell.TileID == -1)
                            continue;

                        LibtcodColorFlags colorFlags = thisCell.TileFlag as LibtcodColorFlags;
                        Color foregroundColor = Color.White;
                        Color backgroundColor = Color.Black;
                        
                        if(colorFlags != null)
                        {
                            if (colorFlags.BackgroundColor == null)
                            {
                                backgroundColor = Color.Black;
                            }
                            else
                            {
                                backgroundColor = colorFlags.BackgroundColor;
                            }

                            if (colorFlags.ForegroundColor == null)
                            {
                                foregroundColor = Color.White;
                            }
                            else
                            {
                                foregroundColor = colorFlags.ForegroundColor;
                            }
                        }

                        var spriteLoc = tileIDToSpriteLocation(thisCell.TileID);

                        //Screen tile coords
                        int screenTileX = screenViewport.X + (x - mapOffset.x);
                        int screenTileY = screenViewport.Y + (y - mapOffset.y);

                        //Screen real coords
                        int screenX = screenTileX * spriteSheetWidth;
                        int screenY = screenTileY * spriteSheetHeight;

                        LogFile.Log.LogEntry("Drawing sprite " + thisCell.TileID + " from " + spriteLoc.X + "/" + spriteLoc.Y + "at: "
                            + screenX + "/" + screenY);

                        SurfaceCacheEntry entry = new SurfaceCacheEntry();
                        entry.Id = thisCell.TileID;
                        entry.ForegroundColor = foregroundColor;
                        entry.BackgroundColor = backgroundColor;

                        Surface spriteSurface;
                        surfaceCache.TryGetValue(entry, out spriteSurface);
                        if (spriteSurface == null)
                        {
                            spriteSurface = new Surface(spriteVideoWidth, spriteVideoHeight);
                            spriteSurface.Blit(spriteSheet,
                                new System.Drawing.Point(0, 0),
                                new Rectangle(spriteLoc, new Size(spriteSheetWidth, spriteSheetHeight)));
                            spriteSurface.ReplaceColor(Color.White, foregroundColor);
                            spriteSurface.ReplaceColor(transparentColor, backgroundColor);

                            surfaceCache.Add(entry, spriteSurface);
                        }

                        videoSurface.Blit(spriteSurface, new System.Drawing.Point(screenX, screenY),
                            new Rectangle(new System.Drawing.Point(0, 0), new Size(spriteSheetWidth, spriteSheetHeight)));
                    }
                }
            }

            
        }

        private System.Drawing.Point tileIDToSpriteLocation(int tileID)
        {
            //Location on sprite sheet
            int spriteTileX = tileID % spritesPerRow;
            int spriteTileY = tileID / spritesPerRow;

            int spriteX = spriteTileX * spriteSheetWidth;
            int spriteY = spriteTileY * spriteSheetHeight;

            return new System.Drawing.Point(spriteX, spriteY);
        }

        public void Sleep(ulong milliseconds)
        {
        }

        public void Setup(int width, int height)
        {
            videoSurface = Video.SetVideoMode(videoWidth, videoHeight, 32, false, false, false, true);

            spriteSheet = new Surface(@"TraumaSprites.png").Convert(videoSurface, true, true);
            spriteSheet.Transparent = true;
            spriteSheet.TransparentColor = transparentColor;

            font = new SdlDotNet.Graphics.Font(@"alexisv3.ttf", 18);
        }


        public void Flush()
        {
            videoSurface.Update();
        }

        public void Clear()
        {
            videoSurface.Fill(Color.Black);
        }

        public void DrawFrame(int x, int y, int width, int height, bool clear, Color color)
        {

        }

        public void PutChar(int x, int y, char c, Color color)
        {
            var spriteLoc = tileIDToSpriteLocation(Convert.ToInt32(c));

            //Screen real coords
            int screenX = x * spriteSheetWidth;
            int screenY = y * spriteSheetHeight;

            LogFile.Log.LogEntry("Drawing char sprite " + c + " from " + spriteLoc.X + "/" + spriteLoc.Y + "at: "
                     + screenX + "/" + screenY);

            videoSurface.Blit(spriteSheet,
                            new System.Drawing.Point(screenX, screenY),
                            new Rectangle(spriteLoc, new Size(spriteSheetWidth, spriteSheetHeight)));
        }

        public void PrintStringRect(string msg, int x, int y, int width, int height, libtcodWrapper.LineAlignment alignment, Color color)
        {
            PrintString(msg, x, y, alignment, color);
        }

        public void PrintString(string msg, int x, int y, libtcodWrapper.LineAlignment alignment, Color color)
        {
            // Create the Font Surfaces
            Surface fontSurface = font.Render(msg, Color.White);

            //Screen real coords
            int screenX = x * spriteSheetWidth;
            int screenY = y * spriteSheetHeight;

            LogFile.Log.LogEntry("Drawing string " + msg + screenX + "/" + screenY);

            videoSurface.Blit(fontSurface,
                            new System.Drawing.Point(screenX, screenY));
        }

        public void ClearRect(int x, int y, int width, int height)
        {

        }
    }
}
