using System;

using libtcodWrapper;
using Console = System.Console;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.IO.Compression;

namespace RogueBasin
{
    public class RogueBase : IDisposable
    {
        DungeonMaker dungeonMaker = null;
        

        //Are we running or have we exited?
        //public bool runMapLoop = true;

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

            while (Game.Dungeon.RunMainLoop)
            {
                try
                {
                    //Increment world clock
                    Game.Dungeon.IncrementWorldClock();

                    //Increment time on all global (dungeon) events
                    Game.Dungeon.IncrementEventTime();

                    //All creatures get IncrementTurnTime() called on them each worldClock tick
                    //They internally keep track of when they should take another turn

                    //IncrementTurnTime() also increments time for all events on that creature

                    foreach (Monster creature in Game.Dungeon.Monsters)
                    {
                        try
                        {
                            if (creature.IncrementTurnTime())
                            {
                                //dungeon.ShowCreatureFOVOnMap(creature);

                                //Creatures may be killed by other creatures so check they are alive before processing
                                if (creature.Alive)
                                {
                                    //Only process creatures on the same level as the player
                                    if (creature.LocationLevel == Game.Dungeon.Player.LocationLevel)
                                    {
                                        creature.ProcessTurn();
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            LogFile.Log.LogEntry("Exception thrown" + e.Message);
                        }
                    }

                    try
                    {
                        //Add summoned monsters
                        Game.Dungeon.AddDynamicMonsters();
                    }
                    catch (Exception e)
                    {
                        LogFile.Log.LogEntry("Exception thrown" + e.Message);
                    }

                    //Remove dead monsters
                    //Isn't there a chance that monsters might attack dead monsters before they are removed? (CHECK?)
                    try
                    {
                        Game.Dungeon.RemoveDeadMonsters();
                    }
                    catch (Exception e)
                    {
                        LogFile.Log.LogEntry("Exception thrown" + e.Message);
                    }
                    try
                    {

                        //Increment time on the PC's events and turn time (all done in IncrementTurnTime)
                        if (Game.Dungeon.Player.IncrementTurnTime())
                        {
                            //Calculate the player's FOV
                            RecalculatePlayerFOV();

                            //Debug: show the FOV of all monsters
                            foreach (Monster monster in Game.Dungeon.Monsters)
                            {
                                Game.Dungeon.ShowCreatureFOVOnMap(monster);
                            }

                            //For effects that end to update the screen correctly
                            if (Game.Dungeon.Player.RecalculateCombatStatsRequired)
                                Game.Dungeon.Player.CalculateCombatStats();

                            //Update screen just before PC's turn
                            UpdateScreen();

                            //Deal with PCs turn as appropriate
                            bool timeAdvances = false;
                            do
                            {
                                timeAdvances = UserInput();
                            } while (!timeAdvances);

                            //Reset the creature FOV display
                            Game.Dungeon.ResetCreatureFOVOnMap();

                            //Game.MessageQueue.AddMessage("Finished PC move");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogFile.Log.LogEntry("Exception thrown" + ex.Message);
                    }
                }
                catch (Exception ex)
                {

                    LogFile.Log.LogEntry("Exception thrown" + ex.Message);

                }
            }
        }

        private void RecalculatePlayerFOV()
        {
            Game.Dungeon.CalculatePlayerFOV();
        }

        //Deal with user input
        //Return code is if the command was successful and time increments (i.e. the player has done a time-using command like moving)
        private bool UserInput()
        {
            bool timeAdvances = false;
            try
            {
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
                                case 'Q':
                                    //Exit from game
                                    bool response = Screen.Instance.YesNoQuestion("Really quit?");
                                    if (response == true)
                                    {
                                        Game.Dungeon.PlayerDeath("quit");
                                        timeAdvances = true;
                                    }
                                    UpdateScreen();
                                    break;
                                case 'S':
                                    //Save the game
                                    timeAdvances = true;
                                    Game.MessageQueue.AddMessage("Saving game...");
                                    UpdateScreen();
                                    Game.Dungeon.SaveGame();
                                    Game.MessageQueue.AddMessage("Press any key to exit the game.");
                                    UpdateScreen();
                                    userKey = Keyboard.WaitForKeyPress(true);
                                    Game.Dungeon.RunMainLoop = false;
                                    
                                    break;
                                case 'F':
                                    //Full screen switch
                                    timeAdvances = false;
                                    RootConsole rootConsole = RootConsole.GetInstance();
                                    rootConsole.SetFullscreen(!rootConsole.IsFullscreen());
                                    rootConsole.Flush();
                                    break;


                                case 'o':
                                    //Open door
                                    timeAdvances = PlayerOpenDoor();
                                    if (!timeAdvances)
                                        UpdateScreen();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;

                                case 'Z':
                                    //Cast spell (just target for now)
                                    timeAdvances = SelectAndCastSpell();
                                    if (!timeAdvances)
                                        UpdateScreen();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;

                                case 'X':
                                    //Recast last spells
                                    timeAdvances = RecastSpell();
                                    if (!timeAdvances)
                                        UpdateScreen();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;

                                case 'C':
                                    //Charm creature
                                    timeAdvances = PlayerCharmCreature();
                                    if (!timeAdvances)
                                        UpdateScreen();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;

                                case 'U':
                                    //Uncharm creature
                                    timeAdvances = PlayerUnCharmCreature();
                                    if (!timeAdvances)
                                        UpdateScreen();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;

                                case 'r':
                                    //Name object
                                    SetPlayerInventorySelectScreen();
                                    UpdateScreen();
                                    //This uses the generic 'select from inventory' input loop
                                    NameObject();
                                    DisablePlayerInventoryScreen();

                                    UpdateScreen();
                                    break;

                                case ',':
                                    //Pick up item
                                    timeAdvances = PickUpItem();
                                    //Only update screen is unsuccessful, otherwise will be updated in main loop (can this be made general)
                                    if (!timeAdvances)
                                        UpdateScreen();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;

                                case '.':
                                    // Do nothing
                                    timeAdvances = DoNothing();
                                    break;
                                
                                case '>':
                                case '<':
                                    //Interact with feature
                                    timeAdvances = InteractWithFeature();
                                    if (!timeAdvances)
                                        UpdateScreen();

                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();

                                    break;

                                case 'i':
                                    //Display the inventory
                                    inputState = InputState.InventoryShow;
                                    SetPlayerInventoryScreen();
                                    UpdateScreen();
                                    timeAdvances = false;
                                    break;
                                case 'd':
                                    //Drop items if in town
                                    DropItems();
                                    UpdateScreen();
                                    timeAdvances = false;
                                    break;

                                case 'k':
                                    //Use an inventory item
                                    SetPlayerInventorySelectScreen();
                                    UpdateScreen();
                                    //This uses the generic 'select from inventory' input loop
                                    //Time advances if the item was used successfully
                                    timeAdvances = UseItem();
                                    DisablePlayerInventoryScreen();
                                    //Only update the screen if the player has another selection to make, otherwise it will be updated automatically before his next go
                                    if (!timeAdvances)
                                        UpdateScreen();

                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;
                                
                                case 'e':
                                    //Display currently equipped items
                                    SetPlayerEquippedItemsScreen();
                                    UpdateScreen();
                                    timeAdvances = DisplayEquipment();
                                    DisablePlayerEquippedItemsScreen();

                                    //Using an item can break a special move sequence
                                    if (!timeAdvances)
                                        UpdateScreen();

                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;

                                    //Vi keys
                                    /*
                                case 'b':
                                    timeAdvances = Game.Dungeon.PCMove(-1, 1);
                                    break;

                                case 'n':
                                    timeAdvances = Game.Dungeon.PCMove(1, 1);
                                    break;

                                case 'y':
                                    timeAdvances = Game.Dungeon.PCMove(-1, -1);
                                    break;

                                case 'u':
                                    timeAdvances = Game.Dungeon.PCMove(1, -1);
                                    break;

                                case 'h':
                                    timeAdvances = Game.Dungeon.PCMove(-1, 0);
                                    break;
                                case 'l':
                                    timeAdvances = Game.Dungeon.PCMove(1, 0);
                                    break;
                                case 'k':
                                    timeAdvances = Game.Dungeon.PCMove(0, -1);
                                    break;
                                case 'j':
                                    timeAdvances = Game.Dungeon.PCMove(0, 1);
                                    break;
                                    */
                                    //Debug events

                                case 'K':
                                    Game.Dungeon.FlipTerrain("river");
                                    Game.Dungeon.FlipTerrain("forest");
                                    Game.Dungeon.FlipTerrain("grave");
                                    Game.Dungeon.FlipTerrain("final");
                                    UpdateScreen();
                                    break;

                                case 'c':
                                    //Level up
                                    Game.Dungeon.Player.LevelUp();
                                    UpdateScreen();
                                    break;

                                case 'm':
                                    //Learn all moves
                                    Game.Dungeon.PlayerLearnsAllMoves();
                                    Game.MessageQueue.AddMessage("Learnt all moves.");
                                    Game.Dungeon.PlayerLearnsAllSpells();
                                    Game.MessageQueue.AddMessage("Learnt all spells.");
                                    UpdateScreen();
                                    timeAdvances = false;
                                    break;

                                case 's':
                                    //Show movies
                                    SetSpecialMoveMovieScreen();
                                    UpdateScreen();
                                    MovieScreenInteraction();
                                    DisableSpecialMoveMovieScreen();
                                    UpdateScreen();
                                    timeAdvances = false;
                                    break;

                                case 'T':
                                    Game.Dungeon.MoveToNextDate();
                                    timeAdvances = true;
                                    break;


                                case 't':
                                    //teleport to stairs
                                    TeleportToDownStairs();
                                    UpdateScreen();
                                    break;

                                case 'y':
                                    //teleport to stairs
                                    TeleportToUpStairs();
                                    UpdateScreen();
                                    break;

                                case 'Y':
                                    //Take me to first dungeon
                                    Game.Dungeon.Player.LocationLevel = 26;
                                    TeleportToDownStairs();
                                    UpdateScreen();
                                    break;

                                case 'z':
                                    //Add a healing event on the player
                                    PlayerEffects.Healing healing = new RogueBasin.PlayerEffects.Healing(Game.Dungeon.Player, 10);
                                    Game.Dungeon.Player.AddEffect(healing);
                                    UpdateScreen();
                                    break;

                                    /*
                                //Debug events
                                case 'w':
                                    //Select an item to equip
                                    SetPlayerEquippedItemsSelectScreen();
                                    UpdateScreen();
                                    timeAdvances = EquipItem();
                                    DisablePlayerEquippedItemsSelectScreen();
                                    if (!timeAdvances)
                                        UpdateScreen();

                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;
                                //debug ones
                                case 'd':
                                    //Drop item
                                    SetPlayerInventorySelectScreen();
                                    UpdateScreen();
                                    timeAdvances = DropItem();
                                    DisablePlayerInventoryScreen();
                                    if (!timeAdvances)
                                        UpdateScreen();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;

                                
                                case 'l':
                                    timeAdvances = false;
                                    LoadGame(Game.Dungeon.Player.Name);
                                    UpdateScreen();
                                    break;


                                case 'y':
                                    //Add a speed up event on the player
                                    PlayerEffects.SpeedUp speedUp = new RogueBasin.PlayerEffects.SpeedUp(Game.Dungeon.Player, 500, 100);
                                    Game.Dungeon.Player.AddEffect(speedUp);
                                    UpdateScreen();
                                    break;
                                case 'v':
                                    //Add a multi damage event on the player
                                    PlayerEffects.MultiDamage multiD = new RogueBasin.PlayerEffects.MultiDamage(Game.Dungeon.Player, 500, 3);
                                    Game.Dungeon.Player.AddEffect(multiD);
                                    UpdateScreen();
                                    break;
                                case 'h':
                                    //Add a healing event on the player
                                    PlayerEffects.Healing healing = new RogueBasin.PlayerEffects.Healing(Game.Dungeon.Player, 10);
                                    Game.Dungeon.Player.AddEffect(healing);
                                    UpdateScreen();
                                    break;
                                case 'x':
                                    //Add a healing event on the player
                                    PlayerEffects.DamageUp healing3 = new RogueBasin.PlayerEffects.DamageUp(Game.Dungeon.Player, 500, 5);
                                    Game.Dungeon.Player.AddEffect(healing3);
                                    PlayerEffects.ToHitUp healing2 = new RogueBasin.PlayerEffects.ToHitUp(Game.Dungeon.Player, 500, 5);
                                    Game.Dungeon.Player.AddEffect(healing2);
                                    UpdateScreen();
                                    break;
                                case 'z':
                                    //Add an anti-healing event on the player
                                    PlayerEffects.Healing zhealing = new RogueBasin.PlayerEffects.Healing(Game.Dungeon.Player, -10);
                                    Game.Dungeon.Player.AddEffect(zhealing);
                                    UpdateScreen();
                                    break;
                                case 'c':
                                    //Level up
                                    Game.Dungeon.Player.LevelUp();
                                    UpdateScreen();
                                    break;
                                    */
                            }
                        }
                        else
                        {
                            //Arrow keys for directions
                            switch (userKey.KeyCode)
                            {
                                case KeyCode.TCODK_KP1:
                                    timeAdvances = Game.Dungeon.PCMove(-1, 1);
                                    break;

                                case KeyCode.TCODK_KP3:
                                    timeAdvances = Game.Dungeon.PCMove(1, 1);
                                    break;

                                case KeyCode.TCODK_KPDEC:
                                case KeyCode.TCODK_KP5:
                                    //Does nothing
                                    timeAdvances = DoNothing();
                                    break;

                                case KeyCode.TCODK_KP7:
                                    timeAdvances = Game.Dungeon.PCMove(-1, -1);
                                    break;
                                case KeyCode.TCODK_KP9:
                                    timeAdvances = Game.Dungeon.PCMove(1, -1);
                                    break;

                                case KeyCode.TCODK_LEFT:
                                case KeyCode.TCODK_KP4:
                                    timeAdvances = Game.Dungeon.PCMove(-1, 0);
                                    break;
                                case KeyCode.TCODK_RIGHT:
                                case KeyCode.TCODK_KP6:
                                    timeAdvances = Game.Dungeon.PCMove(1, 0);
                                    break;
                                case KeyCode.TCODK_UP:
                                case KeyCode.TCODK_KP8:
                                    timeAdvances = Game.Dungeon.PCMove(0, -1);
                                    break;
                                case KeyCode.TCODK_KP2:
                                case KeyCode.TCODK_DOWN:
                                    timeAdvances = Game.Dungeon.PCMove(0, 1);
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
            }
            catch (Exception ex)
            {
                //This should catch most exceptions that happen as a result of user commands
                MessageBox.Show("Exception occurred: " + ex.Message + " but continuing on anyway");
            }
            return timeAdvances;
        }

        /// <summary>
        /// Drop the player's items if in town. Otherwise doesn't do anything
        /// </summary>
        private void DropItems()
        {
            if (Game.Dungeon.Player.LocationLevel == 0)
            {
                Game.Dungeon.PutItemsInStore();
                Game.MessageQueue.AddMessage("You drop the items off in the store.");
                LogFile.Log.LogEntry("Items returned to store.");
            }
            else
            {
                Game.MessageQueue.AddMessage("You don't want to drop your precious items in this place!");
                LogFile.Log.LogEntry("Items drop requested away from town.");
            }
        }



        private bool DoNothing()
        {
            return Game.Dungeon.PCMove(0, 0);
        }

        private void NameObject()
        {
            //User selects which item to name
            int chosenIndex = PlayerChooseFromInventory();

            //Player exited
            if (chosenIndex == -1)
                return;

            Inventory playerInventory = Game.Dungeon.Player.Inventory;

            InventoryListing selectedGroup = playerInventory.InventoryListing[chosenIndex];
            int itemIndex = selectedGroup.ItemIndex[0];
            Item itemToName = playerInventory.Items[itemIndex];

            //Get user string
            string newName = Screen.Instance.GetUserString("New name for " + Game.Dungeon.GetHiddenName(itemToName));
            if (newName != null)
            {
                Game.Dungeon.AssociateNameWithItem(itemToName, newName);
            }
        }

        private void TeleportToDownStairs()
        {
            //Find down stairs on this level
            List<Feature> features = Game.Dungeon.Features;

            Player player = Game.Dungeon.Player;

            Features.StaircaseDown downStairs = null;
            Point stairlocation = new Point(0,0);

            foreach (Feature feature in features)
            {

                if (feature.LocationLevel == Game.Dungeon.Player.LocationLevel &&
                    feature is Features.StaircaseDown)
                {
                    downStairs = feature as Features.StaircaseDown;
                    stairlocation = feature.LocationMap;
                    break;
                }
            }

            if (downStairs == null)
            {
                LogFile.Log.LogEntryDebug("Unable to teleport to stairs", LogDebugLevel.High);
                return;
            }
            
            //Kill any monster there
            Monster m = Game.Dungeon.MonsterAtSpace(player.LocationLevel, player.LocationMap);

            if (m != null)
            {
                Game.Dungeon.KillMonster(m, false);
            }

            //Move the player
            player.LocationMap = stairlocation;

            //featureAtSpace.PlayerInteraction(player);
        }

        private void TeleportToUpStairs()
        {
            //Find down stairs on this level
            List<Feature> features = Game.Dungeon.Features;

            Player player = Game.Dungeon.Player;

            Features.StaircaseUp downStairs = null;
            Features.StaircaseExit exitStairs = null;
            Point stairlocation = new Point(0, 0);

            foreach (Feature feature in features)
            {

                if (feature.LocationLevel == Game.Dungeon.Player.LocationLevel &&
                    feature is Features.StaircaseUp)
                {
                    downStairs = feature as Features.StaircaseUp;
                    stairlocation = feature.LocationMap;
                    break;
                }
                if (feature.LocationLevel == Game.Dungeon.Player.LocationLevel &&
                    feature is Features.StaircaseExit)
                {
                    exitStairs = feature as Features.StaircaseExit;
                    stairlocation = feature.LocationMap;
                    break;
                }
            }

            if (downStairs == null && exitStairs == null)
            {
                LogFile.Log.LogEntryDebug("Unable to teleport to stairs", LogDebugLevel.High);
                return;
            }

            //Kill any monster there
            Monster m = Game.Dungeon.MonsterAtSpace(player.LocationLevel, player.LocationMap);

            if (m != null)
            {
                Game.Dungeon.KillMonster(m, false);
            }

            //Move the player
            player.LocationMap = stairlocation;

            //featureAtSpace.PlayerInteraction(player);
        }

        private void LoadGame(string playerName)
        {
            //Save game filename
            string filename = playerName + ".sav";

            //Deserialize the save game
            XmlSerializer serializer = new XmlSerializer(typeof(SaveGameInfo));

            Stream stream = null;
            GZipStream compStream = null;

            try
            {
                stream = File.OpenRead(filename);

                compStream = new GZipStream(stream, CompressionMode.Decompress, true);
                SaveGameInfo readData = (SaveGameInfo)serializer.Deserialize(compStream);

                //Build a new dungeon object from the stored data
                Dungeon newDungeon = new Dungeon();

                newDungeon.Features = readData.features;
                newDungeon.Items = readData.items;
                newDungeon.Effects = readData.effects;
                newDungeon.Monsters = readData.monsters;
                newDungeon.Player = readData.player;
                newDungeon.SpecialMoves = readData.specialMoves;
                newDungeon.WorldClock = readData.worldClock;
                newDungeon.HiddenNameInfo = readData.hiddenNameInfo;
                newDungeon.Triggers = readData.triggers;
                newDungeon.Difficulty = readData.difficulty;

                //Process the maps back into map objects
                foreach (SerializableMap serialMap in readData.levels)
                {
                    //Add a map. Note that this builds a TCOD map too
                    newDungeon.AddMap(serialMap.MapFromSerializableMap());
                }

                //Build TCOD maps
                newDungeon.RefreshTCODMaps();

                //Worry about inventories generally
                //Problem right now is that items in creature inventories will get made twice, once in dungeon and once on the player/creature
                //Fix is to remove them from dungeon when in a creature's inventory and vice versa

                //Rebuild InventoryListing for the all creatures
                //Recalculate combat stats
                foreach (Monster monster in newDungeon.Monsters)
                {
                    monster.Inventory.RefreshInventoryListing();
                    monster.CalculateCombatStats();
                }

                newDungeon.Player.Inventory.RefreshInventoryListing();

                //Set this new dungeon and player as the current global
                Game.Dungeon = newDungeon;

                newDungeon.Player.CalculateCombatStats();

 

                Game.MessageQueue.AddMessage("Game : " + playerName + " loaded successfully.");
                LogFile.Log.LogEntry("Game : " + playerName + " loaded successfully");
            }
            catch (Exception ex)
            {
                Game.MessageQueue.AddMessage("Game : " + playerName + " failed to load.");
                LogFile.Log.LogEntry("Game : " + playerName + " failed to load: " + ex.Message);
            }
            finally
            {
                if (compStream != null)
                {
                    compStream.Close();
                }
                if (stream != null)
                {
                    stream.Close();
                }
            }

        }

        /// <summary>
        /// Call when time moves on due to a PC action that isn't a move. This may cause some special moves to cancel.
        /// </summary>
        private void SpecialMoveNonMoveAction()
        {
            Game.Dungeon.PCActionNoMove();
        }

        /// <summary>
        /// Get a keypress and interpret it as a direction
        /// </summary>
        /// <returns></returns>
        private bool GetDirectionKeypress(out Point direction)
        {
            //Get direction
            KeyPress userKey = Keyboard.WaitForKeyPress(true);

            if (GetDirectionFromKeypress(userKey, out direction))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get a direction from a keypress. Will return false if not valid. Otherwise in parameter.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private bool GetDirectionFromKeypress(KeyPress userKey, out Point direction) {

            direction = new Point(9, 9);

            //Arrow keys for directions
            switch (userKey.KeyCode)
            {
                case KeyCode.TCODK_KP1:
                    direction = new Point(-1, 1);
                    break;

                case KeyCode.TCODK_KP3:
                    direction = new Point(1, 1);
                    break;

                case KeyCode.TCODK_KPDEC:
                case KeyCode.TCODK_KP5:
                    //Does nothing
                    direction = new Point(0, 0);
                    break;

                case KeyCode.TCODK_KP7:
                    direction = new Point(-1, -1);
                    break;
                case KeyCode.TCODK_KP9:
                    direction = new Point(1, -1);
                    break;

                case KeyCode.TCODK_LEFT:
                case KeyCode.TCODK_KP4:
                    direction = new Point(-1, 0);
                    break;
                case KeyCode.TCODK_RIGHT:
                case KeyCode.TCODK_KP6:
                    direction = new Point(1, 0);
                    break;
                case KeyCode.TCODK_UP:
                case KeyCode.TCODK_KP8:
                    direction = new Point(0, -1);
                    break;
                case KeyCode.TCODK_KP2:
                case KeyCode.TCODK_DOWN:
                    direction = new Point(0, 1);
                    break;
            }

            //Not valid
            if (direction == new Point(9, 9))
                return false;

            return true;
        }

        private bool PlayerOpenDoor()
        {
            //Ask user for a direction
            Game.MessageQueue.AddMessage("Select a direction:");
            UpdateScreen();

            //Get direction
            Point direction = new Point(0, 0);
            bool gotDirection = GetDirectionKeypress(out direction);
            
            if (!gotDirection)
            {
                Game.MessageQueue.AddMessage("No direction");
                return false;
            }

            //Check there is a door here

            Player player = Game.Dungeon.Player;
            Point doorLocation = new Point(direction.x + player.LocationMap.x, direction.y + player.LocationMap.y);
            bool success = Game.Dungeon.OpenDoor(player.LocationLevel, doorLocation);

            if (!success)
            {
                Game.MessageQueue.AddMessage("Not a closed door!");
                return false;
            }
            return true;
        }

        private bool PlayerUnCharmCreature()
        {
            //Ask user for a direction
            Game.MessageQueue.AddMessage("Select a direction:");
            UpdateScreen();

            //Get direction
            Point direction = new Point(0, 0);
            bool gotDirection = GetDirectionKeypress(out direction);

            if (!gotDirection)
            {
                Game.MessageQueue.AddMessage("No direction");
                return false;
            }

            //Attempt to uncharm a monster in that square
            bool timePasses = Game.Dungeon.UnCharmMonsterByPlayer(direction);

            return timePasses;
        }


        private bool PlayerCharmCreature()
        {
            //Ask user for a direction
            Game.MessageQueue.AddMessage("Select a direction:");
            UpdateScreen();

            //Get direction
            Point direction = new Point(0, 0);
            bool gotDirection = GetDirectionKeypress(out direction);

            if (!gotDirection)
            {
                Game.MessageQueue.AddMessage("No direction");
                return false;
            }

            //Attempt to charm a monster in that square
            bool timePasses = Game.Dungeon.AttemptCharmMonsterByPlayer(direction);

            return timePasses;
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

        private void SetPlayerEquippedItemsSelectScreen()
        {
            Screen.Instance.DisplayEquipmentSelect = true;
            Screen.Instance.CurrentInventory = Game.Dungeon.Player.Inventory;
            Screen.Instance.CurrentEquipment = Game.Dungeon.Player.EquipmentSlots;
            Screen.Instance.InventoryTitle = "Equip Item";
            Screen.Instance.InventoryInstructions = "Press the letter of an item to equip or (x) to exit";
        }

        /// <summary>
        /// Set state to display equipped items
        /// </summary>
        private void SetPlayerEquippedItemsScreen()
        {
            Screen.Instance.DisplayEquipment = true;
            Screen.Instance.CurrentEquipment = Game.Dungeon.Player.EquipmentSlots;
            Screen.Instance.CurrentInventory = Game.Dungeon.Player.Inventory;
            Screen.Instance.InventoryTitle = "Equipped Items";
            Screen.Instance.InventoryInstructions = "Press the letter of an item to use (if useable) or (x) to exit";
        }

        /// <summary>
        /// Set state as movie screen
        /// </summary>
        private void SetSpecialMoveMovieScreen()
        {
            Screen.Instance.DisplaySpecialMoveMovies = true;
        }

        /// <summary>
        /// Disable movie overlay
        /// </summary>
        private void DisableSpecialMoveMovieScreen()
        {
            Screen.Instance.DisplaySpecialMoveMovies = false;
        }

        /// <summary>
        /// Disable equipped items overlay
        /// </summary>
        private void DisablePlayerEquippedItemsScreen()
        {
            Screen.Instance.DisplayEquipment = false;
            Screen.Instance.CurrentEquipment = null;
        }

        /// <summary>
        /// Disable equipment select overlay. Really want to make these setup functions methods in Screen
        /// </summary>
        private void DisablePlayerEquippedItemsSelectScreen()
        {
            Screen.Instance.DisplayEquipmentSelect = false;
            Screen.Instance.CurrentEquipment = null;
            Screen.Instance.CurrentInventory = null;
        }

        private bool InteractWithFeature()
        {
            //Preferably just ask the dungeon if there is a feature here, rather than having all the logic in dungeon
            Dungeon dungeon = Game.Dungeon;
            Player player = dungeon.Player;

            Feature featureAtSpace = dungeon.FeatureAtSpace(player.LocationLevel, player.LocationMap);

            if (featureAtSpace == null)
            {
                Game.MessageQueue.AddMessage("Nothing to interact with here");
                return false;
            }

            //Interact with feature - these will normally put success / failure messages in queue
            return featureAtSpace.PlayerInteraction(player);
        }

        Spell lastSpell = null;
        Creature lastSpellTarget = null;

        /// <summary>
        /// Cast a spell. Returns if time passes.
        /// </summary>
        /// <returns></returns>
        private bool SelectAndCastSpell()
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;
            
            //No casting in town or wilderness
            if (player.LocationLevel < 2)
            {
                Game.MessageQueue.AddMessage("You want to save your spells for the dungeons.");
                LogFile.Log.LogEntryDebug("Attempted to cast spell outside of dungeon", LogDebugLevel.Low);

                return false;
            }

            //Get the user's selection
            Spell toCast = SelectSpell();

            //User exited
            if (toCast == null)
                return false;
         
            //Get a target if needed

            Point target = new Point();
            bool targettingSuccess = true;

            if (toCast.NeedsTarget())
            {
                targettingSuccess = TargetSpell(out target);
            }

            //User exited
            if (!targettingSuccess)
                return false;

            bool success = Game.Dungeon.Player.CastSpell(toCast, target);

            //Store details for a recast
           
            //If we successfully cast, store the target
            if (success)
            {
                //Only do this for certain spells
                if (toCast.GetType() != typeof(Spells.MagicMissile) && toCast.GetType() != typeof(Spells.FireLance) && toCast.GetType() != typeof(Spells.FireBall) && toCast.GetType() != typeof(Spells.EnergyBlast))
                    return success;

                lastSpell = toCast;

                //Spell target is the creature (monster or PC)

                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, target);

                //Is there a creature here? If so, store
                if (squareContents.monster != null)
                    lastSpellTarget = squareContents.monster;

                if (squareContents.player != null)
                    lastSpellTarget = squareContents.player;
            }

            //Time only goes past if successfully cast
            return success;
        }

        /// <summary>
        /// Recast the last spell at the same target
        /// </summary>
        /// <returns></returns>
        private bool RecastSpell()
        {
            //No casting in town or wilderness
            if (Game.Dungeon.Player.LocationLevel < 2)
            {
                Game.MessageQueue.AddMessage("You want to save your spells for the dungeons.");
                LogFile.Log.LogEntryDebug("Attempted to cast spell outside of dungeon", LogDebugLevel.Low);

                return false;
            }

            //Do we have a valid spell?
            if (lastSpell == null)
            {
                Game.MessageQueue.AddMessage("Choose a spell first.");
                LogFile.Log.LogEntry("Tried to recast spell with no spell selected");
                return false;
            }

            //Do we need a target?
            if (lastSpell.NeedsTarget())
            {
                if (lastSpellTarget == null)
                {
                    Game.MessageQueue.AddMessage("Choose a spell first.");
                    LogFile.Log.LogEntryDebug("Tried to recast spell with no valid spell target selected", LogDebugLevel.High);
                    return false;
                }
            }
            
            //Try to cast the spell

            //Get the FOV from Dungeon (this also updates the map creature FOV state)
            TCODFov currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

            //Try the last target
            if (lastSpellTarget.Alive && Game.Dungeon.Player.LocationLevel == lastSpellTarget.LocationLevel)
            {
                //Are they still in sight?
                //Is the target in FOV
                if (currentFOV.CheckTileFOV(lastSpellTarget.LocationMap.x, lastSpellTarget.LocationMap.y))
                {
                    //If so, attack
                    LogFile.Log.LogEntryDebug("Recast at last target", LogDebugLevel.Medium);
                    return RecastSpellCastAtCreature(lastSpell, lastSpellTarget);
                }
                
                //If not, new target, fall through
            }

            //Find the next closest creature (need to check charm / passive status)

            lastSpellTarget = Game.Dungeon.FindClosestHostileCreature(Game.Dungeon.Player);

            if (lastSpellTarget == null)
            {
                Game.MessageQueue.AddMessage("No target in sight.");
                LogFile.Log.LogEntryDebug("No new target for quick cast", LogDebugLevel.Medium);
                return false;
            }

            //Check they are in FOV
            if (!currentFOV.CheckTileFOV(lastSpellTarget.LocationMap.x, lastSpellTarget.LocationMap.y))
            {
                LogFile.Log.LogEntryDebug("No targets in FOV", LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("No target in sight.");

                return false;
            }

            //Otherwise target the nearest creature

            Game.MessageQueue.AddMessage("Targetting closest creature.");
            LogFile.Log.LogEntryDebug("New target for quick cast", LogDebugLevel.Medium);            

            return RecastSpellCastAtCreature(lastSpell, lastSpellTarget);
        }

        private bool RecastSpellCastAtCreature(Spell spell, Creature target)
        {
            //Convert the stored Creature last target into a square
            Point spellTargetSq = new Point(target.LocationMap.x, target.LocationMap.y);

            return Game.Dungeon.Player.CastSpell(spell, spellTargetSq);
        }

        /// <summary>
        /// Player selects a spell. Returns the spell into all knownSpells or -1 if none selected
        /// </summary>
        /// <returns></returns>
        private Spell SelectSpell()
        {
            //Select a spell to cast

            Screen.Instance.DisplaySpells = true;
            UpdateScreen();

            //Player presses a key from a-w to select a spell

            //Build a list of the moves (in the same order as displayed)
            List<Spell> knownSpells = Game.Dungeon.Spells.FindAll(x => x.Known);

            int selectedSpell = -1;

            do
            {
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {

                    char keyCode = (char)userKey.Character;

                    if (keyCode == 'x')
                    {
                        //Exit
                        break;
                    }
                    else
                    {
                        //Otherwise, check if it's valid and play the movie
                        int charIndex = (int)keyCode - (int)'a';

                        if (charIndex < 0)
                            continue;

                        if (charIndex < knownSpells.Count && charIndex < 24)
                        {
                            selectedSpell = charIndex;
                            break;
                        }
                    }
                }
            } while (true);

            //Select a spell to cast

            Screen.Instance.DisplaySpells = false;
            UpdateScreen();

            if (selectedSpell == -1)
                return null;

            return knownSpells[selectedSpell];
        }

        /// <summary>
        /// Let the user target something
        /// </summary>
        /// <returns></returns>
        private bool TargetSpell(out Point target)
        {
            Player player = Game.Dungeon.Player;

            //Start on the nearest creature
            Creature closeCreature = Game.Dungeon.FindClosestCreature(player);

            //If no nearby creatures, start on the player
            if (closeCreature == null)
                closeCreature = Game.Dungeon.Player;

            //Get the FOV from Dungeon (this also updates the map creature FOV state)
            TCODFov currentFOV = Game.Dungeon.CalculateCreatureFOV(player);

            Point startPoint;

            //Is that creature in FOV
            if (currentFOV.CheckTileFOV(closeCreature.LocationMap.x, closeCreature.LocationMap.y))
            {
                //If so, target
                startPoint = new Point(closeCreature.LocationMap.x, closeCreature.LocationMap.y);
            }
            else
            {
                //If not, target the PC
                startPoint = new Point(player.LocationMap.x, player.LocationMap.y);
            }

            //Get the desired target from the player

            return GetTargetFromPlayer(startPoint, out target);
        }

        /// <summary>
        /// Gets a target from the player. false showed an escape. otherwise target is the target selected.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private bool GetTargetFromPlayer(Point start, out Point target)
        {

            //Turn targetting mode on the screen
            Screen.Instance.TargettingModeOn();
            Screen.Instance.Target = start;
            Game.MessageQueue.AddMessage("Find a target. Z to fire. ESC to exit.");
            UpdateScreen();

            bool keepLooping = true;
            bool validFire = false;

            target = start;

            do
            {
                //Get direction from the user or 'Z' to fire
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                Point direction = new Point();

                bool validDirection = false;


                if (GetDirectionFromKeypress(userKey, out direction))
                {
                    //Valid direction
                    validDirection = true;
                }
                else
                {
                    //Look for firing
                    if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                    {
                        char keyCode = (char)userKey.Character;
                        switch (keyCode)
                        {

                            case 'Z':

                                validFire = true;
                                keepLooping = false;
                                break;
                        }
                    }

                    if (userKey.KeyCode == KeyCode.TCODK_ESCAPE)
                    {
                        keepLooping = false;
                    }
                }

                //If direction, update the location and redraw

                if (validDirection)
                {
                    Point newPoint = new Point(target.x + direction.x, target.y + direction.y);

                    int level = Game.Dungeon.Player.LocationLevel;

                    if (newPoint.x < 0 || newPoint.x >= Game.Dungeon.Levels[level].width || newPoint.y < 0 || newPoint.y >= Game.Dungeon.Levels[level].height)
                        continue;

                    //Otherwise OK
                    target = newPoint;

                    //Update screen
                    Screen.Instance.Target = newPoint;
                    Game.MessageQueue.AddMessage("Find a target. Z to fire. ESC to exit.");
                    UpdateScreen();

                }
            } while (keepLooping);

            //Turn targetting mode off
            Screen.Instance.TargettingModeOff();
            UpdateScreen();

            return validFire;

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
            int itemIndex = selectedGroup.ItemIndex[0];
            Item itemToName = playerInventory.Items[itemIndex];

            bool usedSuccessfully = Game.Dungeon.Player.UseItem(selectedGroup);
            
            //THE BELOW DOESN'T WORK TOO WELL
            //MESSAGES DON@T APPEAR
            //TO IMPLEMENT PROPERLY PERHAPS USE A MEMORY DISPLAY(ALL POTIONS DRUNK) RATHER THAN THOSE IN INVENTORY
            //WE WOULD HAVE TO STORE THIS IN PLAYER OR DUNGEON THOUGH - TODO
            /*
            Game.MessageQueue.AddMessage("<more>");
            UpdateScreen();
            RunMessageQueue();
            //Screen.Instance.FlushConsole();
            KeyPress userKey = Keyboard.WaitForKeyPress(true);

            //Let the player rename the item


            //Get user string
            string newName = Screen.Instance.GetUserString("New name for " + Game.Dungeon.GetHiddenName(itemToName));
            if (newName != null)
            {
                Game.Dungeon.AssociateNameWithItem(itemToName, newName);
            }
            */
            return usedSuccessfully;
        }

        /// <summary>
        /// Player equips item. Returns true if item was equipped and time should advance
        /// </summary>
        private bool EquipItem()
        {
            //User selects which item to use
            int chosenIndex = PlayerChooseFromInventory();

            //Player exited
            if (chosenIndex == -1)
                return false;

            Inventory playerInventory = Game.Dungeon.Player.Inventory;

            InventoryListing selectedGroup = playerInventory.InventoryListing[chosenIndex];
            bool usedSuccessfully = Game.Dungeon.Player.EquipItem(selectedGroup);

            return usedSuccessfully;
        }

        /// <summary>
        /// Pick up an item if there is one in this square
        /// </summary>
        /// <returns></returns>
        private bool PickUpItem()
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = dungeon.Player;

            Item itemToPickUp = dungeon.ItemAtSpace(player.LocationLevel, player.LocationMap);

            if (itemToPickUp == null)
                return false;

            //Policy for DDRogue is that all equippable items are automatically equipped and never appear in the inventory
            IEquippableItem equipItem = itemToPickUp as IEquippableItem;

            if (equipItem != null)
            {
                //The item is equippable
                player.EquipItemNoSlots(equipItem);
            }
            else
            {
                //Add item to PC inventory
                //Better on player
                player.PickUpItem(itemToPickUp);
                

                //Message

                //Tell the player if there's something behind it...!

                //Use a hidden name if required
                string itemName;
                if (itemToPickUp.UseHiddenName)
                {
                    itemName = Game.Dungeon.GetHiddenName(itemToPickUp);
                }
                else
                    itemName = itemToPickUp.SingleItemDescription;

                if (dungeon.ItemAtSpace(player.LocationLevel, player.LocationMap) != null)
                {
                    Game.MessageQueue.AddMessage(itemName + " picked up. There's something behind it!");
                }
                else
                    Game.MessageQueue.AddMessage(itemName + " picked up.");

                LogFile.Log.LogEntry(itemName + " picked up.");
            }
            return true;
        }

        private void EquipPickedUpItem()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Drop an item from inventory
        /// </summary>
        /// <returns></returns>
        private bool DropItem()
        {
            //User selects which item to use
            int chosenIndex = PlayerChooseFromInventory();

            //Player exited
            if (chosenIndex == -1)
                return false;

            Dungeon dungeon = Game.Dungeon;
            Player player = dungeon.Player;

            Inventory playerInventory = player.Inventory;

            InventoryListing selectedGroup = playerInventory.InventoryListing[chosenIndex];
            Item selectedItem = playerInventory.Items[selectedGroup.ItemIndex[0]];

            //Check there is no item here already
            //if(dungeon.ItemAtSpace(player.LocationLevel, player.LocationMap) != null) {
            //    Game.MessageQueue.AddMessage("Can't drop - already an item here!");
            //    return false;
            //}

            //Remove from player inventory
            player.DropItem(selectedItem);

            Game.MessageQueue.AddMessage(selectedItem.SingleItemDescription + " dropped.");

            return true;
        }

        /// <summary>
        /// Display equipment overlay. If any items are useable let the user select them
        /// </summary>
        private bool DisplayEquipment()
        {
            //User selects which item to use
            int chosenIndex = PlayerChooseFromEquipment();

            //Player exited
            if (chosenIndex == -1)
                return false;

            Inventory playerInventory = Game.Dungeon.Player.Inventory;

            InventoryListing selectedGroup = playerInventory.EquipmentListing[chosenIndex];
            int itemIndex = selectedGroup.ItemIndex[0];

            return Game.Dungeon.Player.UseItem(selectedGroup);
        }

        /// <summary>
        /// Movie screen overlay
        /// </summary>
        private void MovieScreenInteraction()
        {

            //Player presses a key from a-w to select a special move

            //Build a list of the moves (in the same order as displayed)
            List<SpecialMove> knownMoves = Game.Dungeon.SpecialMoves.FindAll(x => x.Known);
            List<Spell> knownSpells = Game.Dungeon.Spells.FindAll(x => x.Known);

            do
            {
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {

                    char keyCode = (char)userKey.Character;

                    if (keyCode == 'x')
                    {
                        //Exit
                        return;
                    }
                    else
                    {
                        //Otherwise, check if it's valid and play the movie
                        int charIndex = (int)keyCode - (int)'a';

                        if (charIndex < 0)
                            continue;

                        if (charIndex < (knownMoves.Count + knownSpells.Count) && charIndex < 24)
                        {
                            if (charIndex < knownMoves.Count)
                            {
                                Screen.Instance.PlayMovie(knownMoves[charIndex].MovieRoot(), false);
                            }
                            else
                            {
                                charIndex = charIndex - knownMoves.Count;
                                Screen.Instance.PlayMovie(knownSpells[charIndex].MovieRoot(), false);
                            }
                        }
                    }
                }
            } while (true);
        }

        private int PlayerChooseFromEquipment() {

            //Player presses a key from e-w to select an equipment listing or x to exit
            //Check how many items are available
            Inventory playerInventory = Game.Dungeon.Player.Inventory;
            int numInventoryListing = playerInventory.EquipmentListing.Count;

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

                        if (charIndex < 0)
                            continue;

                        if (charIndex < numInventoryListing && charIndex < 24)
                            return charIndex;

                    }
                }
            } while (true);

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

                        if (charIndex < 0)
                            continue;

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

            //Make a list of the wrapped strings
            //List<string> wrappedMsg = new List<string>();

            //Stick all the messages together in one long string
            string allMsgs = "";
            foreach (string message in messages)
            {
                allMsgs += message + " ";
            }

            //Strip off the last piece of white space
            allMsgs = allMsgs.Trim();

            //Now make a list of trimmed msgs with <more> appended
            List<string> wrappedMsgs = new List<string>();
            do
            {
                //put function in utility
                string trimmedMsg = Utility.SubstringWordCut(allMsgs, "", 83);
                wrappedMsgs.Add(trimmedMsg);
                //make our allMsgs smaller
                allMsgs = allMsgs.Substring(trimmedMsg.Length);
            } while (allMsgs.Length > 0);

            int noLines = Screen.Instance.msgDisplayNumLines;

            int i = 0;
            do
            {
                //Require moreing
                if (i < wrappedMsgs.Count - noLines)
                {
                    //Add the messages together for PrintMessage
                    string outputMsg = "";

                    for (int j = 0; j < noLines; j++)
                    {
                        outputMsg += wrappedMsgs[i + j].Trim();

                        if (j != noLines - 1)
                            outputMsg += "\n";
                    }

                    //Update line counter
                    i += noLines;

                    outputMsg.Trim();

                    Screen.Instance.PrintMessage(outputMsg + " <more>");
                    Screen.Instance.FlushConsole();

                    //Block for this keypress - may want to listen for exit too
                    KeyPress userKey;
                    userKey = Keyboard.WaitForKeyPress(true);
                }
                else
                {
                    //Add the messages together for PrintMessage
                    string outputMsg = "";

                    for (int j = 0; j < noLines; j++)
                    {
                        if (i + j >= wrappedMsgs.Count)
                            break;

                        outputMsg += wrappedMsgs[i + j].Trim();

                        if (j != noLines - 1)
                            outputMsg += "\n";
                    }

                    outputMsg.Trim();

                    //Update line counter
                    i += noLines;

                    Screen.Instance.PrintMessage(outputMsg);
                    Screen.Instance.FlushConsole();
                }
            } while (i < wrappedMsgs.Count);
            
            Game.MessageQueue.ClearList();

        }

