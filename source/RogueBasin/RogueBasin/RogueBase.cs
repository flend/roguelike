using System;

using libtcodWrapper;
using Console = System.Console;

namespace RogueBasin
{
    public class RogueBase : IDisposable
    {
        //Master object representing the game
        Dungeon dungeon;

        //Master object for the console & screen
        Screen screen;

        public RogueBase()
        {

        }

        public void Dispose()
        {
        }

        public int Run(string[] args)
        {
            SetupGame();

            MainLoop();

            //test code
            /*
            CustomFontRequest fontReq = new CustomFontRequest("terminal.png", 8, 8, CustomFontRequestFontTypes.Grayscale);
            RootConsole.Width = 80;
            RootConsole.Height = 50;
            RootConsole.WindowTitle = "Hello World!";
            RootConsole.Fullscreen = false;
            //RootConsole.Font = fontReq;

            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.PrintLine("Hello world!", 30, 30, LineAlignment.Left);
            rootConsole.Flush();

            Console.WriteLine("debug test message.");
            */
            Keyboard.WaitForKeyPress(true);

            return 1;
        }

        private void MainLoop()
        {
            //Time

            //Spool through list of creatures, giving them turns as appropriate

            //After each turn update screen - may not be required
            UpdateScreen();

            //Deal with PCs turn as appropriate

            //UserInput()

            //ProcessCommand()
            

        }

        private void UpdateScreen()
        {
            //Draw screen 
            screen.Draw();
            

            //Message queue - requires keyboard to advance messages - not sure about this yet
            RunMessageQueue();

        }

        private void RunMessageQueue()
        {

        }

        private void SetupGame()
        {
            //Initial setup

            //Create dungeon
            dungeon = new Dungeon();

            //Create dungeon map (at least level 1)
            MapGeneratorRogue mapGen = new MapGeneratorRogue();
            mapGen.Width = 80;
            mapGen.Height =25;
             
            Map level1 = mapGen.GenerateMap();

            //Give map to dungeon
            dungeon.AddMap(level1);
            dungeon.PCLocation = level1.PCStartLocation;

            //Create creatures and start positions

            //Create objects and start positions

            //Set PC start position


            //Setup screen
            screen = new Screen();
            screen.InitialSetup();
            screen.Dungeon = dungeon; //don't really like this
        }

    }
}
