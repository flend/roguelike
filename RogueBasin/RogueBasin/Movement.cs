using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public class Movement
    {
        private Dungeon dungeon;

        public Movement(Dungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        public enum RunToTargetStatus
        {
            OK, Unwalkable, CantRunToSelf, CantRunBetweenLevels, UnwalkableDestination, CantRouteToDestination
        }

        public RunToTargetStatus CanRunToTarget(Location target)
        {
            Player player = dungeon.Player;
            
            if (target.Level != player.Location.Level)
            {
                return RunToTargetStatus.CantRunBetweenLevels;
            }

            if (target.MapCoord == player.Location.MapCoord)
            {
                return RunToTargetStatus.CantRunToSelf;
            }

            if(!dungeon.MapSquareIsWalkableOrInteractable(target))
            {
                return RunToTargetStatus.UnwalkableDestination;
            }

            if(GetPlayerRunningPath(target.MapCoord).IsEmpty())
            {
                return RunToTargetStatus.CantRouteToDestination;
            }

            return RunToTargetStatus.OK;
        }

        public IEnumerable<Point> GetPlayerRunningPath(Point destination)
        {
            IEnumerable<Point> path = dungeon.Pathing.GetPathToSquare(dungeon.Player.LocationLevel, dungeon.Player.LocationMap, destination, Pathing.PathingPermission.IgnoreDoorsAndLocks, true);
            if (path == null || !path.Skip(1).Any())
            {
                return Enumerable.Empty<Point>();
            }
            return path.Skip(1);
        }

        public bool MeleeTargetAtMovementLocation(Location target)
        {
            var sq = dungeon.MapSquareContents(target);

            if (sq.monster != null)
            {
                return true;
            }

            return false;
        }

    }
}
