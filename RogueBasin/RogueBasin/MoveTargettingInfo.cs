using System.Collections.Generic;

namespace RogueBasin
{
    class MoveTargettingInfo : BasicLineTargettingInfo
    {
        public override bool IsInRange(Player player, Dungeon dungeon, Location targetPoint) {

            return true;
        }

        public override IEnumerable<Point> ToPoints(Player player, Dungeon dungeon, Location targetPoint)
        {
            return dungeon.Movement.GetPlayerRunningPath(targetPoint.MapCoord);
        }
    }
}
