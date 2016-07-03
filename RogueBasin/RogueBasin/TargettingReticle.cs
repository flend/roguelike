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

        int currentTargetLevel = 0;
        Point currentTarget = new Point(0, 0);

        TargettingInfo currentInfo;

        public TargettingReticle(RogueBase rogueBase, Dungeon dungeon, Player player) {
            this.rogueBase = rogueBase;
            this.player = player;
            this.dungeon = dungeon;
        }

        public void RetargetSquare(int level, Point newSquare, CreatureFOV currentFOV)
        {
            currentTarget = newSquare;
            currentTargetLevel = level;

            SetScreenTargettingMode(currentTarget, currentInfo, currentTargettingAction);

            CheckTargetInRange(newSquare, currentTargettingAction, currentInfo);
            SquareContents sqC = rogueBase.SetViewPanelToTargetAtSquare(newSquare);

            //Update screen
            SetTargettingMessage(TargettingMessage, TargettingConfirmChar);
            SetTargettingState(currentTargetLevel, currentTarget, currentInfo, currentTargettingAction, TargettingConfirmChar, TargettingMessage);
        }

        /// <summary>
        /// Gets a target from the player. false showed an escape. otherwise target is the target selected.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public void GetTargetFromPlayer(int level, Point start, TargettingInfo targetInfo, TargettingAction targetAction, string confirmKey, string message)
        {
            SetScreenTargettingMode(start, targetInfo, targetAction);

            CheckTargetInRange(start, targetAction, targetInfo);

            SquareContents sqC = rogueBase.SetViewPanelToTargetAtSquare(start);

            SetTargettingMessage(message, confirmKey);

            SetTargettingState(level, start, targetInfo, targetAction, confirmKey, message);
        }

        private void SetTargettingMessage(string message, string confirmKey)
        {
            Game.MessageQueue.AddMessage(message + " find a target. " + confirmKey + " to confirm. ESC to exit.");
        }

        private void CheckTargetInRange(Point start, TargettingAction targetAction, TargettingInfo targetInfo)
        {
            if (targetAction == TargettingAction.Utility || targetAction == TargettingAction.Weapon || targetAction == TargettingAction.Examine || targetAction == TargettingAction.MoveOrWeapon || targetAction == TargettingAction.MoveOrThrow)
            {
                if (targetInfo.IsInRange(player, dungeon, new Location(player.LocationLevel, start)))
                {
                    Screen.Instance.SetTargetInRange = true;
                }
                else
                {
                    Screen.Instance.SetTargetInRange = false;
                }
            }
            else
            {
                Screen.Instance.SetTargetInRange = true;
            }
        }

        private void SetScreenTargettingMode(Point start, TargettingInfo info, TargettingAction targetAction)
        {
            Screen.Instance.TargettingModeOn();
            Screen.Instance.Target = start;
            Screen.Instance.TargetInfo = info;
            Screen.Instance.TargetAction = targetAction;
        }

        private void SetTargettingState(int level, Point start, TargettingInfo info, TargettingAction targetAction, string confirmKey, string message)
        {
            currentTargettingAction = targetAction;
            TargettingConfirmChar = confirmKey;
            currentTarget = start;
            currentTargetLevel = level;
            currentInfo = info;
            TargettingMessage = message;
        }

        public TargettingAction TargettingAction { get { return currentTargettingAction; } }

        public Point CurrentTarget { get { return currentTarget; } }

        public int CurrentTargetLevel { get { return currentTargetLevel; } }

        public void DisableScreenTargettingMode()
        {
            Screen.Instance.TargettingModeOff();
        }
    }
}
