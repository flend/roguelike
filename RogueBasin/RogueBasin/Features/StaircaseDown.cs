using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Features
{
    public class StaircaseDown : UseableFeature
    {
        public StaircaseDown()
        {
        }

        /// <summary>
        /// Move to the next lowest level
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool PlayerInteraction(Player player)
        {
            Dungeon dungeon = Game.Dungeon;

            //If we are trying to go deeper than the dungeon exists
            if (player.LocationLevel + 1 == dungeon.NoLevels)
            {
                LogFile.Log.LogEntry("Tried to go down stairs to level " + (player.LocationLevel + 1).ToString() + " which doesn't exist");
                Game.MessageQueue.AddMessage("Bizarrely, the stairs don't work.");
                return false;
            }

            //Otherwise move down

            //Increment player level
            player.LocationLevel++;

            //(could consider making a helper function in dungeon for this)

            List<Feature> features = Game.Dungeon.Features;

            Feature foundStaircase = null;
            //Set player's location to up staircase on lower level
            foreach (Feature feature in features)
            {
                if (feature.LocationLevel == player.LocationLevel
                    && feature as Features.StaircaseUp != null)
                {
                    player.LocationMap = feature.LocationMap;
                    foundStaircase = feature as Features.StaircaseUp;
                    break;
                }
            }

            if (foundStaircase == null)
            {
                LogFile.Log.LogEntry("Couldn't find up staircase on level " + (player.LocationLevel).ToString());
                return true;
            }

            //Check there is no monster on the stairs
            //If there is, kill it (for now)
            //(again consider helper function for this)

            List<Monster> monsters = dungeon.Monsters;

            foreach (Monster monster in monsters)
            {
                if (monster.InSameSpace(foundStaircase))
                {
                    dungeon.KillMonster(monster);
                }
            }

            return true;
        }

        protected override char GetRepresentation()
        {
            return '>';
        }
    }
}
