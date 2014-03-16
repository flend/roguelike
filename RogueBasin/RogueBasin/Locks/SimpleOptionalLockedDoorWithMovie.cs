using libtcodWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin.Locks
{
    public class SimpleOptionalLockedDoorWithMovie : SimpleLockedDoor
    {
        private string openMovie;
        private string cantOpenMovie;
        private string confirmationString;

        public SimpleOptionalLockedDoorWithMovie(GraphMap.Door door, string openMovie, string cantOpenMovie, string confirmationString, string idToReport, Color color) : base(door, idToReport, color)
        {
            this.openMovie = openMovie;
            this.cantOpenMovie = cantOpenMovie;
            this.confirmationString = confirmationString;
        }

        public override bool OpenLock(Player player)
        {
            bool canDoorBeOpened = CanDoorBeOpenedWithClues(player);

//            if (Game.Dungeon.AllLocksOpen)
  //              canDoorBeOpened = true;

            if (!canDoorBeOpened)
            {
                Screen.Instance.PlayMovie(cantOpenMovie, true);
                var result = Screen.Instance.YesNoQuestionWithFrame(confirmationString);
                if (result)
                    OpenDoor();

                return result;
            }
            else
            {
                OpenDoor();
                return true;
            }
        }

        private void OpenDoor()
        {
            Screen.Instance.PlayMovie(openMovie, true);
            isOpen = true;
        }
    }
}
