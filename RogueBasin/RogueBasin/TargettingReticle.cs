namespace RogueBasin
{
    /// <summary>
    /// Retains the state about the current targetted action and target location
    /// Also updates the screen flags and view panel on a change of target
    /// </summary>
    class TargettingReticle
    {
        private readonly Player player;
        private readonly Dungeon dungeon;

        private string TargettingConfirmChar { get; set; }
        private string TargettingMessage { get; set; }

        private TargettingAction currentTargettingAction = TargettingAction.Fire;
        private TargettingAction currentTargettingSubAction = TargettingAction.Fire;

        private Location currentTarget = null;

        private TargettingInfo currentTargetInfo;

        public TargettingReticle(Dungeon dungeon, Player player) {
            this.player = player;
            this.dungeon = dungeon;
        }

        public void RetargetSquare(Location newTarget)
        {
            currentTarget = newTarget;

            SetupScreenTargetting(currentTarget, currentTargetInfo, currentTargettingAction, currentTargettingSubAction, TargettingConfirmChar, TargettingMessage);
        }

        public void SetupScreenTargetting(Location newTarget, TargettingInfo targetInfo, TargettingAction targetAction, TargettingAction targetSubAction, string confirmKey, string message)
        {
            currentTargettingSubAction = targetSubAction;
            SetupScreenTargetting(newTarget, targetInfo, targetAction, confirmKey, message);
        }

        public void SetupScreenTargetting(Location newTarget, TargettingInfo targetInfo, TargettingAction targetAction, string confirmKey, string message)
        {
            SetTargettingMessage(message, confirmKey);
            SetTargettingState(newTarget, targetInfo, targetAction, confirmKey, message);

            CheckTargetInRange();
            SetScreenTargettingMode();
            SetViewPanelToTargetAtSquare();
        }

        private void SetTargettingMessage(string message, string confirmKey)
        {
            if (message == null)
                return;

            Game.MessageQueue.AddMessage(message + " find a target. " + confirmKey + " to confirm. ESC to exit.");
        }

        private void CheckTargetInRange()
        {
            Screen.Instance.TargetInRange = currentTargetInfo.IsInRange(player, dungeon, currentTarget);
        }

        private void SetScreenTargettingMode()
        {
            Screen.Instance.TargettingModeOn();
            Screen.Instance.Target = CurrentTarget;
            Screen.Instance.TargetInfo = currentTargetInfo;
            Screen.Instance.TargetAction = TargettingAction;
            Screen.Instance.TargetSubAction = TargettingSubAction;
        }

        private void SetTargettingState(Location target, TargettingInfo info, TargettingAction targetAction, string confirmKey, string message)
        {
            currentTargettingAction = targetAction;
            TargettingConfirmChar = confirmKey;
            currentTarget = target;
            currentTargetInfo = info;
            TargettingMessage = message;
        }

        public TargettingAction TargettingAction { get { return currentTargettingAction; } }

        public TargettingAction TargettingSubAction { get { return currentTargettingSubAction; } }

        public Location CurrentTarget { get { return currentTarget; } }

        public void DisableScreenTargettingMode()
        {
            Screen.Instance.TargettingModeOff();
        }

        private SquareContents SetViewPanelToTargetAtSquare()
        {
            SquareContents sqC = dungeon.MapSquareContents(currentTarget);
            Screen.Instance.CreatureToView = sqC.monster; //may reset to null
            if (sqC.items.Count > 0)
                Screen.Instance.ItemToView = sqC.items[0];
            else
                Screen.Instance.ItemToView = null;

            Screen.Instance.FeatureToView = sqC.feature; //may reset to null
            return sqC;
        }
    }
}
