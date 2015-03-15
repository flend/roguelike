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
        System.Drawing.Color representationColour;

        public StandardDecorativeFeature(char representation, System.Drawing.Color representationColour)
        {
            this.representation = representation;
            this.representationColour = representationColour;
        }

        protected override char GetRepresentation()
        {
            return representation;
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


        protected override string GetGameSprite()
        {
            if (IsBlocking)
                return "hardcover";
            else 
                return "softcover";
        }

        protected override string GetUISprite()
        {
            if (IsBlocking) 
                return "hardcover";
            else
                return "softcover";
        }
    }
}
