using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin.Features
{
    class Elevator : UseableFeature
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

            Game.MessageQueue.AddMessage("You take the elevator.");

            dungeon.MovePCAbsolute(destLevel, destLocation);

            return true;
        }

        protected override char GetRepresentation()
        {
            return '%';
        }
    }
}
