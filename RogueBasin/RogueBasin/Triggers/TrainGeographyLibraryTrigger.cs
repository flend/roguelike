using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Magic library
    /// </summary>
    public class TrainGeographyLibraryTrigger : DungeonSquareTrigger
    {
        public bool Triggered { get; set; }

        public TrainGeographyLibraryTrigger()
        {
            Triggered = false;
        }

        public override bool CheckTrigger(int level, Point mapLocation)
        {
            //Check we are in the right place
            if (CheckLocation(level, mapLocation) == false)
            {
                return false;
            }
            
            //Otherwise in the right place

            if (!Triggered)
            {
                string movieName = "trainmaplibrary";

                if (movieName != "")
                    Screen.Instance.PlayMovie(movieName, false);
            }
            Triggered = true;

            Dungeon dungeon = Game.Dungeon;

            bool doesTraining = false;

            if(dungeon.IsWeekday()) {

                //Adventure weekend
                Game.MessageQueue.AddMessage("You're expected in classes during the week.");
            }
            else if(dungeon.IsNormalWeekend()) {

                //Open up new dungeons

                //Have we opened all the dungeons?

                List<DungeonProfile> notOpen = Game.Dungeon.DungeonInfo.Dungeons.FindAll(y => !y.open);

                //Only last story level left
                if (notOpen.Count == 1)
                {
                    Game.MessageQueue.AddMessage("You search around the old maps for an hour or so but you reckon you've found all can here.");
                    return false;
                }
                
                //Check if we have visited enough dungeons

                if (!dungeon.DungeonInfo.IsDungeonVisited(0) || !dungeon.DungeonInfo.IsDungeonVisited(1))
                {
                    //Not visited any
                    FailedToFindMap();
                    return true;
                }

                if (!dungeon.DungeonInfo.IsDungeonOpen(2))
                {
                    OpenDungeon(2);
                    return true;
                }

                if (!dungeon.DungeonInfo.IsDungeonVisited(2))
                {
                    FailedToFindMap();
                    return true;
                }

                if (!dungeon.DungeonInfo.IsDungeonOpen(3))
                {
                    OpenDungeon(3);
                    return true;
                }

                if (!dungeon.DungeonInfo.IsDungeonVisited(3))
                {
                    FailedToFindMap();
                    return true;
                }

                OpenDungeon(4);
                return true;

                //LogFile.Log.LogEntryDebug("Trying to open a non-existant dungeon", LogDebugLevel.High);
                //return false;
            }
            else {

                //Adventure weekend
                Game.MessageQueue.AddMessage("Surely there's something better to do this weekend!");
            }

            if (doesTraining)
            {
                RunTrainingUI();
            }

            return true;
        }

        private void FailedToFindMap()
        {

            LogFile.Log.LogEntryDebug("Failed to open dungeon", LogDebugLevel.Medium);

            Screen.Instance.PlayMovie("failedToFindMap", false);
            Game.Dungeon.MoveToNextDate();
            //Teleport the user back to the start location
            Game.Dungeon.PlayerBackToTown();


        }

        private void OpenDungeon(int dungeonNumber)
        {

            Screen.Instance.PlayMovie("succeededToFindMap", false);
            
            //Open the dungeon
            switch (dungeonNumber)
            {
                case 2:
                    Game.Dungeon.FlipTerrain("forest");
                    Game.Dungeon.DungeonInfo.OpenDungeon(2);
                    break;
                case 3:
                    Game.Dungeon.FlipTerrain("river");
                    Game.Dungeon.DungeonInfo.OpenDungeon(3);
                    Game.Dungeon.DungeonInfo.OpenDungeon(5);
                    break;
                case 4:
                    Game.Dungeon.FlipTerrain("grave");
                    Game.Dungeon.DungeonInfo.OpenDungeon(4);
                    break;
            }

            LogFile.Log.LogEntryDebug("Opening dungeon " + dungeonNumber, LogDebugLevel.Medium);

            Game.Dungeon.MoveToNextDate();
            //Teleport the user back to the start location
            Game.Dungeon.PlayerBackToTown();


        }


        protected void RunTrainingUI()
        {
            //Show training UI
            Screen.Instance.DisplayTrainingUI = true;
            Screen.Instance.DrawAndFlush();

            bool continueLooking = true;

            while (continueLooking)
            {
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {
                    char keyCode = (char)userKey.Character;

                    //Exit out of inventory
                    if (keyCode == 'x')
                    {
                        Screen.Instance.DisplayTrainingUI = false;
                        Screen.Instance.DrawAndFlush();
                        continueLooking = false;
                    }
                }
            }

            //Increment calendar time
            Game.Dungeon.MoveToNextDate();

            //Teleport the user back to the start location
            Game.Dungeon.PlayerBackToTown();
        }
    }
}
