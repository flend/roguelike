using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Features
{
    public class Corpse : DecorationFeature
    {
        public Corpse()
        {
        }

        protected override char GetRepresentation()
        {
            return '%';
        }
    }
}
