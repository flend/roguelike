using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class Shotgun : Item, IEquippableItem
    {
        public int Ammo { get; set; }

        public Shotgun()
        {
            Ammo = MaxAmmo();
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

            LogFile.Log.LogEntryDebug("Firing shotgun", LogDebugLevel.Medium);

            //The shotgun fires towards its target and does less damage with range

            //Get all squares in range and within FOV (shotgun needs a straight line route to fire)

            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(player);
            List<Point> targetSquares = currentFOV.GetPointsForTriangularTargetInFOV(player.LocationMap, target, RangeFire(), ShotgunSpreadAngle());
            
            //Draw attack
            Screen.Instance.DrawShotgunMissileAttack(targetSquares);

            //Make firing sound
            Game.Dungeon.AddSoundEffect(FireSoundMagnitude(), player.LocationLevel, player.LocationMap);

            //Attack all monsters in the area

            foreach (Point sq in targetSquares)
            {
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, sq);

                Monster m = squareContents.monster;

                //Hit the monster if it's there
                if (m != null)
                {
                    //Calculate range
                    int rangeToMonster = (int)Math.Floor(Dungeon.GetDistanceBetween(player.LocationMap, m.LocationMap));
                    int damage = 10 - rangeToMonster;

                    string combatResultsMsg = "PvM (" + m.Representation + ") Shotgun: Dam: " + damage;
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    //Apply damage
                    player.AttackMonsterRanged(squareContents.monster, damage);
                }
            }

            //Remove 1 ammo
            Ammo--;

            return true;
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
            LogFile.Log.LogEntryDebug("Shotgun equipped", LogDebugLevel.Medium);

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
        /// Throws the item
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enemyTarget"></param>
        /// <returns></returns>
        public Point ThrowItem(Point target)
        {
            //Stun for 3 turns
            return Pistol.ThrowItemGeneric(this, target, 3, true);
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
            get { return "shotgun"; }
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        public override string GroupItemDescription
        {
            get { return "shotguns"; }
        }

        protected override char GetRepresentation()
        {
            return '{';
        }

        public override libtcodWrapper.Color GetColour()
        {
            return ColorPresets.Silver;
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
            return 2;
        }

        public int RemainingAmmo()
        {
            return Ammo;
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
        public double FireSoundMagnitude()
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
            return 0;
        }

    }
}