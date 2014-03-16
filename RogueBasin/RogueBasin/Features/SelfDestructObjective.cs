using GraphMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin.Features
{
    public class SelfDestructObjective : SimpleObjective
    {
        public SelfDestructObjective(GraphMap.Objective objective, IEnumerable<Clue> objectiveProducesClues)
            : base(objective, objectiveProducesClues)
        {

        }

        public override bool PlayerInteraction(Player player)
        {
            if (isComplete)
            {
                Game.MessageQueue.AddMessage("What are you waiting around for!!! Get to the escape pods.");
                return false;
            }

            Dungeon dungeon = Game.Dungeon;

            bool canDoorBeOpened = ObjectiveCanBeOpenedWithClues(player);

            if (Game.Dungeon.AllLocksOpen)
                canDoorBeOpened = true;

            if (!canDoorBeOpened)
            {
                Screen.Instance.PlayMovie("selfdestructlocked", true);
                return false;
            }
            else
            {
                Screen.Instance.PlayMovie("selfdestructunlocked", true);

                //Add clues directly into player's inventory
                GivePlayerObjectiveClues(player);

                //Restock the flight deck and lower atrium levels
                Game.Dungeon.CreateMonstersForLevels(Game.Dungeon.MapInfo, 2, 6);
                Game.Dungeon.CreateMonstersForLevels(Game.Dungeon.MapInfo, 4, 6);

                isComplete = true;
                return true;
            }
        }

        protected override char GetRepresentation()
        {
            return DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer3].representation;
        }

    }
}
