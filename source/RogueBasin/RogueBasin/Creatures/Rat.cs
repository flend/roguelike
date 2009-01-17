using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Creatures
{
    class Rat : MonsterSimpleAI
    {
        const int classMaxHitpoints = 10;
        public Rat()
        {
        }


        protected override int ClassMaxHitpoints()
        {
            return classMaxHitpoints;
        }
    }
}
