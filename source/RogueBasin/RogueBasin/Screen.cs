using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using Console = System.Console;

namespace RogueBasin {

    //Represents our screen
    public class Screen
    {
        static Screen instance = null;

        //Console/screen size
        int width;
        int height;

        //Top left coord to start drawing the map at
        Point mapTopLeft;

        /// <summary>
        /// Dimensions of message display area
        /// </summary>
        Point msgDisplayTopLeft;
        int msgDisplayNumLines;

        Point statsDisplayTopLeft;

        Point hitpointsOffset;
        Point maxHitpointsOffset;
        Point speedOffset;
        Point worldTickOffset;
        Point levelOffset;

        /// <summary>
        /// Mapping of terrain to ASCII characters
        /// </summary>
        Dictionary<MapTerrain, char> terrainChars;

        Color inFOVTerrainColor = ColorPresets.White;
        Color seenNotInFOVTerrainColor = ColorPresets.Gray;
        Color neverSeenFOVTerrainColor;
        Color inMonsterFOVTerrainColor = ColorPresets.Blue;

        Color pcColor = ColorPresets.White;

        Color creatureColor = ColorPresets.White;
        Color itemColor = ColorPresets.White;
        Color featureColor = ColorPresets.White;

        //Keep enough state so that we can draw each screen
        string lastMessage = "";

        //Inventory
        Point inventoryTL;
        Point inventoryTR;
        Point inventoryBL;

        bool displayInventory;
        int selectedInventoryIndex;
        int topInventoryIndex;

        Inventory currentInventory;
        string inventoryTitle;
        string inventoryInstructions;
        
        public static Screen Instance
        {
            get
            {
                if (instance == null)
                    instance = new Screen();
                return instance;
            }
        }


        Screen()
        {
            width = 90;
            height = 35;

            mapTopLeft = new Point(5, 5);

            msgDisplayTopLeft = new Point(0, 1);
            msgDisplayNumLines = 1;

            statsDisplayTopLeft = new Point(0, 31);

            hitpointsOffset = new Point(6, 0);
            maxHitpointsOffset = new Point(13, 0);
            speedOffset = new Point(20, 0);
            worldTickOffset = new Point(30, 0);

            levelOffset = new Point(40, 0);

            inventoryTL = new Point(5, 5);
            inventoryTR = new Point(75, 5);
            inventoryBL = new Point(5, 30);

            terrainChars = new Dictionary<MapTerrain, char>();
            terrainChars.Add(MapTerrain.Empty, '.');
            terrainChars.Add(MapTerrain.Wall, '#');
            terrainChars.Add(MapTerrain.Corridor, '|');
            terrainChars.Add(MapTerrain.Void, ' ');

            //Colors
            neverSeenFOVTerrainColor = Color.FromRGB(90, 90, 90);
        }

        //Setup the screen
        public void InitialSetup()
        {
            //Note that 

            //CustomFontRequest fontReq = new CustomFontRequest("terminal.png", 8, 8, CustomFontRequestFontTypes.Grayscale);
            RootConsole.Width = width;
            RootConsole.Height = height;
            RootConsole.WindowTitle = "RogueBase";
            RootConsole.Fullscreen = false;
            //RootConsole.Font = fontReq;
            /*
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.PrintLine("Hello world!", 30, 30, LineAlignment.Left);
            rootConsole.Flush();
            */
            Console.WriteLine("debug test message.");

        }

        /// <summary>
        /// Call after all drawing is complete to output onto screen
        /// </summary>
        public void FlushConsole()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.Flush();
        }

        //Draw the current dungeon map and objects
        public void Draw()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            Dungeon dungeon = Game.Dungeon;
            Player player = dungeon.Player;

            //Clear screen
            rootConsole.Clear();

            //Draw the map screen

            //Draw terrain
            DrawMap(dungeon.PCMap);

            //Draw fixed features
            DrawFeatures(dungeon.Features);

            //Draw items
            DrawItems(dungeon.Items);

            //Draw creatures
            DrawCreatures(dungeon.Monsters);

            //Draw PC

            Point PClocation = player.LocationMap;

            rootConsole.ForegroundColor = pcColor;
            rootConsole.PutChar(mapTopLeft.x + PClocation.x, mapTopLeft.y + PClocation.y, player.Representation);        

            //Draw Stats
            DrawStats(dungeon.Player);

