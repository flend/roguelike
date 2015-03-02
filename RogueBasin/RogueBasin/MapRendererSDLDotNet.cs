using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SdlDotNet.Graphics;
using SdlDotNet.Core;


namespace RogueBasin
{
    /// <summary>
    /// Renders a multi-layer tile map onto the screen
    /// </summary>
    class MapRendererSDLDotNet : IMapRenderer
    {

        private Surface videoSurface;
        private Surface spriteSheet;
        private int videoWidth = 640;
        private int videoHeight = 480;
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

            //For libtcod
            //tileID = ascii char
            //flags = color

            //Get screen handle

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

                        //Flags is a color for libtcod
                        /*
                        LibtcodColorFlags colorFlags = thisCell.TileFlag as LibtcodColorFlags;
                        if (colorFlags == null)
                        {
                            rootConsole.ForegroundColor = ColorPresets.White;
                            rootConsole.BackgroundColor = ColorPresets.Black;
                        }
                        else
                        {
                            if (colorFlags.BackgroundColor == null)
                            {
                                rootConsole.BackgroundColor = ColorPresets.Black;
                            }
                            else
                            {
                                rootConsole.BackgroundColor = colorFromSystemColor(colorFlags.BackgroundColor);
                            }

                            rootConsole.ForegroundColor = colorFromSystemColor(colorFlags.ForegroundColor);
                        }

                        //Id is the char
                        char screenChar = Convert.ToChar(thisCell.TileID);*/

                        //Location on sprite sheet
                        int spriteTileX = thisCell.TileID % spritesPerRow;
                        int spriteTileY = thisCell.TileID / spritesPerRow;

                        int spriteX = spriteTileX * spriteSheetWidth;
                        int spriteY = spriteTileY * spriteSheetHeight;

                        //Screen tile coords
                        int screenTileX = screenViewport.X + (x - mapOffset.x);
                        int screenTileY = screenViewport.Y + (y - mapOffset.y);

                        //Screen real coords
                        int screenX = screenTileX * spriteSheetWidth;
                        int screenY = screenTileY * spriteSheetHeight;

                        LogFile.Log.LogEntry("Drawing sprite " + thisCell.TileID + " from " + spriteX + "/" + spriteY + "at: "
                            + screenX + "/" + screenY);

                        videoSurface.Blit(spriteSheet,
                            new System.Drawing.Point(screenX, screenY),
                            new Rectangle(new System.Drawing.Point(spriteX, spriteY), new Size(spriteSheetWidth, spriteSheetHeight)));
                    }
                }
            }

            videoSurface.Update();
        }

        public void Sleep(ulong milliseconds)
        {
        }

        public void Setup(int width, int height)
        {
            videoSurface = Video.SetVideoMode(640, 480, 32, false, false, false, true);

            spriteSheet = new Surface(@"TraumaSprites.png").Convert(videoSurface, true, true);
            spriteSheet.Transparent = true;
            spriteSheet.TransparentColor = Color.FromArgb(255, 0, 255);
        }


        public void Flush()
        {

        }

        public void Clear()
        {

        }

        public void DrawFrame(int x, int y, int width, int height, bool clear, Color color)
        {

        }

        public void PutChar(int x, int y, char c, Color color)
        {

        }

        public void PrintStringRect(string msg, int x, int y, int width, int height, libtcodWrapper.LineAlignment alignment, Color color)
        {

        }

        public void PrintString(string msg, int x, int y, libtcodWrapper.LineAlignment alignment, Color color)
        {

        }

        public void ClearRect(int x, int y, int width, int height)
        {

        }
    }
}
