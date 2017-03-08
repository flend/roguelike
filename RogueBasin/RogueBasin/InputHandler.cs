using SdlDotNet.Input;
using SdlDotNet.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    class InputHandler
    {
        public enum InputState
        {
            MapMovement, Targetting, InventoryShow, InventorySelect,
            YesNoPrompt, FPrompt,
            MovieDisplay, PreMapMovement, SpecialScreen
        }

        InputState inputState = InputState.MapMovement;
        ActionState actionState = ActionState.Interactive;

        Targetting targetting;
        Running running;
        PlayerActions playerActions;

        TargettingAction mouseDefaultTargettingAction = TargettingAction.MoveOrWeapon;

        private Point DragTracker { get; set; }
        private bool lastMouseActionWasDrag = false;
        private int mouseDragStartThreshold = 8;

        public ActionState ActionState { get { return actionState; } set { actionState = value; } }

        private Action<KeyboardEventArgs> SpecialScreenKeyboardHandler { get; set; }


        public InputHandler()
        {
            //Or we could inject these
            targetting = new Targetting(Game.Base);
            running = new Running(Game.Base, Game.Dungeon.Player);
            playerActions = new PlayerActions(running, targetting);
        }


        private ActionResult TargettingMouseEvent(Point clickLocation, MouseButton mouseButton)
        {
            //If we clicked where we clicked before, it's confirmation
            //If we clicked elsewhere, it's a retarget, motion will take care of this
            if (clickLocation == targetting.CurrentTarget.MapCoord)
            {
                return ExecuteTargettedAction(false);
            }

            return new ActionResult(false, false);
        }

        private ActionResult TargettingMouseMotionEvent(Point clickLocation)
        {
            targetting.RetargetSquare(new Location(Screen.Instance.LevelToDisplay, clickLocation));

            return new ActionResult(false, false);
        }

        private ActionResult HandleMapMovementClick(Point clickLocation, MouseButton mouseButtons)
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

                return new ActionResult(false, false);
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

            var gameLocation = new Location(Screen.Instance.LevelToDisplay, mousePosition);

            if (mouseDefaultTargettingAction == TargettingAction.MoveOrWeapon)
            {
                targetting.TargetMoveOrFireInstant(gameLocation, shifted);
            }
            else
            {
                targetting.TargetMoveOrThrowInstant(gameLocation, shifted);
            }
        }



        private void HandleMapMovementMouseMotion(Point moveLocation, MouseMotionEventArgs mouseArgs)
        {
            ResetDragTracker();

            MouseFocusOnMap(moveLocation);
        }

        private void ResetDragTracker()
        {
            LogFile.Log.LogEntryDebug("Drag tracker reset", LogDebugLevel.Low);
            DragTracker = new Point(0, 0);
        }

        private bool HandleMapMovementMouseDrag(MouseMotionEventArgs mouseArgs)
        {
            var thisDrag = new Point(mouseArgs.RelativeX, mouseArgs.RelativeY);
            var newDragTotal = DragTracker + thisDrag;
            LogFile.Log.LogEntryDebug("dragTracker: " + DragTracker, LogDebugLevel.Low);
            LogFile.Log.LogEntryDebug("newDragTotal: " + newDragTotal, LogDebugLevel.Low);

            if (!lastMouseActionWasDrag && Math.Abs(mouseArgs.RelativeX + mouseArgs.RelativeY) < mouseDragStartThreshold)
            {
                return false;
            }
            else
            {
                lastMouseActionWasDrag = true;
            }

            var relativeDrag = Screen.Instance.RelativePixelToRelativeCoord(newDragTotal, true);
            DragTracker = relativeDrag.remainder;
            LogFile.Log.LogEntryDebug("dragTracker after: " + DragTracker + " remainder:  " + relativeDrag.remainder, LogDebugLevel.High);

            Screen.Instance.ScrollViewport(relativeDrag.coord, 3);
            return true;
        }

        public bool DoPlayerNextAction(KeyboardEventArgs args, MouseButtonEventArgs mouseArgs, MouseMotionEventArgs mouseMotionArgs, CustomInputArgs customArgs)
        {
            var player = Game.Dungeon.Player;
            try
            {
                //Deal with PCs turn as appropriate
                ActionResult inputResult = new ActionResult(false, false);

                switch (actionState)
                {
                    case ActionState.Running:
                        inputResult = running.RunNextStep();
                        break;

                    case ActionState.Interactive:

                        if (args != null)
                        {
                            inputResult = PlayerAction(actionState, args);
                        }
                        else if (mouseArgs != null)
                        {
                            inputResult = PlayerAction(actionState, mouseArgs);
                        }
                        else if (mouseMotionArgs != null)
                        {
                            inputResult = PlayerAction(actionState, mouseMotionArgs);
                        }
                        else
                        {
                            if (customArgs.action == CustomInputArgsActions.MouseMoveToCurrentLocation)
                            {
                                //This action just updates the screen with the mouse's current position
                                var currentMousePosition = SdlDotNet.Input.Mouse.MousePosition;
                                var fakeMotionArgs = new MouseMotionEventArgs(false, MouseButton.None, (short)currentMousePosition.X, (short)currentMousePosition.Y, 0, 0);
                                inputResult = PlayerAction(actionState, fakeMotionArgs);
                            }
                            else
                            {
                                inputResult = new ActionResult(false, false);
                            }
                        }
                        break;
                }

                if (inputResult.centreOnPC)
                {
                    Screen.Instance.CenterViewOnPoint(player.LocationLevel, player.LocationMap);
                }

                //Some events affect FoV but don't advance time, so recalculate here
                if (Game.Base.GameStarted)
                {
                    RecalculatePlayerFOV();
                }

                //Currently update on all keypresses
                Screen.Instance.NeedsUpdate = true;

                //Play any enqueued sounds
                if (Game.Base.PlaySounds)
                    SoundPlayer.Instance().PlaySounds();

                return inputResult.timeAdvances;
            }

            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Exception thrown" + ex.Message);
            }

            return false;
        }

        //Duplicated from game tick
        private CreatureFOV RecalculatePlayerFOV()
        {
            //Calculate the player's FOV
            var playerFOV = Game.Dungeon.CalculatePlayerFOV();

            //Do any targetting maintenance
            if (Screen.Instance.TargetSelected())
                Screen.Instance.ResetViewPanelIfRequired(playerFOV);

            return playerFOV;
        }

        private ActionResult PlayerAction(ActionState action, MouseMotionEventArgs mouseArgs)
        {
            var clickLocation = Screen.Instance.PixelToCoord(mouseArgs.Position);

            LogFile.Log.LogEntryDebug("Action: mouseMotion: " + mouseArgs.ToString(), LogDebugLevel.Low);

            switch (inputState)
            {
                case InputState.Targetting:
                    return TargettingMouseMotionEvent(clickLocation);

                //Normal movement on the map
                case InputState.MapMovement:

                    if (mouseArgs.ButtonPressed && mouseArgs.Button == MouseButton.PrimaryButton)
                    {
                        var dragNotPositionCorrection = HandleMapMovementMouseDrag(mouseArgs);
                        if (!dragNotPositionCorrection)
                        {
                            lastMouseActionWasDrag = false;
                            HandleMapMovementMouseMotion(clickLocation, mouseArgs);
                        }
                    }
                    else
                    {
                        lastMouseActionWasDrag = false;
                        HandleMapMovementMouseMotion(clickLocation, mouseArgs);
                    }
                    break;
            }

            return new ActionResult(false, false);
        }

        private ActionResult PlayerAction(ActionState action, MouseButtonEventArgs mouseArgs)
        {
            var clickLocation = Screen.Instance.PixelToCoord(mouseArgs.Position);

            bool timeAdvances = false;
            bool centreOnPC = false;

            LogFile.Log.LogEntryDebug("Action: mouseButton: " + mouseArgs.ToString(), LogDebugLevel.Low);

            if (mouseArgs.ButtonPressed == true || lastMouseActionWasDrag)
            {
                lastMouseActionWasDrag = false;
                LogFile.Log.LogEntryDebug("Action: last action was drag so ignoring", LogDebugLevel.Low);
                return new ActionResult(false, false);
            }

            switch (inputState)
            {
                case InputState.Targetting:
                    return TargettingMouseEvent(clickLocation, mouseArgs.Button);
                    break;

                //Normal movement on the map
                case InputState.MapMovement:

                    return HandleMapMovementClick(clickLocation, mouseArgs.Button);

                case InputState.MovieDisplay:

                    MovieDisplayMouseEvent(mouseArgs);
                    break;
            }

            return new ActionResult(timeAdvances, centreOnPC);
        }

        //Return code is if the command was successful and time increments (i.e. the player has done a time-using command like moving)
        private ActionResult PlayerAction(ActionState action, KeyboardEventArgs args)
        {

            LogFile.Log.LogEntryDebug("Action: keyboard: " + args.ToString(), LogDebugLevel.Low);

            //Only on key up

            try
            {
                return ActionOnKeypress(args);
            }
            catch (Exception ex)
            {
                //This should catch most exceptions that happen as a result of user commands
                MessageBox.Show("Exception occurred: " + ex.Message + " but continuing on anyway");
                LogFile.Log.LogEntryDebug("Exception occurred: " + ex.Message + "\n" + ex.StackTrace, LogDebugLevel.High);
                return new ActionResult(false, false);
            }
        }

        private ActionResult ActionOnKeypress(KeyboardEventArgs args)
        {
            bool timeAdvances = false;
            bool centreOnPC = false;

            //Only on key up
            if (args.Down)
            {
                return new ActionResult(false, false);
            }

            //Each interactive state has different keys
            switch (inputState)
            {
                case InputState.Targetting:
                    var actionResult = TargettingKeyboardEvent(args);
                    timeAdvances = actionResult.timeAdvances;
                    centreOnPC = actionResult.centreOnPC;
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

                            case Key.C:
                                //Centre on player
                                Screen.Instance.CenterViewOnPoint(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap);
                                timeAdvances = false;
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
                                var nothingResult = DoNothing();
                                // Don't recentre - useful for viewing
                                timeAdvances = nothingResult.timeAdvances;
                                centreOnPC = nothingResult.centreOnPC;
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
                                    Game.MessageQueue.AddMessage("Cycling room numbering: " + Screen.Instance.ShowRoomNumbering);
                                    Screen.Instance.CycleRoomNumbering();
                                    break;

                                case Key.W:
                                    if (Screen.Instance.SeeDebugMarkers)
                                    {
                                        Screen.Instance.ClearDebugMarkers();
                                    }
                                    else
                                    {
                                        Screen.Instance.SetSeeDebugMarkers();
                                    }
                                    if (Screen.Instance.SeeDebugMarkers)
                                        Game.MessageQueue.AddMessage("Screen debug mode on.");
                                    else
                                        Game.MessageQueue.AddMessage("Screen debug mode off.");
                                    timeAdvances = true; //So full FoV is re-rendered
                                    break;

                                case Key.B:

                                    if (Screen.Instance.SeeAllMap)
                                    {
                                        Screen.Instance.ClearSeeAllMap();
                                    }
                                    else
                                    {
                                        Screen.Instance.SetSeeAllMap();
                                    }
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
                        var actionResultKey = Utility.TimeAdvancesOnMove(Game.Dungeon.PCMove(direction.x, direction.y));
                        timeAdvances = actionResultKey.timeAdvances;
                        centreOnPC = actionResultKey.centreOnPC;
                    }

                    if (wasDirection && (mod == KeyModifier.Numeric) && (args.Mod.HasFlag(ModifierKeys.LeftShift) || args.Mod.HasFlag(ModifierKeys.RightShift)))
                    {
                        var actionResultShifted = running.StartRunning(direction.x, direction.y);
                        timeAdvances = actionResultShifted.timeAdvances;
                        centreOnPC = actionResultShifted.centreOnPC;
                    }

                    if (wasDirection && mod == KeyModifier.Arrow && !(args.Mod.HasFlag(ModifierKeys.LeftControl) || args.Mod.HasFlag(ModifierKeys.RightControl)))
                    {
                        Screen.Instance.ScrollViewport(direction, 4);
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

            return new ActionResult(timeAdvances, centreOnPC);
        }


        public void SetSpecialScreenAndHandler(Action specialScreen, Action<KeyboardEventArgs> specialScreenHandler)
        {
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

        private ActionResult TargettingKeyboardEvent(KeyboardEventArgs args)
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
                Point newPoint = new Point(targetting.CurrentTarget.MapCoord.x + direction.x, targetting.CurrentTarget.MapCoord.y + direction.y);
                RetargetSquare(newPoint);

                return new ActionResult(false, false);
            }

            if (validFire)
            {
                return ExecuteTargettedAction(false);
            }
            else
            {
                targetting.DisableTargettingMode();
            }

            return new ActionResult(false, false);
        }

        private void RetargetSquare(Point pointFromKeyboard)
        {
            int level = Screen.Instance.LevelToDisplay;

            if (pointFromKeyboard.x < 0 || pointFromKeyboard.x >= Game.Dungeon.Levels[level].width || pointFromKeyboard.y < 0 || pointFromKeyboard.y >= Game.Dungeon.Levels[level].height)
                return;

            targetting.RetargetSquare(new Location(level, pointFromKeyboard));
        }

        private ActionResult ExecuteTargettedAction(bool alternativeActionMode)
        {
            ActionResult result = new ActionResult(false, false);
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

                    result = FireTargettedWeapon();
                    break;

                case TargettingAction.Utility:

                    result = ThrowTargettedUtility();
                    break;

                case TargettingAction.Examine:

                    restoreExamine = false;
                    break;

                case TargettingAction.MoveOrWeapon:
                    {
                        SquareContents squareContents = Game.Dungeon.MapSquareContents(targetting.CurrentTarget);
                        if (squareContents.monster != null)
                        {
                            if (!alternativeActionMode)
                            {
                                result = FireTargettedWeapon();
                            }
                            else
                            {
                                result = RunToTargettedDestination();
                            }
                        }
                        else
                        {
                            if (!alternativeActionMode)
                            {
                                result = RunToTargettedDestination();
                            }
                            else
                            {

                                result = FireTargettedWeapon();
                            }
                        }
                    }
                    break;

                case TargettingAction.MoveOrThrow:
                    {
                        SquareContents squareContents = Game.Dungeon.MapSquareContents(targetting.CurrentTarget);
                        if (squareContents.monster != null)
                        {
                            if (!alternativeActionMode)
                            {
                                result = ThrowTargettedUtility();
                            }
                            else
                            {
                                result = RunToTargettedDestination();
                            }
                        }
                        else
                        {
                            if (!alternativeActionMode)
                            {
                                result = RunToTargettedDestination();
                            }
                            else
                            {
                                result = ThrowTargettedUtility();
                            }
                        }
                    }
                    break;

                case TargettingAction.Move:
                    result = RunToTargettedDestination();
                    break;
            }

            if (restoreExamine)
            {
                Screen.Instance.CreatureToView = examineCreature;
                Screen.Instance.ItemToView = examineItem;
                Screen.Instance.FeatureToView = examineFeature;
            }

            return result;
        }
    }
}
