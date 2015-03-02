using System;
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

            Surface spriteSurface;
            surfaceCache.TryGetValue(entry, out spriteSurface);
            if (spriteSurface == null)
            {
                spriteSurface = new Surface(spriteVideoWidth, spriteVideoHeight);
                spriteSurface.Blit(spriteSheet,
                    new System.Drawing.Point(0, 0),
                    new Rectangle(spriteLoc, new Size(spriteSheetWidth, spriteSheetHeight)));
                if(foregroundColor != Color.White)
                    spriteSurface.ReplaceColor(Color.White, foregroundColor);
                if(backgroundColor != transparentColor)
                    spriteSurface.ReplaceColor(transparentColor, backgroundColor);

                surfaceCache.Add(entry, spriteSurface);

                LogFile.Log.LogEntryDebug("Rendering sprite" + id, LogDebugLevel.Profiling);
            }

            LogFile.Log.LogEntryDebug("Drawing sprite " + id + " from " + spriteLoc.X + "/" + spriteLoc.Y + "at: "
                + screenX + "/" + screenY, LogDebugLevel.Profiling);

            videoSurface.Blit(spriteSurface, new System.Drawing.Point(screenX, screenY),
                new Rectangle(new System.Drawing.Point(0, 0), new Size(spriteSheetWidth, spriteSheetHeight)));
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

            DrawSprite(Convert.ToInt32(c), x, y, color, transparentColor);
        }

        public void PrintStringRect(string msg, int x, int y, int width, int height, libtcodWrapper.LineAlignment alignment, Color color)
        {
            PrintString(msg, x, y, alignment, color);
        }

        public void PrintString(string msg, int x, int y, libtcodWrapper.LineAlignment alignment, Color color)
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
