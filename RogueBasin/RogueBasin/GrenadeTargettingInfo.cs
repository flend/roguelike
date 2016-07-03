using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    class GrenadeTargettingInfo : LineTargettingInfo
    {
        double size;

        public GrenadeTargettingInfo(int range, double size)
            : base(range)
        {
            this.size = size;
        }

        public override IEnumerable<Point> TargetPoints(Player player, Dungeon dungeon, Location targetPoint)
        {
            List<Point> grenadeAffects = dungeon.GetPointsForGrenadeTemplate(targetPoint.MapCoord, player.LocationLevel, size);

            //Use FOV from point of explosion (this means grenades don't go round corners or through walls)
            WrappedFOV grenadeFOV = dungeon.CalculateAbstractFOV(player.LocationLevel, targetPoint.MapCoord, 0);

            var grenadeAffectsFiltered = grenadeAffects.Where(sq => grenadeFOV.CheckTileFOV(player.LocationLevel, sq));

            return grenadeAffectsFiltered;
        }
    }
}
