using GraphMap;
using System.Collections.Generic;

namespace RogueBasin.Features
{
    public class SelfDestructPrimeObjective : SimpleObjective
    {
        public SelfDestructPrimeObjective(GraphMap.Objective objective, IEnumerable<Clue> objectiveProducesClues)
            : base(objective, objectiveProducesClues)
        {

        }

        public override bool PlayerInteraction(Player player)
        {
            if (isComplete)
            {
                Game.MessageQueue.AddMessage("The reactor is primed and ready to go.");
                return false;
            }

            Dungeon dungeon = Game.Dungeon;

            bool canDoorBeOpened = ObjectiveCanBeOpenedWithClues(player);

            if (Game.Dungeon.AllLocksOpen)
                canDoorBeOpened = true;


            if (!canDoorBeOpened)
            {
                Game.Base.SystemActions.PlayMovie("reactorlocked", true);
                return false;
            }
            else
            {
                Game.Base.SystemActions.PlayMovie("reactorunlocked", true);

                //Add clues directly into player's inventory
                GivePlayerObjectiveClues(player);

                isComplete = true;
                return true;
            }
        }

        protected override char GetRepresentation()
        {
            return DecorationFeatureDetails.decorationFeatures[DecorationFeatureDetails.DecorationFeatures.Computer3].representation;
        }

        public override string Description
        {
            get
            {
                return "Reactor Override";
            }
        }

        public override string QuestId
        {
            get
            {
                return "reactor-override";
            }
        }

    }
}
