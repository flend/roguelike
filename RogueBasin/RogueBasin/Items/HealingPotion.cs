using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    /// <summary>
    /// Plot item
    /// </summary>
    public class HealingPotion : Item, IUseableItem
    {
        bool usedUp;

        /// <summary>
        /// not used in this game
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public bool CanBeEquippedInSlot(EquipmentSlot slot)
        {
            if (slot == EquipmentSlot.Weapon)
                return true;

            return false;
        }
        /// <summary>
        /// not used in this game
        /// </summary>
        public List<EquipmentSlot> EquipmentSlots
        {
            get
            {
                List<EquipmentSlot> retList = new List<EquipmentSlot>();
                retList.Add(EquipmentSlot.Weapon);

                return retList;
            }
        }

        public bool Equip(Creature user)
        {
            //LogFile.Log.LogEntryDebug("Glove equipped", LogDebugLevel.Medium);

            //This is plot equipment
            //Game.Dungeon.Player.PlotItemsFound++;

            //Level up?
            //Game.Dungeon.Player.LevelUp();

            //Add move?
            //Game.Dungeon.LearnMove(new SpecialMoves.VaultBackstab());

            //Play movies if set
            if (Game.Dungeon.Player.PlayItemMovies)
            {
                //Game.Base.SystemActions.PlayMovie("plotglove", true);
                //Game.Base.SystemActions.PlayMovie("vaultbackstab", false);
            }

            //Messages
            Game.MessageQueue.AddMessage("A sparkling blue healing potion - it's literally overflowing. What luck!");
            //Game.MessageQueue.AddMessage("Learnt Vault Backstab!");

            return true;
        }

        /// <summary>
        /// For a test let the glove me useable
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool Use(Creature user)
        {
            //Currently healing is implemented as a player effect so we need to check the user is a player
            Player player = user as Player;

            //Not a player
            if (player == null)
            {
                return false;
            }


            //Not in a dungeon
            if (player.LocationLevel < 2)
            {
                Game.MessageQueue.AddMessage("You want to save your items for the dungeon.");
                return false;
            }

            if (usedUp)
            {
                Game.MessageQueue.AddMessage("You've already used the potion this adventure.");
                return false;
            }
            Game.MessageQueue.AddMessage("You drink the healing potion and the warming liquid sloshes down your throat.");

            //Apply the healing effect to the player
            int healing = player.MaxHitpoints - player.Hitpoints;
            player.AddEffect(new PlayerEffects.Healing(healing));

            //Add a message


            //This uses up the potion
            usedUp = true;

            return true;
        }

        public bool UsedUp
        {
            set { usedUp = value; }
            get { return usedUp; }
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UnEquip(Creature user)
        {
            LogFile.Log.LogEntryDebug("Healing potion unequipped", LogDebugLevel.Low);
            return true;
        }
        /// <summary>
        /// not used in this game
        /// </summary>
        public override int GetWeight()
        {
            return 50;
        }

        public override string SingleItemDescription
        {
            get { return "healing potion"; }
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        public override string GroupItemDescription
        {
            get { return "healing potion"; }
        }

        public override System.Drawing.Color GetColour()
        {
            return System.Drawing.Color.SkyBlue;
        }


        protected override char GetRepresentation()
        {
            return '!';
        }

        public int ArmourClassModifier()
        {
            return 0;
        }

        public int DamageBase()
        {
            //1d6
            return 0;
        }

        public double DamageModifier()
        {
            return 0;
        }

        public int HitModifier()
        {
            return 0;
        }

    }
}
