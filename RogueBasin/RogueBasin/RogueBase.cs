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
    public class RogueBase
    {
        //Are we running or have we exited?
        //public bool runMapLoop = true;

        public enum InputState
        {
            MapMovement, Targetting, InventoryShow, InventorySelect,
            YesNoPrompt, FPrompt,
            MovieDisplay, PreMapMovement, SpecialScreen
        }


        public enum ActionState
        {
            Running,
            Interactive
        }

        /// <summary>
        /// State determining what functions keys have
        /// </summary>
        InputState inputState = InputState.MapMovement;
        ActionState actionState = ActionState.Interactive;

        Action<bool> promptAction = null;

        Point runningDirection;
        IEnumerable<Point> runningPath;

        Targetting targetting;

        TargettingAction mouseDefaultTargettingAction = TargettingAction.MoveOrWeapon;

        Creature lastSpellTarget = null;


        public RogueBase()
        {
        }

        /// <summary>
        /// This sets up some child classes, who use Game.Dungeon and Game.Dungeon.Player in there constructors as a
        /// way past the global state
        /// </summary>
        public void Initialise()
        {
            targetting = new Targetting(this);
        }

        public bool PlaySounds { get; set; }
        public bool PlayMusic { get; set; }

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

            SetupSDLDotNetEvents();
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

        private void SetupSDLDotNetEvents()
        {
            Events.Quit += new EventHandler<QuitEventArgs>(ApplicationQuitEventHandler);
            Events.Tick += new EventHandler<TickEventArgs>(ApplicationTickEventHandler);
            Events.KeyboardUp += new EventHandler<KeyboardEventArgs>(KeyboardEventHandler);
            Events.MusicFinished += new EventHandler<MusicFinishedEventArgs>(MusicFinishedEventHandler);
            Events.MouseButtonUp += new EventHandler<MouseButtonEventArgs>(MouseButtonHandler);
            Events.MouseMotion += new EventHandler<MouseMotionEventArgs>(MouseMotionHandler);
        }

        private void MusicFinishedEventHandler(object sender, MusicFinishedEventArgs e)
        {
            LogFile.Log.LogEntryDebug("In music end call back", LogDebugLevel.High);
            MusicPlayer.Instance().Play();
        }

        public void StartEventLoop()
        {
            Events.Run();
        }

        bool firstRun = true;

        public void InitializeScreen()
        {
            Screen.Instance.NeedsUpdate = true;
            Screen.Instance.CenterViewOnPoint(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap);
        }

        private void ApplicationQuitEventHandler(object sender, QuitEventArgs args)
        {
            //Do any final cleanup
            LogFile.Log.Close();

            Events.QuitApplication();
        }

        bool waitingForTurnTick = true;

        public bool GameStarted { get; set; }

        private void ApplicationTickEventHandler(object sender, TickEventArgs args)
        {
            if (!Game.Dungeon.RunMainLoop)
            {
                Events.QuitApplication();
                return;
            }

            //ProfileEntry("Tick Event");

            //LogFile.Log.LogEntryDebug("FPS: " + args.Fps, LogDebugLevel.Medium);
            //LogFile.Log.LogEntryDebug("FPS tick: " + args.Tick, LogDebugLevel.Medium);

            if(GameStarted)
                AdvanceDungeonToNextPlayerTick();

            //ProfileEntry("Tick Update Film");

            /*
            if (GameStarted && firstRun)
            {
                //Should be called as a one-off earlier
                InitializeScreen();
                firstRun = false;
            }*/

            Screen.Instance.Update(args.TicksElapsed);

        }

        private void MouseButtonHandler(object sender, MouseButtonEventArgs args)
        {
            //Dungeon click must complete before we take more input
            if (waitingForTurnTick && GameStarted)
            {
                return;
            }

            if (actionState != ActionState.Interactive)
            {
                return;
            }

            bool timeAdvances = DoPlayerNextAction(actionState, null, args, null);

            if (timeAdvances)
            {
                ProfileEntry("After user (mouse)");

                Game.Dungeon.PlayerHadBonusTurn = true;

                waitingForTurnTick = true;
            }
        }

        private void MouseMotionHandler(object sender, MouseMotionEventArgs args)
        {
            //Dungeon click must complete before we take more input
            if (waitingForTurnTick && GameStarted)
            {
                return;
            }

            if (actionState != ActionState.Interactive)
            {
                return;
            }

            bool timeAdvances = DoPlayerNextAction(actionState, null, null, args);

            if (timeAdvances)
            {
                ProfileEntry("After user (mouse)");

                Game.Dungeon.PlayerHadBonusTurn = true;

                waitingForTurnTick = true;
            }
        }

        private void KeyboardEventHandler(object sender, KeyboardEventArgs args)
        {

            //Dungeon click must complete before we take more input
            if (waitingForTurnTick && GameStarted)
            {
                return;
            }

            if (actionState != ActionState.Interactive)
            {
                return;
            }

            bool timeAdvances = DoPlayerNextAction(actionState, args, null, null);
            
            if (timeAdvances)
            {
                ProfileEntry("After user keyboard");

                Game.Dungeon.PlayerHadBonusTurn = true;

                waitingForTurnTick = true;
            }
        }


        public void AdvanceDungeonToNextPlayerTick()
        {
            //Game time
            //Normal creatures have a speed of 100
            //This means it takes 100 ticks for them to take a turn (10,000 is the cut off)

            //Check PC
            //Take a turn if signalled by the internal clock

            //Loop through creatures
            //If their internal clocks signal another turn then take one

            var dungeon = Game.Dungeon;
            var player = Game.Dungeon.Player;

            bool playerNotReady = true;

            if (!waitingForTurnTick)
            {
                return;
            }

            ProfileEntry("Dungeon Turn");

            Game.Dungeon.ResetCreatureFOVOnMap();
            Game.Dungeon.ResetSoundOnMap();

            while (playerNotReady)
            {
                try
                {
                    //If we want to give the PC an extra go for any reason before the creatures
                    //(e.g. has just loaded, has just entered dungeon)

                    bool pcFreeTurn = false;
                    if (!Game.Dungeon.PlayerHadBonusTurn && Game.Dungeon.PlayerBonusTurn)
                        pcFreeTurn = true;

                    //Advance time in the dungeon
                    if (!pcFreeTurn)
                        DungeonActions();

                    //Advance time for the PC
                    playerNotReady = !PlayerPrepareForNextTurn();

                    //Catch the player being killed
                    //if (!Game.Dungeon.RunMainLoop)
                    //   break;
                }
                catch (Exception ex)
                {
                    LogFile.Log.LogEntry("Exception thrown" + ex.Message);
                }
            }

            waitingForTurnTick = false;

            if (actionState == ActionState.Running)
            {
                //If the player is running, take their turn immediately without waiting for input
                DoPlayerNextAction(actionState, null, null, null);
                Game.Dungeon.PlayerHadBonusTurn = true;
                waitingForTurnTick = true;
            }

            //Render any updates to monster FoV or dungeon sounds (debug)
            ShowFOVAndSoundsOnMap();

            //Player has taken turn so update screen
            Screen.Instance.NeedsUpdate = true;

            if (PlayMusic)
            {
                if (!MusicPlayer.Instance().Initialised)
                    MusicPlayer.Instance().Play();
            }

            //Play any enqueued sounds - pre player
           
            if(PlaySounds)
                SoundPlayer.Instance().PlaySounds();
        }

        private bool DoPlayerNextAction(ActionState action, KeyboardEventArgs args, MouseButtonEventArgs mouseArgs, MouseMotionEventArgs mouseMotionArgs)
        {
            var player = Game.Dungeon.Player;
            try
            {
                //Deal with PCs turn as appropriate
                Tuple<bool, bool> inputResult;

                bool timeAdvances = false;
                bool centreOnPC = false;

                switch (action)
                {
                    case ActionState.Running:
                        timeAdvances = RunNextStep();
                        centreOnPC = true;
                        break;

                    case ActionState.Interactive:

                        if (args != null)
                        {
                            inputResult = PlayerAction(action, args);
                        }
                        else if(mouseArgs != null)
                        {
                            inputResult = PlayerAction(action, mouseArgs);
                        }
                        else 
                        {
                            inputResult = PlayerAction(action, mouseMotionArgs);
                        }

                        timeAdvances = inputResult.Item1;
                        centreOnPC = inputResult.Item2;
                        break;
                }

                if (centreOnPC)
                {
                    Screen.Instance.CenterViewOnPoint(player.LocationLevel, player.LocationMap);
                }

                //Some events affect FoV but don't advance time, so recalculate here
                if (GameStarted)
                {
                    RecalculatePlayerFOV();
                }

                //Currently update on all keypresses
                Screen.Instance.NeedsUpdate = true;

                //Play any enqueued sounds
                if (PlaySounds)
                    SoundPlayer.Instance().PlaySounds();

                return timeAdvances;
            }

            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Exception thrown" + ex.Message);
            }

            return false;
        }

        private Tuple<bool, bool> PlayerAction(ActionState action, MouseMotionEventArgs mouseArgs)
        {
            bool timeAdvances = false;
            bool centreOnPC = false;

            var clickLocation = Screen.Instance.PixelToCoord(mouseArgs.Position);

            switch (inputState)
            {
                case InputState.Targetting:
                    timeAdvances = TargettingMouseMotionEvent(clickLocation);
                    break;

                //Normal movement on the map
                case InputState.MapMovement:

                    HandleMapMovementMouseMotion(clickLocation, mouseArgs);
                    break;
            }

            return new Tuple<bool, bool>(timeAdvances, centreOnPC);
        }

        private Tuple<bool, bool> PlayerAction(ActionState action, MouseButtonEventArgs mouseArgs)
        {
            var clickLocation = Screen.Instance.PixelToCoord(mouseArgs.Position);

            bool timeAdvances = false;
            bool centreOnPC = false;

            switch (inputState)
            {
                case InputState.Targetting:
                    timeAdvances = TargettingMouseEvent(clickLocation, mouseArgs.Button);
                    break;
                    
                //Normal movement on the map
                case InputState.MapMovement:

                    timeAdvances = HandleMapMovementClick(clickLocation, mouseArgs.Button);
                    break;

                case InputState.MovieDisplay:

                    MovieDisplayMouseEvent(mouseArgs);
                    break;
            }

            return new Tuple<bool, bool>(timeAdvances, centreOnPC);
        }

        private bool TargettingMouseEvent(Point clickLocation, MouseButton mouseButton)
        {
            bool timeAdvances = false;

            //If we clicked where we clicked before, it's confirmation
            //If we clicked elsewhere, it's a retarget, motion will take care of this
            if (clickLocation == targetting.CurrentTarget)
            {
                timeAdvances = ExecuteTargettedAction(false);
            }

            return timeAdvances;
        }

        private bool TargettingMouseMotionEvent(Point clickLocation)
        {
            bool timeAdvances = false;

            targetting.RetargetSquare(clickLocation);
            
            return timeAdvances;
        }

        private bool HandleMapMovementClick(Point clickLocation, MouseButton mouseButtons)
        {
            if (mouseButtons == MouseButton.PrimaryButton)
            {

                bool shifted = false;

                var keyboardState = new KeyboardState();
                if (keyboardState.IsKeyPressed(Key.LeftShift) || keyboardState.IsKeyPressed(Key.RightShift))
                {
                    shifted = true;
                }

                return ExecuteTargettedAction(shifted);
            }
            else
            {
                if (mouseDefaultTargettingAction == TargettingAction.MoveOrWeapon)
                {
                    mouseDefaultTargettingAction = TargettingAction.MoveOrThrow;
                    MouseFocusOnMap(clickLocation);
                }
                else
                {
                    mouseDefaultTargettingAction = TargettingAction.MoveOrWeapon;
                    MouseFocusOnMap(clickLocation);
                }

                return false;
            }
        }

        public void UpdateMapTargetting()
        {
            var mousePosition = SdlDotNet.Input.Mouse.MousePosition;
            var mouseLocation = Screen.Instance.PixelToCoord(mousePosition);

            MouseFocusOnMap(mouseLocation);

            LogFile.Log.LogEntryDebug("mouse pos: " + new Point(mousePosition), LogDebugLevel.High);
        }

        public void MouseFocusOnMap(Point mousePosition)
        {
            bool shifted = false;

            if (targetting == null)
            {
                return;
            }

            var keyboardState = new KeyboardState();
            if (keyboardState.IsKeyPressed(Key.LeftShift) || keyboardState.IsKeyPressed(Key.RightShift))
            {
                shifted = true;
            }

            if (mouseDefaultTargettingAction == TargettingAction.MoveOrWeapon)
            {
                targetting.TargetMoveOrFireInstant(mousePosition, shifted);
            }
            else
            {
                targetting.TargetMoveOrThrowInstant(mousePosition, shifted);
            }
        }
        
        private void HandleMapMovementMouseMotion(Point moveLocation, MouseMotionEventArgs mouseArgs)
        {
            MouseFocusOnMap(moveLocation);
        }
        
        bool PlayerPrepareForNextTurn()
        {
            //PC turn
            Player player = Game.Dungeon.Player;

            try
            {
                //Increment time on the PC's events and turn time (all done in IncrementTurnTime)
                if (Game.Dungeon.Player.IncrementTurnTime())
                {
                    LogFile.Log.LogEntryDebug("Player taking turn at tick " + player.TurnClock + " (world " + Game.Dungeon.WorldClock + ")", LogDebugLevel.Low);

                    //Remove dead players! Restart mission. Do this here so we don't get healed then beaten up again in our old state
                    if (Game.Dungeon.PlayerDeathOccured)
                    {
                        Game.Dungeon.PlayerDeath(Game.Dungeon.PlayerDeathString);
                    }

                    //For effects that end to update the screen correctly etc.
                    Game.Dungeon.Player.PreTurnActions();

                    //Check the 'on' status of special moves - now unnecessary?
                    //Game.Dungeon.CheckSpecialMoveValidity();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Exception thrown" + ex.Message);
                return true;
            }
        }

        private CreatureFOV RecalculatePlayerFOV()
        {
            ProfileEntry("Pre PC POV");

            //Calculate the player's FOV
            var playerFOV = Game.Dungeon.CalculatePlayerFOV();

            //Do any targetting maintenance
            if (Screen.Instance.TargetSelected())
                CheckTargetInPlayerFOV(playerFOV);

            return playerFOV;
        }

        private void ShowFOVAndSoundsOnMap()
        {

            ProfileEntry("Pre Monster POV");

            //Debug: show the FOV of all monsters. Should flag or comment this for release.
            //This is extremely slow, so restricting to debug mode
            if (Screen.Instance.DebugMode)
            {
                foreach (Monster monster in Game.Dungeon.Monsters)
                {
                    Game.Dungeon.ShowCreatureFOVOnMap(monster);
                }

                Game.Dungeon.ShowSoundsOnMap();
            }

            ProfileEntry("Post Monster POV");
        }
        
        private void DungeonActions()
        {
            //Monsters turn

            //Increment world clock
            Game.Dungeon.IncrementWorldClock();

            //ProfileEntry("Pre event");

            //Increment time on all global (dungeon) events
            //Game.Dungeon.IncrementEventTime();

            //All creatures get IncrementTurnTime() called on them each worldClock tick
            //They internally keep track of when they should take another turn

            //IncrementTurnTime() also increments time for all events on that creature

            //ProfileEntry("Pre monster");

            foreach (Item item in Game.Dungeon.Items)
            {
                //Only process items on the same level as the player
                if (item.LocationLevel == Game.Dungeon.Player.LocationLevel)
                {
                    item.IncrementTurnTime();
                }
            }

            foreach (Monster creature in Game.Dungeon.Monsters)
            {
                try
                {
                    //Only process creatures on the same level as the player
                    if (creature.LocationLevel == Game.Dungeon.Player.LocationLevel)
                    {
                        if (creature.IncrementTurnTime())
                        {
                            //Creatures may be killed by other creatures so check they are alive before processing
                            if (creature.Alive)
                            {
                                LogFile.Log.LogEntryDebug("Creature " + creature.Representation + " taking turn at tick " + creature.TurnClock + " (world " + Game.Dungeon.WorldClock + ")", LogDebugLevel.Low);
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

        private void CheckTargetInPlayerFOV(CreatureFOV playerFOV)
        {
            var targetCreature = Screen.Instance.CreatureToView;
            var targetItem = Screen.Instance.ItemToView;
            var targetFeature = Screen.Instance.FeatureToView;

            if (targetCreature == null && targetItem == null && targetFeature == null)
                return;

            if(targetCreature != null) {
                if(targetCreature.LocationLevel != Game.Dungeon.Player.LocationLevel) {
                    ResetViewPanel();
                    return;
                }
                if (!playerFOV.CheckTileFOV(targetCreature.LocationMap))
                {
                    ResetViewPanel();
                    return;
                }
            }

            if (targetItem != null)
            {
                if (targetItem.LocationLevel != Game.Dungeon.Player.LocationLevel)
                {
                    ResetViewPanel();
                    return;
                }
                if (!playerFOV.CheckTileFOV(targetItem.LocationMap))
                {
                    ResetViewPanel();
                    return;
                }
            }

            if (targetFeature != null)
            {
                if (targetFeature.LocationLevel != Game.Dungeon.Player.LocationLevel)
                {
                    ResetViewPanel();
                    return;
                }
                if (!playerFOV.CheckTileFOV(targetFeature.LocationMap))
                {
                    ResetViewPanel();
                    return;
                }
            }
        }

        private void ProfileEntry(string p)
        {
            if(Game.Dungeon.Profiling)
                LogFile.Log.LogEntryDebug(p + " " + DateTime.Now.Millisecond.ToString(), LogDebugLevel.Profiling);
        }

        //Return code is if the command was successful and time increments (i.e. the player has done a time-using command like moving)
        private Tuple<bool, bool> PlayerAction(ActionState action, KeyboardEventArgs args)
        {
            bool timeAdvances = false;
            bool centreOnPC = false;

            //Only on key up
            
            try
            {
                var result = ActionOnKeypress(args);
                timeAdvances = result.Item1;
                centreOnPC = result.Item2;
            }
            catch (Exception ex)
            {
                //This should catch most exceptions that happen as a result of user commands
                MessageBox.Show("Exception occurred: " + ex.Message + " but continuing on anyway");
                LogFile.Log.LogEntryDebug("Exception occurred: " + ex.Message + "\n" + ex.StackTrace, LogDebugLevel.High);
            }
            return new Tuple<bool, bool>(timeAdvances, centreOnPC);
        }

        private Tuple<bool, bool> ActionOnKeypress(KeyboardEventArgs args)
        {
            bool timeAdvances = false;
            bool centreOnPC = false;

            //Only on key up
            if (args.Down)
            {
                return new Tuple<bool, bool>(false, false);
            }

            //Each interactive state has different keys
            switch (inputState)
            {
                case InputState.Targetting:
                    timeAdvances = TargettingKeyboardEvent(args);
                    break;

                case InputState.YesNoPrompt:
                    YesNoPromptKeyboardEvent(args);
                    break;

                case InputState.FPrompt:
                    FPromptKeyboardEvent(args);
                    break;

                case InputState.MovieDisplay:
                    MovieDisplayKeyboardEvent(args);
                    break;

                case InputState.SpecialScreen:
                    SpecialScreenKeyboardEvent(args);
                    break;

                //Normal movement on the map
                case InputState.MapMovement:

                    if (args.Mod.HasFlag(ModifierKeys.LeftShift) || args.Mod.HasFlag(ModifierKeys.RightShift))
                    {
                        switch (args.Key)
                        {
                            case Key.F:
                                //Full screen switch
                                timeAdvances = false;

                                break;

                            case Key.Q:
                                //Exit from game
                                timeAdvances = false;
                                YesNoQuestion("Really quit?", (result) =>
                                {
                                    if (result)
                                    {
                                        Game.Dungeon.PlayerDeath("quit");
                                        timeAdvances = true;
                                    }
                                });

                                break;

                            case Key.S:
                                //Toggle sounds
                                ToggleSounds();
                                break;

                            case Key.M:
                                ToggleMusic();

                                break;

                            case Key.N:
                                SetMsgHistoryScreen();
                                DisableMsgHistoryScreen();
                                timeAdvances = false;
                                break;

                            case Key.C:
                                SetClueScreen();
                                DisableClueScreen();
                                timeAdvances = false;
                                break;

                            case Key.Slash:
                                Game.Base.PlayMovie("helpkeys", true);
                                Game.Base.PlayMovie("qe_start", true);

                                timeAdvances = false;
                                break;
                        }
                    }

                    if (!args.Mod.HasFlag(ModifierKeys.LeftShift) && !args.Mod.HasFlag(ModifierKeys.RightShift))
                    {

                        switch (args.Key)
                        {
                            case Key.F:
                                //Fire weapon
                                if (Game.Dungeon.Player.GetEquippedRangedWeapon() == null)
                                    break;

                                if (Game.Dungeon.Player.GetEquippedRangedWeapon().HasFireAction())
                                {
                                    targetting.TargetWeapon();
                                    timeAdvances = false;
                                }

                                if (timeAdvances)
                                    SpecialMoveNonMoveAction();

                                centreOnPC = true;
                                break;

                            case Key.T:
                                //Use utility (throw or operate)
                                if (Game.Dungeon.Player.GetEquippedUtility() == null)
                                    break;

                                if (Game.Dungeon.Player.GetEquippedUtility().HasThrowAction())
                                {
                                    targetting.TargetThrowUtility();
                                    timeAdvances = false;
                                }
                                else if (Game.Dungeon.Player.GetEquippedUtility().HasOperateAction())
                                {
                                    timeAdvances = UseUtility();
                                }

                                if (timeAdvances)
                                    SpecialMoveNonMoveAction();

                                centreOnPC = true;
                                break;

                            case Key.E:
                                Game.Dungeon.Player.EquipNextUtilityInventoryItem(-1);
                                centreOnPC = true;
                                break;

                            case Key.R:
                                Game.Dungeon.Player.EquipNextUtilityInventoryItem(1);
                                centreOnPC = true;
                                break;

                            case Key.A:
                                Game.Dungeon.Player.SelectNextWetwareInventoryItem(-1);
                                centreOnPC = true;
                                break;

                            case Key.S:
                                Game.Dungeon.Player.SelectNextWetwareInventoryItem(1);
                                centreOnPC = true;
                                break;

                            case Key.W:
                                Game.Dungeon.Player.EquipSelectedWetware();
                                centreOnPC = true;
                                break;

                            case Key.X:
                                //Examine
                                timeAdvances = Examine();

                                if (timeAdvances)
                                    SpecialMoveNonMoveAction();

                                centreOnPC = true;
                                break;

                            case Key.Period:
                                // Do nothing
                                timeAdvances = DoNothing();
                                // Don't recentre - useful for viewing
                                centreOnPC = false;
                                break;
                        }
                    }


                    if (Game.Config.DebugMode)
                    {
                        if (args.Mod.HasFlag(ModifierKeys.LeftShift) || args.Mod.HasFlag(ModifierKeys.RightShift))
                        {
                            switch (args.Key)
                            {
                                case Key.R:
                                    //Reload
                                    Game.Dungeon.Player.RefillWeapons();
                                    break;

                                case Key.K:
                                    if (!Game.Dungeon.AllLocksOpen)
                                    {
                                        Game.Dungeon.AllLocksOpen = true;
                                        Game.MessageQueue.AddMessage("All locks are now open.");
                                    }
                                    else
                                    {
                                        Game.Dungeon.AllLocksOpen = false;
                                        Game.MessageQueue.AddMessage("All locks are now in their normal state.");
                                    }
                                    break;

                                case Key.I:
                                    Game.MessageQueue.AddMessage("Player levelled up.");
                                    Game.Dungeon.Player.LevelUp();
                                    break;


                                case Key.N:
                                    //screen numbering
                                    Screen.Instance.CycleRoomNumbering();
                                    break;

                                case Key.W:
                                    //screen debug mode
                                    Screen.Instance.DebugMode = !Screen.Instance.DebugMode;
                                    if (Screen.Instance.DebugMode)
                                        Game.MessageQueue.AddMessage("Screen debug mode on.");
                                    else
                                        Game.MessageQueue.AddMessage("Screen debug mode off.");
                                    timeAdvances = true; //So full FoV is re-rendered
                                    break;

                                case Key.B:
                                    //screen see all mode
                                    Screen.Instance.SeeAllMap = Screen.Instance.SeeAllMap ? false : true;
                                    Screen.Instance.SeeAllMonsters = Screen.Instance.SeeAllMonsters ? false : true;
                                    Screen.Instance.NeedsUpdate = true;
                                    break;

                                case Key.Y:
                                    //next level
                                    Game.Dungeon.MoveToLevel(Game.Dungeon.Player.LocationLevel + 1);
                                    centreOnPC = true;
                                    timeAdvances = true;
                                    break;

                                case Key.G:
                                    //last level
                                    Game.Dungeon.MoveToLevel(Game.Dungeon.Player.LocationLevel - 1);
                                    centreOnPC = true;
                                    timeAdvances = true;
                                    break;

                                case Key.J:
                                    //change debug level
                                    LogFile.Log.DebugLevel += 1;
                                    if (LogFile.Log.DebugLevel > 3)
                                        LogFile.Log.DebugLevel = 1;

                                    LogFile.Log.LogEntry("Log Debug level now: " + LogFile.Log.DebugLevel.ToString());

                                    break;

                                case Key.H:
                                    //Add a healing event on the player
                                    Game.Dungeon.Player.HealCompletely();
                                    Game.Dungeon.Player.RefillWeapons();
                                    break;

                                case Key.T:
                                    Game.MessageQueue.AddMessage("Giving all low level weapons & wetware.");
                                    Game.Dungeon.Player.GiveAllWeapons(1);
                                    Game.Dungeon.Player.GiveAllWetware(2);
                                    Game.Dungeon.Player.EquipNextUtility();
                                    break;

                                case Key.U:
                                    Game.Dungeon.Player.GiveAllWeapons(2);
                                    Game.Dungeon.Player.GiveAllWetware(3);
                                    break;

                                case Key.V:
                                    Game.Dungeon.KillAllMonstersOnLevel(Game.Dungeon.Player.LocationLevel);
                                    Game.MessageQueue.AddMessage("Boom");
                                    break;
                            }
                        }
                    }


                    //OLD EVENTS

                    /*
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
                    */

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
                    //No longer needed

                    /*
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
                    */
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

                    /*
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
                    break;*/
                    /*
                case 'u':
                    //Use weapon
                    timeAdvances = UseWeapon();
                    if (!timeAdvances)
                        Screen.Instance.Update();
                    if (timeAdvances)
                        SpecialMoveNonMoveAction();
                    break;*/



                    /*
                case '>':
                case '<':
                    //Interact with feature
                    timeAdvances = Game.Dungeon.InteractWithFeature();
                    if (!timeAdvances)
                        Screen.Instance.Update();

                    if (timeAdvances)
                        SpecialMoveNonMoveAction();

                    break;*/

                    //WETWARE
                    /*case 'S':
                        timeAdvances = Game.Dungeon.Player.ToggleEquipWetware(typeof(Items.ShieldWare));
                        break;

                    case 'D':
                        timeAdvances = Game.Dungeon.Player.ToggleEquipWetware(typeof(Items.StealthWare));
                        break;

                    case 'A':
                        timeAdvances = Game.Dungeon.Player.ToggleEquipWetware(typeof(Items.AimWare));
                        break;
                        */
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

                    //Handle wetware

                    char keyCode = args.KeyboardCharacter[0];

                    foreach (var kv in ItemMapping.WetwareMapping)
                    {
                        if (keyCode == kv.Key)
                        {
                            bool changeWorks = Game.Dungeon.Player.ToggleEquipWetware(kv.Value);

                            if (changeWorks)
                            {
                                //We changed wetware, counts as an action
                                centreOnPC = true;
                            }

                            break;
                        }
                    }


                    //Handle weapons

                    int numberPressed = GetNumberFromNonKeypadKeyPress(args);
                    if (numberPressed != -1)
                    {
                        foreach (var kv in ItemMapping.WeaponMapping)
                        {
                            if (numberPressed == kv.Key)
                            {
                                timeAdvances = Game.Dungeon.Player.EquipInventoryItemType(kv.Value);
                                centreOnPC = true;
                                break;
                            }
                        }
                    }

                    //Handle direction keys (both arrows and vi keys)

                    Point direction = new Point(9, 9);
                    KeyModifier mod = KeyModifier.Arrow;
                    bool wasDirection = GetDirectionFromKeypress(args, out direction, out mod);

                    if (wasDirection && (mod == KeyModifier.Numeric || mod == KeyModifier.Vi) && !(args.Mod.HasFlag(ModifierKeys.LeftShift) || args.Mod.HasFlag(ModifierKeys.RightShift) || args.Mod.HasFlag(ModifierKeys.LeftControl) || args.Mod.HasFlag(ModifierKeys.RightControl) || args.Mod.HasFlag(ModifierKeys.LeftAlt) || args.Mod.HasFlag(ModifierKeys.RightAlt)))
                    {
                        timeAdvances = TimeAdvancesOnMove(Game.Dungeon.PCMove(direction.x, direction.y));
                        centreOnPC = true;
                    }

                    if (wasDirection && (mod == KeyModifier.Numeric) && (args.Mod.HasFlag(ModifierKeys.LeftShift) || args.Mod.HasFlag(ModifierKeys.RightShift)))
                    {
                        timeAdvances = StartRunning(direction.x, direction.y);
                        centreOnPC = true;
                    }

                    if (wasDirection && mod == KeyModifier.Arrow && !(args.Mod.HasFlag(ModifierKeys.LeftControl) || args.Mod.HasFlag(ModifierKeys.RightControl)))
                    {
                        Screen.Instance.ViewportScrollSpeed = 4;
                        Screen.Instance.ScrollViewport(direction);
                        centreOnPC = false;
                    }

                    if (Game.Config.DebugMode)
                    {
                        if (wasDirection && mod == KeyModifier.Arrow && (args.Mod.HasFlag(ModifierKeys.LeftControl) || args.Mod.HasFlag(ModifierKeys.RightControl)))
                        {
                            if (direction == new Point(0, -1))
                            {
                                ScreenLevelUp();
                                centreOnPC = false;
                            }

                            if (direction == new Point(0, 1))
                            {
                                ScreenLevelDown();
                                centreOnPC = false;
                            }
                        }
                    }


                    break;
            }

            return new Tuple<bool, bool>(timeAdvances, centreOnPC);
        }

        private bool TimeAdvancesOnMove(MoveResults moveResults)
        {
            switch (moveResults)
            {
                case MoveResults.AttackedMonster:
                    return true;
                case MoveResults.InteractedWithFeature:
                    return true;
                case MoveResults.InteractedWithObstacle:
                    return false;
                case MoveResults.NormalMove:
                    return true;
                case MoveResults.StoppedByObstacle:
                    return false;
                case MoveResults.SwappedWithMonster:
                    return true;
            }

            return true;
        }

        private bool StartRunning(int directionX, int directionY)
        {
            runningDirection = new Point(directionX, directionY);
            actionState = ActionState.Running;

            return RunNextStep();
        }

        private bool StartRunning(IEnumerable<Point> path)
        {
            runningDirection = null;
            runningPath = path;
            actionState = ActionState.Running;

            return RunNextStep();
        }

        public void StopRunning()
        {
            actionState = ActionState.Interactive;
        }

        private bool RunNextStep()
        {
            Point relativeNextStep = new Point(0,0);

            if (runningPath != null && runningPath.Any())
            {
                relativeNextStep = runningPath.ElementAt(0) - Game.Dungeon.Player.LocationMap;
                runningPath = runningPath.Skip(1);

                if (!runningPath.Any())
                {
                    //Reached end of path
                    StopRunning();
                }
            }
            else if (runningPath != null && runningPath.Any())
            {
                //Empty path
                StopRunning();
            }
            else if (runningPath == null)
            {
                relativeNextStep = new Point(runningDirection.x, runningDirection.y);
            }

            if (relativeNextStep == new Point(0, 0))
            {
                LogFile.Log.LogEntryDebug("Can't run onto yourself", LogDebugLevel.High);
                StopRunning();
            }

            MoveResults results = Game.Dungeon.PCMove(relativeNextStep.x, relativeNextStep.y);

            switch (results) { 
                case MoveResults.AttackedMonster:
                    StopRunning();
                    break;
                case MoveResults.InteractedWithFeature:
                    StopRunning();
                    break;
                case MoveResults.InteractedWithObstacle:
                    StopRunning();
                    break;
                case MoveResults.NormalMove:
                    break;
                case MoveResults.StoppedByObstacle:
                    StopRunning();
                    break;
                case MoveResults.SwappedWithMonster:
                    break;
            }

            return TimeAdvancesOnMove(results);
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

        public void DoArenaSelection()
        {
            this.SetSpecialScreenAndHandler(Screen.Instance.ArenaSelectionScreen, ArenaSelectionKeyHandler);
            SetupArenaSelection();
        }

        public void DoFunModeDeath()
        {
            this.SetSpecialScreenAndHandler(Screen.Instance.FunModeDeathScreen, FunModeDeathKeyHandler);
        }

        private void SetupArenaSelection()
        {
            Screen.Instance.ArenaItems = Game.Dungeon.Items.Where(i => i.LocationLevel == Game.Dungeon.Player.LocationLevel);
            Screen.Instance.ArenaMonsters = Game.Dungeon.Monsters.Where(m => m.LocationLevel == Game.Dungeon.Player.LocationLevel);
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

        private Boolean FunMode { get; set; }

        public void CharacterSelectionKeyHandler(KeyboardEventArgs args)
        {

            if (args.Key == Key.One)
            {
                PostCharacterSelection(PlayerClass.Athlete, FunMode);
            }
            if (args.Key == Key.Two)
            {
                PostCharacterSelection(PlayerClass.Gunner, FunMode);
            }
            if (args.Key == Key.Three)
            {
                PostCharacterSelection(PlayerClass.Sneaker, FunMode);
            }
            if (args.Key == Key.R)
            {
                FunMode = false;
                //Is reset next method, but used to toggle display
                Game.Dungeon.FunMode = false;
            }
            if (args.Key == Key.F)
            {
                FunMode = true;
                Game.Dungeon.FunMode = true;
            }
        }

        public void PostCharacterSelection(PlayerClass playerClass, bool funMode)
        {
            ClearSpecialScreenAndHandler();

            SetupGame();
            Game.Dungeon.Player.SetPlayerClass(playerClass);
            Game.Dungeon.FunMode = funMode;

            //Setup initial levels
            SetupRoyaleEntryLevels();

            Game.Dungeon.Player.LocationLevel = 0;
            Game.Dungeon.Player.LocationMap = Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation;

            PrepareGameEntry();

            //Follow on to initial arena selection
            DoArenaSelection();
        }

        public void ArenaSelectionKeyHandler(KeyboardEventArgs args)
        {
            if (args.Key == Key.LeftArrow)
            {
                Game.Dungeon.TeleportToAdjacentArena(false);
                SetupArenaSelection();
            }
            if (args.Key == Key.RightArrow)
            {
                Game.Dungeon.TeleportToAdjacentArena(true);
                SetupArenaSelection();
            }
            if (args.Key == Key.F)
            {
                ClearSpecialScreenAndHandler();

                StartGame();
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
            DoMenuScreen();
        }

        //Do menu screen, onto character selection
        public void DoMenuScreen()
        {
            var menuScreen = new MenuScreen(DoCharacterSelection);
            SetSpecialScreenAndHandler(menuScreen.DrawMenuScreen, menuScreen.MenuScreenKeyboardHandler);
        }

        public void DoCharacterSelection()
        {
            SetSpecialScreenAndHandler(Screen.Instance.CharacterSelectionScreen, CharacterSelectionKeyHandler);
            LogFile.Log.LogEntryDebug("Requesting character gen screen", LogDebugLevel.High);
        }

        private Action<KeyboardEventArgs> SpecialScreenKeyboardHandler { get; set; }

        private void SpecialScreenKeyboardEvent(KeyboardEventArgs args)
        {
            if (SpecialScreenKeyboardHandler != null)
            {
                SpecialScreenKeyboardHandler(args);
            }
            else
            {
                ClearSpecialScreenAndHandler();
            }
        }

        public void SetSpecialScreenAndHandler(Action specialScreen, Action<KeyboardEventArgs> specialScreenHandler) {
            Screen.Instance.SpecialScreen = specialScreen;
            SpecialScreenKeyboardHandler = specialScreenHandler;
            inputState = InputState.SpecialScreen;
        }

        public void ClearSpecialScreenAndHandler()
        {
            Screen.Instance.SpecialScreen = null;
            SpecialScreenKeyboardHandler = null;
            inputState = InputState.MapMovement;
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

        private void FPromptKeyboardEvent(KeyboardEventArgs args)
        {
            inputState = InputState.MapMovement;
            Screen.Instance.ClearPrompt();

            if (args.Key == Key.F)
            {
                if (promptAction != null)
                {
                    promptAction(true);

                }
                
            }
        }

        private bool TargettingKeyboardEvent(KeyboardEventArgs args)
        {
            Point direction = new Point(9, 9);
            KeyModifier mod = KeyModifier.Arrow;
            bool wasDirection = GetDirectionFromKeypress(args, out direction, out mod);
            bool validFire = false;
            bool escape = false;

            if (!wasDirection)
            {
                //Look for firing
                if (args.Key == Key.F || args.Key == Key.T || args.Key == Key.X)
                {
                    validFire = true;
                }

                if (args.Key == Key.Escape)
                {
                    escape = true;
                }
            }

            //If direction, update the location and redraw

            if (wasDirection)
            {
                Point newPoint = new Point(targetting.CurrentTarget.x + direction.x, targetting.CurrentTarget.y + direction.y);
                RetargetSquare(newPoint);
                
                return false;
            }

            if (validFire)
            {
                return ExecuteTargettedAction(false);
            }
            else
            {
                targetting.DisableTargettingMode();
            }

            return false;
        }

        private void RetargetSquare(Point newPoint)
        {
            int level = Game.Dungeon.Player.LocationLevel;

            if (newPoint.x < 0 || newPoint.x >= Game.Dungeon.Levels[level].width || newPoint.y < 0 || newPoint.y >= Game.Dungeon.Levels[level].height)
                return;

            targetting.RetargetSquare(newPoint);
        }

        private bool ExecuteTargettedAction(bool alternativeActionMode)
        {
            bool timeAdvances = false;
            bool restoreExamine = true;
            Monster examineCreature = Screen.Instance.CreatureToView;
            Item examineItem = Screen.Instance.ItemToView;
            Feature examineFeature = Screen.Instance.FeatureToView;

            var player = Game.Dungeon.Player;

            //Turn targetting mode off
            targetting.DisableTargettingMode();

            //Complete actions
            switch (targetting.TargettingAction)
            {
                case TargettingAction.Weapon:

                    timeAdvances = FireTargettedWeapon();
                    break;

                case TargettingAction.Utility:

                    timeAdvances = ThrowTargettedUtility();
                    break;

                case TargettingAction.Examine:

                    restoreExamine = false;
                    break;

                case TargettingAction.MoveOrWeapon:
                    {
                        SquareContents squareContents = Game.Dungeon.MapSquareContents(Game.Dungeon.Player.LocationLevel, targetting.CurrentTarget);
                        if (squareContents.monster != null)
                        {
                            if (!alternativeActionMode)
                            {
                                timeAdvances = FireTargettedWeapon();
                            }
                            else
                            {
                                timeAdvances = RunToDestination();
                            }
                        }
                        else
                        {
                            if (!alternativeActionMode)
                            {
                                timeAdvances = RunToDestination();
                            }
                            else
                            {
                                timeAdvances = FireTargettedWeapon();
                            }
                        }
                    }
                    break;

                case TargettingAction.MoveOrThrow:
                    {
                        SquareContents squareContents = Game.Dungeon.MapSquareContents(Game.Dungeon.Player.LocationLevel, targetting.CurrentTarget);
                        if (squareContents.monster != null)
                        {
                            if (!alternativeActionMode)
                            {
                                timeAdvances = ThrowTargettedUtility();
                            }
                            else
                            {
                                timeAdvances = RunToDestination();
                            }
                        }
                        else
                        {
                            if (!alternativeActionMode)
                            {
                                timeAdvances = RunToDestination();
                            }
                            else
                            {
                                timeAdvances = ThrowTargettedUtility();
                            }
                        }
                    }
                    break;

                case TargettingAction.Move:
                    timeAdvances = RunToDestination();
                    break;
            }
            
            if (restoreExamine)
            {
                Screen.Instance.CreatureToView = examineCreature;
                Screen.Instance.ItemToView = examineItem;
                Screen.Instance.FeatureToView = examineFeature;
            }

            return timeAdvances;
        }
        

        private bool RunToDestination()
        {
            var player = Game.Dungeon.Player;

            IEnumerable<Point> path = player.GetPlayerRunningPath(targetting.CurrentTarget);

            if (!path.Any())
            {
                return false;
            }
            
            return StartRunning(path);
        }

        private bool ThrowTargettedUtility()
        {
            var player = Game.Dungeon.Player;
            var timeAdvances = ThrowTargettedUtility(targetting.CurrentTarget);
            player.ResetTurnsMoving();
            player.ResetTurnsSinceAction();
            return timeAdvances;
        }

        private bool FireTargettedWeapon()
        {
            var player = Game.Dungeon.Player;
            var timeAdvances = FireTargettedWeapon(targetting.CurrentTarget);
            player.ResetTurnsMoving();
            player.ResetTurnsSinceAction();
            return timeAdvances;
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

        private bool DoNothing()
        {
            return TimeAdvancesOnMove(Game.Dungeon.PCMove(0, 0));
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

        private static void RemoveEffectsDueToThrowing(Player player, Item toThrow)
        {
            //Some items permit stealth
            if (toThrow is Items.SoundGrenade)
                return;

            if (toThrow is Items.AcidGrenade)
                return;

            player.RemoveEffect(typeof(PlayerEffects.StealthBoost));
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

        private bool ThrowTargettedUtility(Point target)
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            IEquippableItem toThrow = player.GetEquippedUtility();
            Item toThrowItem = player.GetEquippedUtilityAsItem();

            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

            if (toThrow == null)
            {
                Game.MessageQueue.AddMessage("No throwable weapon!");
                LogFile.Log.LogEntryDebug("No throwable weapon", LogDebugLevel.Medium);
                return false;
            }

            int range = toThrow.RangeThrow();

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
            RemoveEffectsDueToThrowing(player, toThrowItem);

            //Play any audio
            toThrow.ThrowAudio();

            //Destroy it if required
            if (toThrow.DestroyedOnThrow())
            {
                player.UnequipAndDestroyItem(toThrowItem);

                //Try to reequip another item 
                //player.EquipInventoryItemType(toThrow.GetType());
                player.EquipNextUtility();

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
                    List<Point> freeSqs = dungeon.GetWalkableAdjacentSquaresFreeOfCreatures(player.LocationLevel, destinationSq);

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
        
        private bool FireTargettedWeapon(Point target) {

            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            IEquippableItem weapon = player.GetEquippedRangedWeapon();
            Item weaponI = player.GetEquippedRangedWeaponAsItem();

            if (weapon == null)
            {
                Game.MessageQueue.AddMessage("No weapon to fire");
                LogFile.Log.LogEntryDebug("No weapon to fire", LogDebugLevel.Medium);
            }

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
            int range = weapon.RangeFire();
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

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
                RemoveEffectsDueToFiringWeapon(player);
            }

            //Ditch empty weapons
            /*
            if (weapon.RemainingAmmo() < 1)
            {
                Game.MessageQueue.AddMessage("This " + (weapon as Item).SingleItemDescription + " is all out of ammo! Ditching it!");
                LogFile.Log.LogEntryDebug("Out of range for " + weaponI.SingleItemDescription, LogDebugLevel.Medium);
                player.UnequipAndDestroyItem(weapon as Item);
                player.GivePistol();
            }*/

            //Store details for a recast

            //If we successful, store the target
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

            if (success)
            {
                weapon.FireAudio();
            }

            //Time only goes past if successful
            return success;
        }

        private static void RemoveEffectsDueToFiringWeapon(Player player)
        {
            //player.RemoveEffect(typeof(PlayerEffects.StealthBoost));
            //player.RemoveEffect(typeof(PlayerEffects.SpeedBoost));
            player.CancelBoostDueToAttack();
            player.CancelStealthDueToAttack();
        }



        public void SetInputState(InputState newState)
        {
            inputState = newState;
        }
        
        public SquareContents SetViewPanelToTargetAtSquare(Point start)
        {
            SquareContents sqC = Game.Dungeon.MapSquareContents(Game.Dungeon.Player.LocationLevel, start);
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

        public void SetupRoyaleEntryLevels()
        {
            Game.Dungeon.SetupRoyaleEntryLevels();
        }

        private void PreArenaEntryState() {
            inputState = InputState.PreMapMovement;
            //Screen.Instance.SeeAllMap = true;
            //Screen.Instance.SeeAllMonsters = true;
        }

        private void PostArenaEntryState()
        {
            inputState = InputState.MapMovement;
            //Screen.Instance.SeeAllMap = false;
            //Screen.Instance.SeeAllMonsters = false;
        }

        /// <summary>
        /// Player starts a new level and can choose an arena
        /// </summary>
        /// <param name="level"></param>
        internal void PlayerStartsLevel(int level)
        {
            //Input state where the user can switch levels
            PreArenaEntryState();

            LogFile.Log.LogEntryDebug("Player starts level " + level, LogDebugLevel.Medium);
        }

        /// <summary>
        /// Player actually enters the arena
        /// </summary>
        /// <param name="level"></param>
        internal void PlayerEntersLevel(int level)
        {
            //Normal input state
            PostArenaEntryState();

            //If we are nerd, activate stealth
            if(Game.Dungeon.Player.PlayerClass == PlayerClass.Sneaker) {
                Game.Dungeon.Player.ToggleEquipWetware(typeof(Items.StealthWare));
            }

            LogFile.Log.LogEntryDebug("Player enters level " + level, LogDebugLevel.Medium);
        }

        /// <summary>
        /// Player exists the arena successfully
        /// </summary>
        /// <param name="level"></param>
        internal void PlayerExitsLevel(int level)
        {
            //Switching level state
            PreArenaEntryState();

            LogFile.Log.LogEntryDebug("Player exists level " + level, LogDebugLevel.Medium);

            Game.Dungeon.ExitLevel();
        }

        public void PrepareGameEntry()
        {
            PlayerStartsLevel(0);

        }

        internal void DoEndOfGame(bool lived, bool won, bool quit)
        {
            Screen.Instance.EndOfGameWon = won;
            Screen.Instance.EndOfGameQuit = quit;

            GameStarted = false;

            this.SetSpecialScreenAndHandler(Screen.Instance.EndOfGameScreen, EndOfGameSelectionKeyHandler);
        }

        internal void SetupFunModeDeath()
        {
            this.SetSpecialScreenAndHandler(Screen.Instance.FunModeDeathScreen, FunModeDeathKeyHandler);
        }

        internal void QuitImmediately()
        {
            Game.Dungeon.RunMainLoop = false;
            Events.QuitApplication();
        }
    }
}
