using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using Console = System.Console;

namespace RogueBasin {

    //Represents our screen - could be a singleton
    public class Screen
    {
        //Console/screen size
        int width;
        int height;

        Dungeon dungeon;

        //Top left coord to start drawing the map at
        Point mapTopLeft;

        Dictionary<Map.MapTerrain, char> terrainChars;
        char PCChar;
        
        public Screen()
        {
            width = 100;
            height = 40;

            mapTopLeft = new Point(5, 5);

            terrainChars = new Dictionary<Map.MapTerrain, char>();
            terrainChars.Add(Map.MapTerrain.Empty, '.');
            terrainChars.Add(Map.MapTerrain.Wall, '#');

            PCChar = '@';
        }

        public Dungeon Dungeon
        {
            set
            {
                dungeon = value;
            }
        }

        //Setup the screen
        public void InitialSetup()
        {
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

        //Draw the current dungeon map
        public void Draw()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            //Draw map
            Map map = dungeon.PCMap;

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    rootConsole.PutChar(screenX, screenY, terrainChars[map.mapSquares[i, j]]);

                }
            }
            
            //Draw PC

            Point PClocation = dungeon.PCLocation;

            rootConsole.PutChar(mapTopLeft.x + PClocation.x, mapTopLeft.y + PClocation.y, PCChar);

            //Flush the console
            rootConsole.Flush();
        }


        internal void ConsoleLine(string datedEntry)
        {
            Console.WriteLine(datedEntry);
        }
    }
}
