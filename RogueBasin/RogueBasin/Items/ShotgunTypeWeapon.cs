using System;

namespace RogueBasin.Items
{
    public abstract class ShotgunTypeWeapon : RangedWeapon
    {
        public virtual double ShotgunSpreadAngle()
        {
            return Math.PI / 8;
        }
    }
}
