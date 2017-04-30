using System.Collections.Generic;
using System.Linq;

namespace RogueBasin
{
    class ShotgunTargettingInfo : LineTargettingInfo
    {
        double angle;
        int size;

        public ShotgunTargettingInfo(int range, double angle, int size) : base(range)
        {
            this.angle = angle;
            this.size = size;
        }

        public override IEnumerable<Point> TargetPoints(Player player, Dungeon dungeon, Location targetPoint)
        {
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);
            List<Point> splashSquares = currentFOV.GetPointsForTriangularTargetInFOV(player.LocationMap, targetPoint.MapCoord, Game.Dungeon.Levels[player.LocationLevel], size, angle);
            return splashSquares.Skip(1);
        }
    }
}
