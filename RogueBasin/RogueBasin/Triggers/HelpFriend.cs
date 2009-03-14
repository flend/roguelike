using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the entrance square
    /// </summary>
    public class HelpFriend : DungeonSquareTrigger
    {

        /// <summary>
        /// Not that Triggered is static so triggering one type of event triggers them all. This allows the same event to be put in multiple places and only triggered once
        /// </summary>
        public static bool Triggered { get; set; }

        public HelpFriend()
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
                //Different endings depending upon whether we remember them, whether we're in time and whether we help them


                //hack
                //Game.Dungeon.PlotItemsFound = 5;

                //Game.Dungeon.WorldClock = 2000000;
                //TODO: Add pacifist ending

                //Are we in time
                if (Game.Dungeon.WorldClock < Game.Dungeon.TimeToRescueFriend)
                {

                    Screen.Instance.PlayMovie("helpFriend", true);

                    //Ask for yes / no answer

                    bool help = Screen.Instance.YesNoQuestion("See to their wounds?");

                    //If yes
                    if (help == true)
                    {
                        //Do we remember them
                        if (Game.Dungeon.PercentRemembered() > 80)
                        {
                            Screen.Instance.PlayMovie("romanceEnding", true);
                            Game.Dungeon.EndGame("was reunited with his lover and returned to his previous life.");
                        }
                        else
                        {
                            Screen.Instance.PlayMovie("forgetfulEnding", true);
                            Game.Dungeon.EndGame("never recovered his memories and left to wander the land.");
                        }
                    }
                    else
                    {
                        Screen.Instance.PlayMovie("cruelEnding", true);
                        Game.Dungeon.EndGame("became a feared warlord and defeated all who stood before him.");
                    }
                }
                else
                {
                    //Not in time

                    //Do we remember them
                    if (Game.Dungeon.PercentRemembered() > 80)
                    {
                        Screen.Instance.PlayMovie("deadFriendEnding", true);
                        Game.Dungeon.EndGame("was stricken with grief and swore revenge on all concerned.");
                    }
                    else
                    {
                        Screen.Instance.PlayMovie("forgetfulEndingDeadFriend", true);
                        Game.Dungeon.EndGame("never recovered his memories and left to wander the land.");
                    }
                }
                Triggered = true;

                
            }

            return true;
        }
    }
}
