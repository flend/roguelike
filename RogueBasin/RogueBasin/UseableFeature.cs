using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// The kind of feature everyone wants to interact with
    /// </summary>
    public abstract class UseableFeature : Feature
    {
        /// <summary>
        /// Process a player interacting with this object
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool PlayerInteraction(Player player);
    }
}
