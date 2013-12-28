using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    /// <summary>
    /// An item that blocks the player's way 
    /// </summary>
    public abstract class Lock : MapObject
    {

        protected bool isOpen = false;

        public Lock()
        {

        }

        /// <summary>
        /// Process a player trying to open / clear this object.
        /// Returns true on success (implies the player can pass / walk on top of it)
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool OpenLock(Player player);

        /// <summary>
        /// Process a player trying to close / seal this object.
        /// Returns true on success (implies the player cannot / walk on top of it)
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool CloseLock(Player player);

        public virtual bool IsOpen() {
            return isOpen;
        }

        /// <summary>
        /// Carry out any changes when the lock is first placed, e.g. blocking light
        /// </summary>
        public virtual void OnPlace()
        {

        }

    }
}
