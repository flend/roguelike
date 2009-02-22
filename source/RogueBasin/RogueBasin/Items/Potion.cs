using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Items
{
    class Potion : Item
    {
        bool usedUp;

        public Potion()
        {
            usedUp = false;
        }

        public override bool Use(Creature user)
        {
            //Currently healing is implemented as a player effect so we need to check the user is a player
            Player player = user as Player;

            //Not a player
            if (player == null)
            {
                return false;
            }

            //Apply the healing effect to the player
            player.AddEffect(new PlayerEffects.Healing(player, 10));

            //Add a message
            Game.MessageQueue.AddMessage("You drink the potion");

            //This uses up the potion
            usedUp = true;

            return true;
        }

        public override int GetWeight()
        {
            return 10;
        }

        public override string SingleItemDescription
        {
            get { return "potion"; }
        }

        public override string GroupItemDescription
        {
            get { return "potions"; }
        }

        public override bool UsedUp
        {
            get { return usedUp; }
        }
    }
}
