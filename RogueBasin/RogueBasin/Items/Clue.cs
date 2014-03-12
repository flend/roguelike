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
            string parentId;

            if (mapClue.LockedDoor != null)
            {
                this.id = "door-clue-" + mapClue.LockedDoor.Id;
                parentId = mapClue.LockedDoor.Id;
            }
            else
            {
                this.id = "obj-clue-" + mapClue.LockedObjective.Id;
                parentId = mapClue.LockedObjective.Id;
            }

            color = ColorPresets.Magenta;

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
