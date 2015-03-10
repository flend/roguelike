using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace RogueBasin.Locks
{
    class ExitLock : Lock
    {
        private int level;

        public ExitLock(int level)
        {
            this.level = level;
        }

        public override bool OpenLock(Player player)
        {
            //Player chooses this level

            //Might want to confirm with the player here, which is tricky now in this event driven system

            //Hmm, this probably wants to be a teleporter actually

            //Test to see if all the monsters are dead

            Game.Base.PlayerExitsLevel(level);
            Game.MessageQueue.AddMessage("You escape arena " + level + " !");

            //Don't return true here or we will place an open door in a random place on the new level
            return false;
        }

        public override bool CloseLock(Player player)
        {
            return true;
        }

        protected override char GetRepresentation()
        {
            int shroomWallStartRow = 21;
            int rowLength = 16;

            if (isOpen)
            {
                return (char)((shroomWallStartRow + 2) * rowLength + 3);
            }
            else
                return (char)((shroomWallStartRow + 2) * rowLength + 2);
        }

        public override Color RepresentationColor()
        {
            return Color.LightYellow;
        }
    }
}
