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

    class SpriteCacheEntry
    {
        public string StrId { get; set; }

        public override bool Equals(object other)
        {
            var otherFoo = other as SpriteCacheEntry;
            if (otherFoo == null)
                return false;
            return StrId == otherFoo.StrId;
        }

        public override int GetHashCode()
        {
            return 17 * StrId.GetHashCode();
        }
    }

    /// <summary>
    /// Renders a multi-layer tile map onto the screen
    /// </summary>
    class MapRendererSDLDotNet : IMapRenderer
    {
        SdlDotNet.Graphics.Font font;
        Dictionary<SurfaceCacheEntry, Surface> surfaceCache = new Dictionary<SurfaceCacheEntry,Surface>();
        Dictionary<SpriteCacheEntry, Surface> spriteCache = new Dictionary<SpriteCacheEntry, Surface>();

        private Color transparentColor = Color.FromArgb(255, 0, 255);

        private Surface videoSurface;
        private Surface spriteSheet;
        private int videoWidth = 1024;
        private int videoHeight = 768;
        private int spritesPerRow = 16;
        private int spriteSheetWidth = 16;
        private int spriteSheetHeight = 16;
        private int spriteVideoWidth = 32;
        private int spriteVideoHeight = 32;

        private double xScale = 1.0;
        private double yScale = 1.0;

        private int traumaSpriteScaling = 2;

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
                        Color backgroundColor = transparentColor;
                        
                        if(colorFlags != null)
                        {
                            if (colorFlags.BackgroundColor != null)
                            {
                                backgroundColor = colorFlags.BackgroundColor;
                            }

                            if (colorFlags.ForegroundColor != null)
                            {
                                foregroundColor = colorFlags.ForegroundColor;
                            }
                        }

                        //Screen tile coords
                        int screenTileX = screenViewport.X + (x - mapOffset.x);
                        int screenTileY = screenViewport.Y + (y - mapOffset.y);

                        DrawSprite(thisCell.TileID, screenTileX, screenTileY, foregroundColor, backgroundColor);
                    }
                }
            }
        }

        private void DrawSprite(int id, int x, int y, Color foregroundColor, Color backgroundColor)
        {
            var spriteLoc = tileIDToSpriteLocation(id);

            //Screen real coords
            int screenX = x * spriteVideoWidth;
            int screenY = y * spriteVideoHeight;

            SurfaceCacheEntry entry = new SurfaceCacheEntry();
            entry.Id = id;
            entry.ForegroundColor = foregroundColor;
            entry.BackgroundColor = backgroundColor;

            Surface spriteSurface = GetTraumaSprite(entry);

            //LogFile.Log.LogEntryDebug("Drawing sprite " + id + " from " + spriteLoc.X + "/" + spriteLoc.Y + "at: "
            //    + screenX + "/" + screenY, LogDebugLevel.Profiling);

            videoSurface.Blit(spriteSurface, new System.Drawing.Point(screenX, screenY));
        }

        private Surface GetTraumaSprite(SurfaceCacheEntry entry)
        {
            var spriteLoc = tileIDToSpriteLocation(entry.Id);

            Surface spriteSurface;
            surfaceCache.TryGetValue(entry, out spriteSurface);
            if (spriteSurface == null)
            {
                Surface tempSpriteSurface = new Surface(spriteSheetWidth, spriteSheetHeight);

                tempSpriteSurface.Blit(spriteSheet,
                    new System.Drawing.Point(0, 0),
                    new Rectangle(spriteLoc, new Size(spriteSheetWidth, spriteSheetHeight)));

                if (entry.ForegroundColor != Color.White)
                    tempSpriteSurface.ReplaceColor(Color.White, entry.ForegroundColor);
                if (entry.BackgroundColor != transparentColor)
                    tempSpriteSurface.ReplaceColor(transparentColor, entry.BackgroundColor);

                spriteSurface = tempSpriteSurface;

                if (traumaSpriteScaling > 1)
                {
                    if (traumaSpriteScaling == 2)
                    {
                        spriteSurface = tempSpriteSurface.CreateScaleDoubleSurface(false);
                    }
                    else
                    {
                        spriteSurface = tempSpriteSurface.CreateScaledSurface(traumaSpriteScaling, false);
                    }
                }

                //Set this after doing background replacement
                spriteSurface.Transparent = true;
                spriteSurface.TransparentColor = transparentColor;

                surfaceCache.Add(entry, spriteSurface);

                LogFile.Log.LogEntryDebug("Rendering sprite" + entry.Id, LogDebugLevel.Profiling);
            }
            return spriteSurface;
        }

        private string GameSpritePath(string id)
        {
            return "graphics/game/" + id + ".png";
        }

        private string UISpritePath(string id)
        {
            return "graphics/ui/" + id + ".png";
        }

        public void DrawUISprite(string id, int x, int y)
        {
            DrawSprite(UISpritePath(id), x, y);
        }

        public void DrawTraumaSprite(int id, int x, int y)
        {
            SurfaceCacheEntry entry = new SurfaceCacheEntry();
            entry.Id = id;
            entry.ForegroundColor = Color.Wheat;
            entry.BackgroundColor = Color.Black;

            Surface traumaSprite = GetTraumaSprite(entry);

            videoSurface.Blit(traumaSprite, new System.Drawing.Point(x, y));
        }

        public Size GetUISpriteDimensions(string id)
        {
            SpriteCacheEntry entry = new SpriteCacheEntry();
            entry.StrId = UISpritePath(id);

            return GetSpriteFromCache(entry).Size;
        }

        public Size GetTraumaSpriteDimensions(int id)
        {
            return new Size(spriteVideoWidth, spriteVideoHeight);
        }

        /// <summary>
        /// Draw sprite in absolute screen coordinates
        /// </summary>
        /// <param name="id"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void DrawSprite(string filePath, int x, int y)
        {
            SpriteCacheEntry entry = new SpriteCacheEntry();
            entry.StrId = filePath;

            Surface spriteSurface = GetSpriteFromCache(entry);

            videoSurface.Blit(spriteSurface, new System.Drawing.Point(x, y));
        }

        private Surface GetSpriteFromCache(SpriteCacheEntry entry)
        {
            Surface spriteSurface;
            spriteCache.TryGetValue(entry, out spriteSurface);

            if (spriteSurface == null)
            {
                //Convert knocks out alpha for some goddamn reason
                spriteSurface = new Surface(entry.StrId);//.Convert(videoSurface, true, false);

                spriteCache.Add(entry, spriteSurface);

                LogFile.Log.LogEntryDebug("Storing ui sprite" + entry.StrId, LogDebugLevel.Profiling);
            }
            return spriteSurface;
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
            videoSurface.AlphaBlending = true;

            spriteSheet = new Surface(@"TraumaSprites.png").Convert(videoSurface, true, true);

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

        public void DrawFrame(int tlx, int tly, int width, int height, bool clear, Color color)
        {
            if (clear)
            {
                videoSurface.Fill(new Rectangle(tlx * spriteSheetWidth, tly * spriteSheetHeight, width * spriteSheetWidth, height * spriteSheetHeight), Color.Black);
            }

            for (int x = tlx + 1; x < tlx + width - 1; x++)
            {
                PutChar(x, tly, '-', color);
            }
            for (int x = tlx + 1; x < tlx + width - 1; x++)
            {
                PutChar(x, tly + height - 1, '-', color);
            }
            for (int y = tly + 1; y < tly + height - 1; y++)
            {
                PutChar(tlx, y, '|', color);
            }
            for (int y = tly + 1; y < tly + height - 1; y++)
            {
                PutChar(tlx + width - 1, y, '|', color);
            }
            PutChar(tlx, tly, '+', color);
            PutChar(tlx + width - 1, tly, '+', color);
            PutChar(tlx, tly + height - 1, '+', color);
            PutChar(tlx + width - 1, tly + height - 1, '+', color);
        }

        public void PutChar(int x, int y, char c, Color color)
        {
            var spriteLoc = tileIDToSpriteLocation(Convert.ToInt32(c));

            DrawSprite(Convert.ToInt32(c), x, y, color, transparentColor);
        }

        public void PrintStringRect(string msg, int x, int y, int width, int height, LineAlignment alignment, Color color)
        {
            int xCoord = x;
            int yCoord = y;

            if (alignment == LineAlignment.Center)
            {
                xCoord = x + Math.Max(width - msg.Length, 0) / 2;
                yCoord = y + Math.Max(height - 1, 0) / 2;
            }

            PrintString(msg, xCoord, yCoord, color);
        }

        public void PrintString(string msg, int x, int y, Color color)
        {
            // Create the Font Surfaces
            Surface fontSurface = font.Render(msg, color);

            //Screen real coords
            int screenX = x * spriteSheetWidth;
            int screenY = y * spriteSheetHeight;

            LogFile.Log.LogEntryDebug("Drawing string " + msg + screenX + "/" + screenY, LogDebugLevel.Profiling);

            videoSurface.Blit(fontSurface,
                            new System.Drawing.Point(screenX, screenY));
        }

        public void ClearRect(int x, int y, int width, int height)
        {

        }
    }
}
