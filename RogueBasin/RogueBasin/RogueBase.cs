using System;

using libtcodWrapper;
using Console = System.Console;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.IO.Compression;
using System.Linq;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Input;
using SdlDotNet.Audio;

namespace RogueBasin
{

    public struct ActionResult
    {
        public readonly bool timeAdvances;
        public readonly bool centreOnPC;

        public ActionResult(bool timeAdvances, bool centreOnPC)
        {
            this.timeAdvances = timeAdvances;
            this.centreOnPC = centreOnPC;
        }
    }

    public enum ActionState
    {
        Running,
        Interactive
    }

    public class RogueBase
    {

        private InputEvents events;



        Action<bool> promptAction = null;



        public bool PlaySounds { get; set; }
        public bool PlayMusic { get; set; }
        private Boolean FunMode { get; set; }



        public bool GameStarted { get; set; }
        
        public RogueBase()
        {
        }

        public void Initialise()
        {
            events = new InputEvents();
            events.SetupSDLDotNetEvents();

            events.Initialise();
        }
        /// <summary>
        /// Setup internal systems
        /// </summary>
        /// <returns></returns>
        public void SetupSystem()
        {
            //Initial setup
            //See all debug messages
            LogFile.Log.DebugLevel = 0;

            //Load config
            Game.Config = new Config("config.txt");

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

            //Setup message queue
            Game.MessageQueue = new MessageQueue();

            SetupFromConfig();

        }

        public void SetupGame()
        {
            var dungeonInfo = new DungeonInfo();
            Game.Dungeon = new Dungeon(dungeonInfo);

            Game.Dungeon.Player.StartGameSetup();

            Game.Dungeon.Difficulty = GameDifficulty.Medium;
            Game.Dungeon.Player.Name = "Gladiator";
            Game.Dungeon.Player.PlayItemMovies = false;

            Game.Dungeon.AllLocksOpen = false;

            if(Game.Config.DebugMode)
                Game.Dungeon.PlayerImmortal = true;


            //Game.Dungeon.FunMode = true;

            ResetScreen();
        }

        private static void ResetScreen()
        {
            Screen.Instance.CreatureToView = null;
            Screen.Instance.ItemToView = null;
            Screen.Instance.FeatureToView = null;

            Game.MessageQueue.ClearList();
        }

        private void SetupFromConfig()
        {
            if(Game.Config.Sound)
            {   
                PlaySounds = true;
            }

            if(Game.Config.Music)
                PlayMusic = true;
        }


        public void InitializeScreen()
        {
            Screen.Instance.NeedsUpdate = true;
            Screen.Instance.CenterViewOnPoint(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap);
        }




        
        public void ToggleSounds()
        {
            if (PlaySounds)
            {
                SoundsOff();
            }
            else
            {
                SoundsOn();
            }
        }

        private void SoundsOn()
        {
            Game.MessageQueue.AddMessage("Sounds on");
            PlaySounds = true;
        }

        private void SoundsOff()
        {
            Game.MessageQueue.AddMessage("Sounds off");
            PlaySounds = false;
        }

        public void ToggleMusic()
        {
            if (PlayMusic)
            {
                MusicStop();
            }
            else
            {
                MusicStart();
            }
        }

        private void MusicStart()
        {
            Game.MessageQueue.AddMessage("Music on");
            MusicPlayer.Instance().Play();
            PlayMusic = true;
        }

        private void MusicStop()
        {
            Game.MessageQueue.AddMessage("Music off");
            MusicPlayer.Instance().Stop();
            PlayMusic = false;
        }
        
        private void MovieDisplayKeyboardEvent(KeyboardEventArgs args)
        {
            if (args.Key == Key.Return)
            {
                FinishMovie();
            }
        }

        private void FinishMovie()
        {
            //Finish movie
            Screen.Instance.DequeueFirstMovie();
            //Out of movie mode if no more to display
            if (!Screen.Instance.MoviesToPlay())
                inputState = InputState.MapMovement;
        }

        private void MovieDisplayMouseEvent(MouseButtonEventArgs args)
        {
            if (args.Button == MouseButton.PrimaryButton)
            {
                FinishMovie();
            }
        }

        public void StartGame()
        {
            Initialise();

            GameStarted = true;
            Screen.Instance.ShowMessageQueue = true;

            Game.Dungeon.CalculatePlayerFOV();
            Screen.Instance.CenterViewOnPoint(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap);

            Screen.Instance.NeedsUpdate = true;
        }

        public void FunModeDeathKeyHandler(KeyboardEventArgs args)
        {
            if (args.Key == Key.F)
            {
                ClearSpecialScreenAndHandler();
                ResetScreen();
                StartGame();
            }
        }

        public void EndOfGameSelectionKeyHandler(KeyboardEventArgs args)
        {
            if (args.Key == Key.Return)
            {
                ClearSpecialScreenAndHandler();
                RestartGameAfterDeath();
            }
        }

        private void RestartGameAfterDeath()
        {
            SetupGame();
        }


