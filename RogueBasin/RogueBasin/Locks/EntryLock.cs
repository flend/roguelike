using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace RogueBasin.Locks
{
    public class EntryLock : Lock
    {
        private int level;

        public EntryLock(int level)
        {
            this.level = level;
        }

        public override bool OpenLock(Player player)
        {
            //Player chooses this level

            //Might want to confirm with the player here, which is tricky now in this event driven system

            Game.Base.PlayerEntersLevel(level);
            Game.MessageQueue.AddMessage("You bravely enter arena " + (Game.Dungeon.ArenaLevelNumber() + 1));

            return true;
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
            return Color.HotPink;
        }
    }
}
