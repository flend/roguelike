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
        public static Screen screen;

        //Are we running or have we exited?
        bool runMapLoop = true;

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
            //Keyboard.WaitForKeyPress(true);

            return 1;
        }

        private void MainLoop()
        {
            //Time

            //Increment world clock

            //Check PC
            //Take a turn if signalled by the internal clock

            //Loop through creatures
            //If their internal clocks signal another turn then take one



            while(runMapLoop) {

                //Add a time slice for the creature and process turn if applicable
                foreach (Creature creature in dungeon.Creatures)
                {
                    
                    creature.IncrementTurnTime();
                }
                

            //After each turn update screen - may not be required
            UpdateScreen();

            //Deal with PCs turn as appropriate

            UserInput();

            //ProcessCommand()
            }

        }

        //Deal with user input
        //Return code is if the command was successful. If not, don't increment time - not sure yet
        private bool UserInput()
        {
            bool commandSuccess = false;

            KeyPress userKey = Keyboard.CheckForKeypress(KeyPressType.Pressed);

            if (userKey.KeyCode == KeyCode.TCODK_CHAR)
            {
                char keyCode = (char)userKey.Character;
                switch (keyCode)
                {
                    case 'x':
                        runMapLoop = false;
                        break;
                }
            }
            else
            {
                switch (userKey.KeyCode)
                {
                    case KeyCode.TCODK_LEFT:
                        commandSuccess = dungeon.PCMove(-1, 0);
                        break;
                    case KeyCode.TCODK_RIGHT:
                        commandSuccess = dungeon.PCMove(1, 0);
                        break;
                    case KeyCode.TCODK_UP:
                        commandSuccess = dungeon.PCMove(0, -1);
                        break;
                    case KeyCode.TCODK_DOWN:
                        commandSuccess = dungeon.PCMove(0, 1);
                        break;
                }
            }

            return commandSuccess;
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

            //Setup screen
            screen = new Screen();
            screen.InitialSetup();

            //Setup logfile
            try
            {
                LogFile.Log.LogEntry("Starting log file");
            }
            catch (Exception e)
            {
                screen.ConsoleLine("Error creating log file: " + e.Message);
            }
                

            //Create dungeon
            dungeon = new Dungeon();

            //Tell screen about dungeon
            screen.Dungeon = dungeon; //don't really like this

            //Create dungeon map (at least level 1)
            MapGeneratorBSP mapGen = new MapGeneratorBSP();
            //MapGeneratorRogue mapGen = new MapGeneratorRogue();
            mapGen.Width = 80;
            mapGen.Height = 25;
             

            Map level1 = mapGen.GenerateMap();

            //Test
            //for (int i = 0; i < 10000; i++)
            //{
            //    mapGen.GenerateMap();
            //}

            //Give map to dungeon
            dungeon.AddMap(level1);

            //Set PC start location
            dungeon.PCLocation = level1.PCStartLocation;

            //Create creatures and start positions
            
            //Add some random creatures

            Random rand = new Random();
            int noCreatures = rand.Next(10);

            for (int i = 0; i < noCreatures; i++)
            {
                Creature creature = new Creature();
                creature.Representation = Convert.ToChar(65 + rand.Next(26));

                int level = 0;
                Point location;
                
                //Loop until we find an acceptable location and the add works
                do
                {
                    location = mapGen.RandomPointInRoom();
                }
                while (!dungeon.AddCreature(creature, level, location));
            }


            //Create objects and start positions

            //Set PC start position


            
        }

    }
}
