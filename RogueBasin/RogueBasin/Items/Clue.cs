using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class Clue : Item
    {
        System.Drawing.Color color;
        string id = "";
        string parentId;

        GraphMap.Clue mapClue;

        public Clue(GraphMap.Clue mapClue)
        {
            Setup(mapClue);

            color = System.Drawing.Color.Magenta;
            /*
            //Map id to color
            switch (parentId)
            {
                case "red":
                    color = System.Drawing.Color.Red;
                    break;
                case "green":
                    color = System.Drawing.Color.Green;
                    break;
                case "blue":
                    color = System.Drawing.Color.Blue;
                    break;
                case "yellow":
                    color = System.Drawing.Color.Yellow;
                    break;
                case "escape":
                    color = System.Drawing.Color.Yellow;
                    break;
                case "self-destruct":
                    color = System.Drawing.Color.LimeGreen;
                    break;
            }*/
        }

        public Clue(GraphMap.Clue mapClue, System.Drawing.Color lockColour, string id)
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
            get { return id; }
        }

        public override string SingleItemDescription
        {
            get { return id; }
        }

        public override string GroupItemDescription
        {
            get { return id; }
        }

        public override System.Drawing.Color GetColour()
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

        public override string QuestId
        {
            get
            {
                return "id-" + id + " (" + parentId + ")";
            }
        }
    }
}
