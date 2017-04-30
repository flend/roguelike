﻿using System.Linq;

namespace RogueBasin.Locks
{
    public class SimpleLockedDoor : Lock
    {
        protected GraphMap.Door mapDoor;
        private string idToReport;
        private System.Drawing.Color color;

        public SimpleLockedDoor(GraphMap.Door door, string idToReport, System.Drawing.Color color)
        {
            this.mapDoor = door;
            this.idToReport = idToReport;
            this.color = color;
        }

        public SimpleLockedDoor(GraphMap.Door door)
        {
            this.mapDoor = door;
            this.idToReport = door.Id;
        }

        public override bool OpenLock(Player player)
        {
            bool canDoorBeOpened = CanDoorBeOpenedWithClues(player);

            if (Game.Dungeon.AllLocksOpen)
                canDoorBeOpened = true;

            if (!canDoorBeOpened)
            {
                if (mapDoor.NumCluesRequired == 1)
                    Game.MessageQueue.AddMessage("The door won't open. It needs a " + idToReport);
                else
                    Game.MessageQueue.AddMessage("The door won't open. It needs " + mapDoor.NumCluesRequired + " " + idToReport + "s");
                return false;
            }
            else
            {
                if (mapDoor.NumCluesRequired == 1)
                    Game.MessageQueue.AddMessage("You open the door with your " + idToReport);
                else
                    Game.MessageQueue.AddMessage("You open the door with your " + idToReport + "s.");
                isOpen = true;
                return true;
            }
        }

        public bool CanDoorBeOpenedWithClues(Player player)
        {
            var allPlayerClueItems = player.Inventory.GetItemsOfType<Items.Clue>();
            var allPlayerClues = allPlayerClueItems.Select(i => i.MapClue);

            bool canDoorBeOpened = mapDoor.CanDoorBeUnlockedWithClues(allPlayerClues);
            return canDoorBeOpened;
        }

        public override bool CloseLock(Player player)
        {
            return true;
        }

        protected override char GetRepresentation()
        {
            int shroomWallStartRow = 21;
            int shroomWallSkip = 7;
            int rowLength = 16;

            if (isOpen)
            {
                return (char)((shroomWallStartRow + 2) * rowLength + 3);
            }
            else
                return (char)((shroomWallStartRow + 2) * rowLength + 2);
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return color;
        }
    }
}
