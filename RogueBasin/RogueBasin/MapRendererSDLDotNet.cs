using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SdlDotNet.Graphics;
using SdlDotNet.Core;
using System.Reflection;
using System.IO;
using System.Windows.Forms;


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
        public double AlphaOverride { get; set; }
        public double Scaling { get; set; }

        public SpriteCacheEntry()
        {
            Scaling = 1.0;
        }

        public override bool Equals(object other)
        {
            var otherFoo = other as SpriteCacheEntry;
            if (otherFoo == null)
                return false;
            return StrId == otherFoo.StrId && AlphaOverride - otherFoo.AlphaOverride < 0.001 && Scaling - otherFoo.Scaling < 0.001;
        }

        public override int GetHashCode()
        {
            return 17 * StrId.GetHashCode() + 17 * AlphaOverride.GetHashCode() + 17 * Scaling.GetHashCode();
        }
    }

    /// <summary>
    /// Renders a multi-layer tile map onto the screen
    /// </summary>
    class MapRendererSDLDotNet : IMapRenderer
    {
        Dictionary<SurfaceCacheEntry, Surface> surfaceCache = new Dictionary<SurfaceCacheEntry,Surface>();
        Dictionary<SurfaceCacheEntry, Surface> surfaceUICache = new Dictionary<SurfaceCacheEntry, Surface>();

        Dictionary<SpriteCacheEntry, Surface> spriteCache = new Dictionary<SpriteCacheEntry, Surface>();
        Dictionary<SpriteCacheEntry, Surface> scaledSpriteCache = new Dictionary<SpriteCacheEntry, Surface>();

        Dictionary<int, SdlDotNet.Graphics.Font> fontCache = new Dictionary<int, SdlDotNet.Graphics.Font>();

        private Color transparentColor = Color.FromArgb(255, 0, 255);

        private Surface videoSurface;
        private Surface spriteSheet;
        private int spritesPerRow = 16;
        private int spriteSheetWidth = 16;
        private int spriteSheetHeight = 16;
        private int tileSpriteSheetWidth = 64;
        private int tileSpriteSheetHeight = 64;

        private int traumaSpriteScaling = 4;
        private int traumaUISpriteScaling = 4;

        public int spriteVideoWidth { get; set; }
        public int spriteVideoHeight { get; set; }

        private double spriteVideoWidthScaling = 1.0;
        private double spriteVideoHeightScaling = 1.0;

        public MapRendererSDLDotNet()
        {
            spriteVideoWidth = 64;
            spriteVideoHeight = 64;
        }

        public void SetSpriteVideoSize(int width, int height)
        {
            spriteVideoWidth = width;
            spriteVideoHeight = height;

            spriteVideoWidthScaling = spriteVideoWidth / (double)tileSpriteSheetWidth;
            spriteVideoHeightScaling = spriteVideoHeight / (double)tileSpriteSheetHeight;

            //Drop scaled cache
            scaledSpriteCache.Clear();
        }

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

            int layerNumber = 0;

            //Render layers in order
            foreach (TileEngine.TileLayer layer in mapToRender.Layer)
            {
                for (int y = mapOffset.y; y <= maxRow; y++)
                {
                    for (int x = mapOffset.x; x <= maxColumn; x++)
                    {
                        TileEngine.TileCell thisCell = layer.Rows[y].Columns[x];
                        
                        //Screen tile coords
                        int screenTileX = screenViewport.X + (x - mapOffset.x);
                        int screenTileY = screenViewport.Y + (y - mapOffset.y);

                        int offsetX = 0;
                        int offsetY = 0;
                        
                        if (thisCell.TileSprite != null)
                        {
                            if (layerNumber == (int)RogueBasin.Screen.TileLevel.CreatureDecoration)
                            {
                                if (thisCell.TileSprite == "knife")
                                {
                                    offsetY = 5;
                                    offsetX = -5;
                                }
                                else if (thisCell.TileSprite == "pole")
                                {
                                    offsetY = 25;
                                }
                                else
                                {
                                    offsetY = 15;
                                }
                            }

                            if (layerNumber == (int)RogueBasin.Screen.TileLevel.Animations)
                            {
                                if (thisCell.Animation != null && !thisCell.Animation.Displayed)
                                    continue;
                            }

                            try
                            {
                                if (thisCell.TileSprite.Length > 14 && thisCell.TileSprite.Substring(0, 14) == "monster_level_")
                                {
                                    var thisLevel = thisCell.TileSprite.Substring(14);
                                    var thisLevelNum = Convert.ToInt32(thisLevel);
                                    var thisTens = thisLevelNum / 10;
                                    var thisRem = thisTens % 10;

                                    if (thisTens == 0)
                                    {
                                        DrawTileSprite(thisCell.TileSprite, new Point(screenTileX, screenTileY), new Point(offsetX, offsetY), thisCell.Transparency, 0, false);
                                    }
                                    else
                                    {
                                        var tens = "monster_level_" + thisTens.ToString();
                                        var units = "monster_level_" + thisRem.ToString();

                                        DrawTileSprite(units, new Point(screenTileX, screenTileY), new Point(offsetX, offsetY), thisCell.Transparency, 0, false);
                                        offsetX = -15;
                                        DrawTileSprite(tens, new Point(screenTileX, screenTileY), new Point(offsetX, offsetY), thisCell.Transparency, 0, false);
                                    }
                                    continue;
                                }
                            }
                            catch (Exception)
                            {
                                //parsing
                            }

                            //Handle 2 frame anims

                            int frameNo = 0;
                            bool isAnimated = false;
                            if (thisCell.RecurringAnimation != null)
                            {
                                frameNo = thisCell.RecurringAnimation.FrameNo;
                                isAnimated = true;
                            }

                            DrawTileSprite(thisCell.TileSprite, new Point(screenTileX, screenTileY), new Point(offsetX, offsetY), thisCell.Transparency, frameNo, isAnimated);
                            continue;
                        }
                    }
                }
                layerNumber++;
            }
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

        private Surface GetTraumaUISprite(SurfaceCacheEntry entry)
        {
            var spriteLoc = tileIDToSpriteLocation(entry.Id);

            Surface spriteSurface;
            surfaceUICache.TryGetValue(entry, out spriteSurface);
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

                if (traumaUISpriteScaling > 1)
                {
                    if (traumaUISpriteScaling == 2)
                    {
                        spriteSurface = tempSpriteSurface.CreateScaleDoubleSurface(false);
                    }
                    else
                    {
                        spriteSurface = tempSpriteSurface.CreateScaledSurface(traumaUISpriteScaling, false);
                    }
                }

                //Set this after doing background replacement
                spriteSurface.Transparent = true;
                spriteSurface.TransparentColor = transparentColor;

                surfaceUICache.Add(entry, spriteSurface);

                LogFile.Log.LogEntryDebug("Rendering ui sprite" + entry.Id, LogDebugLevel.Profiling);
            }
            return spriteSurface;
        }

        private string TileSpritePath(string id)
        {
            return "tiles." + id + ".png";
        }

        private string UISpritePath(string id)
        {
            return "ui." + id + ".png";
        }

        public void DrawUISprite(string id, int x, int y, double scaling)
        {
            DrawScaledSprite(UISpritePath(id), x, y, scaling, 1.0);
        }

        public void DrawTileSprite(string id, int x, int y, double scaling = 1.0)
        {
            DrawScaledSprite(TileSpritePath(id), x, y, scaling, 1.0);
        }

        private void DrawTileSprite(string id, Point tileCoords, Point offset, double alpha,  int frameNo = 0, bool isAnimated = false)
        {
            //Tile x, y are the top left of a 64x64 tile
            //Our tile sprites may not be 64x64, but are aligned to the BOTTOM-LEFT of a 64x64 tile (I hope)
            Size tileDimensions = GetTileSpriteDimensions(id);

            //Offset into source bitmap
            Point offsetIntoSourceBitmap = new Point(offset.x, -(tileDimensions.Height - tileSpriteSheetHeight) + offset.y);

            if (id == "boss")
            {
                offsetIntoSourceBitmap = new Point(offset.x - tileSpriteSheetWidth / 2, offset.y - 128);
            }

            Point screenOffset = new Point((int)Math.Floor(offsetIntoSourceBitmap.x * spriteVideoWidthScaling),
                (int)Math.Floor(offsetIntoSourceBitmap.y * spriteVideoHeightScaling));

            //Screen real coords
            Point screenCoords = new Point(tileCoords.x * spriteVideoWidth + screenOffset.x,
                tileCoords.y * spriteVideoHeight + screenOffset.y);

            DrawScaledTileSprite(TileSpritePath(id), screenCoords, alpha, frameNo, isAnimated);
        }

        public void DrawTraumaSprite(int id, int x, int y)
        {
            SurfaceCacheEntry entry = new SurfaceCacheEntry();
            entry.Id = id;
            entry.ForegroundColor = Color.Wheat;
            entry.BackgroundColor = Color.Black;

            /*
            Surface traumaSprite = GetTraumaSprite(entry);

            videoSurface.Blit(traumaSprite, new System.Drawing.Point(x, y));*/
        }

        public void DrawTraumaUISprite(int id, int x, int y)
        {
            SurfaceCacheEntry entry = new SurfaceCacheEntry();
            entry.Id = id;
            entry.ForegroundColor = Color.Wheat;
            entry.BackgroundColor = Color.Black;

            /*
            Surface traumaSprite = GetTraumaUISprite(entry);

            videoSurface.Blit(traumaSprite, new System.Drawing.Point(x, y));*/
        }

        public Size GetUISpriteDimensions(string id)
        {
            SpriteCacheEntry entry = new SpriteCacheEntry();
            entry.StrId = UISpritePath(id);

            try
            {
                return GetSpriteFromCache(entry).Size;
            }
            catch (Exception)
            {
                return new Size(0, 0);
            }
        }

        public Size GetTileSpriteDimensions(string id)
        {
            SpriteCacheEntry entry = new SpriteCacheEntry();
            entry.StrId = TileSpritePath(id);

            try
            {
                return GetSpriteFromCache(entry).Size;
            }
            catch (Exception)
            {
                return new Size(0, 0);
            }
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
        private void DrawScaledSprite(string filePath, int x, int y, double spriteScaling, double alpha = 1.0, int frameNo = 0, bool isAnimated = false)
        {
            SpriteCacheEntry entry = new SpriteCacheEntry();
            entry.StrId = filePath;
            entry.AlphaOverride = alpha;
            entry.Scaling = spriteScaling;

            try
            {
                Surface spriteSurface = GetSpriteFromCache(entry);

                if (!isAnimated)
                {
                    videoSurface.Blit(spriteSurface, new System.Drawing.Point(x, y));
                }
                else
                {
                    videoSurface.Blit(spriteSurface, new System.Drawing.Point(x, y), new Rectangle(frameNo * 64, 0, 64, 64));
                }

               
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Can't find sprite " + filePath, LogDebugLevel.High);
            }
        }

        /// <summary>
        /// Draw sprite in absolute screen coordinates, with scaling
        /// </summary>
        /// <param name="id"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void DrawScaledTileSprite(string filePath, Point screenCoords, double alpha = 1.0, int frameNo = 0, bool isAnimated = false)
        {
            SpriteCacheEntry entry = new SpriteCacheEntry();
            entry.StrId = filePath;
            entry.AlphaOverride = alpha;

            try
            {
                Surface spriteSurface = GetScaledTileSpriteFromCache(entry);

                if (!isAnimated)
                {
                    videoSurface.Blit(spriteSurface, screenCoords.ToPoint());
                }
                else
                {
                    videoSurface.Blit(spriteSurface, screenCoords.ToPoint(), new Rectangle(frameNo * spriteVideoWidth, 0, spriteVideoWidth, spriteVideoHeight));
                }
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Can't find sprite " + filePath, LogDebugLevel.High);
            }
        }

        private Surface GetScaledTileSpriteFromCache(SpriteCacheEntry entry)
        {
            Surface spriteSurface;
            scaledSpriteCache.TryGetValue(entry, out spriteSurface);

            if (spriteSurface == null)
            {
                Assembly _assembly = Assembly.GetExecutingAssembly();
                string filename = "RogueBasin.bin.Debug.graphics." + entry.StrId;
                Stream fileStream = _assembly.GetManifestResourceStream(filename);
                MemoryStream memoryStream = new MemoryStream();
                fileStream.CopyTo(memoryStream);

                spriteSurface = new Surface(memoryStream);//.Convert(videoSurface, true, false);
                if (entry.AlphaOverride > 0.01)
                {
                    ModifyAlpha(spriteSurface, entry.AlphaOverride);
                }

                //Scale if required
                Surface scaledSpriteSurface = spriteSurface;
                
                if(spriteVideoWidth != tileSpriteSheetWidth || spriteVideoHeight != tileSpriteSheetHeight) {
                    scaledSpriteSurface = spriteSurface.CreateScaledSurface(spriteVideoWidthScaling, spriteVideoHeightScaling, true);
                }

                scaledSpriteCache.Add(entry, scaledSpriteSurface);

                spriteSurface = scaledSpriteSurface;

                LogFile.Log.LogEntryDebug("Storing scaled sprite" + entry.StrId, LogDebugLevel.Profiling);
            }
            return spriteSurface;
        }

        private Surface GetSpriteFromCache(SpriteCacheEntry entry)
        {
            Surface spriteSurface;
            spriteCache.TryGetValue(entry, out spriteSurface);

            if (spriteSurface == null)
            {
                //Convert knocks out alpha for some goddamn reason
                Assembly _assembly = Assembly.GetExecutingAssembly();
                string filename = "RogueBasin.bin.Debug.graphics." + entry.StrId;
                Stream fileStream = _assembly.GetManifestResourceStream(filename);
                MemoryStream memoryStream = new MemoryStream();
                fileStream.CopyTo(memoryStream);

                spriteSurface = new Surface(memoryStream);//.Convert(videoSurface, true, false);
                if (entry.AlphaOverride > 0.01)
                {
                    ModifyAlpha(spriteSurface, entry.AlphaOverride);
                }

                //Scale if required
                Surface scaledSpriteSurface = spriteSurface;

                if (entry.Scaling > 1.001 || entry.Scaling < 0.999)
                {
                    scaledSpriteSurface = spriteSurface.CreateScaledSurface(entry.Scaling, true);
                }

                scaledSpriteCache.Add(entry, scaledSpriteSurface);

                spriteSurface = scaledSpriteSurface;

                spriteCache.Add(entry, spriteSurface);

                LogFile.Log.LogEntryDebug("Storing double scaled sprite" + entry.StrId, LogDebugLevel.Profiling);
            }
            return spriteSurface;
        }

        private void ModifyAlpha(Surface spriteSurface, double alpha)
        {
            for (int i = 0; i < spriteSurface.Width; i++)
            {
                for (int j = 0; j < spriteSurface.Height; j++)
                {
                    var pixelColor = spriteSurface.GetPixel(new System.Drawing.Point(i, j));
                    var originalAlpha = pixelColor.A;
                    var newAlpha = (byte)Math.Floor(pixelColor.A * alpha);
                    //LogFile.Log.LogEntryDebug("Setting transparency (override) " + alpha + " from " + originalAlpha + " to " + newAlpha, LogDebugLevel.Medium);
                    var transparentColor = Color.FromArgb(newAlpha, pixelColor.R, pixelColor.G, pixelColor.B);
                    Color[,] newPixel = new Color[1, 1];
                    newPixel[0, 0] = transparentColor;
                    spriteSurface.SetPixels(new System.Drawing.Point(i, j), newPixel);
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
            videoSurface = Video.SetVideoMode(width, height, 32, false, false, false, true);
            videoSurface.AlphaBlending = true;

            //spriteSheet = new Surface(@"TraumaSprites.png").Convert(videoSurface, true, true);
        }


        public void Flush()
        {
            videoSurface.Update();
        }

        public void Clear()
        {
            videoSurface.Fill(Color.Black);
        }

        public void DrawFramePixel(int x, int y, int width, int height, bool clear, Color color)
        {
            if (clear)
            {
                videoSurface.Fill(new Rectangle(x, y, width, height), Color.Black);
            }
        }

        public SdlDotNet.Graphics.Font GetFontSurfaceFromCache(int size) {
            
            SdlDotNet.Graphics.Font font;
            fontCache.TryGetValue(size, out font);

            if (font == null)
            {
                font = new SdlDotNet.Graphics.Font(@"alexisv3.ttf", size);
                fontCache.Add(size, font);
            }

            return font;
        }

        public void DrawText(string msg, int x, int y, int size, LineAlignment lineAlignment, Color foregroundColor, Color backgroundColor)
        {
            SdlDotNet.Graphics.Font font = GetFontSurfaceFromCache(size);
            Surface fontSurface = font.Render(msg, foregroundColor, backgroundColor, true);

            var pointToDraw = new System.Drawing.Point(x, y);

            if (lineAlignment == LineAlignment.Center)
            {
                var dimensions = font.SizeText(msg);
                pointToDraw = new System.Drawing.Point(x - dimensions.Width / 2, y - dimensions.Height / 2);
            }

            videoSurface.Blit(fontSurface, pointToDraw);
        }

        public void DrawText(string msg, int x, int y, int size, LineAlignment lineAlignment, Color foregroundColor)
        {
            SdlDotNet.Graphics.Font font = GetFontSurfaceFromCache(size);
            Surface fontSurface = font.Render(msg, foregroundColor, Color.Black, true);
            fontSurface.Transparent = true;
            fontSurface.TransparentColor = Color.FromArgb(0, 0, 0);

            var pointToDraw = new System.Drawing.Point(x, y);

            if (lineAlignment == LineAlignment.Center)
            {
                var dimensions = font.SizeText(msg);
                pointToDraw = new System.Drawing.Point(x - dimensions.Width / 2, y - dimensions.Height / 2);
            }

            videoSurface.Blit(fontSurface, pointToDraw);
        }

        public void DrawTextWidth(string msg, int x, int y, int size, int width, Color color)
        {
            SdlDotNet.Graphics.Font font = GetFontSurfaceFromCache(size);
            Surface fontSurface = font.Render(msg, color, Color.Black, true, width, 100);
            fontSurface.Transparent = true;
            fontSurface.TransparentColor = Color.FromArgb(0, 0, 0);

            var pointToDraw = new System.Drawing.Point(x, y);

            videoSurface.Blit(fontSurface, pointToDraw);
        }

        public void DrawTextWidth(string msg, int x, int y, int size, int width, Color foregroundColor, Color backgroundColor)
        {
            SdlDotNet.Graphics.Font font = GetFontSurfaceFromCache(size);
            Surface fontSurface = font.Render(msg, foregroundColor, backgroundColor, true, width, 100);
            fontSurface.Transparent = true;
            fontSurface.TransparentColor = Color.FromArgb(0, 0, 0);

            var pointToDraw = new System.Drawing.Point(x, y);

            videoSurface.Blit(fontSurface, pointToDraw);
        }

        public Size TextSize(string msg, int size)
        {
            SdlDotNet.Graphics.Font font = GetFontSurfaceFromCache(size);
            return font.SizeText(msg);
        }
        
        private void DrawSmallText(string msg, int x, int y, LineAlignment lineAlignment, Color color)
        {
            DrawText(msg, x, y, 12, lineAlignment, color);
        }

        private void DrawLargeText(string msg, int x, int y, LineAlignment lineAlignment, Color color)
        {
            DrawText(msg, x, y, 22, lineAlignment, color);
        }

        public void ClearRect(int x, int y, int width, int height)
        {

        }
    }
}
