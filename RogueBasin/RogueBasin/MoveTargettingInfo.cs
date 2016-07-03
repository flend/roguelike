using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    class MoveTargettingInfo : BasicLineTargettingInfo
    {
        public override bool IsInRange(Player player, Dungeon dungeon, Location targetPoint) {

            return true;
        }

        public override IEnumerable<Point> ToPoints(Player player, Dungeon dungeon, Location targetPoint)
        {
            return player.GetPlayerRunningPath(targetPoint.MapCoord);
        }
    }
}
