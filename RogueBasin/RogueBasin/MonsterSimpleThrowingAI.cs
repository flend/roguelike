using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    /// <summary>
    /// Class for a throwing creature that doesn't back away. All functionality is now in MonsterThrowAndRunAI with GetChanceBackAway = 0
    /// </summary>
    public abstract class MonsterSimpleThrowingAI : MonsterFightAndRunAI
    {
        public MonsterSimpleThrowingAI() : base()
        {
        }

        protected abstract double GetMissileRange();

        protected abstract string GetWeaponName();

        /// <summary>
        /// Color of the projectile
        /// </summary>
        /// <returns></returns>
        protected virtual Color GetWeaponColor()
        {
            return ColorPresets.DarkGray;
        }


    }
}
