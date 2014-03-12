using libtcodWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin.Features
{
    public class SimpleObjective : UseableFeature
    {
        GraphMap.Objective obj;
        bool isComplete;
        IEnumerable<string> objectiveProducesKeyIds = new List<string>();

        public SimpleObjective(GraphMap.Objective objective)
        {
            this.obj = objective;
        }

        public SimpleObjective(GraphMap.Objective objective, IEnumerable<string> objectiveProducesKeyIds)
        {
            this.obj = objective;
            this.objectiveProducesKeyIds = objectiveProducesKeyIds;
        }

        public override bool PlayerInteraction(Player player)
        {
            if (isComplete)
            {
                Game.MessageQueue.AddMessage("This system is no longer functioning.");
                return false;
            }

            Dungeon dungeon = Game.Dungeon;
            
            var allPlayerClueItems = player.Inventory.GetItemsOfType<Items.Clue>();
            var allPlayerClues = allPlayerClueItems.Select(i => i.MapClue);

            bool canDoorBeOpened = obj.CanBeOpenedWithClues(allPlayerClues);

            if (!canDoorBeOpened)
            {
                Game.MessageQueue.AddMessage("You can't operate the system. You need " + obj.NumCluesRequired + " " + obj.Id + " keys.");
                return false;
            }
            else
            {
                var keysYouGet = new StringBuilder();
                foreach(var id in objectiveProducesKeyIds) {
                    keysYouGet.Append(id);
                    keysYouGet.Append(" ");
                }
                Game.MessageQueue.AddMessage("You operate the system with your " + obj.NumCluesRequired + " " + obj.Id + " keys. You get: " + keysYouGet.ToString());
                isComplete = true;
                return true;
            }
        }

        protected override char GetRepresentation()
        {
            return (char)365;
        }

        public override Color RepresentationColor()
        {
            return ColorPresets.LimeGreen;
        }

        public override Color RepresentationBackgroundColor()
        {
            if(!isComplete)
                return ColorPresets.Aqua;
            return ColorPresets.Black;
        }
    }
}
