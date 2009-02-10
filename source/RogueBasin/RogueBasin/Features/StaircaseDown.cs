using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Features
{
    class StaircaseDown : Feature
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
            return Game.Dungeon.PCDownStaircase();
        }

        protected override char GetRepresentation()
        {
            return '>';
        }
    }
}
