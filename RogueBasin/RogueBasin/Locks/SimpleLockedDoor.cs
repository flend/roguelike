using libtcodWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin.Locks
{
    class SimpleLockedDoor : Lock
    {
        private GraphMap.Door mapDoor;

        public SimpleLockedDoor(GraphMap.Door door)
        {
            this.mapDoor = door;
        }

        public override bool OpenLock(Player player)
        {
            isOpen = true;

            return true;
        }

        public override bool CloseLock(Player player)
        {
            return true;
        }

        protected override char GetRepresentation()
        {
            if (isOpen)
            {
                return '-';
            }
            else
                return '+';
        }

        public override libtcodWrapper.Color RepresentationColor()
        {
            switch (mapDoor.Id)
            {
                case "red":
                    return ColorPresets.Red;
                case "green":
                    return ColorPresets.Green;
                case "blue":
                    return ColorPresets.Blue;
                case "yellow":
                    return ColorPresets.Yellow;
            }
            return ColorPresets.Magenta;
        }
    }
}
