using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Features
{
    public class DockBay : DecorationFeature
    {
        public DockBay()
        {
        }

        /// <summary>
        /// We don't do searching
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool PlayerInteraction(Player player)
        {
            //Game.MessageQueue.AddMessage("Yuck! I'm not going to eat a corpse - what type of person do you think I am?");

            //Does not advance time
            return false;
        }

        protected override char GetRepresentation()
        {
            return '\xe8';
        }

        public override Color RepresentationColor()
        {
            return ColorPresets.Cyan;
        }
    }
}
