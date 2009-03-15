using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Items
{
    public class Badge : Item, IEquippableItem
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
            LogFile.Log.LogEntryDebug("Crystal badge equipped", LogDebugLevel.Medium);

            Game.Dungeon.PlotItemsFound++;

            //This is plot equipment

            //Give player story. Mention level up if one will occur.

            if (Game.Dungeon.Player.PlayItemMovies)
            {
                Screen.Instance.PlayMovie("plotbadge", true);
                Screen.Instance.PlayMovie("multiattack", false);
            }

            //Messages
            Game.MessageQueue.AddMessage("Learnt MultiAttack!");

            //Screen.Instance.PlayMovie("plotbadge", true);

            //Level up?
            //Game.Dungeon.Player.LevelUp();

            //Add move?
            Game.Dungeon.LearnMove(new SpecialMoves.MultiAttack());
            //Screen.Instance.PlayMovie("multiattack", false);

            //Add any equipped (actually permanent) effects
            //Game.Dungeon.Player.Speed += 10;

            return true;
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UnEquip(Creature user)
        {
            LogFile.Log.LogEntryDebug("Crystal badge unequipped", LogDebugLevel.Low);
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
            get { return "steel badge"; }
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        public override string GroupItemDescription
        {
            get { return "steel badge"; }
        }

        protected override char GetRepresentation()
        {
            return 'x';
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