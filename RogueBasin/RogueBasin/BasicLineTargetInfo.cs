using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public abstract class BasicLineTargettingInfo : TargettingInfo
    {
        public override IEnumerable<Point> ToPoints(Player player, Dungeon dungeon, Location targetPoint)
        {
            var pointsFromPlayer = Utility.GetPointsOnLine(player.LocationMap, targetPoint.MapCoord);
            return pointsFromPlayer.Skip(1).Take(pointsFromPlayer.Count() - 2);
        }

        public override IEnumerable<Point> TargetPoints(Player player, Dungeon dungeon, Location targetPoint)
        {
            var pointsFromPlayer = Utility.GetPointsOnLine(player.LocationMap, targetPoint.MapCoord);
            return new List<Point>() { pointsFromPlayer.Last() };
        }
    }
}
