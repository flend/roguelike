using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class Pistol : Item, IEquippableItem
    {

        /// <summary>
        /// Public for serialization
        /// </summary>
        public int Ammo { get; set; }

        public Pistol()
        {
            Ammo = MaxAmmo();
        }

        /// <summary>
        /// Equipment slots where we can be equipped
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
            LogFile.Log.LogEntryDebug("Pistol equipped", LogDebugLevel.Medium);

            //Give player story. Mention level up if one will occur.

            if (Game.Dungeon.Player.PlayItemMovies)
            {
                //Screen.Instance.PlayMovie("plotbadge", true);
                //Screen.Instance.PlayMovie("multiattack", false);
            }

            //Messages
            //Game.MessageQueue.AddMessage("A fine short sword - good for slicing and dicing.");

            //Screen.Instance.PlayMovie("plotbadge", true);

            //Level up?
            //Game.Dungeon.Player.LevelUp();

            //Add move?
            //Game.Dungeon.LearnMove(new SpecialMoves.MultiAttack());
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
            return (char)272;
        }

        public override libtcodWrapper.Color GetColour()
        {
            return ColorPresets.HotPink;
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

        public bool HasMeleeAction()
        {
            return false;
        }

        public override int ItemCost()
        {
            return 10;
        }


        public bool HasFireAction()
        {
            return true;
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

        public int MaxAmmo()
        {
            return 3;
        }

        public int RemainingAmmo()
        {
            return Ammo;
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
            //Should be guaranteed in range by caller

            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            LogFile.Log.LogEntryDebug("Firing pistol", LogDebugLevel.Medium);

            //Remove 1 ammo
            Ammo--;

            //Make firing sound
            Game.Dungeon.AddSoundEffect(FireSoundMagnitude(), player.LocationLevel, player.LocationMap);

            //Find monster target

            targetSquares = Game.Dungeon.CalculateTrajectory(target);
            Monster monster = Game.Dungeon.FirstMonsterInTrajectory(targetSquares);

            if(monster == null) {
                LogFile.Log.LogEntryDebug("No monster in target for Pistol.Ammo used anyway.", LogDebugLevel.Medium);
                return true;
            }

            //Draw attack

            Screen.Instance.DrawAreaAttackAnimation(targetSquares, ColorPresets.Gray);

            //Damage monster
            
            int damageBase = 3;

            string combatResultsMsg = "PvM (" + monster.Representation + ")Pistol: Dam: 2";
            LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

            //Apply damage
            player.AttackMonsterRanged(monster, damageBase);

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
            return 8;
        }

        public double FireSoundMagnitude()
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

        /// <summary>
        /// Destroyed on use
        /// </summary>
        /// <returns></returns>
        public bool DestroyedOnUse()
        {
            return false;
        }


    }
}