        private void SetupGame()
        {
            //Initial setup

            //Setup screen
            Screen.Instance.InitialSetup();

            //Setup global random
            Random rand = new Random();

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

            //Intro screen pre-game (must come after screen)
            
            /*
            GameIntro intro = new GameIntro();
            intro.ShowIntroScreen();

            string playerName = intro.PlayerName;
            bool showMovies = intro.ShowMovies;
            GameDifficulty diff = intro.Difficulty;
            */

            string playerName = "Dave";
            bool showMovies = false;
            GameDifficulty diff = GameDifficulty.Easy;


            //Setup dungeon

            //Is there a save game to load?
            if (Utility.DoesSaveGameExist(playerName))
            {
                LoadGame(playerName);
            }
            else {

                //If not, make a new dungeon for the new player
                //Dungeon really contains all the state, so also sets up player etc.

                dungeonMaker = new DungeonMaker(diff);
                Game.Dungeon = dungeonMaker.SpawnNewDungeon();

                Game.Dungeon.Player.Name = playerName;
                Game.Dungeon.Player.PlayItemMovies = showMovies;
                Game.Dungeon.Difficulty = diff;

                //Move the player to the start location, triggering any triggers
                Game.Dungeon.MovePCAbsolute(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap.x, Game.Dungeon.Player.LocationMap.y);
            }

            //Fall into the main loop
        }

