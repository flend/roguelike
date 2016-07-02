using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using libtcodWrapper;

namespace RogueBasin
{
    /// <summary>
    /// Renders a multi-layer tile map onto the screen
    /// </summary>
    class MapRendererLibTCod : IMapRenderer
    {
        private libtcodWrapper.Color colorFromSystemColor(System.Drawing.Color color)
        {
            return libtcodWrapper.Color.FromRGB(color.R, color.G, color.B);
        }

        /// <summary>
        /// Render the map, with TL in map at mapOffset. Screenviewport is the screen viewport in tile dimensions (for now)
        /// </summary>
        /// <param name="mapToRender"></param>
        /// <param name="mapOffset"></param>
        /// <param name="screenViewport"></param>
        public void RenderMap(TileEngine.TileMap mapToRender, Point mapOffset, Rectangle screenViewport) {

            //For libtcod
            //tileID = ascii char
            //flags = color

            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

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
                        char screenChar = Convert.ToChar(thisCell.TileID);

                        //Screen coords
                        int screenX = screenViewport.X + (x - mapOffset.x);
                        int screenY = screenViewport.Y + (y - mapOffset.y);
                        
                        rootConsole.PutChar(screenX, screenY, screenChar);
                    }
                }
            }

            //Reset colors - this matters for systems that don't use the tile renderer
            rootConsole.ForegroundColor = libtcodWrapper.ColorPresets.White;
            rootConsole.BackgroundColor = libtcodWrapper.ColorPresets.Black;
        }

        public void Sleep(ulong milliseconds)
        {
            TCODSystem.Sleep((uint)milliseconds);
        }

        public void Setup(int width, int height)
        {

            int tileSize = 16;

            try
            {
                tileSize = Convert.ToInt16(Game.Config.Entries["tilesize"]);
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Error getting tilesize from config file", LogDebugLevel.High);
            }

            string tileMapFilename = "FileSupport.dll";
            if (tileSize == 32)
                tileMapFilename = "StreamSupport.dll";

            CustomFontRequest fontReq = new CustomFontRequest(tileMapFilename, tileSize, tileSize, CustomFontRequestFontTypes.LayoutAsciiInRow);

            RootConsole.Width = width;
            RootConsole.Height = height;
            RootConsole.WindowTitle = "TraumaRL";
            RootConsole.Fullscreen = false;
            RootConsole.Font = fontReq;
        }

        public void Flush()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.Flush();
        }

        public void Clear()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.Clear();
        }

        public void DrawFrame(int x, int y, int width, int height, bool clear, System.Drawing.Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            if (color != null)
            {
                rootConsole.ForegroundColor = colorFromSystemColor(color);
            }

            //Draw frame - same as inventory
            rootConsole.DrawFrame(x, y, width, height, clear);

            rootConsole.ForegroundColor = libtcodWrapper.ColorPresets.White;
        }

        public void PutChar(int x, int y, char c, System.Drawing.Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();
            rootConsole.ForegroundColor = colorFromSystemColor(color);

            rootConsole.PutChar(x, y, c);

            rootConsole.ForegroundColor = libtcodWrapper.ColorPresets.White;
        }

        public void PrintStringRect(string msg, int x, int y, int width, int height, LineAlignment alignment, System.Drawing.Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.ForegroundColor = colorFromSystemColor(color);
            rootConsole.PrintLineRect(msg, x, y, width, height, LineAlignmentToTCOD(alignment));
            rootConsole.ForegroundColor = libtcodWrapper.ColorPresets.White;
        }

        public void PrintString(string msg, int x, int y, System.Drawing.Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();
            rootConsole.ForegroundColor = colorFromSystemColor(color);

            rootConsole.PrintLine(msg, x, y, libtcodWrapper.LineAlignment.Left);
            rootConsole.ForegroundColor = libtcodWrapper.ColorPresets.White;
        }

        public void ClearRect(int x, int y, int width, int height)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.DrawRect(x, y, width, height, true);
        }

        private libtcodWrapper.LineAlignment LineAlignmentToTCOD(LineAlignment align) {

            if (align == LineAlignment.Center)
            {
                return libtcodWrapper.LineAlignment.Center;
            }

            if (align == LineAlignment.Right)
            {
                return libtcodWrapper.LineAlignment.Right;
            }

            return libtcodWrapper.LineAlignment.Left;
        }
    }
}
