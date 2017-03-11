using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public class Targetting
    {
        Dungeon dungeon;
        Player player;

        TargettingReticle targetReticle;
        InputHandler inputHandler;

        public Targetting(Dungeon dungeon, Player player) {

            this.dungeon = dungeon;
            this.player = player;

            targetReticle = new TargettingReticle(dungeon, player);
        }

        public void SetInputHandler(InputHandler inputHandler)
        {
            this.inputHandler = inputHandler;
        }

        public void TargetWeapon()
        {
            if (!CheckFireableWeapon())
            {
                Game.MessageQueue.AddMessage("Need a weapon that can fire.");
                return;
            }

            IEquippableItem weapon = player.GetEquippedRangedWeapon();
            Location defaultTarget = GetDefaultTarget(weapon.RangeFire());

            SetupWeaponTarget(defaultTarget);
        }

        public void TargetWeapon(Location target)
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
            Location defaultTarget = GetDefaultTarget(utility.RangeThrow());

            SetupThrowTarget(defaultTarget);
        }

        public void TargetThrowUtility(Location target)
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
            Location defaultTarget = GetDefaultTarget(-1);

            SetupExamineTarget(defaultTarget);
        }

        public void TargetMoveOrFireInstant(Location target, bool showFireTarget)
        {
            SetupMoveOrFireTargetInstant(target, showFireTarget);
        }

        public void TargetMoveOrThrowInstant(Location target, bool showThrowTarget)
        {
            SetupMoveOrThrowTargetInstant(target, showThrowTarget);
        }

        public void TargetMove(Location target)
        {
            SetupMoveTarget(target);
        }

        private void SetupWeaponTarget(Location target)
        {
            SetupWeaponTypeTarget(target, TargettingAction.Weapon, "f", "Fire Weapon");
            inputHandler.SetInputState(InputHandler.InputState.Targetting);
        }

        private void SetupWeaponTypeTarget(Location target, TargettingAction action, string confirmKey, string message)
        {
            IEquippableItem weapon = player.GetEquippedRangedWeapon();

            TargettingInfo targetInfo = new NullTargettingInfo();

            if (weapon != null)
            {
                targetInfo = weapon.TargettingInfo();
            }

            TargetAction(target, targetInfo, action, confirmKey, message);
        }

        private void SetupMoveTarget(Location target)
        {
            TargetAction(target, new MoveTargettingInfo(), TargettingAction.Move, "ENTER", "Move");
            inputHandler.SetInputState(InputHandler.InputState.Targetting);
        }

        private void SetupThrowTarget(Location target)
        {
            SetupThrowTypeTarget(target, RogueBasin.TargettingAction.Utility, "t", "Throw Item");
            inputHandler.SetInputState(InputHandler.InputState.Targetting);
        }

        private void SetupThrowTypeTarget(Location target, TargettingAction action, string confirmKey, string message)
        {
            IEquippableItem utility = player.GetEquippedUtility();

            TargettingInfo targetType = new NullTargettingInfo();

            if (utility != null)
            {
                targetType = utility.TargettingInfo();
            }

            TargetAction(target, targetType, action, confirmKey, message);
        }

        private void SetupExamineTarget(Location target)
        {
            TargetAction(target, new ExamineTargettingInfo(), TargettingAction.Examine, "x", "Examine");
            inputHandler.SetInputState(InputHandler.InputState.Targetting);
        }

        private void SetupExamineTargetInstant(Location target)
        {
            TargetAction(target, new ExamineTargettingInfo(), TargettingAction.Examine, "x", "Examine");
        }

        private void SetupMoveOrFireTargetInstant(Location target, bool showFireTarget)
        {
            SquareContents squareContents = dungeon.MapSquareContents(target);

            if (dungeon.IsSquareInPlayerFOV(target) && (squareContents.monster != null || showFireTarget))
            {
                IEquippableItem weapon = player.GetEquippedRangedWeapon();

                TargettingInfo targetInfo = new NullTargettingInfo();

                if (weapon != null)
                {
                    targetInfo = weapon.TargettingInfo();
                }

                TargetAction(target, targetInfo, TargettingAction.MoveOrWeapon, "f", null);
            }
            else
            {
                TargetAction(target, new MoveTargettingInfo(), TargettingAction.MoveOrWeapon, "f", null);
            }
        }

        private void SetupMoveOrThrowTargetInstant(Location target, bool showThrowTarget)
        {
            SquareContents squareContents = Game.Dungeon.MapSquareContents(target);

            if (dungeon.IsSquareInPlayerFOV(target) && squareContents.monster != null || showThrowTarget)
            {
                IEquippableItem utility = player.GetEquippedUtility();

                TargettingInfo targetInfo = new NullTargettingInfo();

                if (utility != null)
                {
                    targetInfo = utility.TargettingInfo();
                }

                TargetAction(target, targetInfo, RogueBasin.TargettingAction.MoveOrThrow, "t", null);
            }
            else
            {
                TargetAction(target, new MoveTargettingInfo(), RogueBasin.TargettingAction.MoveOrThrow, "t", null);
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

        private void TargetAction(Location target, TargettingInfo targetInfo, TargettingAction targetAction, string confirmChar, string message)
        {
            targetReticle.GetTargetFromPlayer(target, targetInfo, targetAction, confirmChar, message);
        }

        private Location GetDefaultTarget(int range)
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

            Location startLocation;

            if (Utility.TestRange(Game.Dungeon.Player, closeCreature, range) || range == -1)
            {
                startLocation = closeCreature.Location;
            }
            else
            {
                startLocation = player.Location;
            }

            return startLocation;
        }

        internal void RetargetSquare(Location newPoint)
        {
            CreatureFOV currentFOV = dungeon.CalculateCreatureFOV(player);
            targetReticle.RetargetSquare(newPoint, currentFOV);
        }

        public TargettingAction TargettingAction { get { return targetReticle.TargettingAction; } }

        public Location CurrentTarget { get { return targetReticle.CurrentTarget; } }

        public void DisableTargettingMode()
        {
            inputHandler.SetInputState(InputHandler.InputState.MapMovement);
            targetReticle.DisableScreenTargettingMode();
        }
    }
}
