using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class Shotgun : ShotgunTypeWeapon, IEquippableItem
    {
        public Shotgun()
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
            //Should be guaranteed in range by caller

            var scaledDamage = Game.Dungeon.Player.ScaleRangedDamage(this, DamageBase());
            Game.Dungeon.FireShotgunWeapon(target, this, scaledDamage, scaledDamage / 10, scaledDamage / 10);

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
            LogFile.Log.LogEntryDebug("Shotgun equipped", LogDebugLevel.Medium);

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
            LogFile.Log.LogEntryDebug("Shotgun unequipped", LogDebugLevel.Low);
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
            get { return "Shotgun"; }
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        public override string GroupItemDescription
        {
            get { return "Shotguns"; }
        }

        protected override char GetRepresentation()
        {
            return (char)274;
        }

        protected override string GetGameSprite()
        {
            return "shotgun";
        }

        protected override string GetUISprite()
        {
            return "ui-shotgun";
        }

        public override System.Drawing.Color GetColour()
        {
            return System.Drawing.Color.Cyan;
        }

        public int ArmourClassModifier()
        {
            return 0;
        }

        public int DamageBase()
        {
            //1d6
            return 40;
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
            return 0.5;
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
            SoundPlayer.Instance().EnqueueSound("shotgun");
        }

        public void ThrowAudio()
        {
            return;
        }
    }
}
