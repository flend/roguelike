using System.Collections.Generic;
using System.Linq;

namespace RogueBasin
{
    class LineThroughTargettingInfo : LineTargettingInfo
    {
        int range;

        public LineThroughTargettingInfo(int range) : base(range)
        {

        }

        public override IEnumerable<Point> TargetPoints(Player player, Dungeon dungeon, Location targetPoint)
        {
            //Cast a line which terminates on the edge of the map
            Point projectedLine = dungeon.GetEndOfLine(player.LocationMap, targetPoint.MapCoord, player.LocationLevel);

            //Get the in-FOV points up to that end point
            WrappedFOV currentFOV2 = dungeon.CalculateAbstractFOV(player.LocationLevel, player.LocationMap, 80);
            List<Point> lineSquares = dungeon.GetPathLinePointsInFOV(player.LocationLevel, player.LocationMap, projectedLine, currentFOV2);
            return lineSquares.Skip(1);
        }
    }
}