        private void YesNoPromptKeyboardEvent(KeyboardEventArgs args)
        {
            if (args.Key == Key.Y)
            {
                if (promptAction != null)
                {
                    promptAction(true);
                    ResetPrompt();
                }
            }

            if (args.Key == Key.N)
            {
                if (promptAction != null)
                {
                    promptAction(false);
                    ResetPrompt();
                }
            }
        }

        private void ResetPrompt()
        {
            //Only reset input state if called function doesn't set it to something else
            if (inputState == InputState.YesNoPrompt)
                inputState = InputState.MapMovement;
            Screen.Instance.ClearPrompt();
        }


        private void ScreenLevelDown()
        {
            if (Screen.Instance.LevelToDisplay > 0)
                Screen.Instance.LevelToDisplay--;
        }

        private void ScreenLevelUp()
        {
            if (Screen.Instance.LevelToDisplay < Game.Dungeon.NoLevels - 1)
                Screen.Instance.LevelToDisplay++;

        }

        private ActionResult DoNothing()
        {
            return Utility.TimeAdvancesOnMove(Game.Dungeon.PCMove(0, 0));
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

                //newDungeon.Features = readData.features;
                newDungeon.Items = readData.items;
                newDungeon.Effects = readData.effects;
                newDungeon.Monsters = readData.monsters;
                newDungeon.Spells = readData.spells;
                newDungeon.Player = readData.player;
                newDungeon.SpecialMoves = readData.specialMoves;
                newDungeon.WorldClock = readData.worldClock;
                newDungeon.HiddenNameInfo = readData.hiddenNameInfo;
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
                newDungeon.RefreshAllLevelPathingAndFOV();

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
            //KeyPress userKey = libtcodWrapper.Keyboard.WaitForKeyPress(true);

            if (GetDirectionFromKeypress(null, out direction, out mod))
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

        private int GetNumberFromNonKeypadKeyPress(KeyboardEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Zero:
                    return 0;

                case Key.One:
                    return 1;

                case Key.Two:
                    return 2;

                case Key.Three:
                    return 3;

                case Key.Four:
                    return 4;

                case Key.Five:
                    return 5;

                case Key.Six:
                    return 6;

                case Key.Seven:
                    return 7;

                case Key.Eight:
                    return 8;

                case Key.Nine:
                    return 9;
            }

            return -1;
        }

