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
        string parentId;

        GraphMap.Clue mapClue;

        public Clue(GraphMap.Clue mapClue)
        {
            Setup(mapClue);

            color = ColorPresets.Magenta;
            /*
            //Map id to color
            switch (parentId)
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
                case "escape":
                    color = ColorPresets.Yellow;
                    break;
                case "self-destruct":
                    color = ColorPresets.LimeGreen;
                    break;
            }*/
        }

        public Clue(GraphMap.Clue mapClue, Color lockColour, string id)
        {
            Setup(mapClue);

            color = lockColour;
            this.id = id;

        }

        private void Setup(GraphMap.Clue mapClue)
        {
            this.mapClue = mapClue;

            if (mapClue.LockedDoor != null)
            {
                parentId = mapClue.LockedDoor.Id;
            }
            else
            {
                parentId = mapClue.LockedObjective.Id;
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

        public string ClueDescription
        {
            get { return id + "(" + parentId + ")"; }
        }

        public override string SingleItemDescription
        {
            get { return id; }
        }

        public override string GroupItemDescription
        {
            get { return id; }
        }

        public override libtcodWrapper.Color GetColour()
        {
            return color;
        }

        protected override char GetRepresentation()
        {
            return (char)307;
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
