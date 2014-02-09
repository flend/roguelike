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
            return (char)144;
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
        /// Widely used functions are static public
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static List<Point> CalculateTrajectory(Point target)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //Get the points along the line of where we are firing
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(player);
            List<Point> trajPoints = currentFOV.GetPathLinePointsInFOV(player.LocationMap, target);

            //Also exclude unwalkable points (since we will use this to determine where our item falls
            List<Point> walkableSq = new List<Point>();
            foreach (Point p in trajPoints)
            {
                if (Game.Dungeon.MapSquareIsWalkable(Game.Dungeon.Player.LocationLevel, p))
                    walkableSq.Add(p);
            }

            return walkableSq;
        }

        /// <summary>
        /// Widely used functions are static public
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Monster FirstMonsterInTrajectory(List<Point> squares) {

            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //Hit the first monster only
            Monster monster = null;
            foreach (Point p in squares)
            {
                //Check there is a monster at target
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, p);

                //Hit the monster if it's there
                if (squareContents.monster != null)
                {
                    monster = squareContents.monster;
                    break;
                }
            }

            return monster;

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

            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            LogFile.Log.LogEntryDebug("Firing pistol", LogDebugLevel.Medium);

            //Remove 1 ammo
            Ammo--;

            //Make firing sound
            Game.Dungeon.AddSoundEffect(FireSoundMagnitude(), player.LocationLevel, player.LocationMap);

            //Find monster target

            targetSquares = Pistol.CalculateTrajectory(target);
            Monster monster = Pistol.FirstMonsterInTrajectory(targetSquares);

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
            return Pistol.ThrowItemGeneric(this, target, 3, true);
        }

        /// <summary>
        /// Generic throw method for most normal items
        /// </summary>
        /// <param name="item"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Point ThrowItemGeneric(IEquippableItem item, Point target, int damageOrStunTurns, bool stunDamage)
        {
            Item itemAsItem = item as Item;

            LogFile.Log.LogEntryDebug("Throwing " + itemAsItem.SingleItemDescription, LogDebugLevel.Medium);

            Player player = Game.Dungeon.Player;

            //Find target

            List<Point> targetSquares = Pistol.CalculateTrajectory(target);
            Monster monster = Pistol.FirstMonsterInTrajectory(targetSquares);

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
            Game.Dungeon.AddSoundEffect(item.ThrowSoundMagnitude(), player.LocationLevel, destination);

            //Draw throw
            Screen.Instance.DrawAreaAttackAnimation(targetSquares, ColorPresets.Gray);

            if (stunDamage)
            {
                if (monster != null && damageOrStunTurns > 0)
                {
                    player.ApplyStunDamageToMonster(monster, damageOrStunTurns);
                }
            }
            else
            {
                if (monster != null && damageOrStunTurns > 0)
                {
                    player.AttackMonsterThrown(monster, damageOrStunTurns);
                }
            }

            return destination;
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