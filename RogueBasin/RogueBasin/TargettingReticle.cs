namespace RogueBasin
{
    /// <summary>
    /// Retains the state about the current targetted action and target
    /// Also updates the screen flags on a change of target
    /// </summary>
    class TargettingReticle
    {
        private readonly Player player;
        private readonly Dungeon dungeon;

        private string TargettingConfirmChar { get; set; }
        private string TargettingMessage { get; set; }

        private TargettingAction currentTargettingAction = TargettingAction.Weapon;

        private Location currentTarget = null;

        private TargettingInfo currentInfo;

        private bool alternateTargettingMode = false;

        public TargettingReticle(Dungeon dungeon, Player player) {
            this.player = player;
            this.dungeon = dungeon;
        }

        public void RetargetSquare(Location newTarget, CreatureFOV currentFOV)
        {
            currentTarget = newTarget;

            SetScreenTargettingMode(currentTarget, currentInfo, currentTargettingAction, AlternativeTargettingMode);

            CheckTargetInRange(newTarget, currentTargettingAction, currentInfo);
            SquareContents sqC = SetViewPanelToTargetAtSquare(newTarget);

            //Update screen
            SetTargettingMessage(TargettingMessage, TargettingConfirmChar);
            SetTargettingState(currentTarget, currentInfo, currentTargettingAction, TargettingConfirmChar, TargettingMessage);
        }

        public void SetupScreenTargetting(Location newTarget, TargettingInfo targetInfo, TargettingAction targetAction, string confirmKey, string message, bool alternateTargettingMode)
        {
            this.alternateTargettingMode = alternateTargettingMode;

            SetScreenTargettingMode(newTarget, targetInfo, targetAction, alternateTargettingMode);

            CheckTargetInRange(newTarget, targetAction, targetInfo);

            SquareContents sqC = SetViewPanelToTargetAtSquare(newTarget);

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
            Screen.Instance.TargetInRange = targetInfo.IsInRange(player, dungeon, start);
        }

        private void SetScreenTargettingMode(Location start, TargettingInfo info, TargettingAction targetAction, bool alternativeTargettingMode)
        {
            Screen.Instance.TargettingModeOn();
            Screen.Instance.Target = start;
            Screen.Instance.TargetInfo = info;
            Screen.Instance.TargetAction = targetAction;
            Screen.Instance.AlternativeTargettingMode = alternateTargettingMode;
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

        public bool AlternativeTargettingMode { get { return alternateTargettingMode; } }

        public void DisableScreenTargettingMode()
        {
            Screen.Instance.TargettingModeOff();
        }

        private SquareContents SetViewPanelToTargetAtSquare(Location start)
        {
            SquareContents sqC = dungeon.MapSquareContents(start);
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
