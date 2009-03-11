using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    static class Utility
    {
        public static int d20()
        {
            return 1 + Game.Random.Next(20);
        }

        public static int DamageRoll(int damageBase)
        {
            return 1 + Game.Random.Next(damageBase);
        }
    }
}
