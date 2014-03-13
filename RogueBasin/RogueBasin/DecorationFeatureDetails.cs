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
            Machine, Machine2, Bucket, Filing, Filing2, CoffeePC,
            SquarePC, DesktopPC, Stool, Chair1, Chair2, EggChair,
            Instrument1, Instrument2, Instrument3, Plant1, Plant2,
            HighTechBench, Plant3, Safe1, Safe2, Egg1, Egg2, Egg3,
            Bone, Skeleton, Bin, HumanCorpse, HumanCorpse2
        }
        
        public class Decoration {
            public char representation;
            public Color colour;
            public bool isBlocking;

            public Decoration(char representation, Color colour, bool isBlocking) {
                this.representation = representation;
                this.colour = colour;
                this.isBlocking = isBlocking;
            }
        }

        public static readonly Dictionary<DecorationFeatures, Decoration> decorationFeatures = new Dictionary<DecorationFeatures,Decoration>();

        static DecorationFeatureDetails()
        {
            var boneColor = ColorPresets.Ivory;
            var blockingColor = ColorPresets.BlueViolet;
            var nonBlockingColor = ColorPresets.RosyBrown;

            decorationFeatures.Add(DecorationFeatures.Bone, new Decoration((char)314, boneColor, false));
            decorationFeatures.Add(DecorationFeatures.Skeleton, new Decoration((char)315, boneColor, false));
            decorationFeatures.Add(DecorationFeatures.Bin, new Decoration((char)349, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Filing, new Decoration((char)350, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Filing2, new Decoration((char)351, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.CoffeePC, new Decoration((char)365, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Machine, new Decoration((char)366, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Machine2, new Decoration((char)367, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.SquarePC, new Decoration((char)381, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.DesktopPC, new Decoration((char)382, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Stool, new Decoration((char)383, nonBlockingColor, false));
            decorationFeatures.Add(DecorationFeatures.Chair1, new Decoration((char)397, nonBlockingColor, false));
            decorationFeatures.Add(DecorationFeatures.Chair2, new Decoration((char)398, nonBlockingColor, false));
            decorationFeatures.Add(DecorationFeatures.EggChair, new Decoration((char)399, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Instrument1, new Decoration((char)413, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Instrument2, new Decoration((char)414, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Instrument3, new Decoration((char)415, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Plant1, new Decoration((char)429, nonBlockingColor, false));
            decorationFeatures.Add(DecorationFeatures.Plant2, new Decoration((char)430, nonBlockingColor, false));
            decorationFeatures.Add(DecorationFeatures.HighTechBench, new Decoration((char)431, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Plant3, new Decoration((char)445, nonBlockingColor, false));
            decorationFeatures.Add(DecorationFeatures.Safe1, new Decoration((char)446, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Safe2, new Decoration((char)447, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Egg1, new Decoration((char)445, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Egg2, new Decoration((char)446, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Egg3, new Decoration((char)447, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.HumanCorpse, new Decoration((char)510, boneColor, false));
            decorationFeatures.Add(DecorationFeatures.HumanCorpse2, new Decoration((char)511, boneColor, false));
            

        }
    }
}
