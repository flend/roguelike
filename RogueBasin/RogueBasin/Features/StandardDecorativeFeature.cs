using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Features
{
    public class StandardDecorativeFeature : DecorationFeature
    {
        char representation;
        Color representationColour;

        public StandardDecorativeFeature(char representation, Color representationColour)
        {
            this.representation = representation;
            this.representationColour = representationColour;
        }

        protected override char GetRepresentation()
        {
            return representation;
        }

        public override Color RepresentationColor()
        {
            return representationColour;
        }

        public override string Description
        {
            get
            {
                if (IsBlocking)
                    return "Hard cover";
                else
                    return "Soft cover";
            }
        }

    }
}
