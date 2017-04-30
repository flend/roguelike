using GraphMap;
using System.Collections.Generic;

namespace RogueBasin.Features
{
    public class AntennaeObjective : SimpleObjective
    {
        public AntennaeObjective(GraphMap.Objective objective, IEnumerable<Clue> objectiveProducesClues)
            : base(objective, objectiveProducesClues)
        {

        }

        public override bool PlayerInteraction(Player player)
        {
            if (isComplete)
            {
                Game.MessageQueue.AddMessage("This system is no longer functioning.");
                return false;
            }

            Dungeon dungeon = Game.Dungeon;

            bool canDoorBeOpened = ObjectiveCanBeOpenedWithClues(player);

            if (!canDoorBeOpened)
            {
                Game.Base.SystemActions.PlayMovie("antennaelocked", true);
                return false;
            }
            else
            {
                Game.Base.SystemActions.PlayMovie("antennaeunlocked", true);

                //Add clues directly into player's inventory
                GivePlayerObjectiveClues(player);

                isComplete = true;
                return true;
            }
        }

        protected override char GetRepresentation()
        {
            return (char)570;
        }

        public override string Description
        {
            get
            {
                return "Comms Antennae";
            }
        }

        public override string QuestId
        {
            get
            {
                return "comms-antennae";
            }
        }

    }
}
