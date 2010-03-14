using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class Potion : Item, IUseableItem
    {
        bool usedUp;

        public Potion()
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
            
            Game.MessageQueue.AddMessage("You eat the berry.");

            //Apply the healing effect to the player

            int delta = (int)Math.Ceiling(Game.Dungeon.Player.MaxHitpointsStat / 4.0);
            if (delta < 10)
                delta = 10;

            int healing = 10 + Game.Random.Next(delta);
            player.AddEffect(new PlayerEffects.Healing(player, healing));

            //Add a message
            

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
            get { return "green berry"; }
        }

        public override string GroupItemDescription
        {
            get { return "green berries"; }
        }

        public override libtcodWrapper.Color GetColour()
        {
            return ColorPresets.Green;
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
