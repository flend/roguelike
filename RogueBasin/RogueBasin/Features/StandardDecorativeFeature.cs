using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Features
{
    public class StandardDecorativeFeature : DecorationFeature
    {
        System.Drawing.Color representationColour;

        public StandardDecorativeFeature(char representation, System.Drawing.Color representationColour, bool isBlocking)
        {
            Representation = representation;
            this.representationColour = representationColour;
            this.IsBlocking = isBlocking;
        }

        public override System.Drawing.Color RepresentationColor()
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
