using System.Collections.Generic;

namespace RogueBasin
{
    public abstract class TargettingInfo
    {
        public abstract bool IsInRange(Player player, Dungeon dungeon, Location targetPoint);
        public abstract IEnumerable<Point> ToPoints(Player player, Dungeon dungeon, Location targetPoint);
        public abstract IEnumerable<Point> TargetPoints(Player player, Dungeon dungeon, Location targetPoint);
    }
}
