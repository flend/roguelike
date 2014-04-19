using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public abstract class ShotgunTypeWeapon : RangedWeapon
    {
        public virtual double ShotgunSpreadAngle()
        {
            return Math.PI / 4;
        }
    }
}