        private void SetupDungeon()
        {
            /*

            //Create dungeon and set it as current in Game
            Game.Dungeon = new Dungeon();

 

            //Create dungeon map (at least level 1)
            MapGeneratorBSPCave mapGen1 = new MapGeneratorBSPCave();
            //MapGeneratorRogue mapGen = new MapGeneratorRogue();
            mapGen1.Width = 80;
            mapGen1.Height = 25;
            int extraCorridors = rand.Next(10);

            Map level1 = mapGen1.GenerateMap(extraCorridors);

            MapGeneratorBSP mapGen2 = new MapGeneratorBSP();
            mapGen2.Width = 80;
            mapGen2.Height = 25;
            Map level2 = mapGen2.GenerateMap(extraCorridors);

            MapGeneratorCave cave1 = new MapGeneratorCave();
            cave1.Width = 80;
            cave1.Height = 25;

            Map cave = cave1.GenerateMap(true); ;

 
            //KeyPress userKey = Keyboard.WaitForKeyPress(true);


            //Test
            //for (int i = 0; i < 10000; i++)
            //{
            //    mapGen.GenerateMap();
            //}

            //Give map to dungeon
            Game.Dungeon.AddMap(level1); //level 1
            //dungeon.AddMap(level2); //level 2

            int caveLevel = Game.Dungeon.AddMap(cave);
            cave1.AddStaircases(caveLevel);


            //Load level 3 from file
            try
            {
                MapGeneratorFromASCIIFile mapGen3 = new MapGeneratorFromASCIIFile();
                mapGen3.LoadASCIIFile("test1.txt");
                mapGen3.AddMapToDungeon();
            }
            catch (Exception ex)
            {
                MessageBox.Show("failed to add level 3: " + ex.Message);
            }
            //Setup PC
            Player player = Game.Dungeon.Player;


            //Give the player some items
            //player.PickUpItem(new Items.Potion());

            //Add a down staircase where the player is standing
            AddFeatureToDungeon(new Features.StaircaseDown(), 0, new Point(player.LocationMap.x, player.LocationMap.y));

            //Add a test short sword
            Game.Dungeon.AddItem(new Items.ShortSword(), 0, new Point(player.LocationMap.x, player.LocationMap.y));

            //Create creatures and start positions

            //Add some random creatures


            
            int noCreatures = rand.Next(10) + 15;

            for (int i = 0; i < noCreatures; i++)
            {
                Monster creature = new Creatures.Rat();
                creature.Representation = Convert.ToChar(65 + rand.Next(26));

                //int level = rand.Next(2);
                int level = 0;
                Point location = new Point(0, 0);

                //Loop until we find an acceptable location and the add works
                do
                {
                    if (level == 0)
                        location = mapGen1.RandomWalkablePoint();
                    else if (level == 1)
                        location = mapGen2.RandomWalkablePoint();
                    LogFile.Log.LogEntryDebug("Creature " + i.ToString() + " pos x: " + location.x + " y: " + location.y, LogDebugLevel.Low);
                }
                while (!Game.Dungeon.AddMonster(creature, level, location));
            }

            //Create features

            //Add some random features
            
            int noFeatures = rand.Next(5) + 2;

            for (int i = 0; i < noFeatures; i++)
            {
                Features.StaircaseUp feature = new Features.StaircaseUp();

                feature.Representation = Convert.ToChar(58 + rand.Next(6));
                AddFeatureToDungeon(feature, mapGen1, 0);
            }

            //Add staircases to dungeons level 1 and 2
            AddFeatureToDungeonRandomPoint(new Features.StaircaseDown(), mapGen1, 0);
            //AddFeatureToDungeonRandomPoint(new Features.StaircaseUp(), mapGen2, 1);
            

            //Create objects and start positions

            //Add some random objects

            int noItems = rand.Next(10) + 5;

            for (int i = 0; i < noItems; i++)
            {
                Item item;

            //    if (rand.Next(2) < 1)
              //  {
                    item = new Items.Potion();
                //}
            //    else
            //    {
            //        item = new Items.ShortSword();
            //    }

            //    //item.Representation = Convert.ToChar(33 + rand.Next(12));

                int level = 0;
                Point location;

            //    //Loop until we find an acceptable location and the add works
                do
                {
                    location = mapGen1.RandomWalkablePoint();
                }
                while (!Game.Dungeon.AddItem(item, level, location));
            }*/
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
                location = mapGen.RandomWalkablePoint();

            }
            while (!Game.Dungeon.AddFeature(feature, level, location));
        }

        /// <summary>
        /// Add a feature to the dungeon. Guarantees an acceptable placement.
        /// Might loop forever if all squares in the dungeon are already taken up
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        void AddFeatureToDungeonRandomPoint(Feature feature, MapGeneratorBSPCave mapGen, int level)
        {
            Point location;
            //Loop until we find an acceptable location and the add works
            do
            {
                location = mapGen.RandomWalkablePoint();

            }
            while (!Game.Dungeon.AddFeature(feature, level, location));
        }

        /// <summary>
        /// Add a feature to the dungeon at a certain place. May fail if something is already there.
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="level"></param>
        /// <param name="location"></param>
        bool AddFeatureToDungeon(Feature feature, int level, Point location)
        {
            return Game.Dungeon.AddFeature(feature, level, location);
        }
    }
}
