using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Items
{
    public class PotionSightUp : Item, IUseableItem
    {
        bool usedUp;

        public PotionSightUp()
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
            int duration = 30 * Creature.turnTicks + Game.Random.Next(80 * Creature.turnTicks);

            player.AddEffect(new PlayerEffects.SightRadiusUp(duration, 1));



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
            get { return "puckered berry"; }
        }

        public override string GroupItemDescription
        {
            get { return "puckered berries"; }
        }

        public override System.Drawing.Color GetColour()
        {
            return System.Drawing.Color.Plum;
        }

        public bool UsedUp
        {
            set { usedUp = value; }
            get { return usedUp; }
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
