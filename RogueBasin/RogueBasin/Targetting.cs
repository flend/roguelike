using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    class Targetting
    {
        RogueBase rogueBase;
        Dungeon dungeon;
        Player player;

        TargettingReticle targetReticle;

        public Targetting(RogueBase rogueBase) {

            targetReticle = new TargettingReticle(rogueBase);

            this.rogueBase = rogueBase;
            dungeon = Game.Dungeon;
            player = Game.Dungeon.Player;
        }

        public void TargetWeapon()
        {
            if (!CheckFireableWeapon())
            {
                Game.MessageQueue.AddMessage("Need a weapon that can fire.");
                return;
            }

            IEquippableItem weapon = player.GetEquippedRangedWeapon();
            Point defaultTarget = GetDefaultTarget(weapon.RangeFire());

            SetupWeaponTarget(defaultTarget);
        }

        public void TargetWeapon(Point target)
        {
            if (!CheckFireableWeapon())
            {
                Game.MessageQueue.AddMessage("Need a weapon that can fire.");
                return;
            }

            SetupWeaponTarget(target);
        }

        public void TargetThrowUtility()
        {
            if (!CheckThrowableUtility())
            {
                Game.MessageQueue.AddMessage("Need a utility that can be thrown.");
                return;
            }

            IEquippableItem utility = player.GetEquippedUtility();
            Point defaultTarget = GetDefaultTarget(utility.RangeThrow());

            SetupThrowTarget(defaultTarget);
        }

        public void TargetThrowUtility(Point target)
        {
            if (!CheckThrowableUtility())
            {
                Game.MessageQueue.AddMessage("Need a utility that can be thrown.");
                return;
            }

            SetupThrowTarget(target);
        }

        public void TargetExamine()
        {
            Point defaultTarget = GetDefaultTarget(-1);

            SetupExamineTarget(defaultTarget);
        }

        public void TargetExamine(Point target)
        {
            SetupExamineTarget(target);
        }

        public void TargetMove(Point target)
        {
            SetupMoveTarget(target);
        }

        private void SetupWeaponTarget(Point target)
        {
            IEquippableItem weapon = player.GetEquippedRangedWeapon();

            //Find weapon range
            int range = weapon.RangeFire();
            TargettingType targetType = weapon.TargetTypeFire();
            double spreadAngle = weapon.ShotgunSpreadAngle();

            //Calculate FOV
            CreatureFOV currentFOV = dungeon.CalculateCreatureFOV(player);

            TargetAction(target, range, targetType, TargettingAction.Weapon, spreadAngle, "f", currentFOV);
        }

        private void SetupMoveTarget(Point target)
        {
            //Calculate FOV
            CreatureFOV currentFOV = dungeon.CalculateCreatureFOV(player);

            TargetAction(target, 0, TargettingType.Line, TargettingAction.Move, 0.0, "ENTER", currentFOV);
        }

        private void SetupThrowTarget(Point target)
        {
            IEquippableItem utility = player.GetEquippedUtility();

            //Find weapon range
            int range = utility.RangeThrow();
            TargettingType targetType = utility.TargetTypeThrow();

            //Calculate FOV
            CreatureFOV currentFOV = dungeon.CalculateCreatureFOV(player);

            TargetAction(target, range, targetType, TargettingAction.Utility, 0.0, "t", currentFOV);
        }

        private void SetupExamineTarget(Point target)
        {
            //Calculate FOV
            CreatureFOV currentFOV = dungeon.CalculateCreatureFOV(player);

            TargetAction(target, -1, TargettingType.Line, TargettingAction.Examine, 0.0, "x", currentFOV);
        }

        private bool CheckFireableWeapon()
        {
            IEquippableItem weapon = player.GetEquippedRangedWeapon();

            if (weapon == null || !weapon.HasFireAction())
            {
                return false;
            }
            return true;
        }

        private bool CheckThrowableUtility()
        {
            IEquippableItem weapon = player.GetEquippedUtility();

            if (weapon == null || !weapon.HasThrowAction())
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Let the user target something
        /// </summary>
        /// <returns></returns>
        private void TargetAction(Point target, int range, TargettingType targetType, TargettingAction targetAction, double spreadAngle, string confirmChar, CreatureFOV currentFOV)
        {
            //Get the desired target from the player
            targetReticle.GetTargetFromPlayer(player.LocationLevel, target, targetType, targetAction, range, spreadAngle, confirmChar, currentFOV);
        }

        private Point GetDefaultTarget(int range)
        {
            //Start on the nearest creature
            Creature closeCreature = dungeon.FindClosestHostileCreatureInFOV(player);

            if (closeCreature == null)
            {
                var allCreatures = dungeon.FindClosestCreaturesInPlayerFOV();
                if (allCreatures.Any())
                    closeCreature = allCreatures.First();
            }

            //If no nearby creatures, start on the player
            if (closeCreature == null)
                closeCreature = player;

            Point startPoint;

            if (Utility.TestRange(Game.Dungeon.Player, closeCreature, range) || range == -1)
            {
                startPoint = new Point(closeCreature.LocationMap.x, closeCreature.LocationMap.y);
            }
            else
            {
                startPoint = new Point(player.LocationMap.x, player.LocationMap.y);
            }

            return startPoint;
        }

        internal void RetargetSquare(Point newPoint)
        {
            CreatureFOV currentFOV = dungeon.CalculateCreatureFOV(player);
            targetReticle.RetargetSquare(targetReticle.CurrentTargetLevel, newPoint, currentFOV);
        }

        public TargettingAction TargettingAction { get { return targetReticle.TargettingAction; } }

        public Point CurrentTarget { get { return targetReticle.CurrentTarget; } }
    }
}
