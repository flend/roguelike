﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class FragGrenade : Item, IEquippableItem
    {
 
        /// <summary>
        /// Equipment slots where we can be equipped
        /// </summary>
        public List<EquipmentSlot> EquipmentSlots
        {
            get
            {
                List<EquipmentSlot> retList = new List<EquipmentSlot>();
                retList.Add(EquipmentSlot.Utility);

                return retList;
            }
        }

        public bool Equip(Creature user)
        {
            LogFile.Log.LogEntryDebug("FragGrenade equipped", LogDebugLevel.Medium);

            //Give player story. Mention level up if one will occur.

            if (Game.Dungeon.Player.PlayItemMovies)
            {
                //Game.Base.PlayMovie("plotbadge", true);
                //Game.Base.PlayMovie("multiattack", false);
            }

            //Messages
            //Game.MessageQueue.AddMessage("A fine short sword - good for slicing and dicing.");

            //Game.Base.PlayMovie("plotbadge", true);

            //Level up?
            //Game.Dungeon.Player.LevelUp();

            //Add move?
            //Game.Dungeon.LearnMove(new SpecialMoves.MultiAttack());
            //Game.Base.PlayMovie("multiattack", false);

            //Add any equipped (actually permanent) effectsf
            //Game.Dungeon.Player.Speed += 10;

            return true;
        }

        public void FireAudio()
        {
            return;
        }

        public void ThrowAudio()
        {
            SoundPlayer.Instance().EnqueueSound("explosion");
        }

        /// <summary>
        /// Throws the item. Can use generic, it's just 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enemyTarget"></param>
        /// <returns></returns>
        public Point ThrowItem(Point target)
        {
            //Stun for 0 rounds
            Game.MessageQueue.AddMessage("The fragmentation grenade explodes!");
            Point dest = Game.Dungeon.ThrowItemGrenadeLike(this, Game.Dungeon.Player.LocationLevel, target, 2.0, Game.Dungeon.Player.ScaleRangedDamage(this, DamageBase()));
            
            return dest;
        }

        public TargettingInfo TargettingInfo()
        {
            return new GrenadeTargettingInfo(RangeThrow(), 2);
        }
        
        /// <summary>
        /// not used in this game
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UnEquip(Creature user)
        {
            LogFile.Log.LogEntryDebug("Frag Grenade unequipped", LogDebugLevel.Low);
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
            get { return "Frag grenade"; }
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        public override string GroupItemDescription
        {
            get { return "Frag grenades"; }
        }

        protected override char GetRepresentation()
        {
            return (char)297;
        }

        protected override string GetGameSprite()
        {
            return "grenade";
        }

        protected override string GetUISprite()
        {
            return "ui-grenade";
        }

        public override System.Drawing.Color GetColour()
        {
            return System.Drawing.Color.Red;
        }

        public int ArmourClassModifier()
        {
            return 0;
        }

        public int DamageBase()
        {
            //1d6
            return 30;
        }

        public double DamageModifier()
        {
            return 0;
        }

        public int HitModifier()
        {
            return 0;
        }

        public bool HasFireAction()
        {
            return false;
        }

        public bool HasMeleeAction()
        {
            return false;
        }


        /// <summary>
        /// Can be thrown
        /// </summary>
        /// <returns></returns>
        public bool HasThrowAction()
        {

            return true;
        }

        /// <summary>
        /// Can be operated
        /// </summary>
        /// <returns></returns>
        public bool HasOperateAction()
        {
            return false;
        }

        public int RemainingAmmo()
        {

            return 0;
        }

        public int MaxAmmo()
        {
            return 0;
        }

        /// <summary>
        /// Fires the item - probably should be a method
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enemyTarget"></param>
        /// <returns></returns>
        public bool FireItem(Point target)
        {
            return false;
        }


        /// <summary>
        /// Operates the item - definitely a method
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enemyTarget"></param>
        /// <returns></returns>
        public bool OperateItem()
        {
            return false;
        }

        /// <summary>
        /// What type of targetting reticle is needed? [for throwing]
        /// </summary>
        /// <returns></returns>
        public virtual TargettingType TargetTypeThrow()
        {
            return TargettingType.Line;
        }

        /// <summary>
        /// What type of targetting reticle is needed? [for firing]
        /// </summary>
        /// <returns></returns>
        public virtual TargettingType TargetTypeFire()
        {
            return TargettingType.Line;
        }

        /// <summary>
        /// Throwing range
        /// </summary>
        /// <returns></returns>
        public int RangeThrow()
        {
            return 6;
        }

        /// <summary>
        /// Firing range
        /// </summary>
        /// <returns></returns>
        public int RangeFire()
        {
            return 5;
        }

        /// <summary>
        /// Noise mag of this weapon on firing
        /// </summary>
        /// <returns></returns>
        public double FireSoundMagnitude()
        {
            return 0.0;
        }

        /// <summary>
        /// Noise mag of this weapon on throwing
        /// </summary>
        /// <returns></returns>
        public double ThrowSoundMagnitude()
        {
            return 0.8;
        }

        /// <summary>
        /// Destroyed on throw
        /// </summary>
        /// <returns></returns>
        public bool DestroyedOnThrow()
        {
            return true;
        }

        /// <summary>
        /// How much damage we do
        /// </summary>
        /// <returns></returns>
        public int MeleeDamage()
        {
            return 0;
        }

        /// <summary>
        /// Spread for shotgun target
        /// </summary>
        /// <returns></returns>
        public virtual double ShotgunSpreadAngle()
        {
            return 0.0;
        }

        public override int ItemCost()
        {
            return 8;
        }

        /// <summary>
        /// Destroyed on use
        /// </summary>
        /// <returns></returns>
        public bool DestroyedOnUse()
        {
            return false;
        }
        public int GetEnergyDrain()
        {
            return 0;
        }
    }
}
