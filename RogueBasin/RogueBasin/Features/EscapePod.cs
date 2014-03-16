using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin.Features
{
    public class EscapePod : UseableFeature
    {
        int destLevel;
        Point destLocation;

        public EscapePod()
        {

        }

        /// <summary>
        /// Escape pod end game
        /// </summary>
        public override bool PlayerInteraction(Player player)
        {
            Dungeon dungeon = Game.Dungeon;

            Game.MessageQueue.AddMessage("You take the escape pod. The game ends");

            dungeon.EndOfGame(true);

            return true;
        }

        protected override char GetRepresentation()
        {
            return (char)538;
        }
    }
}
