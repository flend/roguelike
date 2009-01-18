using System;

using libtcodWrapper;
using Console = System.Console;
using System.Collections.Generic;

namespace RogueBasin
{
    public class RogueBase : IDisposable
    {
        //Master object representing the game
        Dungeon dungeon;

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
            dungeon.IncrementWorldClock();

            //Check PC
            //Take a turn if signalled by the internal clock

            //Loop through creatures
            //If their internal clocks signal another turn then take one



            while (runMapLoop)
            {

                //Add a time slice for the creature and process turn if applicable
                //Creatures may be killed by other creatures so check they are alive before processing
                foreach (Monster creature in dungeon.Monsters)
                {
                    if (creature.IncrementTurnTime())
                    {
                        if (creature.Alive)
                        {
                            creature.ProcessTurn();
                            RecalculateMapAfterMove();
                            dungeon.ShowCreatureFOVOnMap(creature);
                        }
                    }
                }

                //Remove dead monsters
                dungeon.RemoveDeadMonsters();

                //Check if the PC gets a turn
                if (dungeon.Player.IncrementTurnTime())
                {
                    //Calculate the player's FOV
                    RecalculatePlayerFOV();

                    //Update screen just before PC's turn
                    UpdateScreen();

                    //Deal with PCs turn as appropriate
                    bool timeAdvances = false;
                    do
                    {
                        timeAdvances = UserInput();
                    } while (!timeAdvances);

                    RecalculateMapAfterMove();

                    //Reset the creature FOV display
                    Game.Dungeon.ResetCreatureFOVOnMap();

                    //UpdateScreen();

                    //Game.MessageQueue.AddMessage("Finished PC move");
                }
            }
        }

        private void RecalculatePlayerFOV()
        {
            Game.Dungeon.CalculatePlayerFOV();
        }

        /// <summary>
        /// After each move, recalculate the walkable parameter on each map square.
        /// This could be optimised a lot (far better to set creature's previous location as empty and new location as full)
        /// </summary>
        private void RecalculateMapAfterMove()
        {
            //Recalculate walkable
            Game.Dungeon.RecalculateWalkable();

            //Light blocking doesn't change

            //Refresh the TCOD maps
            Game.Dungeon.RefreshTCODMaps();
        }

        //Deal with user input
        //Return code is if the command was successful and time increments (i.e. the player has done a time-using command like moving)
        private bool UserInput()
        {
            bool timeAdvances = false;

            KeyPress userKey = Keyboard.WaitForKeyPress(true);

            if (userKey.KeyCode == KeyCode.TCODK_CHAR)
            {
                char keyCode = (char)userKey.Character;
                switch (keyCode)
                {
                    case 'x':
                        runMapLoop = false;
                        timeAdvances = true;
                        break;
                    case '.':
                        // Do nothing
                        timeAdvances = true;
                        break;
                }
            }
            else
            {
                switch (userKey.KeyCode)
                {
                    case KeyCode.TCODK_KP1:
                        timeAdvances = dungeon.PCMove(-1, 1);
                        break;

                    case KeyCode.TCODK_KP3:
                        timeAdvances = dungeon.PCMove(1, 1);
                        break;

                    case KeyCode.TCODK_KP5:
                        //Don't move
                        timeAdvances = true;
                        break;

                    case KeyCode.TCODK_KP7:
                        timeAdvances = dungeon.PCMove(-1, -1);
                        break;
                    case KeyCode.TCODK_KP9:
                        timeAdvances = dungeon.PCMove(1, -1);
                        break;

                    case KeyCode.TCODK_LEFT:
                    case KeyCode.TCODK_KP4:
                        timeAdvances = dungeon.PCMove(-1, 0);
                        break;
                    case KeyCode.TCODK_RIGHT:
                    case KeyCode.TCODK_KP6:
                        timeAdvances = dungeon.PCMove(1, 0);
                        break;
                    case KeyCode.TCODK_UP:
                    case KeyCode.TCODK_KP8:
                        timeAdvances = dungeon.PCMove(0, -1);
                        break;
                    case KeyCode.TCODK_KP2:
                    case KeyCode.TCODK_DOWN:
                        timeAdvances = dungeon.PCMove(0, 1);
                        break;
                }
            }

            return timeAdvances;
        }

