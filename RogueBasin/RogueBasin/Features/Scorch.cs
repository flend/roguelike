using System;

namespace RogueBasin.Features
{
    public class Scorch : DecorationFeature
    {
        char representation;
        System.Drawing.Color representationColor;
        Type monsterType;

        public Scorch()
        {
        }

        protected override char GetRepresentation()
        {
            return 's';
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.Black;
        }

        protected override string GetGameSprite()
        {
            return "scorch";
        }

        protected override string GetUISprite()
        {
            return "scorch";
        }

        public override string Description
        {
            get
            {
                return "Scorch";
            }
        }
    }
}
