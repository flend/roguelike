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
            //Try to run the graph example
            //var example7 = new graphtestc.Example7();
            //example7.OutputGraph();


            SetupSystem();
            bool loadedGame = SetupGame();

            MainLoop(loadedGame);

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

        public void MainLoop(bool loadedGame)
        {
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
                    //If we want to give the PC an extra go for any reason before the creatures
                    //(e.g. has just loaded, has just entered dungeon)
                    //test here

                    bool pcFreeTurn = false;

                    if (!Game.Dungeon.PlayerHadBonusTurn && Game.Dungeon.PlayerBonusTurn)
                        pcFreeTurn = true;

                    //Monsters turn

                    if (!pcFreeTurn)
                    {
                        //Increment world clock
                        Game.Dungeon.IncrementWorldClock();

                        //ProfileEntry("Pre event");

                        //Increment time on all global (dungeon) events
                        //Game.Dungeon.IncrementEventTime();

                        //All creatures get IncrementTurnTime() called on them each worldClock tick
                        //They internally keep track of when they should take another turn

                        //IncrementTurnTime() also increments time for all events on that creature

                        //ProfileEntry("Pre monster");

                        foreach (Monster creature in Game.Dungeon.Monsters)
                        {
                            try
                            {
                                //Only process creatures on the same level as the player
                                if (creature.LocationLevel == Game.Dungeon.Player.LocationLevel)
                                {
                                    if (creature.IncrementTurnTime())
                                    {
                                        //dungeon.ShowCreatureFOVOnMap(creature);

                                        //Creatures may be killed by other creatures so check they are alive before processing
                                        if (creature.Alive)
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

                        //ProfileEntry("Post monster");

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
                        
                    }
                    
                    //PC turn
                    try
                    {
                        

                        //Increment time on the PC's events and turn time (all done in IncrementTurnTime)
                        if (Game.Dungeon.Player.IncrementTurnTime())
                        {
                            //Remove dead players! Restart mission. Do this here so we don't get healed then beaten up again in our old state
                            if (Game.Dungeon.PlayerDeathOccured)
                                Game.Dungeon.PlayerDeath(Game.Dungeon.PlayerDeathString);

                            ProfileEntry("Pre PC POV");

                            //Calculate the player's FOV
                            Game.Dungeon.CalculatePlayerFOV();

                            ProfileEntry("Pre Monster POV");

                            //Debug: show the FOV of all monsters. Should flag or comment this for release.
                            foreach (Monster monster in Game.Dungeon.Monsters)
                            {
                                Game.Dungeon.ShowCreatureFOVOnMap(monster);
                            }

                            Game.Dungeon.ShowSoundsOnMap();


                            ProfileEntry("Post Monster POV");

                            //For effects that end to update the screen correctly
                            if (Game.Dungeon.Player.RecalculateCombatStatsRequired)
                                Game.Dungeon.Player.CalculateCombatStats();

                            //Check the 'on' status of special moves - now unnecessary?
                            //Game.Dungeon.CheckSpecialMoveValidity();

                            ProfileEntry("Pre Screen Update");

                            //Update screen just before PC's turn
                            Screen.Instance.CenterViewOnPoint(Game.Dungeon.Player.LocationMap);
                            Screen.Instance.Update();

                            ProfileEntry("Post Screen Update");

                            //Deal with PCs turn as appropriate
                            bool timeAdvances = false;
                            do
                            {
                                timeAdvances = UserInput();
                            } while (!timeAdvances);

                            ProfileEntry("After user");

                            //Reset the creature FOV display
                            Game.Dungeon.ResetCreatureFOVOnMap();
                            Game.Dungeon.ResetSoundOnMap();

                            //Game.MessageQueue.AddMessage("Finished PC move");
                            Game.Dungeon.PlayerHadBonusTurn = true;
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

        private void ProfileEntry(string p)
        {
            if(Game.Dungeon.Profiling)
                LogFile.Log.LogEntryDebug(p + " " + DateTime.Now.Millisecond.ToString(), LogDebugLevel.Profiling);
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
                                    Screen.Instance.Update();
                                    break;

                                case 'S':
                                    //Save the game
                                    timeAdvances = true;
                                    Game.MessageQueue.AddMessage("Saving game...");
                                    Screen.Instance.Update();
                                    Game.Dungeon.SaveGame();
                                    Game.MessageQueue.AddMessage("Press any key to exit the game.");
                                    Screen.Instance.Update();
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

                                    //No longer needed
                                    /*
                                case 'o':
                                case 'O':
                                    //Open door
                                    timeAdvances = PlayerOpenDoor();
                                    if (!timeAdvances)
                                        Screen.Instance.Update();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;
                                    */
                                    /*
                                case 'c':
                                case 'C':
                                    //Close door
                                    timeAdvances = PlayerCloseDoor();
                                    if (!timeAdvances)
                                        Screen.Instance.Update();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;
                                    */
                                    //Repeatidly closing doors and lurking behind them was kind of abusive

                                case 'f':
                                //case 'Z':
                                    //Fire weapon
                                    timeAdvances = FireWeapon();
                                    if (!timeAdvances)
                                        Screen.Instance.Update();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;

                                case 't':
                                    //Throw weapon
                                    timeAdvances = ThrowWeapon();
                                    if (!timeAdvances)
                                        Screen.Instance.Update();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;

                                case 'T':
                                    //Throw utility
                                    timeAdvances = ThrowUtility();
                                    if (!timeAdvances)
                                        Screen.Instance.Update();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;

                                case 'U':
                                    //Use utility
                                    timeAdvances = UseUtility();
                                    if (!timeAdvances)
                                        Screen.Instance.Update();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;
                                    /*
                                case 'u':
                                    //Use weapon
                                    timeAdvances = UseWeapon();
                                    if (!timeAdvances)
                                        Screen.Instance.Update();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;*/

                                case 'x':
                                    //Examine
                                    timeAdvances = Examine();
                                    if (!timeAdvances)
                                        Screen.Instance.Update();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;

                                    /*
                                case 'x':
                                case 'X':
                                    //Recast last spells
                                    timeAdvances = RecastSpell();
                                    if (!timeAdvances)
                                        Screen.Instance.Update();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;*/

                                    /*
                                case 'c':
                                case 'C':
                                    //Charm creature
                                    timeAdvances = PlayerCharmCreature();
                                    if (!timeAdvances)
                                        Screen.Instance.Update();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;
                                    */

                                    /*
                                case ',':
                                case 'g':
                                    //Pick up item
                                    timeAdvances = PickUpItem();
                                    //Only update screen is unsuccessful, otherwise will be updated in main loop (can this be made general)
                                    if (!timeAdvances)
                                        Screen.Instance.Update();
                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;*/

                                case '.':
                                    // Do nothing
                                    timeAdvances = DoNothing();
                                    break;
                                    
                                case '>':
                                case '<':
                                    //Interact with feature
                                    timeAdvances = InteractWithFeature();
                                    if (!timeAdvances)
                                        Screen.Instance.Update();

                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();

                                    break;
                                    /*
                                case 'd':
                                case 'D':
                                    //Drop items if in town
                                    //DropItems();
                                    Screen.Instance.Update();
                                    timeAdvances = false;
                                    break;*/
                                    /*
                                case 'i':
                                case 'I':
                                    //Use an inventory item
                                    SetPlayerInventorySelectScreen();
                                    Screen.Instance.Update();
                                    //This uses the generic 'select from inventory' input loop
                                    //Time advances if the item was used successfully
                                    timeAdvances = UseItem();
                                    DisablePlayerInventoryScreen();
                                    //Only update the screen if the player has another selection to make, otherwise it will be updated automatically before his next go
                                    if (!timeAdvances)
                                        Screen.Instance.Update();

                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;


                                case 'e':
                                case 'E':
                                    //Display currently equipped items
                                    SetPlayerEquippedItemsScreen();
                                    Screen.Instance.Update();
                                    timeAdvances = DisplayEquipment();
                                    DisablePlayerEquippedItemsScreen();

                                    //Using an item can break a special move sequence
                                    if (!timeAdvances)
                                        Screen.Instance.Update();

                                    if (timeAdvances)
                                        SpecialMoveNonMoveAction();
                                    break;


                                case 'm':
                                
                                    //Show movies
                                    SetSpecialMoveMovieScreen();
                                    Screen.Instance.Update();
                                    MovieScreenInteraction();
                                    DisableSpecialMoveMovieScreen();
                                    Screen.Instance.Update();
                                    timeAdvances = false;
                                    break;*/

                                case 'M':
                                    SetMsgHistoryScreen();
                                    Screen.Instance.Update();
                                    DisableMsgHistoryScreen();
                                    Screen.Instance.Update();
                                    timeAdvances = false;
                                    break;

                                case '?':
                                    Screen.Instance.PlayMovie("helpkeys", true);
                                    timeAdvances = false;
                                    break;

                                    
                                //Debug events
                                    
                                    
                                //Stats up. Use in town
                                case 'w':
                                    Game.Dungeon.Player.AttackStat = 100;
                                    Game.Dungeon.Player.CharmStat = 100;
                                    Game.Dungeon.Player.MagicStat = 100;
                                    Game.Dungeon.Player.HitpointsStat = 200;
                                    Game.Dungeon.Player.MaxHitpointsStat = 200;

                                    Game.Dungeon.Player.Hitpoints = 100;
                                    Game.Dungeon.Player.MagicPoints = 100;

                                    //Game.Dungeon.DungeonInfo.DragonDead = false;

                                    //Game.Dungeon.EndOfGame();

                                    break;

                                case 'K':
                                    //Add a sound at the player's location
                                    Game.Dungeon.AddSoundEffect(1.0, Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap);
                                    //refresh the sound display
                                    Game.Dungeon.ShowSoundsOnMap();

                                    Screen.Instance.Update();
                                    break;

                                case 'z':
                                    Game.Dungeon.ExplodeAllMonsters();
         
                                    break;

                                case 'N':
                                    //screen numbering
                                    Screen.Instance.CycleRoomNumbering();
                                    Screen.Instance.Update();
                                    break;

                                case 'W':
                                    //screen debug mode
                                    Screen.Instance.DebugMode = !Screen.Instance.DebugMode;
                                    Screen.Instance.Update();
                                    break;

                                case 'V':
                                    //screen debug mode
                                    Game.Dungeon.Player.AddEffect(new PlayerEffects.SightRadiusUp(2000, 1));
                                    Screen.Instance.Update();
                                    break;
                                case 'y':
                                    //next mission
                                    Game.Dungeon.MoveToNextMission();
                                    break;

                                case 'Y':
                                    //Take me to first dungeon
                                    //Game.Dungeon.DungeonInfo.LastMission = true;
                                    Game.Dungeon.Player.LocationLevel = 2;
                                    TeleportToDungeon1Entrance();
                                    Screen.Instance.Update();
                                    break;

                                case 'J':
                                    //change debug level
                                    LogFile.Log.DebugLevel += 1;
                                    if (LogFile.Log.DebugLevel > 3)
                                        LogFile.Log.DebugLevel = 1;

                                    LogFile.Log.LogEntry("Log Debug level now: " + LogFile.Log.DebugLevel.ToString());
                                    
                                    break;

                                case 'v':
                                    //Add a healing event on the player
                                    PlayerEffects.Healing healing = new RogueBasin.PlayerEffects.Healing(10);
                                    Game.Dungeon.Player.AddEffect(healing);
                                    Screen.Instance.Update();
                                    break;
                               
                                /*
                            case 'k':
                                //Display the inventory
                                inputState = InputState.InventoryShow;
                                SetPlayerInventoryScreen();
                                UpdateScreen();
                                timeAdvances = false;
                                break;
                                
                            

                            //case 'c':
                            //    //Level up
                            //    Game.Dungeon.Player.LevelUp();
                            //    UpdateScreen();
                            //    break;

                            

                            case 'M':
                                //Learn all moves
                                Game.Dungeon.LearnMove(new SpecialMoves.CloseQuarters());
                                Game.Dungeon.LearnMove(new SpecialMoves.ChargeAttack());
                                Game.Dungeon.LearnMove(new SpecialMoves.WallVault());
                                Game.Dungeon.LearnMove(new SpecialMoves.VaultBackstab());
                                Game.Dungeon.LearnMove(new SpecialMoves.WallLeap());
                                Game.MessageQueue.AddMessage("Learnt all moves.");
                                //Game.Dungeon.PlayerLearnsAllSpells();
                                //Game.MessageQueue.AddMessage("Learnt all spells.");
                                UpdateScreen();
                                timeAdvances = false;
                                break;




                           

                           

                            case 'B':
                                Screen.Instance.SaveCurrentLevelToDisk();
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

                        //Handle direction keys (both arrows and vi keys)
                        Point direction = new Point(9,9);
                        KeyModifier mod = KeyModifier.Arrow;
                        bool wasDirection = GetDirectionFromKeypress(userKey, out direction, out mod);

                        if (wasDirection && (mod == KeyModifier.Numeric || mod == KeyModifier.Vi))
                        {
                            timeAdvances = Game.Dungeon.PCMove(direction.x, direction.y);
                        }

                        if (wasDirection && mod == KeyModifier.Arrow)
                        {
                            Screen.Instance.ViewportScrollSpeed = 4;
                            Screen.Instance.ScrollViewport(direction);
                            Screen.Instance.Update();
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
                                
                                Screen.Instance.Update();
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
                //Game.Dungeon.PutItemsInStore();
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

        /// <summary>
        /// Debug function - teleport to 1st dungeon
        /// </summary>
        private void TeleportToDungeon1Entrance()
        {
            //Find down stairs on this level
            List<Feature> features = Game.Dungeon.Features;

            Player player = Game.Dungeon.Player;

            Features.StaircaseEntry entryStairs = null;
            Point stairlocation = new Point(0, 0);

            foreach (Feature feature in features)
            {

                if (feature is Features.StaircaseEntry)
                {
                    entryStairs = feature as Features.StaircaseEntry;
                    if (entryStairs.dungeonStartLevel == 2)
                    {
                        stairlocation = feature.LocationMap;
                        break;
                    }
                }
            }

            if (entryStairs == null)
            {
                LogFile.Log.LogEntryDebug("Unable to teleport to stairs", LogDebugLevel.High);
                return;
            }

            //Move the player
            //Wilderness
            player.LocationLevel = 1;
            player.LocationMap = stairlocation;
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

                //compStream = new GZipStream(stream, CompressionMode.Decompress, true);
                //SaveGameInfo readData = (SaveGameInfo)serializer.Deserialize(compStream);
                SaveGameInfo readData = (SaveGameInfo)serializer.Deserialize(stream);

                //Build a new dungeon object from the stored data
                Dungeon newDungeon = new Dungeon();

                newDungeon.Features = readData.features;
                newDungeon.Items = readData.items;
                newDungeon.Effects = readData.effects;
                newDungeon.Monsters = readData.monsters;
                newDungeon.Spells = readData.spells;
                newDungeon.Player = readData.player;
                newDungeon.SpecialMoves = readData.specialMoves;
                newDungeon.WorldClock = readData.worldClock;
                newDungeon.HiddenNameInfo = readData.hiddenNameInfo;
                newDungeon.Triggers = readData.triggers;
                newDungeon.Difficulty = readData.difficulty;
                newDungeon.DungeonInfo = readData.dungeonInfo;
                newDungeon.dateCounter = readData.dateCounter;
                newDungeon.nextUniqueID = readData.nextUniqueID;
                newDungeon.nextUniqueSoundID = readData.nextUniqueSoundID;
                newDungeon.Effects = readData.effects;
                newDungeon.DungeonMaker = readData.dungeonMaker;

                Game.MessageQueue.TakeMessageHistoryFromList(readData.messageLog);
                
                //Process the maps back into map objects
                foreach (SerializableMap serialMap in readData.levels)
                {
                    //Add a map. Note that this builds a TCOD map too
                    newDungeon.AddMap(serialMap.MapFromSerializableMap());
                }

                //Build TCOD maps
                newDungeon.RefreshAllLevelPathing();

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

                //Give player free turn (save was on player's turn so don't give the monsters a free go cos they saved)
                Game.Dungeon.PlayerBonusTurn = true;

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
        private bool GetDirectionKeypress(out Point direction, out KeyModifier mod)
        {
            //Get direction
            KeyPress userKey = Keyboard.WaitForKeyPress(true);

            if (GetDirectionFromKeypress(userKey, out direction, out mod))
            {
                return true;
            }

            return false;
        }

        private enum KeyModifier {
            Vi,
            Numeric,
            Arrow
        }


        /// <summary>
        /// Get a direction from a keypress. Will return false if not valid. Otherwise in parameter.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private bool GetDirectionFromKeypress(KeyPress userKey, out Point direction, out KeyModifier mod) {

            direction = new Point(9, 9);
            mod = KeyModifier.Arrow;
            
            //Vi keys for directions

            if (userKey.KeyCode == KeyCode.TCODK_CHAR)
            {
                char keyCode = (char)userKey.Character;
                switch (keyCode)
                {
                    case 'b':
                        direction = new Point(-1, 1);
                        mod = KeyModifier.Vi;
                        break;

                    case 'n':
                        direction = new Point(1, 1);
                        mod = KeyModifier.Vi;
                        break;

                    case 'y':
                        direction = new Point(-1, -1);
                        mod = KeyModifier.Vi;
                        break;

                    case 'u':
                        direction = new Point(1, -1);
                        mod = KeyModifier.Vi;
                        break;

                    case 'h':
                        direction = new Point(-1, 0);
                        mod = KeyModifier.Vi;
                        break;

                    case 'l':
                        direction = new Point(1, 0);
                        mod = KeyModifier.Vi;
                        break;

                    case 'k':
                        direction = new Point(0, -1);
                        mod = KeyModifier.Vi;
                        break;

                    case 'j':
                        direction = new Point(0, 1);
                        mod = KeyModifier.Vi;
                        break;
                }
            }
            else
            {
                //Arrow keys for directions

                switch (userKey.KeyCode)
                {
                    case KeyCode.TCODK_KP1:
                        direction = new Point(-1, 1);
                        mod = KeyModifier.Numeric;
                        break;

                    case KeyCode.TCODK_KP3:
                        direction = new Point(1, 1);
                        mod = KeyModifier.Numeric;
                        break;

                    case KeyCode.TCODK_KPDEC:
                        direction = new Point(0, 0);
                        mod = KeyModifier.Arrow;
                        break;

                    case KeyCode.TCODK_KP5:
                        direction = new Point(0, 0);
                        mod = KeyModifier.Numeric;
                        break;

                    case KeyCode.TCODK_KP7:
                        direction = new Point(-1, -1);
                        mod = KeyModifier.Numeric;
                        break;
                    case KeyCode.TCODK_KP9:
                        direction = new Point(1, -1);
                        mod = KeyModifier.Numeric;
                        break;

                    case KeyCode.TCODK_LEFT:
                        direction = new Point(-1, 0);
                        mod = KeyModifier.Arrow;
                        break;

                    case KeyCode.TCODK_KP4:
                        direction = new Point(-1, 0);
                        mod = KeyModifier.Numeric;
                        break;
                    case KeyCode.TCODK_RIGHT:
                        direction = new Point(1, 0);
                        mod = KeyModifier.Arrow;
                        break;
                    case KeyCode.TCODK_KP6:
                        direction = new Point(1, 0);
                        mod = KeyModifier.Numeric;
                        break;
                    case KeyCode.TCODK_UP:
                        direction = new Point(0, -1);
                        mod = KeyModifier.Arrow;
                        break;
                    case KeyCode.TCODK_KP8:
                        direction = new Point(0, -1);
                        mod = KeyModifier.Numeric;
                        break;
                    case KeyCode.TCODK_KP2:
                        direction = new Point(0, 1);
                        mod = KeyModifier.Numeric;
                        break;
                    case KeyCode.TCODK_DOWN:
                        direction = new Point(0, 1);
                        mod = KeyModifier.Arrow;
                        break;
                }
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
            Screen.Instance.Update();

            //Get direction
            Point direction = new Point(0, 0);
            KeyModifier mod = KeyModifier.Arrow;
            bool gotDirection = GetDirectionKeypress(out direction, out mod);
            
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

        private bool PlayerCloseDoor()
        {
            //Ask user for a direction
            Game.MessageQueue.AddMessage("Select a direction:");
            Screen.Instance.Update();

            //Get direction
            Point direction = new Point(0, 0);
            KeyModifier mod = KeyModifier.Arrow;
            bool gotDirection = GetDirectionKeypress(out direction, out mod);

            if (!gotDirection)
            {
                Game.MessageQueue.AddMessage("No direction");
                return false;
            }

            //Check there is a door here

            Player player = Game.Dungeon.Player;
            Point doorLocation = new Point(direction.x + player.LocationMap.x, direction.y + player.LocationMap.y);
            bool success = Game.Dungeon.CloseDoor(player.LocationLevel, doorLocation);

            if (!success)
            {
                Game.MessageQueue.AddMessage("Not an open door!");
                return false;
            }
            return true;
        }

        private bool PlayerUnCharmCreature()
        {
            //Ask user for a direction
            Game.MessageQueue.AddMessage("Select a direction:");
            Screen.Instance.Update();

            //Get direction
            Point direction = new Point(0, 0);
            KeyModifier mod = KeyModifier.Arrow;
            bool gotDirection = GetDirectionKeypress(out direction, out mod);

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
            Screen.Instance.Update();

            //Get direction
            Point direction = new Point(0, 0);
            KeyModifier mod = KeyModifier.Arrow;
            bool gotDirection = GetDirectionKeypress(out direction, out mod);

            if (!gotDirection)
            {
                Game.MessageQueue.AddMessage("No direction");
                return false;
            }

            //Attempt to charm a monster in that square
            bool timePasses = Game.Dungeon.AttemptCharmMonsterByPlayer(direction);

            return timePasses;
        }


        /// <summary>
        /// Set state as movie screen
        /// </summary>
        private void SetMsgHistoryScreen()
        {
            Screen.Instance.ShowMsgHistory = true;
        }

        /// <summary>
        /// Disables state as movie screen
        /// </summary>
        private void DisableMsgHistoryScreen()
        {
            Screen.Instance.ShowMsgHistory = false;
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
            /*
            if (player.LocationLevel < 2)
            {
                Game.MessageQueue.AddMessage("You want to save your spells for the dungeons.");
                LogFile.Log.LogEntryDebug("Attempted to cast spell outside of dungeon", LogDebugLevel.Low);

                return false;
            }*/

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
                //Find spell range
                int range = toCast.GetRange();
                TargettingType targetType = toCast.TargetType();
                //Calculate FOV
                CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);
                targettingSuccess = TargetAttack(out target, range, targetType, 0, 'z', currentFOV);
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
            else
            {
                //Failure store the spell anyway
                lastSpell = toCast;
            }

            //Time only goes past if successfully cast
            return success;
        }

        /// <summary>
        /// Throw weapon. Returns if time passes.
        /// </summary>
        /// <returns></returns>
        private bool ThrowWeapon()
        {
            return ThrowWeaponOrUtility(true);
        }

        private bool UseUtility() {
            return UseUtilityOrWeapon(true);
        }

        private bool UseWeapon()
        {
            return UseUtilityOrWeapon(false);
        }

        /// <summary>
        /// Use a utility
        /// </summary>
        private bool UseUtilityOrWeapon(bool isUtility) {

            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            //Check we have a useable item

            IEquippableItem toUse = null;
            Item toUseItem = null;

            if (isUtility)
            {
                toUse = player.GetEquippedUtility();
                toUseItem = player.GetEquippedUtilityAsItem();
            }
            else
            {
                toUse = player.GetEquippedWeapon();
                toUseItem = player.GetEquippedWeaponAsItem();
            }

            if (toUse == null || !toUse.HasOperateAction())
            {
                Game.MessageQueue.AddMessage("Need an item that can be operated.");
                LogFile.Log.LogEntryDebug("Can't use " + toUseItem.SingleItemDescription, LogDebugLevel.Medium);
                return false;
            }

            //Use the item
            LogFile.Log.LogEntryDebug("Using " + toUseItem.SingleItemDescription, LogDebugLevel.Medium);
            bool success = toUse.OperateItem();

            if(success)
                //Destroy the item
                player.UnequipAndDestroyItem(toUseItem);

            return success;
            
        }

        /// <summary>
        /// Throw utility. Returns if time passes.
        /// </summary>
        /// <returns></returns>
        private bool ThrowUtility()
        {
            return ThrowWeaponOrUtility(false);
        }

        private bool ThrowWeaponOrUtility(bool isWeapon) {

            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            //Check we have a throwable item

            IEquippableItem toThrow = null;
            Item toThrowItem = null;

            char confirmChar = 't';
            if (isWeapon)
            {
                toThrow = player.GetEquippedWeapon();
                toThrowItem = player.GetEquippedWeaponAsItem();
            }
            else
            {
                toThrow = player.GetEquippedUtility();
                toThrowItem = player.GetEquippedUtilityAsItem();
                confirmChar = 'T';
            }

            if (toThrow == null || !toThrow.HasThrowAction())
            {
                Game.MessageQueue.AddMessage("Need an item that can be thrown.");
                return false;
            }

            Point target = new Point();
            bool targettingSuccess = true;

            //Find spell range
            int range = toThrow.RangeThrow();
            TargettingType targetType = toThrow.TargetTypeThrow();
            double angle = 0.0; //no shotgun angle for line targets

            //Calculate FOV
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

            targettingSuccess = TargetAttack(out target, range, targetType, angle, confirmChar, currentFOV);

            //User exited
            if (!targettingSuccess)
                return false;

            //Check we are in range of target (not done above)
            if (!Utility.TestRangeFOVForWeapon(Game.Dungeon.Player, target, range, currentFOV))
            {
                Game.MessageQueue.AddMessage("Out of range!");
                LogFile.Log.LogEntryDebug("Out of range for " + toThrowItem.SingleItemDescription, LogDebugLevel.Medium);

                return false;
            }

            //Actually do throwing action
            Point destinationSq = toThrow.ThrowItem(target);

            //Remove stealth
            player.RemoveEffect(typeof(PlayerEffects.StealthField));
            
            //Destroy it if required
            if (toThrow.DestroyedOnThrow())
            {
                player.UnequipAndDestroyItem(toThrowItem);
                return true;
            }

            if (destinationSq != null)
            {
                //Drop the item at the end point

                Point dropTarget = destinationSq;

                //If there is a creature at the end point, try to find a free area
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, destinationSq);

                //Is there a creature here? If so, try to find another location
                if (squareContents.monster != null)
                {
                    //Get surrounding squares
                    List<Point> freeSqs = dungeon.GetFreeAdjacentSquares(player.LocationLevel, destinationSq);

                    if (freeSqs.Count > 0)
                    {
                        dropTarget = freeSqs[Game.Random.Next(freeSqs.Count)];
                    }
                }

                player.UnequipAndDropItem(toThrowItem, player.LocationLevel, dropTarget);
            }

            //Time only goes past if successfully thrown
            return true;
        }

        /// <summary>
        /// Examine using the target. Returns if time passes.
        /// </summary>
        /// <returns></returns>
        private bool Examine()
        {
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

            Point target = new Point();
            bool targettingSuccess = true;

            Monster oldTargetCreature = Screen.Instance.CreatureToView;
            Item oldTargetItem = Screen.Instance.ItemToView;

            targettingSuccess = TargetAttack(out target, 0, TargettingType.Line, 0, 'x', currentFOV);

            if (!targettingSuccess)
            {
                Screen.Instance.CreatureToView = oldTargetCreature;
                Screen.Instance.ItemToView = oldTargetItem;
            }

            return false;
        }


        /// <summary>
        /// Fire weapon. Returns if time passes.
        /// </summary>
        /// <returns></returns>
        private bool FireWeapon()
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            //Check we have a fireable weapon
            IEquippableItem weapon = player.GetEquippedWeapon();
            Item weaponI = player.GetEquippedWeaponAsItem();

            if (weapon == null || !weapon.HasFireAction())
            {
                Game.MessageQueue.AddMessage("Need a weapon that can fire.");
                return false;
            }

            Point target = new Point();
            bool targettingSuccess = true;

            //Find weapon range
            int range = weapon.RangeFire();
            TargettingType targetType = weapon.TargetTypeFire();
            double spreadAngle = weapon.ShotgunSpreadAngle();

            //Calculate FOV
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

            targettingSuccess = TargetAttack(out target, range, targetType, spreadAngle, 'f', currentFOV);

            //User exited
            if (!targettingSuccess)
                return false;

            if (target.x == player.LocationMap.x && target.y == player.LocationMap.y)
            {
                Game.MessageQueue.AddMessage("Can't target self with " + weaponI.SingleItemDescription + ".");
                LogFile.Log.LogEntryDebug("Can't target self with " + weaponI.SingleItemDescription, LogDebugLevel.Medium);
                return false;
            }

            //Check ammo
            if (weapon.RemainingAmmo() < 1)
            {
                Game.MessageQueue.AddMessage("Not enough ammo for " + weaponI.SingleItemDescription);
                LogFile.Log.LogEntryDebug("Not enough ammo for " + weaponI.SingleItemDescription, LogDebugLevel.Medium);

                return false;
            }

            //Check we are in range of target (not done above)
            if (!Utility.TestRangeFOVForWeapon(Game.Dungeon.Player, target, range, currentFOV))
            {
                Game.MessageQueue.AddMessage("Out of range!");
                LogFile.Log.LogEntryDebug("Out of range for " + weaponI.SingleItemDescription, LogDebugLevel.Medium);

                return false;
            }

            //Actually do firing action
            bool success = weapon.FireItem(target);

            if (success)
            {
                player.RemoveEffect(typeof(PlayerEffects.StealthField));
            }

            //Store details for a recast

            //If we successfully cast, store the target
            if (success)
            {
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
            /*
            if (lastSpell.NeedsTarget())
            {
                if (lastSpellTarget == null)
                {
                    Game.MessageQueue.AddMessage("Choose a spell first.");
                    LogFile.Log.LogEntryDebug("Tried to recast spell with no valid spell target selected", LogDebugLevel.High);
                    return false;
                }
            }*/
            
            //Try to cast the spell

            //Get the FOV from Dungeon (this also updates the map creature FOV state)
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

            //Try the last target
            if (lastSpellTarget != null)
            {
                if (lastSpellTarget.Alive && Game.Dungeon.Player.LocationLevel == lastSpellTarget.LocationLevel)
                {
                    //Check they are not charmed
                    bool charmed = false;
                    Monster mon = lastSpellTarget as Monster;
                    if (mon != null)
                    {
                        if (mon.Charmed)
                        {
                            charmed = true;
                        }
                    }

                    //Are they still in sight?
                    //Is the target in FOV
                    if (currentFOV.CheckTileFOV(lastSpellTarget.LocationMap.x, lastSpellTarget.LocationMap.y) && !charmed)
                    {
                        //If so, attack
                        LogFile.Log.LogEntryDebug("Recast at last target", LogDebugLevel.Medium);
                        return RecastSpellCastAtCreature(lastSpell, lastSpellTarget);
                    }

                    //If not, new target, fall through
                }
            }
            //Find the next closest creature (need to check charm / passive status)

            //Need to replace this one with find closest hostile creature in FOV.
            //The nearest creature might not be in FOV but a further away one might be

            lastSpellTarget = Game.Dungeon.FindClosestHostileCreatureInFOV(Game.Dungeon.Player);

            if (lastSpellTarget == null)
            {
                Game.MessageQueue.AddMessage("No target in sight.");
                LogFile.Log.LogEntryDebug("No new target for quick cast", LogDebugLevel.Medium);
                return false;
            }
            /*
            //Check they are in FOV
            if (!currentFOV.CheckTileFOV(lastSpellTarget.LocationMap.x, lastSpellTarget.LocationMap.y))
            {
                LogFile.Log.LogEntryDebug("No targets in FOV", LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("No target in sight.");

                return false;
            }*/

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

            Screen.Instance.Update();

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

            Screen.Instance.Update();

            if (selectedSpell == -1)
                return null;

            return knownSpells[selectedSpell];
        }

        /// <summary>
        /// Let the user target something
        /// </summary>
        /// <returns></returns>
        private bool TargetAttack(out Point target, int range, TargettingType targetType, double spreadAngle, char confirmChar, CreatureFOV currentFOV)
        {
            Player player = Game.Dungeon.Player;

            //Start on the nearest creature
            Creature closeCreature = Game.Dungeon.FindClosestHostileCreatureInFOV(player);

            //If no nearby creatures, start on the player
            if (closeCreature == null)
                closeCreature = Game.Dungeon.Player;

            Point startPoint;

            if (Utility.TestRange(Game.Dungeon.Player, closeCreature, range))
            {
                startPoint = new Point(closeCreature.LocationMap.x, closeCreature.LocationMap.y);
            }
            else
            {
                startPoint = new Point(player.LocationMap.x, player.LocationMap.y);
            }
            /*
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
            */
            //Get the desired target from the player

            return GetTargetFromPlayer(startPoint, out target, targetType, range, spreadAngle, confirmChar, currentFOV);
        }

        /// <summary>
        /// Gets a target from the player. false showed an escape. otherwise target is the target selected.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private bool GetTargetFromPlayer(Point start, out Point target, TargettingType type, int range, double spreadAngle, char confirmChar, CreatureFOV currentFOV)
        {
            //Turn targetting mode on the screen
            Screen.Instance.TargettingModeOn();
            Screen.Instance.Target = start;
            Screen.Instance.TargetType = type;
            Screen.Instance.TargetRange = range;
            Screen.Instance.TargetPermissiveAngle = spreadAngle;

            if (Utility.TestRangeFOVForWeapon(Game.Dungeon.Player, start, range, currentFOV))
            {
                Screen.Instance.SetTargetInRange = true;
            }
            else
                Screen.Instance.SetTargetInRange = false;

            //Update the last creature looked at
            //initial start square
            SquareContents sqC = Game.Dungeon.MapSquareContents(Game.Dungeon.Player.LocationLevel, start);
            Screen.Instance.CreatureToView = sqC.monster;
            if (sqC.items.Count > 0)
                Screen.Instance.ItemToView = sqC.items[0];
            else
                Screen.Instance.ItemToView = null;
            // (may reset to null)

            Game.MessageQueue.AddMessage("Find a target. " + confirmChar + " to confirm. ESC to exit.");
            Screen.Instance.Update();

            bool keepLooping = true;
            bool validFire = false;

            target = start;

            do
            {
                //Get direction from the user or 'z' to fire
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                Point direction = new Point();
                KeyModifier mod = KeyModifier.Arrow;
                bool validDirection = false;


                if (GetDirectionFromKeypress(userKey, out direction, out mod))
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
                        if(keyCode == confirmChar) {

                                validFire = true;
                                keepLooping = false;
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

                    if (Utility.TestRangeFOVForWeapon(Game.Dungeon.Player, newPoint, range, currentFOV))
                    {
                        Screen.Instance.SetTargetInRange = true;
                    }
                    else
                        Screen.Instance.SetTargetInRange = false;

                    //Update the last creature looked at
                    sqC = Game.Dungeon.MapSquareContents(Game.Dungeon.Player.LocationLevel, target);
                    Screen.Instance.CreatureToView = sqC.monster;
                    if(sqC.items.Count > 0)
                        Screen.Instance.ItemToView = sqC.items[0];
                    else
                        Screen.Instance.ItemToView = null;
                    // (may reset to null)

                    //Update screen
                    Screen.Instance.Target = newPoint;
                    Game.MessageQueue.AddMessage("Find a target. " + confirmChar + " to confirm. ESC to exit.");
                    Screen.Instance.Update();

                }
            } while (keepLooping);

            //Turn targetting mode off
            Screen.Instance.TargettingModeOff();
            Screen.Instance.Update();

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
        static public bool PickUpItem()
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

                //Place in an equipment slot and drop the old item
                player.EquipAndReplaceItem(itemToPickUp);
            }
            else
            {
                //Add item to PC inventory
                //Better on player
                player.PickUpItem(itemToPickUp);

                //Play help movie
                if (Game.Dungeon.Player.PlayItemMovies && Game.Dungeon.Player.TempItemHelpMovieSeen == false)
                {
                    Screen.Instance.PlayMovie("helptempitems", true);
                    Game.Dungeon.Player.TempItemHelpMovieSeen = true;
                }


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

        /// <summary>
        /// Setup internal systems
        /// </summary>
        /// <returns></returns>
        public void SetupSystem()
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
            LogFile.Log.DebugLevel = 4;

            //Setup message queue
            Game.MessageQueue = new MessageQueue();
        }

        public bool SetupGame()
        {

            //Intro screen pre-game (must come after screen)


            //  GameIntro intro = new GameIntro();
            //  intro.ShowIntroScreen();

            string playerName = "Dave";
            bool showMovies = false;
            GameDifficulty diff = GameDifficulty.Easy;
            /*

            string playerName = "Dave";
            bool showMovies = true;
            GameDifficulty diff = GameDifficulty.Easy;
            */

            //Setup dungeon

            //Is there a save game to load?
            //   if (Utility.DoesSaveGameExist(playerName))
            //    {
            //         LoadGame(playerName);
            //         return true;
            //    }
            //    else {

            //If not, make a new dungeon for the new player
            //Dungeon really contains all the state, so also sets up player etc.

            dungeonMaker = new DungeonMaker(diff);
            Game.Dungeon = dungeonMaker.SpawnNewDungeon();

            Game.Dungeon.Player.Name = playerName;
            Game.Dungeon.Player.PlayItemMovies = showMovies;
            Game.Dungeon.Difficulty = diff;

            //Do final player player
            Game.Dungeon.Player.StartGameSetup();

            //Move the player to the start location, triggering any triggers etc.
            Game.Dungeon.MoveToFirstMission();

            //Game.Dungeon.MovePCAbsolute(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap.x, Game.Dungeon.Player.LocationMap.y);

            return false;
            //      }

            //Fall into the main loop
        }


    }
}
