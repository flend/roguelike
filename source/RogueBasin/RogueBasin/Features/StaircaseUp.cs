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
            return Game.Dungeon.PCDownStaircase();
        }

        protected override char GetRepresentation()
        {
            return '<';
        }
    }
}
