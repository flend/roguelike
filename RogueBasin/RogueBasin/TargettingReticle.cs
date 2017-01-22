using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    class TargettingReticle
    {
        private readonly RogueBase rogueBase;
        private readonly Player player;
        private readonly Dungeon dungeon;

        private string TargettingConfirmChar { get; set; }
        private string TargettingMessage { get; set; }

        TargettingAction currentTargettingAction = TargettingAction.Weapon;

        Location currentTarget = null;

        TargettingInfo currentInfo;

        public TargettingReticle(RogueBase rogueBase, Dungeon dungeon, Player player) {
            this.rogueBase = rogueBase;
            this.player = player;
            this.dungeon = dungeon;
        }

        public void RetargetSquare(Location newTarget, CreatureFOV currentFOV)
        {
            currentTarget = newTarget;

            SetScreenTargettingMode(currentTarget, currentInfo, currentTargettingAction);

            CheckTargetInRange(newTarget, currentTargettingAction, currentInfo);
            SquareContents sqC = rogueBase.SetViewPanelToTargetAtSquare(newTarget);

            //Update screen
            SetTargettingMessage(TargettingMessage, TargettingConfirmChar);
            SetTargettingState(currentTarget, currentInfo, currentTargettingAction, TargettingConfirmChar, TargettingMessage);
        }

        /// <summary>
        /// Gets a target from the player. false showed an escape. otherwise target is the target selected.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public void GetTargetFromPlayer(Location newTarget, TargettingInfo targetInfo, TargettingAction targetAction, string confirmKey, string message)
        {
            SetScreenTargettingMode(newTarget, targetInfo, targetAction);

            CheckTargetInRange(newTarget, targetAction, targetInfo);

            SquareContents sqC = rogueBase.SetViewPanelToTargetAtSquare(newTarget);

            SetTargettingMessage(message, confirmKey);

            SetTargettingState(newTarget, targetInfo, targetAction, confirmKey, message);
        }

        private void SetTargettingMessage(string message, string confirmKey)
        {
            if (message == null)
                return;

            Game.MessageQueue.AddMessage(message + " find a target. " + confirmKey + " to confirm. ESC to exit.");
        }

        private void CheckTargetInRange(Location start, TargettingAction targetAction, TargettingInfo targetInfo)
        {
            if (targetAction == TargettingAction.Utility || targetAction == TargettingAction.Weapon || targetAction == TargettingAction.Examine || targetAction == TargettingAction.MoveOrWeapon || targetAction == TargettingAction.MoveOrThrow)
            {
                if (targetInfo.IsInRange(player, dungeon, start))
                {
                    Screen.Instance.TargetInRange = true;
                }
                else
                {
                    Screen.Instance.TargetInRange = false;
                }
            }
            else
            {
                Screen.Instance.TargetInRange = true;
            }
        }

        private void SetScreenTargettingMode(Location start, TargettingInfo info, TargettingAction targetAction)
        {
            Screen.Instance.TargettingModeOn();
            Screen.Instance.Target = start;
            Screen.Instance.TargetInfo = info;
            Screen.Instance.TargetAction = targetAction;
        }

        private void SetTargettingState(Location target, TargettingInfo info, TargettingAction targetAction, string confirmKey, string message)
        {
            currentTargettingAction = targetAction;
            TargettingConfirmChar = confirmKey;
            currentTarget = target;
            currentInfo = info;
            TargettingMessage = message;
        }

        public TargettingAction TargettingAction { get { return currentTargettingAction; } }

        public Location CurrentTarget { get { return currentTarget; } }

        public void DisableScreenTargettingMode()
        {
            Screen.Instance.TargettingModeOff();
        }
    }
}
