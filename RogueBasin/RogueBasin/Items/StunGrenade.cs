using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class StunGrenade : Item, IEquippableItem
    {
 
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
            LogFile.Log.LogEntryDebug("StunGrenade equipped", LogDebugLevel.Medium);

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

            //Add any equipped (actually permanent) effectsf
            //Game.Dungeon.Player.Speed += 10;

            return true;
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
            Game.MessageQueue.AddMessage("The stun grenade explodes!");
           
            Point dest = StunGrenade.ThrowItemGrenadeLikeStun(this, target, 4, 6);
            
            return dest;
        }

        
         /// <summary>
        /// Stun grenade - goes through walls
        /// Should sync with above method
        /// </summary>
        /// <param name="item"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Point ThrowItemGrenadeLikeStun(IEquippableItem item, Point target, double size, int damage) {

            Item itemAsItem = item as Item;

            LogFile.Log.LogEntryDebug("Throwing " + itemAsItem.SingleItemDescription, LogDebugLevel.Medium);

            Player player = Game.Dungeon.Player;

            //Find target

            List<Point> targetSquares = Game.Dungeon.CalculateTrajectory(target);
            Monster monster = Game.Dungeon.FirstMonsterInTrajectory(targetSquares);

            //Find where it landed

            //Destination will be the last square in trajectory
            Point destination;
            if (targetSquares.Count > 0)
                destination = targetSquares[targetSquares.Count - 1];
            else
                //Threw it on themselves!
                destination = player.LocationMap;
           
             
            //Stopped by a monster
            if (monster != null)
            {
                destination = monster.LocationMap;
            }

            //Make throwing sound AT target location
            Game.Dungeon.AddSoundEffect(item.ThrowSoundMagnitude(), Game.Dungeon.Player.LocationLevel, destination);

            //Work out grenade splash and damage
            
            List<Point> grenadeAffects = Game.Dungeon.GetPointsForGrenadeTemplate(destination, Game.Dungeon.Player.LocationLevel, size);
            
            //Draw attack
            Screen.Instance.DrawAreaAttackAnimation(grenadeAffects, ColorPresets.DodgerBlue);

            //Attack all monsters in the area

            foreach (Point sq in grenadeAffects)
            {
                SquareContents squareContents = Game.Dungeon.MapSquareContents(player.LocationLevel, sq);

                Monster m = squareContents.monster;

                //Hit the monster if it's there
                if (m != null)
                {
                    string combatResultsMsg = "PvM (" + m.Representation + ") Grenade: Stun: " + damage;
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    //Apply damage
                    player.ApplyStunDamageToMonster(m, damage);
                }
            }

            //Doesn't affect the player

            return(destination);

        }



        /// <summary>
        /// not used in this game
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UnEquip(Creature user)
        {
            LogFile.Log.LogEntryDebug("EMP unequipped", LogDebugLevel.Low);
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
            get { return "EMP grenade"; }
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        public override string GroupItemDescription
        {
            get { return "EMP grenades"; }
        }

        protected override char GetRepresentation()
        {
            return (char)298;
        }

        public override libtcodWrapper.Color GetColour()
        {
            return ColorPresets.DodgerBlue;
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
            return 10;
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
            //quiet
            return 0.2;
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
