namespace RogueBasin.Locks
{
    public class SimpleLockedDoorWithMovie : SimpleLockedDoor
    {
        private string openMovie;
        private string cantOpenMovie;

        public SimpleLockedDoorWithMovie(GraphMap.Door door, string openMovie, string cantOpenMovie, string idToReport, System.Drawing.Color color) : base(door, idToReport, color)
        {
            this.openMovie = openMovie;
            this.cantOpenMovie = cantOpenMovie;
        }

        public override bool OpenLock(Player player)
        {
            bool canDoorBeOpened = CanDoorBeOpenedWithClues(player);

            if (Game.Dungeon.AllLocksOpen)
                canDoorBeOpened = true;

            if (!canDoorBeOpened)
            {
                Game.Base.SystemActions.PlayMovie(cantOpenMovie, true);
                return false;
            }
            else
            {
                Game.Base.SystemActions.PlayMovie(openMovie, true);
                isOpen = true;
                return true;
            }
        }
    }
}
