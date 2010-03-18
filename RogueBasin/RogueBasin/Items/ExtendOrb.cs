using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    /// <summary>
    /// Plot item
    /// </summary>
    public class ExtendOrb : Item, IEquippableItem
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
                //Screen.Instance.PlayMovie("plotglove", true);
                //Screen.Instance.PlayMovie("vaultbackstab", false);
            }

            //Messages
            Game.MessageQueue.AddMessage("Your reflection in the orb seems to stretch away forever. All around you distances distort and become shorter.");
            //Game.MessageQueue.AddMessage("Learnt Vault Backstab!");

            return true;
        }

        
        /// <summary>
        /// not used in this game
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UnEquip(Creature user)
        {
            LogFile.Log.LogEntryDebug("Extend orb unequipped", LogDebugLevel.Low);
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
            get { return "distant orb"; }
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        public override string GroupItemDescription
        {
            get { return "distant orb"; }
        }

        public override libtcodWrapper.Color GetColour()
        {
            return ColorPresets.Turquoise;
        }


        protected override char GetRepresentation()
        {
            return (char)233;;
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
