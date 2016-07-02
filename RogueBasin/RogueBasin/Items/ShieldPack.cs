﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class ShieldPack : UseableItemUseOnPickup
    {
        bool usedUp;

        const int decayDuration = 250;

        public ShieldPack()
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
            
            Game.MessageQueue.AddMessage("Shield pack applied.");

            //Apply the healing effect to the player

            var maxShield = player.MaxShield;

            var shieldBonus = (int)Math.Ceiling(maxShield / 5.0);

            player.AddShield(shieldBonus);
                        usedUp = true;

            return true;
        }

        public override bool OnDrop(Creature droppingCreature)
        {
            this.AddEffect(new ItemEffects.Decay(decayDuration));

            return true;
        }

        public override bool UsedUp
        {
            get { return usedUp;  }
            set { usedUp = value;  }
        }

        public override int GetWeight()
        {
            return 10;
        }

        public override string SingleItemDescription
        {
            get { return "shield pack"; }
        }

        public override string GroupItemDescription
        {
            get { return "shield packs"; }
        }

        public override System.Drawing.Color GetColour()
        {
            return System.Drawing.Color.Lime;
        }

        protected override char GetRepresentation()
        {
            return (char)561;
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
