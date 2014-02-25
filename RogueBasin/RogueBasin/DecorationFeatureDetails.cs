using libtcodWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    public static class DecorationFeatureDetails
    {
        public enum DecorationFeatures
        {
            Machine
        }
        
        public class Decoration {
            public char representation;
            public Color colour;

            public Decoration(char representation, Color colour) {
                this.representation = representation;
                this.colour = colour;
            }
        }

        public static readonly Dictionary<DecorationFeatures, Decoration> decorationFeatures = new Dictionary<DecorationFeatures,Decoration>();

        static DecorationFeatureDetails()
        {
            decorationFeatures.Add(DecorationFeatures.Machine, new Decoration((char)366, ColorPresets.BlueViolet));
        }
    }
}
