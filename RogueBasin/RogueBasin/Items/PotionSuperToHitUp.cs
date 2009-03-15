using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Items
{
    public class PotionSuperToHitUp : Item, IUseableItem
    {
        bool usedUp;

        public PotionSuperToHitUp()
        {
            usedUp = false;
        }

        public bool Use(Creature user)
        {
            //Currently healing is implemented as a player effect so we need to check the user is a player
            Player player = user as Player;

            //Not a player
            if (player == null)
            {
                return false;
            }

            //Apply the healing effect to the player
            //Duration note 100 is normally 1 turn for a non-sped up player

            int duration = 1000 + Game.Random.Next(2000);
            int toHitUp = 10 + Game.Random.Next(10);

            player.AddEffect(new PlayerEffects.ToHitUp(player, duration, toHitUp));

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

        public bool UsedUp
        {
            get { return usedUp; }
        }

        protected override char GetRepresentation()
        {
            return '!';
        }

        public override bool UseHiddenName { get { return true; } }

        public override string HiddenSuffix
        {
            get
            {
                return "vial";
            }
        }
    }
}
