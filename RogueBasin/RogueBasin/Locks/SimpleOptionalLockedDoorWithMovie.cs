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

        public SimpleOptionalLockedDoorWithMovie(GraphMap.Door door, string openMovie, string cantOpenMovie, string confirmationString, string idToReport, System.Drawing.Color color) : base(door, idToReport, color)
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
                Game.Base.SystemActions.PlayMovie(cantOpenMovie, true);
                //var result = Screen.Instance.YesNoQuestionWithFrame(confirmationString, 0, System.Drawing.Color.Khaki, System.Drawing.Color.MediumSeaGreen);
                var result = true;
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
            Game.Base.SystemActions.PlayMovie(openMovie, true);
            isOpen = true;
        }
    }
}
