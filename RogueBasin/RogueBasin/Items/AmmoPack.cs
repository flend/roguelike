using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class AmmoPack : UseableItemUseOnPickup
    {
        bool usedUp;

        const int decayDuration = 250;

        public AmmoPack()
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
            
            //Game.MessageQueue.AddMessage("Ammo pack applied.");

            Game.Dungeon.Player.AddAmmoToCurrentWeapon();

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
            get { return "Ammo pack"; }
        }

        public override string GroupItemDescription
        {
            get { return "Ammo packs"; }
        }

        public override libtcodWrapper.Color GetColour()
        {
            return ColorPresets.Lime;
        }

        protected override char GetRepresentation()
        {
            return (char)283;
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
