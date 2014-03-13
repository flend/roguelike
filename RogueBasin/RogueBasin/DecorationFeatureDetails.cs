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
            bool isBlocking;

            public Decoration(char representation, Color colour, bool isBlocking) {
                this.representation = representation;
                this.colour = colour;
                this.isBlocking = isBlocking;
            }
        }

        public static readonly Dictionary<DecorationFeatures, Decoration> decorationFeatures = new Dictionary<DecorationFeatures,Decoration>();

        static DecorationFeatureDetails()
        {
            decorationFeatures.Add(DecorationFeatures.Bone, new Decoration((char)314, ColorPresets.BlueViolet, false));
            decorationFeatures.Add(DecorationFeatures.Skeleton, new Decoration((char)315, ColorPresets.BlueViolet, false));
            decorationFeatures.Add(DecorationFeatures.Bin, new Decoration((char)349, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Filing, new Decoration((char)350, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Filing2, new Decoration((char)351, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.CoffeePC, new Decoration((char)365, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Machine, new Decoration((char)366, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Machine2, new Decoration((char)367, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.SquarePC, new Decoration((char)381, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.DesktopPC, new Decoration((char)382, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Stool, new Decoration((char)383, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Chair1, new Decoration((char)397, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Chair2, new Decoration((char)398, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.EggChair, new Decoration((char)399, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Instrument1, new Decoration((char)413, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Instrument2, new Decoration((char)414, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Instrument3, new Decoration((char)415, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Plant1, new Decoration((char)429, ColorPresets.BlueViolet, false));
            decorationFeatures.Add(DecorationFeatures.Plant2, new Decoration((char)430, ColorPresets.BlueViolet, false));
            decorationFeatures.Add(DecorationFeatures.HighTechBench, new Decoration((char)431, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Plant3, new Decoration((char)445, ColorPresets.BlueViolet, false));
            decorationFeatures.Add(DecorationFeatures.Safe1, new Decoration((char)446, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Safe2, new Decoration((char)447, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Egg1, new Decoration((char)445, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Egg2, new Decoration((char)446, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.Egg3, new Decoration((char)447, ColorPresets.BlueViolet, true));
            decorationFeatures.Add(DecorationFeatures.HumanCorpse, new Decoration((char)510, ColorPresets.Crimson, true));
            decorationFeatures.Add(DecorationFeatures.HumanCorpse2, new Decoration((char)411, ColorPresets.Crimson, true));
            

        }
    }
}
