using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Items
{
    public class PotionSpeedUp : Item, IUseableItem
    {
        bool usedUp;

        public PotionSpeedUp()
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

            //Apply the speed up effect to the player
            //Duration note 100 is normally 1 turn for a non-sped up player

            int duration = 10 * Creature.turnTicks + Game.Random.Next(10 * Creature.turnTicks);
            int speedUpAmount = 50 + Game.Random.Next(20);

            player.AddEffect(new PlayerEffects.SpeedUp(duration, speedUpAmount));


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
            get { return "long berry"; }
        }

        public override string GroupItemDescription
        {
            get { return "long berries"; }
        }

        public bool UsedUp
        {
            set { usedUp = value; }
            get { return usedUp; }
        }

        public override System.Drawing.Color GetColour()
        {
            return System.Drawing.Color.Purple;
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
