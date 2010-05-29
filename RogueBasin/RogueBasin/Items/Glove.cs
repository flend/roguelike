using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Items
{
    /// <summary>
    /// Plot item
    /// </summary>
    public class Glove : Item, IEquippableItem
    {

        /// <summary>
        /// not used in this game
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public bool CanBeEquippedInSlot(EquipmentSlot slot)
        {
            if (slot == EquipmentSlot.RightHand)
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
                retList.Add(EquipmentSlot.RightHand);

                return retList;
            }
        }

        public bool Equip(Creature user)
        {
            LogFile.Log.LogEntryDebug("Glove equipped", LogDebugLevel.Medium);

            //This is plot equipment
            //Game.Dungeon.Player.PlotItemsFound++;

            //Level up?
            //Game.Dungeon.Player.LevelUp();

            //Add move?
            //Game.Dungeon.LearnMove(new SpecialMoves.VaultBackstab());

            //Play movies if set
            if (Game.Dungeon.Player.PlayItemMovies)
            {
                //Screen.Instance.PlayMovie("plotglove", true);
                //Screen.Instance.PlayMovie("vaultbackstab", false);
            }

            //Messages
            Game.MessageQueue.AddMessage("This glove fits neatly on to your hand. The glove seems to respond to your moves - you feel a warm energy charging.");
            //Game.MessageQueue.AddMessage("Learnt Vault Backstab!");

            return true;
        }

        /*
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

            Game.MessageQueue.AddMessage("You drink the potion.");

            //Apply the healing effect to the player
            int healing = 10 + Game.Random.Next(10);
            player.AddEffect(new PlayerEffects.Healing(player, healing));

            //Add a message


            //This uses up the potion
            usedUp = true;

            return true;
        }

        public bool UsedUp
        {
            set { usedUp = value; }
            get { return usedUp; }
        }*/

        /// <summary>
        /// not used in this game
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UnEquip(Creature user)
        {
            LogFile.Log.LogEntryDebug("Glove unequipped", LogDebugLevel.Low);
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
            get { return "ornate glove"; }
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        public override string GroupItemDescription
        {
            get { return "ornate gloves"; }
        }

        protected override char GetRepresentation()
        {
            return (char)20;
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

        public int DamageModifier()
        {
            return 0;
        }

        public int HitModifier()
        {
            return 0;
        }

    }
}
