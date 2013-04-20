using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using libtcodWrapper;

namespace RogueBasin
{
    /// <summary>
    /// Renders a multi-layer tile map onto the screen
    /// </summary>
    class MapRendererLibTCod
    {
        /// <summary>
        /// Render the map, with TL in map at mapOffset. Screenviewport is the screen viewport in tile dimensions (for now)
        /// </summary>
        /// <param name="mapToRender"></param>
        /// <param name="mapOffset"></param>
        /// <param name="screenViewport"></param>
        public static void RenderMap(TileEngine.TileMap mapToRender, Point mapOffset, Rectangle screenViewport) {

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
                                rootConsole.BackgroundColor = colorFlags.BackgroundColor;
                            }

                            rootConsole.ForegroundColor = colorFlags.ForegroundColor;
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

        }
    }
}
