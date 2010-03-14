using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the entrance square
    /// </summary>
    public class TerrainFlipTrigger : DungeonSquareTrigger
    {

        public MapTerrain flipToTerrain;

        public string triggerID;

        /// <summary>
        /// Not that Triggered is static so triggering one type of event triggers them all. This allows the same event to be put in multiple places and only triggered once
        /// </summary>
        public static bool Triggered { get; set; }

        public TerrainFlipTrigger() { }

        public TerrainFlipTrigger(MapTerrain terrainToFlipTo, string triggerID)
        {
            Triggered = false;
            flipToTerrain = terrainToFlipTo;
            this.triggerID = triggerID;
        }

        public override bool CheckTrigger(int level, Point mapLocation)
        {
            //Check we are in the right place
            if (CheckLocation(level, mapLocation) == false)
            {
                return false;
            }
            //Otherwise in the right place

            return true;
        }

        public void FlipTerrain() {
            try {
                Game.Dungeon.Levels[this.Level].mapSquares[this.mapPosition.x, this.mapPosition.y].Terrain = flipToTerrain;
                if (Dungeon.IsTerrainWalkable(flipToTerrain))
                {
                    Game.Dungeon.Levels[this.Level].mapSquares[this.mapPosition.x, this.mapPosition.y].Walkable = true;
                    Game.Dungeon.Levels[this.Level].mapSquares[this.mapPosition.x, this.mapPosition.y].BlocksLight = false;
                }
            }
            catch(Exception ex) {
                LogFile.Log.LogEntryDebug("Problem flipping terrain at level " + this.Level + " x: " + this.mapPosition.x + " y: " + this.mapPosition.y + "ex msg: " + ex.Message, LogDebugLevel.High);
            }
        }
    }
}
