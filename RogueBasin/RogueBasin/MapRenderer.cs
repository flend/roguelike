using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    class MapRenderer
    {
        private IScreenRenderer renderer;

        public int spriteVideoWidth { get; set; }
        public int spriteVideoHeight { get; set; }

        private double spriteVideoWidthScaling = 1.0;
        private double spriteVideoHeightScaling = 1.0;

        //This is replicated in ScreenRendererSDLDotNet - since it needs to know it for animations
        private int tileSpriteSheetWidth = 64;
        private int tileSpriteSheetHeight = 64;

        public MapRenderer(IScreenRenderer renderer)
        {
            this.renderer = renderer;
       
            spriteVideoWidth = 64;
            spriteVideoHeight = 64;
        }

        public void SetSpriteVideoSize(int width, int height)
        {
            spriteVideoWidth = width;
            spriteVideoHeight = height;

            spriteVideoWidthScaling = spriteVideoWidth / (double)tileSpriteSheetWidth;
            spriteVideoHeightScaling = spriteVideoHeight / (double)tileSpriteSheetHeight;
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

                        if (thisCell.TileSprite == null && thisCell.TileID == -1)
                        {
                            continue;
                        }

                        //Screen tile coords
                        int screenTileX = screenViewport.X + (x - mapOffset.x);
                        int screenTileY = screenViewport.Y + (y - mapOffset.y);

                        int offsetX = 0;
                        int offsetY = 0;

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
                            else if (thisCell.TileSprite == "shotgun" || thisCell.TileSprite == "laser" || thisCell.TileSprite == "rifle")
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
                            if (thisCell.TileSprite != null && thisCell.TileSprite.Length > 14 &&
                                (thisCell.TileSprite.Substring(0, 14) == "monster_level_" || thisCell.TileSprite.Substring(0, 14) == "room_numbering"))
                            {
                                var thisLevel = thisCell.TileSprite.Substring(thisCell.TileSprite.LastIndexOf('_') + 1);
                                var thisLevelNum = Convert.ToInt32(thisLevel);
                                var thisTens = thisLevelNum / 10;
                                var thisRem = thisLevelNum % 10;

                                if (thisTens == 0)
                                {
                                    var units = new TileEngine.TileCell("monster_level_" + thisRem.ToString());
                                    DrawTileSprite(units, new Point(screenTileX, screenTileY), new Point(offsetX, offsetY), thisCell.Transparency, 0, false);
                                }
                                else
                                {
                                    var tens = new TileEngine.TileCell("monster_level_" + thisTens.ToString());
                                    var units = new TileEngine.TileCell("monster_level_" + thisRem.ToString());

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

                        DrawTileSprite(thisCell, new Point(screenTileX, screenTileY), new Point(offsetX, offsetY), thisCell.Transparency, frameNo, isAnimated);
                        continue;
                    }
                }
                layerNumber++;
            }
        }


        private string TileSpritePath(string spriteId)
        {
            return "tiles." + spriteId + ".png";
        }

        private void DrawTileSprite(TileEngine.TileCell cell, Point tileCoords, Point offset, double alpha, int frameNo = 0, bool isAnimated = false)
        {
            //Tile x, y are the top left of a 64x64 tile
            //Our tile sprites may not be 64x64, but are aligned to the BOTTOM-LEFT of a 64x64 tile (I hope)
            Size tileDimensions = GetTileSpriteDimensions(cell);

            //Offset into source bitmap
            Point offsetIntoSourceBitmap = new Point(offset.x, -(tileDimensions.Height - tileSpriteSheetHeight) + offset.y);

            if (cell.TileSprite == "boss")
            {
                offsetIntoSourceBitmap = new Point(offset.x - tileSpriteSheetWidth / 2, offset.y - 128);
            }

            Point screenOffset = new Point((int)Math.Floor(offsetIntoSourceBitmap.x * spriteVideoWidthScaling),
                (int)Math.Floor(offsetIntoSourceBitmap.y * spriteVideoHeightScaling));

            //Screen real coords
            Point screenCoords = new Point(tileCoords.x * spriteVideoWidth + screenOffset.x,
                tileCoords.y * spriteVideoHeight + screenOffset.y);

            DrawScaledTileSprite(cell, screenCoords, alpha, frameNo, isAnimated);
        }

        private void DrawScaledTileSprite(TileEngine.TileCell cell, Point screenCoords, double alpha = 1.0, int frameNo = 0, bool isAnimated = false)
        {
            if (cell.TileSprite == null)
            {
                renderer.DrawTraumaSprite(cell.TileID, screenCoords.x, screenCoords.y, cell.TileFlag as LibtcodColorFlags, spriteVideoWidthScaling, alpha);
            }
            else
            {
                renderer.DrawScaledSprite(TileSpritePath(cell.TileSprite), screenCoords.x, screenCoords.y, spriteVideoWidthScaling, alpha, isAnimated, frameNo);
            }
        }

        public Size GetTileSpriteDimensions(TileEngine.TileCell cell)
        {
            if (cell.TileSprite == null)
            {
                return renderer.GetTraumaSpriteDimensions(cell.TileID);
            }
            else
            {
                return renderer.GetSpriteDimensions(TileSpritePath(cell.TileSprite));
            }
        }
    }
}
