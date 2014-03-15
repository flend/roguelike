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
            Bone, Skeleton, Bin, HumanCorpse, HumanCorpse2, Spike,
            Pillar1, Pillar2, Pillar3, Pillar4, Crate, CorpseinGoo,
            Screen1, Screen2, Screen3, Screen4, AutomatMachine, MedicalAutomat,
            Screen5, MachinePart1, Computer1, MachinePart2, MachinePart3, Statue1, Statue2,
            Statue3, Statue4, Screen6, Screen7, Screen8,
            CleaningDevice, WheelChair, ShopAutomat1, ShopAutomat2, Screen9,
            Computer2, Computer3
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
            var boneColor = ColorPresets.Khaki;
            var blockingColor = ColorPresets.BlueViolet;
            var nonBlockingColor = ColorPresets.RosyBrown;
            var corpseColor = ColorPresets.Maroon;

            decorationFeatures.Add(DecorationFeatures.Bone, new Decoration((char)314, boneColor, false));
            decorationFeatures.Add(DecorationFeatures.Skeleton, new Decoration((char)315, boneColor, false));
            decorationFeatures.Add(DecorationFeatures.CleaningDevice, new Decoration((char)324, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.WheelChair, new Decoration((char)325, blockingColor, true));
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
            decorationFeatures.Add(DecorationFeatures.Egg1, new Decoration((char)461, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Egg2, new Decoration((char)462, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Egg3, new Decoration((char)463, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.HumanCorpse, new Decoration((char)510, corpseColor, false));
            decorationFeatures.Add(DecorationFeatures.HumanCorpse2, new Decoration((char)511, corpseColor, false));
            decorationFeatures.Add(DecorationFeatures.Spike, new Decoration((char)523, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Pillar1, new Decoration((char)524, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Pillar2, new Decoration((char)525, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Pillar3, new Decoration((char)526, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Crate, new Decoration((char)527, nonBlockingColor, false));
            decorationFeatures.Add(DecorationFeatures.CorpseinGoo, new Decoration((char)539, corpseColor, false));
            decorationFeatures.Add(DecorationFeatures.Screen1, new Decoration((char)540, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Screen2, new Decoration((char)541, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Screen3, new Decoration((char)542, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Screen4, new Decoration((char)543, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.AutomatMachine, new Decoration((char)554, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.MedicalAutomat, new Decoration((char)555, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Computer1, new Decoration((char)556, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.MachinePart1, new Decoration((char)557, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.MachinePart2, new Decoration((char)558, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.MachinePart3, new Decoration((char)559, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Statue1, new Decoration((char)572, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Statue2, new Decoration((char)573, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Statue3, new Decoration((char)574, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Statue4, new Decoration((char)575, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.ShopAutomat1, new Decoration((char)586, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.ShopAutomat2, new Decoration((char)587, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Screen6, new Decoration((char)588, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Screen7, new Decoration((char)589, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Screen8, new Decoration((char)590, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Screen9, new Decoration((char)591, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Computer2, new Decoration((char)605, blockingColor, true));
            decorationFeatures.Add(DecorationFeatures.Computer3, new Decoration((char)606, blockingColor, true));

        }
    }
}
