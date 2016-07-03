using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    class ExamineTargettingInfo : TargettingInfo
    {
        public override bool IsInRange(Player player, Dungeon dungeon, Location targetPoint)
        {
            return true;
        }

        public override IEnumerable<Point> TargetPoints(Player player, Dungeon dungeon, Location targetPoint)
        {
            return new List<Point>() { targetPoint.MapCoord };
        }
    }
}