        /// <summary>
        /// Get a direction from a keypress. Will return false if not valid. Otherwise in parameter.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private bool GetDirectionFromKeypress(KeyboardEventArgs args, out Point direction, out KeyModifier mod)
        {

            direction = new Point(9, 9);
            mod = KeyModifier.Arrow;

            //Vi keys for directions
            switch (args.Key)
            {
                case Key.B:
                    direction = new Point(-1, 1);
                    mod = KeyModifier.Vi;
                    break;

                case Key.N:
                    direction = new Point(1, 1);
                    mod = KeyModifier.Vi;
                    break;

                case Key.Y:
                    direction = new Point(-1, -1);
                    mod = KeyModifier.Vi;
                    break;

                case Key.U:
                    direction = new Point(1, -1);
                    mod = KeyModifier.Vi;
                    break;

                case Key.H:
                    direction = new Point(-1, 0);
                    mod = KeyModifier.Vi;
                    break;

                case Key.L:
                    direction = new Point(1, 0);
                    mod = KeyModifier.Vi;
                    break;

                case Key.K:
                    direction = new Point(0, -1);
                    mod = KeyModifier.Vi;
                    break;

                case Key.J:
                    direction = new Point(0, 1);
                    mod = KeyModifier.Vi;
                    break;


                //Arrow keys for directions


                case Key.Keypad1:
                    direction = new Point(-1, 1);
                    mod = KeyModifier.Numeric;
                    break;

                case Key.Keypad3:
                    direction = new Point(1, 1);
                    mod = KeyModifier.Numeric;
                    break;

                case Key.KeypadPeriod:
                    direction = new Point(0, 0);
                    mod = KeyModifier.Arrow;
                    break;

                case Key.Keypad5:
                    direction = new Point(0, 0);
                    mod = KeyModifier.Numeric;
                    break;

                case Key.Keypad7:
                    direction = new Point(-1, -1);
                    mod = KeyModifier.Numeric;
                    break;
                case Key.Keypad9:
                    direction = new Point(1, -1);
                    mod = KeyModifier.Numeric;
                    break;

                case Key.LeftArrow:
                    direction = new Point(-1, 0);
                    mod = KeyModifier.Arrow;
                    break;

                case Key.Keypad4:
                    direction = new Point(-1, 0);
                    mod = KeyModifier.Numeric;
                    break;
                case Key.RightArrow:
                    direction = new Point(1, 0);
                    mod = KeyModifier.Arrow;
                    break;
                case Key.Keypad6:
                    direction = new Point(1, 0);
                    mod = KeyModifier.Numeric;
                    break;
                case Key.UpArrow:
                    direction = new Point(0, -1);
                    mod = KeyModifier.Arrow;
                    break;
                case Key.Keypad8:
                    direction = new Point(0, -1);
                    mod = KeyModifier.Numeric;
                    break;
                case Key.Keypad2:
                    direction = new Point(0, 1);
                    mod = KeyModifier.Numeric;
                    break;
                case Key.DownArrow:
                    direction = new Point(0, 1);
                    mod = KeyModifier.Arrow;
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
            Screen.Instance.Update(0);

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
            Screen.Instance.Update(0);

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
            Screen.Instance.Update(0);

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
            Screen.Instance.Update(0);

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


        private void SetMsgHistoryScreen()
        {
            Screen.Instance.ShowMsgHistory = true;
        }

        private void DisableMsgHistoryScreen()
        {
            Screen.Instance.ShowMsgHistory = false;
        }

        private void SetClueScreen()
        {
            Screen.Instance.ShowClueList = true;
        }

        private void DisableClueScreen()
        {
            Screen.Instance.ShowClueList = false;
        }

        private void SetLogScreen()
        {
            Screen.Instance.ShowLogList = true;
        }

        private void DisableLogScreen()
        {
            Screen.Instance.ShowLogList = false;
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
                toUse = player.GetEquippedRangedWeapon();
                toUseItem = player.GetEquippedRangedWeaponAsItem();
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

            if (success && toUse.DestroyedOnUse()) { 
                //Destroy the item
                player.UnequipAndDestroyItem(toUseItem);
                player.EquipNextUtility();
            };

            return success;
            
        }




        /// <summary>
        /// Examine using the target. Returns if time passes.
        /// </summary>
        /// <returns></returns>
        private bool Examine()
        {
            targetting.TargetExamine();
            return false;
        }        




        public void SetInputState(InputState newState)
        {
            inputState = newState;
        }
        
        public SquareContents SetViewPanelToTargetAtSquare(Location start)
        {
            SquareContents sqC = Game.Dungeon.MapSquareContents(start);
            Screen.Instance.CreatureToView = sqC.monster; //may reset to null
            if (sqC.items.Count > 0)
                Screen.Instance.ItemToView = sqC.items[0];
            else
                Screen.Instance.ItemToView = null;

            Screen.Instance.FeatureToView = sqC.feature; //may reset to null
            return sqC;
        }

        private void ResetViewPanel()
        {
            Screen.Instance.ResetViewPanel();
        }

        public void YesNoQuestion(string introMessage, Action<bool> action)
        {
            Screen.Instance.SetPrompt(introMessage + " (y / n):");
            Screen.Instance.NeedsUpdate = true;

            inputState = InputState.YesNoPrompt;
            promptAction = action;
        }

        public void FPrompt(string introMessage, Action<bool> action)
        {
            Screen.Instance.SetPrompt(introMessage);
            Screen.Instance.NeedsUpdate = true;

            inputState = InputState.FPrompt;
            promptAction = action;
        }


        public void PlayLog(LogEntry logEntry)
        {
            try
            {
                var movieFrames = new List<MovieFrame>();
                
                var logFrame = new MovieFrame();
                var allLines = new List<string>();
                allLines.Add(logEntry.title);
                allLines.AddRange(logEntry.lines);
                logFrame.ScanLines = allLines;

                movieFrames.Add(logFrame);
                var movie = new Movie(movieFrames);

                PlayMovie(movie, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to play movie from frames " + ex.Message);
            }
        }

        public void PlayMovie(string filename, bool keypressBetweenFrames)
        {
            inputState = InputState.MovieDisplay;
            Screen.Instance.EnqueueMovie(filename);
            Screen.Instance.NeedsUpdate = true;
        }

        public void PlayMovie(Movie movie, bool keypressBetweenFrames)
        {
            inputState = InputState.MovieDisplay;
            Screen.Instance.EnqueueMovie(movie);
            Screen.Instance.NeedsUpdate = true;
        }
        
        internal void DoEndOfGame(bool lived, bool won, bool quit)
        {
            Screen.Instance.EndOfGameWon = won;
            Screen.Instance.EndOfGameQuit = quit;

            GameStarted = false;

            this.SetSpecialScreenAndHandler(Screen.Instance.EndOfGameScreen, EndOfGameSelectionKeyHandler);
        }

        internal void QuitImmediately()
        {
            Game.Dungeon.RunMainLoop = false;
            Events.QuitApplication();
        }

        public void NotifyMonsterEvent(MonsterEvent monsterEvent)
        {
            switch (monsterEvent.EventType)
            {
                case MonsterEvent.MonsterEventType.MonsterAttacksPlayer:

                    running.StopRunning();
                    
                    if (!Screen.Instance.TargetSelected())
                    {
                        Screen.Instance.CreatureToView = monsterEvent.Monster;
                    }

                    break;

                case MonsterEvent.MonsterEventType.MonsterSeenByPlayer:

                    running.StopRunning();

                    Game.MessageQueue.AddMessage("You see a " + monsterEvent.Monster.SingleDescription + ".");

                    break;
            }
        }

        public void SimulateMouseEventInCurrentPosition()
        {
            DoPlayerNextAction(null, null, null, new CustomInputArgs(CustomInputArgsActions.MouseMoveToCurrentLocation));
        }
    }
}
