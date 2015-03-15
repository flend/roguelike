using libtcodWrapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Features
{
    public class Corpse : DecorationFeature
    {
        char representation;
        System.Drawing.Color representationColor;
        Type monsterType;

        public Corpse()
        {
            representation = (char)479;
            representationColor = System.Drawing.Color.White;
        }

        public Corpse(char representation, System.Drawing.Color representationColor)
        {
            this.representationColor = representationColor;
            this.representation = representation;
        }

        protected override char GetRepresentation()
        {
            return representation;
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return representationColor;
        }

        protected override string GetGameSprite()
        {
            return "corpse";
        }

        public override string Description
        {
            get
            {
                return "Corpse";
            }
        }
    }
}
