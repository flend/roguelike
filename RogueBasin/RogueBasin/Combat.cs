using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public class Combat
    {
        private Dungeon dungeon;

        public Combat(Dungeon dungeon)
        {
            this.dungeon = dungeon;
        }

        public enum FireOnTargetStatus
        {
            NoWeapon, CantTargetSelf, CantFireBetweenLevels, NotEnoughAmmo, OutOfRange, OK
        }

        public FireOnTargetStatus CanFireOnTargetWithEquippedWeapon(Location target)
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            IEquippableItem weapon = player.GetEquippedRangedWeapon();
            Item weaponI = player.GetEquippedRangedWeaponAsItem();

            if (weapon == null)
            {
                return FireOnTargetStatus.NoWeapon;
            }

            if (target.Level != player.Location.Level)
            {
                return FireOnTargetStatus.CantFireBetweenLevels;
            }

            if (target.MapCoord == player.Location.MapCoord)
            {
                return FireOnTargetStatus.CantTargetSelf;
            }

            if (weapon.RemainingAmmo() < 1)
            {
                return FireOnTargetStatus.NotEnoughAmmo;
            }

            //Check we are in range of target (not done above)
            int range = weapon.RangeFire();
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

            if (!Utility.TestRangeFOVForWeapon(Game.Dungeon.Player, target.MapCoord, range, currentFOV))
            {
                return FireOnTargetStatus.OutOfRange;
            }

            return FireOnTargetStatus.OK;
        }
    }
}
