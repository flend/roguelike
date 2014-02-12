using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class Clue : Item
    {
        Color color;
        string id;

        GraphMap.Clue mapClue;

        public Clue(GraphMap.Clue mapClue)
        {
            this.mapClue = mapClue;
            this.id = mapClue.LockedDoor.Id;

            color = ColorPresets.Magenta;

            //Map id to color
            switch (id)
            {
                case "red":
                    color = ColorPresets.Red;
                    break;
                case "green":
                    color = ColorPresets.Green;
                    break;
                case "blue":
                    color = ColorPresets.Blue;
                    break;
                case "yellow":
                    color = ColorPresets.Yellow;
                    break;
            }
        }

        public GraphMap.Clue MapClue
        {
            get { return mapClue;  }
        }

        public override int GetWeight()
        {
            return 10;
        }

        public override string SingleItemDescription
        {
            get { return id + " key"; }
        }

        public override string GroupItemDescription
        {
            get { return id + " key"; }
        }

        public override libtcodWrapper.Color GetColour()
        {
            return color;
        }

        protected override char GetRepresentation()
        {
            return (char)272;
        }

        public override bool UseHiddenName { get { return false; } }

        public override string HiddenSuffix
        {
            get
            {
                return "key";
            }
        }
    }
}
