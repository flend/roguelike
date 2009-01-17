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
        /// Process a player interacting with this feature
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool PlayerInteraction(Player player)
        {
            return true;
        }
    }
}
