using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public abstract class TargettingInfo
    {
        public abstract bool IsInRange(Player player, Dungeon dungeon, Location targetPoint);
        public abstract IEnumerable<Point> ToPoints(Player player, Dungeon dungeon, Location targetPoint);
        public abstract IEnumerable<Point> TargetPoints(Player player, Dungeon dungeon, Location targetPoint);
    }
}
