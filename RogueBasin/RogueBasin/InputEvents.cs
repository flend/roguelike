using SdlDotNet.Audio;
using SdlDotNet.Core;
using SdlDotNet.Input;
using System;

namespace RogueBasin
{
    public class InputEvents
    {
        private GameTick gameTick;
        private InputHandler inputHandler;

        public InputEvents(GameTick gameTick, InputHandler inputHandler)
        {
            this.gameTick = gameTick;
            this.inputHandler = inputHandler;
        }

        public void SetupSDLDotNetEvents()
        {
            Events.Quit += new EventHandler<QuitEventArgs>(ApplicationQuitEventHandler);
            Events.Tick += new EventHandler<TickEventArgs>(ApplicationTickEventHandler);
            Events.KeyboardUp += new EventHandler<KeyboardEventArgs>(KeyboardEventHandler);
            Events.KeyboardDown += new EventHandler<KeyboardEventArgs>(KeyboardEventHandler);
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


        private void ProfileEntry(string p)
        {
            if (Game.Dungeon.Profiling)
                LogFile.Log.LogEntryDebug(p + " " + DateTime.Now.Millisecond.ToString(), LogDebugLevel.Profiling);
        }

        private void ApplicationQuitEventHandler(object sender, QuitEventArgs args)
        {
            //Do any final cleanup
            LogFile.Log.Close();

            Events.QuitApplication();
        }

        public void QuitApplication()
        {
            Events.QuitApplication();
        }

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

            if (Game.Base.GameStarted)
                gameTick.AdvanceDungeonToNextPlayerTick();

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
            if (gameTick.WaitingForTurnTick && Game.Base.GameStarted)
            {
                return;
            }

            if (inputHandler.ActionState != ActionState.Interactive)
            {
                return;
            }

            bool timeAdvances = inputHandler.DoPlayerNextAction(null, args, null, null);

            if (timeAdvances)
            {
                ProfileEntry("After user (mouse)");

                Game.Dungeon.PlayerHadBonusTurn = true;

                gameTick.WaitingForTurnTick = true;
            }
        }

        private void MouseMotionHandler(object sender, MouseMotionEventArgs args)
        {
            //Dungeon click must complete before we take more input
            if (gameTick.WaitingForTurnTick && Game.Base.GameStarted)
            {
                return;
            }

            if (inputHandler.ActionState != ActionState.Interactive)
            {
                return;
            }

            bool timeAdvances = inputHandler.DoPlayerNextAction(null, null, args, null);

            if (timeAdvances)
            {
                ProfileEntry("After user (mouse)");

                Game.Dungeon.PlayerHadBonusTurn = true;

                gameTick.WaitingForTurnTick = true;
            }
        }

        private void KeyboardEventHandler(object sender, KeyboardEventArgs args)
        {

            //Dungeon click must complete before we take more input
            if (gameTick.WaitingForTurnTick && Game.Base.GameStarted)
            {
                return;
            }

            if (inputHandler.ActionState != ActionState.Interactive)
            {
                return;
            }

            bool timeAdvances = inputHandler.DoPlayerNextAction(args, null, null, null);

            if (timeAdvances)
            {
                ProfileEntry("After user keyboard");

                Game.Dungeon.PlayerHadBonusTurn = true;

                gameTick.WaitingForTurnTick = true;
            }
        }
    }
}
