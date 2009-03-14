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

            Game.Dungeon.PlotItemsFound++;


            //This is plot equipment

            //Give player story. Mention level up if one will occur.

            Screen.Instance.PlayMovie("plotglove", true);

            //Level up?
            Game.Dungeon.Player.LevelUp();

            //Add move?
            Game.Dungeon.LearnMove(new SpecialMoves.VaultBackstab());
            Screen.Instance.PlayMovie("vaultbackstab", false);

            return true;
        }

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
            return '"';
        }

        public int ArmourClassModifier()
        {
            return 1;
        }

        public int DamageBase()
        {
            //1d6
            return 6;
        }

        public int DamageModifier()
        {
            return 1;
        }

        public int HitModifier()
        {
            return 2;
        }

    }
}
