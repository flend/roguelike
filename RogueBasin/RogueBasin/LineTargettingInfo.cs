namespace RogueBasin
{
    class LineTargettingInfo : BasicLineTargettingInfo
    {
        int range;

        public LineTargettingInfo(int range) {
            this.range = range;
        }

        public override bool IsInRange(Player player, Dungeon dungeon, Location targetPoint) {

            if (player.LocationLevel != targetPoint.Level)
            {
                return false;
            }
            else
            {
                CreatureFOV currentFOV = dungeon.CalculateCreatureFOV(player);
                return Utility.TestRangeFOVForWeapon(player, targetPoint.MapCoord, (double)range, currentFOV);
            }            
        }
    }
}
