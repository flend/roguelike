using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class MovieClue : Clue
    {
        string pickupMovie;
        char representation;
        string description;

        GraphMap.Clue mapClue;

        public MovieClue(GraphMap.Clue mapClue, char representation, string pickupMovie, string description) : base(mapClue, ColorPresets.LimeGreen, pickupMovie)
        {
            Setup(mapClue);
            this.pickupMovie = pickupMovie;
            this.representation = representation;
        }

        public override bool OnPickup(Creature pickupCreature)
        {
            Screen.Instance.PlayMovie(pickupMovie, true);

            return true;
        }

        private void Setup(GraphMap.Clue mapClue)
        {
            this.mapClue = mapClue;
        }

        public override string SingleItemDescription
        {
            get { return description; }
        }

        public override string GroupItemDescription
        {
            get { return description; }
        }

        public GraphMap.Clue MapClue
        {
            get { return mapClue;  }
        }

        public override int GetWeight()
        {
            return 10;
        }

        public override libtcodWrapper.Color GetColour()
        {
            return ColorPresets.LimeGreen;
;
        }

        protected override char GetRepresentation()
        {
            return representation;
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
