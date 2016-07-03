using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    class ExamineTargettingInfo : BasicLineTargettingInfo
    {
        public override bool IsInRange(Player player, Dungeon dungeon, Location targetPoint)
        {
            CreatureFOV currentFOV = dungeon.CalculateCreatureFOV(player);
            return currentFOV.CheckTileFOV(targetPoint.MapCoord.x, targetPoint.MapCoord.y);
        }
    }
}
