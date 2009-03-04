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
            //Really this logic should be here, not in dungeon.
            //Otherwise dungeon gets really crowded and these functions are kind of pointless.
            return Game.Dungeon.PCUpStaircase();
        }

        protected override char GetRepresentation()
        {
            return '<';
        }
    }
}
