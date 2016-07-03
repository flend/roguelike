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

            targetReticle = new TargettingReticle(rogueBase, Game.Dungeon, Game.Dungeon.Player);

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
        
        public void TargetMoveOrFireInstant(Point target, bool showFireTarget)
        {
            SetupMoveOrFireTargetInstant(target, showFireTarget);
        }

        public void TargetMoveOrThrowInstant(Point target, bool showThrowTarget)
        {
            SetupMoveOrThrowTargetInstant(target, showThrowTarget);
        }

        public void TargetMove(Point target)
        {
            SetupMoveTarget(target);
        }

        private void SetupWeaponTarget(Point target)
        {
            SetupWeaponTypeTarget(target, TargettingAction.Weapon, "f", "Fire Weapon");
            rogueBase.SetInputState(RogueBase.InputState.Targetting);
        }

        private void SetupWeaponTypeTarget(Point target, TargettingAction action, string confirmKey, string message)
        {
            IEquippableItem weapon = player.GetEquippedRangedWeapon();

            TargettingInfo targetInfo = new NullTargettingInfo();

            if (weapon != null)
            {
                targetInfo = weapon.TargettingInfo();
            }

            TargetAction(target, targetInfo, action, confirmKey, message);
        }

        private void SetupMoveTarget(Point target)
        {
            TargetAction(target, new MoveTargettingInfo(), TargettingAction.Move, "ENTER", "Move");
            rogueBase.SetInputState(RogueBase.InputState.Targetting);
        }

        private void SetupThrowTarget(Point target)
        {
            SetupThrowTypeTarget(target, RogueBasin.TargettingAction.Utility, "t", "Throw Item");
            rogueBase.SetInputState(RogueBase.InputState.Targetting);
        }

        private void SetupThrowTypeTarget(Point target, TargettingAction action, string confirmKey, string message)
        {
            IEquippableItem utility = player.GetEquippedUtility();

            TargettingInfo targetType = new NullTargettingInfo();

            if (utility != null)
            {
                targetType = utility.TargettingInfo();
            }

            TargetAction(target, targetType, action, confirmKey, message);
        }

        private void SetupExamineTarget(Point target)
        {
            TargetAction(target, new ExamineTargettingInfo(), TargettingAction.Examine, "x", "Examine");
            rogueBase.SetInputState(RogueBase.InputState.Targetting);
        }

        private void SetupExamineTargetInstant(Point target) {
            TargetAction(target, new ExamineTargettingInfo(), TargettingAction.Examine, "x", "Examine");
        }

        private void SetupMoveOrFireTargetInstant(Point target, bool showFireTarget)
        {
            SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, target);

            if (dungeon.IsSquareInPlayerFOV(target) && (squareContents.monster != null || showFireTarget))
            {
                IEquippableItem weapon = player.GetEquippedRangedWeapon();

                TargettingInfo targetInfo = new NullTargettingInfo();

                if (weapon != null)
                {
                    targetInfo = weapon.TargettingInfo();
                }

                TargetAction(target, targetInfo, TargettingAction.MoveOrWeapon, "f", "Move or Fire");
            }
            else
            {
                TargetAction(target, new MoveTargettingInfo(), TargettingAction.MoveOrWeapon, "f", "Move or Fire");
            }
        }

        private void SetupMoveOrThrowTargetInstant(Point target, bool showThrowTarget)
        {
            SquareContents squareContents = Game.Dungeon.MapSquareContents(Game.Dungeon.Player.LocationLevel, target);

            if (dungeon.IsSquareInPlayerFOV(target) && squareContents.monster != null || showThrowTarget)
            {
                IEquippableItem utility = player.GetEquippedUtility();

                TargettingInfo targetInfo = new NullTargettingInfo();

                if (utility != null)
                {
                    targetInfo = utility.TargettingInfo();
                }

                TargetAction(target, targetInfo, RogueBasin.TargettingAction.MoveOrThrow, "t", "Move or throw");
            }
            else
            {
                TargetAction(target, new MoveTargettingInfo(), RogueBasin.TargettingAction.MoveOrThrow, "t", "Move or throw");
            }
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

        private void TargetAction(Point target, TargettingInfo targetInfo, TargettingAction targetAction, string confirmChar, string message)
        {
            targetReticle.GetTargetFromPlayer(player.LocationLevel, target, targetInfo, targetAction, confirmChar, message);
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

        public void DisableTargettingMode()
        {
            rogueBase.SetInputState(RogueBasin.RogueBase.InputState.MapMovement);
            targetReticle.DisableScreenTargettingMode();
        }
    }
}
