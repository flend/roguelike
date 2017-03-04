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
        public int Id { get; set; }
        public double AlphaOverride { get; set; }
        public double Scaling { get; set; }
        public LibtcodColorFlags Flags { get; set; }

        public SpriteCacheEntry(string strId)
        {
            Scaling = 1.0;
            StrId = strId;
        }

        public SpriteCacheEntry(int id, LibtcodColorFlags flags)
        {
            Scaling = 1.0;
            Id = id;
            Flags = flags;
        }

        public override bool Equals(object other)
        {
            var otherFoo = other as SpriteCacheEntry;
            if (otherFoo == null)
                return false;
            return GetUniqueIdString() == otherFoo.GetUniqueIdString() && AlphaOverride - otherFoo.AlphaOverride < 0.001 && Scaling - otherFoo.Scaling < 0.001;
        }

        public string GetUniqueIdString() {

            if(StrId != null) {
                return StrId;
            }
            else {
                string cacheStr = "trauma" + Id;
                if (Flags != null)
                {
                    if (Flags.ForegroundColor != null)
                    {
                        cacheStr += Flags.ForegroundColor;
                    }
                    cacheStr += "/";
                    if (Flags.BackgroundColor != null) {
                        cacheStr += Flags.BackgroundColor;
                    }
                }
                return cacheStr;
            }
        }

        public override int GetHashCode()
        {
           return 17 * GetUniqueIdString().GetHashCode() + 17 * AlphaOverride.GetHashCode() + 17 * Scaling.GetHashCode();

        }
    }

    /// <summary>
    /// Renders a multi-layer tile map onto the screen
    /// </summary>
    class ScreenRendererSDLDotNet : IScreenRenderer
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

        private int traumaSpriteSheetWidth = 16;
        private int traumaSpriteSheetHeight = 16;

        private int traumaSpriteScaling = 4;


        public ScreenRendererSDLDotNet()
        {

        }

        private Surface GetTraumaSprite(SpriteCacheEntry entry)
        {
            var spriteLoc = tileIDToSpriteLocation(entry.Id);

            Surface spriteSurface;
            Surface tempSpriteSurface = new Surface(spriteSheetWidth, spriteSheetHeight);

             tempSpriteSurface.Blit(spriteSheet,
                new System.Drawing.Point(0, 0),
                new Rectangle(spriteLoc, new Size(spriteSheetWidth, spriteSheetHeight)));

            var colorFlags = entry.Flags as LibtcodColorFlags;

            if (colorFlags.ForegroundColor != Color.White)
                tempSpriteSurface.ReplaceColor(Color.White, colorFlags.ForegroundColor);
            if (colorFlags.BackgroundColor != Color.Transparent)
                tempSpriteSurface.ReplaceColor(transparentColor, colorFlags.BackgroundColor);

            spriteSurface = tempSpriteSurface;

            double totalScaling = traumaSpriteScaling * entry.Scaling;

            if (totalScaling > 1.001 || totalScaling < 0.999)
            {
                if (totalScaling < 2.12 && traumaSpriteScaling > 1.98)
                {
                    spriteSurface = tempSpriteSurface.CreateScaleDoubleSurface(false);
                }
                else
                {
                    spriteSurface = tempSpriteSurface.CreateScaledSurface(totalScaling, false);
                }
            }

            //Set this after doing background replacement
            spriteSurface.Transparent = true;
            spriteSurface.TransparentColor = transparentColor;

            LogFile.Log.LogEntryDebug("Loading trauma sprite: " + entry.Id, LogDebugLevel.Medium);

            return spriteSurface;
        }

        public void DrawScaledSprite(string id, int x, int y, double scaling = 1.0, double alpha = 1.0, bool isAnimated = false, int frameNo = 0)
        {
            DrawScaledSprite(id, x, y, scaling, alpha, frameNo, isAnimated);
        }

        public void DrawTraumaSprite(int id, int x, int y, LibtcodColorFlags flags, double scaling = 1.0, double alpha = 1.0)
        {
            DrawScaledSprite(id, new Point(x, y), flags, scaling, alpha);     
        }

        public Size GetSpriteDimensions(string id)
        {
            var entry = new SpriteCacheEntry(id);

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
            return new Size(traumaSpriteSheetWidth * traumaSpriteScaling, traumaSpriteSheetHeight * traumaSpriteScaling);
        }

        /// <summary>
        /// Draw sprite in absolute screen coordinates
        /// </summary>
        /// <param name="id"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void DrawScaledSprite(string filePath, int x, int y, double spriteScaling, double alpha = 1.0, int frameNo = 0, bool isAnimated = false)
        {
            SpriteCacheEntry entry = new SpriteCacheEntry(filePath);
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
                    //Probably unsafe with scaling
                    videoSurface.Blit(spriteSurface, new System.Drawing.Point(x, y), new Rectangle(frameNo * tileSpriteSheetWidth, 0,
                        tileSpriteSheetWidth, tileSpriteSheetHeight));
                }

               
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Can't find sprite " + filePath, LogDebugLevel.High);
            }
        }

        private void DrawScaledSprite(int id, Point p, LibtcodColorFlags flags, double spriteScaling, double alpha = 1.0)
        {
            SpriteCacheEntry entry = new SpriteCacheEntry(id, flags);
            entry.AlphaOverride = alpha;
            entry.Scaling = spriteScaling;

            try
            {
                Surface spriteSurface = GetSpriteFromCache(entry);

                videoSurface.Blit(spriteSurface, p.ToPoint());

            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Can't find sprite id: " + id, LogDebugLevel.High);
            }
        }

        private string GetSpriteAssetPath(SpriteCacheEntry entry)
        {
            string filenameBase = "RogueBasin.bin.Debug.graphics.";

            string filenameSuffix;
            filenameSuffix = entry.StrId;

            string filename = filenameBase + filenameSuffix;
            return filename;
        }

        private Surface GetSpriteFromCache(SpriteCacheEntry entry)
        {
            Surface spriteSurface;
            spriteCache.TryGetValue(entry, out spriteSurface);
            
            if (spriteSurface == null)
            {
                if (entry.StrId == null)
                {
                    spriteSurface = GetTraumaSprite(entry);
                }
                else
                {
                    spriteSurface = GetFileSprite(spriteSurface, entry);
                }
                
                if (entry.AlphaOverride > 0.01)
                {
                    ModifyAlpha(spriteSurface, entry.AlphaOverride);
                }

                spriteCache.Add(entry, spriteSurface);

                LogFile.Log.LogEntryDebug("Storing double scaled sprite" + entry.StrId, LogDebugLevel.Profiling);
            }
            return spriteSurface;
        }

        private Surface GetFileSprite(Surface spriteSurface, SpriteCacheEntry entry)
        {
            //Convert knocks out alpha for some goddamn reason
            string filename = GetSpriteAssetPath(entry);

            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream fileStream = _assembly.GetManifestResourceStream(filename);
            MemoryStream memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);

            spriteSurface = new Surface(memoryStream);//.Convert(videoSurface, true, false);

            //Scale if required
            Surface scaledSpriteSurface = spriteSurface;

            if (entry.StrId != null && (entry.Scaling > 1.001 || entry.Scaling < 0.999))
            {
                scaledSpriteSurface = spriteSurface.CreateScaledSurface(entry.Scaling, true);
            }

            return scaledSpriteSurface;
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

            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream fileStream = _assembly.GetManifestResourceStream("RogueBasin.bin.Debug.graphics.tiles.trauma_tiles.png");
            MemoryStream memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            spriteSheet = new Surface(memoryStream).Convert(videoSurface, true, true);
        }


        public void Flush()
        {
            videoSurface.Update();
        }

        public void Clear()
        {
            videoSurface.Fill(Color.Black);
        }

        public void DrawLine(Point p1, Point p2, Color color)
        {
            IPrimitive line = new SdlDotNet.Graphics.Primitives.Line(p1.ToPoint(), p2.ToPoint());
            line.Draw(videoSurface, color);
        }

        public void DrawRectangle(Rectangle rect, Color color) 
        {
            videoSurface.Fill(rect, color);
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

            var pointToDraw = new System.Drawing.Point(x, y);

            videoSurface.Blit(fontSurface, pointToDraw);
        }

        public Size TextSize(string msg, int size)
        {
            SdlDotNet.Graphics.Font font = GetFontSurfaceFromCache(size);
            return font.SizeText(msg);
        }
       
    }
}
