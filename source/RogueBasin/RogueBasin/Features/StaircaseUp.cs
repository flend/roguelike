using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Features
{
    class StaircaseUp : Feature
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
            //Is this the top level?
            //If so, tell the player they can't escape yet
            if (player.LocationLevel == 1)
            {
                Game.MessageQueue.AddMessage("You can't escape that easily!");
                return false;
            }
            else
            {
                player.LocationLevel--;
                return true;
            }
        }

        protected override char GetRepresentation()
        {
            return '<';
        }
    }
}
