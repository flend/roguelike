using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Spells
{
    /// <summary>
    /// Teleports to a nearby location
    /// </summary>
    public class Blink : Spell
    {
        int spellRange = 5;

        public override bool DoSpell(Point target)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //Find a nearby location in the dungeon

            Map thisMap = dungeon.Levels[player.LocationLevel];

            bool endLoop = false;
            int loopCount = 0;

            int x, y;

            spellRange = 5 + player.MagicStat / 30;

            do {
                loopCount++;
                //Random location in dungeon
                
                x = Game.Random.Next(thisMap.width);
                y = Game.Random.Next(thisMap.height);

                //Within range?
                if(! (Math.Pow(x - player.LocationMap.x, 2) + Math.Pow(y - player.LocationMap.y,2) < Math.Pow(spellRange, 2))) {
                    continue;
                }

                //Not too near - better just to run from the outside in but live with this for now
                if (!(Math.Pow(x - player.LocationMap.x, 2) + Math.Pow(y - player.LocationMap.y, 2) > 3))
                {
                    continue;
                }

                //Walkable
                if(!dungeon.MapSquareIsWalkable(player.LocationLevel, new Point(x,y)))
                    continue;

                //Empty

                SquareContents sq = dungeon.MapSquareContents(player.LocationLevel, new Point(x, y));

                if(!sq.empty)
                    continue;

                //Otherwise OK
                endLoop = true;
                
            } while(!endLoop && loopCount < 1000);

            //Sanity check
            if(loopCount == 1000) {
                LogFile.Log.LogEntryDebug("Failed to find a place to blink to", LogDebugLevel.High);
                Game.MessageQueue.AddMessage("You feel rooted to the spot.");
                return false;
            }

            //Otherwise move there
            Game.MessageQueue.AddMessage("Blink! The world shifts momentarily.");
            dungeon.MovePCAbsoluteSameLevel(new Point(x, y));

            return true;
        }

        public override int MPCost()
        {
            return 5;
        }

        public override bool NeedsTarget()
        {
            return false;
        }

        public override string SpellName()
        {
            return "Blink";
        }

        public override string Abbreviation()
        {
            return "Bl";
        }

        internal override int GetRequiredMagic()
        {
            return 50;
        }

        internal override string MovieRoot()
        {
            return "spellblink";
        }
    }
}
