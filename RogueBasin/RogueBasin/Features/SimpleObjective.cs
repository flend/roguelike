﻿using GraphMap;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin.Features
{
    public class SimpleObjective : UseableFeature
    {
        protected GraphMap.Objective obj;
        protected bool isComplete;
        protected IEnumerable<Clue> objectiveProducesClues;

        public SimpleObjective(GraphMap.Objective objective, IEnumerable<Clue> objectiveProducesClues)
        {
            this.obj = objective;
            this.objectiveProducesClues = objectiveProducesClues;
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
                Game.MessageQueue.AddMessage("You can't operate the system. You need " + obj.NumCluesRequired + " " + obj.Id + " keys.");
                return false;
            }
            else
            {
                //Add clues directly into player's inventory
                GivePlayerObjectiveClues(player);

                var keysYouGet = new StringBuilder();
                foreach (var id in objectiveProducesClues.Select(c => c.LockedDoor != null ? c.LockedDoor.Id : c.LockedObjective.Id))
                {
                    keysYouGet.Append(id);
                    keysYouGet.Append(" ");
                }
                Game.MessageQueue.AddMessage("You operate the system with your " + obj.NumCluesRequired + " " + obj.Id + " keys. You get: " + keysYouGet.ToString());
                isComplete = true;
                return true;
            }
        }

        protected void GivePlayerObjectiveClues(Player player)
        {
            foreach (var producedClue in objectiveProducesClues)
            {
                var clue = new Items.Clue(producedClue);
                player.Inventory.AddItemNotFromDungeon(clue);
            }
        }

        protected bool ObjectiveCanBeOpenedWithClues(Player player)
        {
            var allPlayerClueItems = player.Inventory.GetItemsOfType<Items.Clue>();
            var allPlayerClues = allPlayerClueItems.Select(i => i.MapClue);

            bool canDoorBeOpened = obj.CanBeOpenedWithClues(allPlayerClues);
            return canDoorBeOpened;
        }

        protected override char GetRepresentation()
        {
            return (char)365;
        }

        public override System.Drawing.Color RepresentationColor()
        {
            return System.Drawing.Color.LimeGreen;
        }

        public override System.Drawing.Color RepresentationBackgroundColor()
        {
            if(!isComplete)
                return System.Drawing.Color.Aqua;
            return System.Drawing.Color.Black;
        }
    }
}
