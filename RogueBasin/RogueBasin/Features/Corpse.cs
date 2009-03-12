using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Features
{
    public class Corpse : DecorationFeature
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
            Game.MessageQueue.AddMessage("Yuck! I'm not going to eat a corpse - what type of person do you think I am?");

            //Does not advance time
            return false;
        }

        protected override char GetRepresentation()
        {
            return '%';
        }
    }
}
