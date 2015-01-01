using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Magic library
    /// </summary>
    public class LeaveByDock : DungeonSquareTrigger
    {

        public LeaveByDock()
        {

        }

        public override bool CheckTrigger(int level, Point mapLocation)
        {
            //Check we are in the right place - should be in the base I think
            if (CheckLocation(level, mapLocation) == false)
            {
                return false;
            }

            //This trigger won't fire until we have actually left
            if (Game.Dungeon.DungeonInfo.Dungeons[level].PlayerLeftDock == false)
                return false;

            //The player may want to abort the mission

            //Not finished yet
            if (Game.Dungeon.DungeonInfo.Dungeons[level].LevelObjectiveComplete == false)
            {
                bool decision = Screen.Instance.YesNoQuestion("ABORT the mission, soldier?");

                if (decision == false)
                {
                    LogFile.Log.LogEntryDebug("Player chose not to Abort", LogDebugLevel.Medium);
                    return false;
                }

                //Go back to town
                LogFile.Log.LogEntryDebug("Player Aborting level: " + level, LogDebugLevel.Medium);

                
                Game.Dungeon.MissionAborted();

                return true;
            }
            else
            {
                //All done

                bool decision = Screen.Instance.YesNoQuestion("Job well DONE, Soldier! COMPLETE the mission?");

                if (decision == false)
                {
                    LogFile.Log.LogEntryDebug("Player chose not to Complete", LogDebugLevel.Medium);
                    return false;
                }

                //Go back to town
                LogFile.Log.LogEntryDebug("Player Completing level: " + level, LogDebugLevel.Medium);

                Game.Dungeon.MissionComplete();

                return true;
            }

        }
    }
}
