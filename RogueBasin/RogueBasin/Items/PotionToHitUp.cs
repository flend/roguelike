﻿namespace RogueBasin.Items
{
    public class PotionToHitUp : Item, IUseableItem
    {
        bool usedUp;

        public PotionToHitUp()
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

            //Add a message
            Game.MessageQueue.AddMessage("You eat the berry.");

            //Apply the healing effect to the player
            //Duration note 100 is normally 1 turn for a non-sped up player

            int duration = 10 * Creature.turnTicks + Game.Random.Next(20 * Creature.turnTicks);
            int toHitUp = 1 + Game.Random.Next(3);

            player.AddEffect(new PlayerEffects.ToHitUp(duration, toHitUp));



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
            get { return "red berry"; }
        }

        public override string GroupItemDescription
        {
            get { return "red berries"; }
        }

        public bool UsedUp
        {
            set { usedUp = value; }
            get { return usedUp; }
        }

        public override System.Drawing.Color GetColour()
        {
            return System.Drawing.Color.MediumVioletRed;
        }

        protected override char GetRepresentation()
        {
            return (char)236;
        }

        public override bool UseHiddenName { get { return false; } }

        public override string HiddenSuffix
        {
            get
            {
                return "vial";
            }
        }
    }
}
