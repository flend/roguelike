using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    /// <summary>
    /// Class for a throwing creature that doesn't back away. All functionality is now in MonsterThrowAndRunAI with GetChanceBackAway = 0
    /// </summary>
    public abstract class MonsterSimpleThrowingAI : MonsterFightAndRunAI
    {
        //public SimpleAIStates AIState { get; set; }
        //protected Creature currentTarget;

        public MonsterSimpleThrowingAI() : base()
        {
        }

        protected abstract double GetMissileRange();

        protected abstract string GetWeaponName();

        /// <summary>
        /// Color of the projectile
        /// </summary>
        /// <returns></returns>
        protected virtual Color GetWeaponColor()
        {
            return ColorPresets.DarkGray;
        }

        /// <summary>
        /// Override the following code from the hand to hand AI to give us some range
        /// </summary>
        /// <param name="newTarget"></param>
        protected override void FollowAndAttack(Creature newTarget)
        {
            //If we are in range and can see the target, fire
            double range = Game.Dungeon.GetDistanceBetween(this, newTarget);

            if (range < GetMissileRange() + 0.005)
            {
                //Check FOV. If not in FOV (e.g. player hiding behind a wall), continue chasing
                TCODFov currentFOV = Game.Dungeon.CalculateCreatureFOV(this);

                if (!currentFOV.CheckTileFOV(newTarget.LocationMap.x, newTarget.LocationMap.y))
                {
                    ContinueChasing(newTarget);
                    return;
                }
                
                //In range

                //Fire at the player
                CombatResults result;

                if (newTarget == Game.Dungeon.Player)
                {
                    result = AttackPlayer(newTarget as Player);
                }
                else
                {
                    //It's a normal creature
                    result = AttackMonster(newTarget as Monster);
                }

                //Missile animation
                Screen.Instance.DrawMissileAttack(this, new Point(LocationMap.x, LocationMap.y), new Point(newTarget.LocationMap.x, newTarget.LocationMap.y), GetWeaponColor());
            }
            else
            {
                //If not, move towards the target

                //Find location of next step on the path towards them
                ContinueChasing(newTarget);
            }
        }

        private void ContinueChasing(Creature newTarget)
        {

            //Return if we can't move
            if (!CanMove())
                return;

            Point nextStep = Game.Dungeon.GetPathTo(this, newTarget);
            LocationMap = nextStep;
        }

        protected override string HitsPlayerCombatString()
        {
            string combatStr = "";

            if(!Unique)
                combatStr = "The ";

             return combatStr + this.SingleDescription + " " + GetWeaponName() + " at you. It hits.";
        }

        protected override string MissesPlayerCombatString()
        {
            string combatStr = "";

            if (!Unique)
                combatStr = "The ";

            return combatStr + this.SingleDescription + " " + GetWeaponName() + " at you. It misses.";
        }

        protected override string HitsMonsterCombatString(Monster target)
        {
            string combatStr = "";

            if (!Unique)
                combatStr = "The ";

            return combatStr + this.SingleDescription + " " + GetWeaponName() + " at the " + target.SingleDescription + ". It hits.";
        }

        protected override string MissesMonsterCombatString(Monster target)
        {
            string combatStr = "";

            if (!Unique)
                combatStr = "The ";

            return combatStr + this.SingleDescription + " " + GetWeaponName() + " at the " + target.SingleDescription + ". It misses.";
        }

    }
}
