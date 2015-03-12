using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// The kind of feature everyone HAS to interact with
    /// </summary>
    public abstract class ActiveFeature : Feature
    {
        /// <summary>
        /// Process a player interacting with this object
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool PlayerInteraction(Player player);
    }
}