            //Draw any overlay screens
            if (displayInventory)
                DrawInventory();

        }

        /// <summary>
        /// Display inventory overlay
        /// </summary>
        private void DrawInventory()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect(inventoryTitle, (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect(inventoryInstructions, (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //List the inventory
            
            //Inventory area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            List<InventoryListing> inventoryList = currentInventory.InventoryListing;

            for (int i = 0; i < inventoryListH; i++)
            {
                int inventoryIndex = topInventoryIndex + i;

                //End of inventory
                if (inventoryIndex == inventoryList.Count)
                    break;

                //Create entry string
                char selectionChar = (char)((int)'a' + i);
                string entryString = "(" + selectionChar.ToString() + ") " + inventoryList[inventoryIndex].Description;

                //Print entry
                rootConsole.PrintLineRect(entryString, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left);
            }
        }

        private void DrawStats(Player player)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            string hitpointsString = "HP: " + player.Hitpoints.ToString();
            string maxHitpointsString = "/" + player.MaxHitpoints.ToString();

            rootConsole.PrintLine(hitpointsString, statsDisplayTopLeft.x + hitpointsOffset.x, statsDisplayTopLeft.y + hitpointsOffset.y, LineAlignment.Left);
            rootConsole.PrintLine(maxHitpointsString, statsDisplayTopLeft.x + maxHitpointsOffset.x, statsDisplayTopLeft.y + maxHitpointsOffset.y, LineAlignment.Left);

            string speedString = "Sp: " + player.Speed.ToString();

            rootConsole.PrintLine(speedString, statsDisplayTopLeft.x + speedOffset.x, statsDisplayTopLeft.y + speedOffset.y, LineAlignment.Left);

            string ticksString = "Tk: " + Game.Dungeon.WorldClock.ToString();

            rootConsole.PrintLine(ticksString, statsDisplayTopLeft.x + worldTickOffset.x, statsDisplayTopLeft.y + worldTickOffset.y, LineAlignment.Left);

            string levelString = "Level: " + Game.Dungeon.Player.LocationLevel.ToString();

            rootConsole.PrintLine(levelString, statsDisplayTopLeft.x + levelOffset.x, statsDisplayTopLeft.y + levelOffset.y, LineAlignment.Left);
        }

        private void DrawItems(List<Item> itemList)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Set default colour
            rootConsole.ForegroundColor = itemColor;

            //Could consider storing here and sorting to give an accurate representation of multiple objects

            foreach (Item item in itemList)
            {
                //Don't draw items on creatures
                if (item.InInventory)
                    continue;

                //Don't draw items on other levels
                if (item.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                //Colour depending on FOV (for development)
                MapSquare itemSquare = Game.Dungeon.Levels[item.LocationLevel].mapSquares[item.LocationMap.x, item.LocationMap.y];

                if (itemSquare.InPlayerFOV)
                {
                    //In FOV
                    rootConsole.ForegroundColor = inFOVTerrainColor;
                }
                else if (itemSquare.SeenByPlayer)
                {
                    //Not in FOV but seen
                    rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                }
                else
                {
                    //Never in FOV
                    rootConsole.ForegroundColor = neverSeenFOVTerrainColor;
                }

                rootConsole.PutChar(mapTopLeft.x + item.LocationMap.x, mapTopLeft.y + item.LocationMap.y, item.Representation);
            }

        }

        private void DrawFeatures(List<Feature> featureList)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Set default colour
            //rootConsole.ForegroundColor = featureColor;

            //Could consider storing here and sorting to give an accurate representation of multiple objects

            foreach (Feature feature in featureList)
            {
                //Don't draw features on other levels
                if (feature.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                //Colour depending on FOV (for development)
                MapSquare featureSquare = Game.Dungeon.Levels[feature.LocationLevel].mapSquares[feature.LocationMap.x, feature.LocationMap.y];

                if (featureSquare.InPlayerFOV)
                {
                    //In FOV
                    rootConsole.ForegroundColor = inFOVTerrainColor;
                }
                else if (featureSquare.SeenByPlayer)
                {
                    //Not in FOV but seen
                    rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                }
                else
                {
                    //Never in FOV
                    rootConsole.ForegroundColor = neverSeenFOVTerrainColor;
                }

                rootConsole.PutChar(mapTopLeft.x + feature.LocationMap.x, mapTopLeft.y + feature.LocationMap.y, feature.Representation);
            }

        }

        private void DrawCreatures(List<Monster> creatureList)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Set default colour
            //rootConsole.ForegroundColor = creatureColor;

            foreach (Monster creature in creatureList)
            {
                //Not on this level
                if (creature.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                //Colour depending on FOV (for development)
                MapSquare creatureSquare = Game.Dungeon.Levels[creature.LocationLevel].mapSquares[creature.LocationMap.x, creature.LocationMap.y];

                if (creatureSquare.InPlayerFOV)
                {
                    //In FOV
                    rootConsole.ForegroundColor = inFOVTerrainColor;
                }
                else if (creatureSquare.SeenByPlayer)
                {
                    //Not in FOV but seen
                    rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                }
                else
                {
                    //Never in FOV
                    rootConsole.ForegroundColor = neverSeenFOVTerrainColor;
                }

                rootConsole.PutChar(mapTopLeft.x + creature.LocationMap.x, mapTopLeft.y + creature.LocationMap.y, creature.Representation);
            }
        }

        public void DrawFOVDebug(int levelNo)
        {
            Map map = Game.Dungeon.Levels[levelNo];
            TCODFov fov = Game.Dungeon.FOVs[levelNo];

            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    bool trans;
                    bool walkable;

                    fov.GetCell(i, j, out trans, out walkable);

                    Color drawColor = inFOVTerrainColor;

                    if (walkable)
                    {
                        drawColor = inFOVTerrainColor;
                    }
                    else
                    {
                        drawColor = inMonsterFOVTerrainColor;
                    }

                    rootConsole.ForegroundColor = drawColor;
                    char screenChar = terrainChars[map.mapSquares[i, j].Terrain];
                    screenChar = '#';
                    rootConsole.PutChar(screenX, screenY, screenChar);

                    rootConsole.Flush();
                }
            }

        }

        //Draw a map only (useful for debugging)
        public void DrawMapDebug(Map map)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    char screenChar = terrainChars[map.mapSquares[i, j].Terrain];

                    Color drawColor = inFOVTerrainColor;

                    if (!map.mapSquares[i, j].BlocksLight)
                    {
                        //In FOV
                        rootConsole.ForegroundColor = inFOVTerrainColor;
                    }
                    else
                    {
                        //Not in FOV but seen
                        rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                    }
                    rootConsole.PutChar(screenX, screenY, screenChar);
                }
            }
            
            //Flush the console
            rootConsole.Flush();
        }

        //Draw a map only (useful for debugging)
        public void DrawMapDebugHighlight(Map map, int x1, int y1, int x2, int y2)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    char screenChar = terrainChars[map.mapSquares[i, j].Terrain];

                    Color drawColor = inFOVTerrainColor;

                    if (i == x1 && j == y1)
                    {
                        drawColor = ColorPresets.Red;
                    }

                    if (i == x2 && j == y2)
                    {
                        drawColor = ColorPresets.Red;
                    }
                    rootConsole.ForegroundColor = drawColor;
                    /*
                    if (!map.mapSquares[i, j].BlocksLight)
                    {
                        //In FOV
                        rootConsole.ForegroundColor = inFOVTerrainColor;
                    }
                    else
                    {
                        //Not in FOV but seen
                        rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                    }*/
                    rootConsole.PutChar(screenX, screenY, screenChar);
                }
            }

            //Flush the console
            rootConsole.Flush();
        }

        private void DrawMap(Map map)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    char screenChar = terrainChars[map.mapSquares[i, j].Terrain];

                    Color drawColor = inFOVTerrainColor;

                    if (map.mapSquares[i, j].InPlayerFOV)
                    {
                        //In FOV
                        rootConsole.ForegroundColor = inFOVTerrainColor;
                    }
                    else if (map.mapSquares[i, j].InMonsterFOV)
                    {
                        //Monster can see it
                        rootConsole.ForegroundColor = inMonsterFOVTerrainColor;
                    }
                    else if (map.mapSquares[i, j].SeenByPlayer)
                    {
                        //Not in FOV but seen
                        rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                    }
                    else
                    {
                        //Never in FOV
                        rootConsole.ForegroundColor = neverSeenFOVTerrainColor;
                    }
                    rootConsole.PutChar(screenX, screenY, screenChar);
                }
            }
        }
        internal void ConsoleLine(string datedEntry)
        {
            Console.WriteLine(datedEntry);
        }

        internal void ClearMessageLine()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            lastMessage = null;

            ClearMessageBar();
        }

        internal void PrintMessage(string message)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Update state
            lastMessage = message;

            //Clear message bar
            ClearMessageBar();

            //Display new message
            rootConsole.PrintLineRect(message, msgDisplayTopLeft.x, msgDisplayTopLeft.y, width - msgDisplayTopLeft.x, msgDisplayNumLines, LineAlignment.Left);
        }

        void ClearMessageBar()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.DrawRect(msgDisplayTopLeft.x, msgDisplayTopLeft.y, width - msgDisplayTopLeft.x, msgDisplayNumLines, true);
        }

        public bool DisplayInventory
        {
            set
            {
                displayInventory = value;
            }
        }

        public int SelectedInventoryIndex
        {
            set
            {
                selectedInventoryIndex = value;
            }
        }

        public int TopInventoryIndex
        {
            set
            {
                topInventoryIndex = value;
            }
        }

        public Inventory CurrentInventory
        {
            set
            {
                currentInventory = value;
            }
        }

        /// <summary>
        /// String displayed at the top of the inventory
        /// </summary>
        public string InventoryTitle
        {
            set
            {
                inventoryTitle = value;
            }
        }

        /// <summary>
        /// String displayed at the bottom of the inventory
        /// </summary>
        public string InventoryInstructions
        {
            set
            {
                inventoryInstructions = value;
            }
        }
    }
}
