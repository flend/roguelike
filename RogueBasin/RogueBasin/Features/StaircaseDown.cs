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

            //Turn all charmed creatures on this level into passified creatures
            //Just do it for all monsters
            foreach (Monster monster in Game.Dungeon.Monsters)
            {
                if (monster.Charmed)
                {
                    monster.UncharmCreature();
                    //monster.PassifyCreature();

                    Game.Dungeon.Player.RemoveCharmedCreature();
                }
            }

            //Increment player level
            player.LocationLevel++;

            //Set vision
            double sightRatio = Game.Dungeon.Player.NormalSightRadius / 5.0;
            player.SightRadius = (int)Math.Ceiling(Game.Dungeon.Levels[player.LocationLevel].LightLevel * sightRatio);
            //player.SightRadius = (int)Math.Ceiling(player.NormalSightRadius * Game.Dungeon.Levels[player.LocationLevel].LightLevel);

            PlacePlayerOnUpstairs();

            return true;
        }

        protected void PlacePlayerOnUpstairs() {

            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

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
                    //Use the dungeon move system to trigger any triggers
                    Game.Dungeon.MovePCAbsolute(player.LocationLevel, player.LocationMap.x, player.LocationMap.y);
                    break;
                }
            }

            if (foundStaircase == null)
            {
                LogFile.Log.LogEntry("Couldn't find up staircase on level " + (player.LocationLevel).ToString());
                return;
            }

            //Check there is no monster on the stairs
            //If there is, kill it (for now)
            //(again consider helper function for this)

            List<Monster> monsters = dungeon.Monsters;

            foreach (Monster monster in monsters)
            {
                if (monster.InSameSpace(foundStaircase))
                {
                    dungeon.KillMonster(monster, false);
                }
            }
        }

        protected override char GetRepresentation()
        {
            return '>';
        }
    }
}
