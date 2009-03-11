using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Features
{
    class Corpse : DecorationFeature
    {
        public Corpse()
        {
        }

        /// <summary>
        /// We don't do searching
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool PlayerInteraction(Player player)
        {
            Game.MessageQueue.AddMessage("You search the corpse but find nothing");

            //Does not advance time
            return false;
        }

        protected override char GetRepresentation()
        {
            return '%';
        }
    }
}
