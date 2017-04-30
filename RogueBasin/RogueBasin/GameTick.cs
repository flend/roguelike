using System;

namespace RogueBasin
{
    public class GameTick
    {
        bool waitingForTurnTick = true;
        private InputHandler handler;

        public GameTick(InputHandler handler)
        {
            this.handler = handler;
        }

        public bool WaitingForTurnTick { get { return waitingForTurnTick; } set { waitingForTurnTick = value; } }

        private void ProfileEntry(string p)
        {
            if (Game.Dungeon.Profiling)
                LogFile.Log.LogEntryDebug(p + " " + DateTime.Now.Millisecond.ToString(), LogDebugLevel.Profiling);
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

            //If we are waiting on the user's input, do not advance the dungeon
            if (!waitingForTurnTick && handler.ActionState == RogueBasin.ActionState.Interactive)
            {
                return;
            }

            ProfileEntry("Dungeon Turn");

            Game.Dungeon.ResetCreatureFOVOnMap();
            Game.Dungeon.ResetSoundOnMap();

            //Run the dungeon to the player's next turn if required
            //Sometimes running does no-time events (like opening doors) - we want the next run step to continue
            if (waitingForTurnTick)
            {
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
            }

            waitingForTurnTick = false;

            if (handler.ActionState == ActionState.Running)
            {
                //If the player is running, take their turn immediately without waiting for input
                handler.DoPlayerNextAction(null, null, null, null);
                Game.Dungeon.PlayerHadBonusTurn = true;
                waitingForTurnTick = true;
            }

            //Render any updates to monster FoV or dungeon sounds (debug)
            ShowFOVAndSoundsOnMap();

            //Player has taken turn so update screen
            Screen.Instance.NeedsUpdate = true;

            if (Game.Base.PlayMusic)
            {
                if (!MusicPlayer.Instance().Initialised)
                    MusicPlayer.Instance().Play();
            }

            //Play any enqueued sounds - pre player

            if (Game.Base.PlaySounds)
                SoundPlayer.Instance().PlaySounds();
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
                Screen.Instance.ResetViewPanelIfRequired(playerFOV);

            return playerFOV;
        }


        private void ShowFOVAndSoundsOnMap()
        {

            ProfileEntry("Pre Monster POV");

            //Debug: show the FOV of all monsters. Should flag or comment this for release.
            //This is extremely slow, so restricting to debug mode
            if (Screen.Instance.SeeDebugMarkers)
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
    }
}