        private void UpdateScreen()
        {
            //Draw screen 
            Screen.Instance.Draw();

            //Message queue - requires keyboard to advance messages - not sure about this yet
            RunMessageQueue();

        }

        /// <summary>
        /// Run through the messages for the user and require a key press after each one
        /// </summary>
        private void RunMessageQueue()
        {
            List<string> messages = Game.MessageQueue.GetMessages();

            Screen.Instance.ClearMessageLine();

            if (messages.Count == 0)
            {
                Screen.Instance.FlushConsole();
                return;
            }

            if (messages.Count == 1)
            {
                //Single message just print it
                Screen.Instance.PrintMessage(messages[0]);

                Game.MessageQueue.ClearList();

                Screen.Instance.FlushConsole();
                return;
            }

            //Otherwise require a space bar press between each
            //TODO: Wrap messages
            for (int i = 0; i < messages.Count; i++)
            {
                if (i != messages.Count - 1)
                {
                    Screen.Instance.PrintMessage(messages[i] + " <more>");
                    Screen.Instance.FlushConsole();

                    //Block for this keypress - may want to listen for exit too
                    KeyPress userKey;
                    userKey = Keyboard.WaitForKeyPress(true);
                }
                else
                {
                    Screen.Instance.PrintMessage(messages[i]);
                }
            }
            
            Game.MessageQueue.ClearList();

        }

        private void SetupGame()
        {
            //Initial setup

            //Setup screen
            Screen.Instance.InitialSetup();

            //Setup logfile
            try
            {
                LogFile.Log.LogEntry("Starting log file");
            }
            catch (Exception e)
            {
                Screen.Instance.ConsoleLine("Error creating log file: " + e.Message);
            }
                

            //Setup message queue
            Game.MessageQueue = new MessageQueue();

            //Create dungeon and set it as current in Game
            dungeon = new Dungeon();
            Game.Dungeon = dungeon;

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

            //Setup PC
            Player player = dungeon.Player;

            player.Representation = '@';
            player.LocationMap = level1.PCStartLocation;

            player.Hitpoints = 100;
            player.MaxHitpoints = 100;

            //Create creatures and start positions
            
            //Add some random creatures

            Random rand = new Random();
            int noCreatures = rand.Next(10) + 5;

            for (int i = 0; i < noCreatures; i++)
            {
                Monster creature = new Creatures.Rat();
                creature.Representation = Convert.ToChar(65 + rand.Next(26));

                int level = 0;
                Point location;
                
                //Loop until we find an acceptable location and the add works
                do
                {
                    location = mapGen.RandomPointInRoom();
                }
                while (!dungeon.AddMonster(creature, level, location));
            }

            //Create features

            //Add some random features

            int noFeatures = rand.Next(5) + 2;

            for (int i = 0; i < noFeatures; i++)
            {
                Features.StaircaseUp feature = new Features.StaircaseUp();

                feature.Representation = Convert.ToChar(58 + rand.Next(6));

                int level = 0;
                Point location;

                //Loop until we find an acceptable location and the add works
                do
                {
                    location = mapGen.RandomPointInRoom();
                }
                while (!dungeon.AddFeature(feature, level, location));
            }

            //Create objects and start positions

            //Add some random objects

            int noItems = rand.Next(10) + 5;

            for (int i = 0; i < noItems; i++)
            {
                Items.Potion item = new Items.Potion();

                item.Representation = Convert.ToChar(33 + rand.Next(12));

                int level = 0;
                Point location;

                //Loop until we find an acceptable location and the add works
                do
                {
                    location = mapGen.RandomPointInRoom();
                }
                while (!dungeon.AddItem(item, level, location));
            }

            
        }

    }
}
