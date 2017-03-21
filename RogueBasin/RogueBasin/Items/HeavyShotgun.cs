﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class HeavyShotgun : ShotgunTypeWeapon, IEquippableItem
    {
        public HeavyShotgun()
        {
        }

        /// <summary>
        /// Fires the item - probably should be a method
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enemyTarget"></param>
        /// <returns></returns>
        public bool FireItem(Point target)
        {
            Game.Dungeon.Combat.FireShotgunWeapon(target, this, 150, 15, 15);

            //Remove 1 ammo
            Ammo--;

            return true;
        }

        public TargettingInfo TargettingInfo()
        {
            return new ShotgunTargettingInfo(RangeFire(), ShotgunSpreadAngle(), RangeFire());
        }

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
            LogFile.Log.LogEntryDebug("Heavy Shotgun equipped", LogDebugLevel.Medium);

            return true;
        }

        public bool HasMeleeAction()
        {
            return false;
        }

        public bool HasFireAction()
        {
            return true;
        }

        /// <summary>
        /// Throws the item
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enemyTarget"></param>
        /// <returns></returns>
        public Point ThrowItem(Point target)
        {
            //Stun for 3 turns
            return Game.Dungeon.Player.ThrowItemGeneric(this, target, 3, true);
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UnEquip(Creature user)
        {
            LogFile.Log.LogEntryDebug("Heavy Shotgun unequipped", LogDebugLevel.Low);
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
            get { return "Heavy Shotgun"; }
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        public override string GroupItemDescription
        {
            get { return "Heavy Shotguns"; }
        }

        protected override char GetRepresentation()
        {
            return (char)276;
        }

        public override System.Drawing.Color GetColour()
        {
            return System.Drawing.Color.Gold;
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

        public override int MaxAmmo()
        {
            return 2;
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
            return TargettingType.Shotgun;
        }

        public virtual double ShotgunSpreadAngle()
        {
            return Math.PI / 4;
        }

        /// <summary>
        /// Throwing range
        /// </summary>
        /// <returns></returns>
        public int RangeThrow()
        {
            return 3;
        }

        /// <summary>
        /// Firing range
        /// </summary>
        /// <returns></returns>
        public int RangeFire()
        {
            return 10;
        }

        /// <summary>
        /// Noise mag of this weapon on firing
        /// </summary>
        /// <returns></returns>
        public override double FireSoundMagnitude()
        {
            return 1.0;
        }

        /// <summary>
        /// Noise mag of this weapon on throwing
        /// </summary>
        /// <returns></returns>
        public double ThrowSoundMagnitude()
        {
            return 0.3;
        }

        /// <summary>
        /// Destroyed on throw
        /// </summary>
        /// <returns></returns>
        public bool DestroyedOnThrow()
        {
            return false;
        }

        /// <summary>
        /// How much damage we do
        /// </summary>
        /// <returns></returns>
        public int MeleeDamage()
        {
            return 5;
        }

        public override int ItemCost()
        {
            return 25;
        }
        /// Can be thrown
        /// </summary>
        /// <returns></returns>
        public bool HasThrowAction()
        {
            return false;
        }

        /// <summary>
        /// Can be operated
        /// </summary>
        /// <returns></returns>
        public bool HasOperateAction()
        {
            return false;
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

        public void FireAudio()
        {
            return;
        }

        public void ThrowAudio()
        {
            return;
        }
    }
}
