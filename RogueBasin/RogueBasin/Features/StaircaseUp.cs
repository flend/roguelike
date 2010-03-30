using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Features
{
    public class StaircaseUp : UseableFeature
    {

        public StaircaseUp()
        {
        }

        /// <summary>
        /// Staircase up. Either leave the dungeon or go up to a higher level
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool PlayerInteraction(Player player)
        {
            Dungeon dungeon = Game.Dungeon;

            //If we are trying to up from the highest level
            if (player.LocationLevel - 1 < 0)
            {
                LogFile.Log.LogEntry("Player tried to escape");
                Game.MessageQueue.AddMessage("You can't escape that easily!");
                return false;
            }

            //Otherwise move up

            //Increment player level
            player.LocationLevel--;

            //Set vision
            double sightRatio = player.NormalSightRadius / 5.0;
            player.SightRadius = (int)Math.Ceiling(player.NormalSightRadius * Game.Dungeon.Levels[player.LocationLevel].LightLevel);

            List<Feature> features = Game.Dungeon.Features;

            Feature foundStaircase = null;

            //Set player's location to up staircase on lower level
            foreach (Feature feature in features)
            {
                if (feature.LocationLevel == player.LocationLevel
                    && feature as Features.StaircaseDown != null)
                {
                    player.LocationMap = feature.LocationMap;
                    //Use the dungeon move system to trigger any triggers
                    Game.Dungeon.MovePCAbsolute(player.LocationLevel, player.LocationMap.x, player.LocationMap.y);

                    foundStaircase = feature as Features.StaircaseDown;
                    break;
                }
            }

            if (foundStaircase == null)
            {
                LogFile.Log.LogEntry("Couldn't find down staircase on level " + (player.LocationLevel).ToString());
                return true;
            }

            //Check there is no monster on the stairs
            //If there is, kill it (for now)
            List<Monster> monsters = dungeon.Monsters;

            foreach (Monster monster in monsters)
            {
                if (monster.InSameSpace(foundStaircase))
                {
                    dungeon.KillMonster(monster, false);
                }
            }

            return true;
        }

        protected override char GetRepresentation()
        {
            return '<';
        }
    }
}
