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
                Game.Base.PlayMovie("selfdestructlocked", true);
                return false;
            }
            else
            {
                Game.Base.PlayMovie("selfdestructunlocked", true);

                //Add clues directly into player's inventory
                GivePlayerObjectiveClues(player);

                //Restock the flight deck and lower atrium levels
                Game.Dungeon.MonsterPlacement.CreateMonstersForLevelsAndPopulateInDungeon(Game.Dungeon.MapState, Game.Dungeon.Difficulty, 2, 12);
                Game.Dungeon.MonsterPlacement.CreateMonstersForLevelsAndPopulateInDungeon(Game.Dungeon.MapState, Game.Dungeon.Difficulty, 4, 12);

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
                return "Self Destruct";
            }
        }

        public override string QuestId
        {
            get
            {
                return "self-destruct";
            }
        }

    }
}
