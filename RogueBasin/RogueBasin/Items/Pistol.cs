﻿using System.Collections.Generic;

namespace RogueBasin.Items
{
    public class Pistol : RangedWeapon, IEquippableItem
    {

        public Pistol()
        {
        }



        public bool Equip(Creature user)
        {
            LogFile.Log.LogEntryDebug("Pistol equipped", LogDebugLevel.Medium);

            //Give player story. Mention level up if one will occur.

            if (Game.Dungeon.Player.PlayItemMovies)
            {
                //Game.Base.SystemActions.PlayMovie("plotbadge", true);
                //Game.Base.SystemActions.PlayMovie("multiattack", false);
            }

            //Messages
            //Game.MessageQueue.AddMessage("A fine short sword - good for slicing and dicing.");

            //Game.Base.SystemActions.PlayMovie("plotbadge", true);

            //Level up?
            //Game.Dungeon.Player.LevelUp();

            //Add move?
            //Game.Dungeon.LearnMove(new SpecialMoves.MultiAttack());
            //Game.Base.SystemActions.PlayMovie("multiattack", false);

            //Add any equipped (actually permanent) effects
            //Game.Dungeon.Player.Speed += 10;

            return true;
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

        /// <summary>
        /// not used in this game
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UnEquip(Creature user)
        {
            LogFile.Log.LogEntryDebug("Pistol unequipped", LogDebugLevel.Low);
            return true;
        }
        /// <summary>
        /// not used in this game
        /// </summary>
        public override int GetWeight()
        {
            return 50;
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

        public override string SingleItemDescription
        {
            get { return "Pistol"; }
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        public override string GroupItemDescription
        {
            get { return "Pistols"; }
        }

        protected override char GetRepresentation()
        {
            return (char)273;
        }

        public override System.Drawing.Color GetColour()
        {
            return System.Drawing.Color.HotPink;
        }

        public int ArmourClassModifier()
        {
            return 0;
        }

        public int DamageBase()
        {
            //1d6
            return 20;
        }

        public double DamageModifier()
        {
            return 0;
        }

        public int HitModifier()
        {
            return 0;
        }

        public override int ItemCost()
        {
            return 10;
        }

        public override int MaxAmmo()
        {
            return 10;
        }

        public bool HasMeleeAction()
        {
            return false;
        }

        public bool HasFireAction()
        {
            return true;
        }

        List<Point> targetSquares = null;

        /// <summary>
        /// Fires the item - probably should be a method
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enemyTarget"></param>
        /// <returns></returns>
        public bool FireItem(Point target)
        {
            //Pistol has unlimited ammo
            return Game.Dungeon.Combat.FirePistolLineWeapon(target, this, Game.Dungeon.Player.ScaleRangedDamage(this, DamageBase()));
        }

        /// <summary>
        /// Throws the item
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enemyTarget"></param>
        /// <returns></returns>
        public Point ThrowItem(Point target)
        {
            return Game.Dungeon.Player.ThrowItemGeneric(this, target, 3, true);
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
            return 5;
        }

        /// <summary>
        /// Firing range
        /// </summary>
        /// <returns></returns>
        public int RangeFire()
        {
            return 3;
        }

        public override double FireSoundMagnitude()
        {
            return 0.4;
        }

        /// <summary>
        /// Noise mag of this weapon on throwing
        /// </summary>
        /// <returns></returns>
        public double ThrowSoundMagnitude() {
            return 0.2;   
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

        /// <summary>
        /// Spread for shotgun target
        /// </summary>
        /// <returns></returns>
        public virtual double ShotgunSpreadAngle()
        {
            return 0.0;
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

        protected override string GetGameSprite()
        {
            return "rifle";
        }

        protected override string GetUISprite()
        {
            return "ui-pistol";
        }

        public void FireAudio()
        {
            SoundPlayer.Instance().EnqueueSound("gunshot");
        }

        public void ThrowAudio()
        {
            return;
        }

        public TargettingInfo TargettingInfo()
        {
            return new LineTargettingInfo(RangeFire());
        }

        public override int Index()
        {
            return 1;
        }
    }
}
