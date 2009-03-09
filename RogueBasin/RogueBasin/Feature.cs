using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Non-pickupable objects in the dungeon
    /// </summary>
    public abstract class Feature : MapObject
    {

        public Feature()
        {

        }

        /// <summary>
        /// Process a player interacting with this object
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool PlayerInteraction(Player player);
        
    }
}
