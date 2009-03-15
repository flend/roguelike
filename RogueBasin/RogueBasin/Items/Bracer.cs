using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Items
{
    public class Bracer : Item, IEquippableItem
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
            LogFile.Log.LogEntryDebug("Bracer equipped", LogDebugLevel.Medium);

            Game.Dungeon.PlotItemsFound++;


            //This is plot equipment

            //Give player story. Mention level up if one will occur.
            if (Game.Dungeon.PlayItemMovies)
            {
                Screen.Instance.PlayMovie("plotbracer", true);
                Screen.Instance.PlayMovie("closequarters", false);
            }

            //Messages
            //Game.MessageQueue.AddMessage("Levelled up!");
            Game.MessageQueue.AddMessage("Learnt Close Quarters!");

            //Screen.Instance.PlayMovie("plotbracer", true);

            //Level up?
            //Game.Dungeon.Player.LevelUp();

            //Add move?
            Game.Dungeon.LearnMove(new SpecialMoves.CloseQuarters());
            //Screen.Instance.PlayMovie("closequarters", false);

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
            LogFile.Log.LogEntryDebug("Bracer unequipped", LogDebugLevel.Low);
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
            get { return "leather bracer"; }
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        public override string GroupItemDescription
        {
            get { return "leather bracers"; }
        }

        protected override char GetRepresentation()
        {
            return '~';
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
            return 2;
        }

        public int HitModifier()
        {
            return 1;
        }
    }
}
