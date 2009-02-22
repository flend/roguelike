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

        enum InputState
        {
            MapMovement, InventoryShow, InventorySelect
        }

        /// <summary>
        /// State determining what functions keys have
        /// </summary>
        InputState inputState = InputState.MapMovement;

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


            //Game time
            //Normal creatures have a speed of 100
            //This means it takes 100 ticks for them to take a turn (10,000 is the cut off)

            //Check PC
            //Take a turn if signalled by the internal clock

            //Loop through creatures
            //If their internal clocks signal another turn then take one

            //Refresh the map with walkable etc. information
            //Now that creatures don't cause non-walkable squares, we only need to do this if the terrain changes
            RecalculateMapAfterMove();

            while (runMapLoop)
            {
                //Increment world clock
                dungeon.IncrementWorldClock();

                //Increment time on all global (dungeon) events
                dungeon.IncrementEventTime();

                //All creatures get IncrementTurnTime() called on them each worldClock tick
                //They internally keep track of when they should take another turn

                //IncrementTurnTime() also increments time for all events on that creature
                
                foreach (Monster creature in dungeon.Monsters)
                {
                    if (creature.IncrementTurnTime())
                    {
                        //dungeon.ShowCreatureFOVOnMap(creature);

                        //Creatures may be killed by other creatures so check they are alive before processing
                        if (creature.Alive)
                        {
                            creature.ProcessTurn();
                            //RecalculateMapAfterMove();
                        }
                    }
                }

                //Remove dead monsters
                dungeon.RemoveDeadMonsters();

                //Increment time on the PC's events and turn time (all done in IncrementTurnTime)
                if (dungeon.Player.IncrementTurnTime())
                {
                    //Calculate the player's FOV
                    RecalculatePlayerFOV();

                    //Debug: show the FOV of all monsters
                    foreach (Monster monster in dungeon.Monsters)
                    {
                        dungeon.ShowCreatureFOVOnMap(monster);
                    }

                    //Update screen just before PC's turn
                    UpdateScreen();

                    //KeyPress userKey = Keyboard.WaitForKeyPress(true);

                    //Screen.Instance.DrawFOVDebug(0);


                    //Deal with PCs turn as appropriate
                    bool timeAdvances = false;
                    do
                    {
                        timeAdvances = UserInput();
                    } while (!timeAdvances);

                    //RecalculateMapAfterMove();

                    //Reset the creature FOV display
                    Game.Dungeon.ResetCreatureFOVOnMap();

                    //these 2 go together to generate a new dungeon on every keypress
                    //SetupDungeon();
                    //RecalculateMapAfterMove();

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
        /// Now only required on a change of terrain features
        /// May need to keep with/without doors version
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

            //Each state has different keys

            switch (inputState)
            {

                //Normal movement on the map
                case InputState.MapMovement:

                    if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                    {
                        char keyCode = (char)userKey.Character;
                        switch (keyCode)
                        {
                            case 'x':
                                //Exit from game
                                runMapLoop = false;
                                timeAdvances = true;
                                break;
                            case '.':
                                // Do nothing
                                timeAdvances = true;
                                break;
                            case 'i':
                                //Interact with feature
                                InteractWithFeature();
                                timeAdvances = true;
                                break;
                            case 'j':
                                //Set up the inventory
                                inputState = InputState.InventoryShow;
                                SetPlayerInventoryScreen();
                                UpdateScreen();
                                timeAdvances = false;
                                break;
                            case 'u':
                                //Use an inventory item
                                SetPlayerInventorySelectScreen();
                                UpdateScreen();
                                //This uses the generic 'select from inventory' input loop
                                //Time advances if the item was used successfully
                                timeAdvances = UseItem();
                                DisablePlayerInventoryScreen();
                                //Only update the screen if the player has another selection to make, otherwise it will be updated automatically before his next go
                                if(!timeAdvances)
                                    UpdateScreen();
                                break;


                            //Debug events

                            case 's':
                                //Add a speed up event on the player
                                PlayerEffects.SpeedUp speedUp = new RogueBasin.PlayerEffects.SpeedUp(Game.Dungeon.Player, 500, 100);
                                Game.Dungeon.Player.AddEffect(speedUp);
                                UpdateScreen();
                                break;
                            case 'h':
                                //Add a healing event on the player
                                PlayerEffects.Healing healing = new RogueBasin.PlayerEffects.Healing(Game.Dungeon.Player, 10);
                                Game.Dungeon.Player.AddEffect(healing);
                                UpdateScreen();
                                break;
                            case 'z':
                                //Add an anti-healing event on the player
                                PlayerEffects.Healing zhealing = new RogueBasin.PlayerEffects.Healing(Game.Dungeon.Player, -10);
                                Game.Dungeon.Player.AddEffect(zhealing);
                                UpdateScreen();
                                break;
                        }
                    }
                    else
                    {
                        //Arrow keys for directions
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
                    break;

                //Inventory is displayed
                case InputState.InventoryShow:

                    if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                    {
                        char keyCode = (char)userKey.Character;

                        //Exit out of inventory
                        if (keyCode == 'x')
                        {
                            inputState = InputState.MapMovement;
                            DisablePlayerInventoryScreen();
                            UpdateScreen();
                            timeAdvances = false;
                        }
                    }

                    break;
                
                //Select an item in the inventory
                //case InputState.InventoryShow:
                //    break;
            }
            
            return timeAdvances;
        }

        private static void DisablePlayerInventoryScreen()
        {
            Screen.Instance.DisplayInventory = false;
            Screen.Instance.CurrentInventory = null;
        }

        private static void SetPlayerInventoryScreen()
        {
            Screen.Instance.DisplayInventory = true;
            Screen.Instance.CurrentInventory = Game.Dungeon.Player.Inventory;
            Screen.Instance.InventoryTitle = "Inventory";
            Screen.Instance.InventoryInstructions = "Press (x) to exit";
        }

        private void SetPlayerInventorySelectScreen()
        {
            Screen.Instance.DisplayInventory = true;
            Screen.Instance.CurrentInventory = Game.Dungeon.Player.Inventory;
            Screen.Instance.InventoryTitle = "Inventory";
            Screen.Instance.InventoryInstructions = "Press the letter of an item to select or (x) to exit";
        }

        private void InteractWithFeature()
        {
            Game.Dungeon.PCInteractWithFeature();
        }

        /// <summary>
        /// Player uses item. Returns true if item was used and time should advance
        /// </summary>
        private bool UseItem()
        {
            //User selects which item to use
            int chosenIndex = PlayerChooseFromInventory();

            //Player exited
            if (chosenIndex == -1)
                return false;

            Inventory playerInventory = Game.Dungeon.Player.Inventory;
            
            InventoryListing selectedGroup = playerInventory.InventoryListing[chosenIndex];
            bool usedSuccessfully = Game.Dungeon.Player.UseItem(selectedGroup);

            return usedSuccessfully;
        }

        /// <summary>
        /// Returns the index of the inventory group selected or -1 to exit
        /// </summary>
        /// <returns></returns>
        private int PlayerChooseFromInventory()
        {
            //Player presses a key from a-w to select an inventory listing or x to exit

            //Check how many items are available
            Inventory playerInventory = Game.Dungeon.Player.Inventory;
            int numInventoryListing = playerInventory.InventoryListing.Count;

            do {

                KeyPress userKey = Keyboard.WaitForKeyPress(true);
            
                if (userKey.KeyCode == KeyCode.TCODK_CHAR) {
                    
                    char keyCode = (char)userKey.Character;

                    if (keyCode == 'x')
                    {
                        return -1;
                    }
                    else
                    {
                        int charIndex = (int)keyCode - (int)'a';
                        if (charIndex < numInventoryListing && charIndex < 24)
                            return charIndex;

                    }
                }
            } while (true);
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
                    //KeyPress userKey;
                    //userKey = Keyboard.WaitForKeyPress(true);
                }
                else
                {
                    Screen.Instance.PrintMessage(messages[i]);
                    Screen.Instance.FlushConsole();
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

            //See all debug messages
            LogFile.Log.DebugLevel = 2;

            //Setup message queue
            Game.MessageQueue = new MessageQueue();

            SetupDungeon();
        }

        private void SetupDungeon()
        {
            //Create dungeon and set it as current in Game
            dungeon = new Dungeon();
            Game.Dungeon = dungeon;

            //Randomer
            Random rand = new Random();

            //Create dungeon map (at least level 1)
            MapGeneratorBSP mapGen1 = new MapGeneratorBSP();
            //MapGeneratorRogue mapGen = new MapGeneratorRogue();
            mapGen1.Width = 80;
            mapGen1.Height = 25;
            int extraCorridors = rand.Next(10);

            Map level1 = mapGen1.GenerateMap(extraCorridors);

            MapGeneratorBSP mapGen2 = new MapGeneratorBSP();
            mapGen2.Width = 80;
            mapGen2.Height = 25;
            Map level2 = mapGen2.GenerateMap(extraCorridors);

            //Test
            //for (int i = 0; i < 10000; i++)
            //{
            //    mapGen.GenerateMap();
            //}

            //Give map to dungeon
            dungeon.AddMap(level1); //level 1
            dungeon.AddMap(level2); //level 2

            //Setup PC
            Player player = dungeon.Player;

            player.Representation = '@';
            player.LocationMap = level1.PCStartLocation;

            player.Hitpoints = 100;
            player.MaxHitpoints = 100;

            //Give the player some items
            player.PickUpItem(new Items.Potion());

            //Add a down staircase where the player is standing
            AddFeatureToDungeon(new Features.StaircaseDown(), 0, new Point(player.LocationMap.x, player.LocationMap.y));

            //Create creatures and start positions

            //Add some random creatures


            //May infinite loop too early - not sure
            int noCreatures = rand.Next(10) + 215;

            for (int i = 0; i < noCreatures; i++)
            {
                Monster creature = new Creatures.Rat();
                creature.Representation = Convert.ToChar(65 + rand.Next(26));

                int level = rand.Next(2);
                Point location = new Point(0, 0);

                //Loop until we find an acceptable location and the add works
                do
                {
                    if (level == 0)
                        location = mapGen1.RandomPointInRoom();
                    else if (level == 1)
                        location = mapGen2.RandomPointInRoom();
                    LogFile.Log.LogEntryDebug("Creature " + i.ToString() + " pos x: " + location.x + " y: " + location.y, LogDebugLevel.Low);
                }
                while (!dungeon.AddMonster(creature, level, location));
            }

            //Create features

            //Add some random features
            /*
            int noFeatures = rand.Next(5) + 2;

            for (int i = 0; i < noFeatures; i++)
            {
                Features.StaircaseUp feature = new Features.StaircaseUp();

                feature.Representation = Convert.ToChar(58 + rand.Next(6));
                AddFeatureToDungeon(feature, mapGen1, 0);
            }*/

            //Add staircases to dungeons level 1 and 2
            AddFeatureToDungeonRandomPoint(new Features.StaircaseDown(), mapGen1, 0);
            AddFeatureToDungeonRandomPoint(new Features.StaircaseUp(), mapGen2, 1);
            

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
                    location = mapGen1.RandomPointInRoom();
                }
                while (!dungeon.AddItem(item, level, location));
            }
        }

        /// <summary>
        /// Add a feature to the dungeon. Guarantees an acceptable placement.
        /// Might loop forever if all squares in the dungeon are already taken up
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        void AddFeatureToDungeonRandomPoint(Feature feature, MapGeneratorBSP mapGen, int level)
        {
            Point location;
            //Loop until we find an acceptable location and the add works
            do
            {
                location = mapGen.RandomPointInRoom();

            }
            while (!dungeon.AddFeature(feature, level, location));
        }

        /// <summary>
        /// Add a feature to the dungeon at a certain place. May fail if something is already there.
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        bool AddFeatureToDungeon(Feature feature, int level, Point location)
        {
            return dungeon.AddFeature(feature, level, location);
        }
    }
}
