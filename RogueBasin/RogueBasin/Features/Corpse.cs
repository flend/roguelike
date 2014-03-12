using libtcodWrapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Features
{
    public class Corpse : DecorationFeature
    {
        char representation;
        Color representationColor;

        public Corpse()
        {
            representation = '%';
            representationColor = ColorPresets.White;
        }

        public Corpse(char representation, Color representationColor)
        {
            this.representationColor = representationColor;
            this.representation = representation;
        }

        protected override char GetRepresentation()
        {
            return representation;
        }

        public override Color RepresentationColor()
        {
            return representationColor;
        }
    }
}
