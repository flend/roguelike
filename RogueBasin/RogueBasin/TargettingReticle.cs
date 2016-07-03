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

        public string TargettingConfirmChar { get; set; }

        TargettingAction currentTargettingAction = TargettingAction.Weapon;

        int currentTargetLevel = 0;
        Point currentTarget = new Point(0, 0);

        int currentTargetRange = 0;
        double currentSpreadAngle;
        TargettingType currentType;


        public TargettingReticle(RogueBase rogueBase) {
            this.rogueBase = rogueBase;
        }

        public void RetargetSquare(int level, Point newSquare, CreatureFOV currentFOV)
        {
            currentTarget = newSquare;
            currentTargetLevel = level;

            SetScreenTargettingMode(currentTarget, currentType, currentTargettingAction, currentTargetRange, currentSpreadAngle);

            CheckTargetInRange(newSquare, currentTargettingAction, currentTargetRange, currentFOV);
            SquareContents sqC = rogueBase.SetViewPanelToTargetAtSquare(newSquare);

            //Update screen
            SetTargettingMessage(TargettingConfirmChar);
            SetTargettingState(currentTargetLevel, currentTarget, currentType, currentTargettingAction, currentTargetRange, currentSpreadAngle, TargettingConfirmChar);
        }

        /// <summary>
        /// Gets a target from the player. false showed an escape. otherwise target is the target selected.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public void GetTargetFromPlayer(int level, Point start, TargettingType type, TargettingAction targetAction, int range, double spreadAngle, string confirmKey, CreatureFOV currentFOV)
        {
            SetScreenTargettingMode(start, type, targetAction, range, spreadAngle);

            CheckTargetInRange(start, targetAction, range, currentFOV);

            SquareContents sqC = rogueBase.SetViewPanelToTargetAtSquare(start);

            SetTargettingMessage(confirmKey);

            SetTargettingState(level, start, type, targetAction, range, spreadAngle, confirmKey);
        }

        public void GetTargetFromPlayerNoRange(int level, Point start, TargettingType type, TargettingAction targetAction, string confirmKey)
        {
            SetScreenTargettingMode(start, type, targetAction, 0, 0);

            Screen.Instance.SetTargetInRange = true;

            SquareContents sqC = rogueBase.SetViewPanelToTargetAtSquare(start);

            SetTargettingMessage(confirmKey);

            SetTargettingState(level, start, type, targetAction, -1, 0.0, confirmKey);
        }

        private void SetTargettingMessage(string confirmKey)
        {
            Game.MessageQueue.AddMessage("Find a target. " + confirmKey + " to confirm. ESC to exit.");
        }

        private void CheckTargetInRange(Point start, TargettingAction targetAction, int range, CreatureFOV currentFOV)
        {
            if (targetAction == TargettingAction.Utility || targetAction == TargettingAction.Weapon || targetAction == TargettingAction.Examine)
            {
                if ((range == -1 && currentFOV.CheckTileFOV(start.x, start.y))
                    || Utility.TestRangeFOVForWeapon(Game.Dungeon.Player, start, range, currentFOV))
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

        private void SetScreenTargettingMode(Point start, TargettingType type, TargettingAction targetAction, int range, double spreadAngle)
        {
            Screen.Instance.TargettingModeOn();
            Screen.Instance.Target = start;
            Screen.Instance.TargetType = type;
            Screen.Instance.TargetAction = targetAction;
            Screen.Instance.TargetRange = range;
            Screen.Instance.TargetPermissiveAngle = spreadAngle;
        }

        private void SetTargettingState(int level, Point start, TargettingType type, TargettingAction targetAction, int range, double spreadAngle, string confirmKey)
        {
            rogueBase.SetInputState(RogueBase.InputState.Targetting);

            currentTargettingAction = targetAction;
            TargettingConfirmChar = confirmKey;
            currentTarget = start;
            currentTargetLevel = level;
            currentTargetRange = range;
            currentSpreadAngle = spreadAngle;
            currentType = type;
        }

        public TargettingAction TargettingAction { get { return currentTargettingAction; } }

        public Point CurrentTarget { get { return currentTarget; } }

        public int CurrentTargetLevel { get { return currentTargetLevel; } }
    }
}
