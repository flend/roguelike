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

        /// <summary>
        /// Mapping of terrain to ASCII characters
        /// </summary>
        Dictionary<MapTerrain, char> terrainChars;

        //Keep enough state so that we can draw each screen
        string lastMessage = "";



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

            terrainChars = new Dictionary<MapTerrain, char>();
            terrainChars.Add(MapTerrain.Empty, '.');
            terrainChars.Add(MapTerrain.Wall, '#');
            terrainChars.Add(MapTerrain.Corridor, '|');
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

            DrawMap(dungeon.PCMap);

            //Draw fixed features
            DrawFeatures(dungeon.Features);

            //Draw items

            DrawItems(dungeon.Items);

            //Draw creatures

            DrawCreatures(dungeon.Monsters);

            //Draw PC

            Point PClocation = player.LocationMap;

            rootConsole.PutChar(mapTopLeft.x + PClocation.x, mapTopLeft.y + PClocation.y, player.Representation);        

            //Draw Stats
            DrawStats(dungeon.Player);
        }

        private void DrawStats(Player player)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            string hitpointsString = "HP: " + player.Hitpoints.ToString();
            string maxHitpointsString = "/" + player.MaxHitpoints.ToString();

            rootConsole.PrintLine(hitpointsString, statsDisplayTopLeft.x + hitpointsOffset.x, statsDisplayTopLeft.y + hitpointsOffset.y, LineAlignment.Left);
            rootConsole.PrintLine(maxHitpointsString, statsDisplayTopLeft.x + maxHitpointsOffset.x, statsDisplayTopLeft.y + maxHitpointsOffset.y, LineAlignment.Left);
        }

        private void DrawItems(List<Item> itemList)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Could consider storing here and sorting to give an accurate representation of multiple objects

            foreach (Item item in itemList)
            {
                //Don't draw items on creatures
                if (item.InInventory)
                    continue;

                //Don't draw items on other levels
                if (item.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                rootConsole.PutChar(mapTopLeft.x + item.LocationMap.x, mapTopLeft.y + item.LocationMap.y, item.Representation);
            }

        }

        private void DrawFeatures(List<Feature> featureList)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Could consider storing here and sorting to give an accurate representation of multiple objects

            foreach (Feature feature in featureList)
            {
                //Don't draw features on other levels
                if (feature.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                rootConsole.PutChar(mapTopLeft.x + feature.LocationMap.x, mapTopLeft.y + feature.LocationMap.y, feature.Representation);
            }

        }

        private void DrawCreatures(List<Monster> creatureList)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            foreach (Monster creature in creatureList)
            {
                //Not on this level
                if (creature.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                rootConsole.PutChar(mapTopLeft.x + creature.LocationMap.x, mapTopLeft.y + creature.LocationMap.y, creature.Representation);
            }
        }

        //Draw a map only (useful for debugging)
        public void DrawMapDebug(Map map)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            DrawMap(map);
            
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

                    rootConsole.PutChar(screenX, screenY, terrainChars[map.mapSquares[i, j].Terrain]);

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

    }
}
