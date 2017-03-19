using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Features
{
    /// <summary>
    /// Enter a dungeon. i.e. teleport to a particular level from the wilderness
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public class StaircaseEntry : UseableFeature
    {
        public int dungeonID;
        public int dungeonStartLevel;

        public StaircaseEntry() { }

        /// <summary>
        /// Construct with the level of the dungeon to transition to
        /// </summary>
        /// <param name="dungeonStartLevel"></param>
        public StaircaseEntry(int dungeonID, int dungeonStartLevel)
        {
            this.dungeonID = dungeonID;
            this.dungeonStartLevel = dungeonStartLevel;
        }

        public override bool PlayerInteraction(Player player)
        {
            Dungeon dungeon = Game.Dungeon;

            //Can't enter normal dungeons in the last mission
            if (Game.Dungeon.DungeonInfo.LastMission && dungeonID != 6)
            {
                Game.MessageQueue.AddMessage("You don't want to go here - there's more important things to do!");
                return false;
            }

            //Enter dungeon
            player.LocationLevel = dungeonStartLevel;

            //Set player vision
            player.CalculateSightRadius();

            //Set the current dungeon in Player
            player.CurrentDungeon = dungeonID;

            //Set the current dungeon as visited
            dungeon.DungeonInfo.VisitedDungeon(dungeonID);

            //Give the player an extra turn in case they spawn next to a monster
            dungeon.PlayerBonusTurn = true;

            //debug
            //player.SightRadius = 20;

            PlacePlayerOnUpstairs();
            
            return true;
        }

        protected void PlacePlayerOnUpstairs()
        {

            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            List<Feature> features = Game.Dungeon.Features;

            Feature foundStaircase = null;
            //Set player's location to up staircase on lower level
            foreach (Feature feature in features)
            {
                if (feature.LocationLevel == player.LocationLevel
                    && feature as Features.StaircaseExit != null)
                {
                    player.LocationMap = feature.LocationMap;
                    foundStaircase = feature as Features.StaircaseExit;
                    //Use the dungeon move system to trigger any triggers
                    Game.Dungeon.Movement.MovePCAbsoluteNoIteractions(player.Location, true);
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
                    dungeon.KillMonster(monster, true);
                }
            }
        }

        protected override char GetRepresentation()
        {
            return '>';
        }
    }
}
