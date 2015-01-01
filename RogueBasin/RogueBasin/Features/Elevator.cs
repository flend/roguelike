using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin.Features
{
    public class Elevator : UseableFeature
    {
        int destLevel;
        Point destLocation;

        public Elevator(int levelDestination, Point locDestination)
        {
            this.destLevel = levelDestination;
            this.destLocation = locDestination;
        }

        /// <summary>
        /// Elevator - teleport to destination
        /// </summary>
        public override bool PlayerInteraction(Player player)
        {
            Dungeon dungeon = Game.Dungeon;

            Game.MessageQueue.AddMessage("You take the elevator. Welcome to " + Game.Dungeon.DungeonInfo.LevelNaming[destLevel]);

            dungeon.MovePCAbsolute(destLevel, destLocation);

            return true;
        }

        public int DestLevel { get { return destLevel;  } }
        public Point DestLocation { get { return destLocation; } }

        protected override char GetRepresentation()
        {
            return (char)538;
        }

        public override string Description
        {
            get
            {
                return "Elevator";
            }
        }
    }
}
