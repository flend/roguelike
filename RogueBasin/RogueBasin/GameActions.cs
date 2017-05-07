namespace RogueBasin
{
    public class GameActions
    {

        public void ScreenLevelDown()
        {
            if (Screen.Instance.LevelToDisplay > 0)
                Screen.Instance.LevelToDisplay--;
        }

        public void ScreenLevelUp()
        {
            if (Screen.Instance.LevelToDisplay < Game.Dungeon.NoLevels - 1)
                Screen.Instance.LevelToDisplay++;

        }

        public void SetMsgHistoryScreen()
        {
            Screen.Instance.ShowMsgHistory = true;
        }

        public void DisableMsgHistoryScreen()
        {
            Screen.Instance.ShowMsgHistory = false;
        }

        public void SetClueScreen()
        {
            Screen.Instance.ShowClueList = true;
        }

        public void DisableClueScreen()
        {
            Screen.Instance.ShowClueList = false;
        }

        public void SetLogScreen()
        {
            Screen.Instance.ShowLogList = true;
        }

        public void DisableLogScreen()
        {
            Screen.Instance.ShowLogList = false;
        }

        public ActionResult WeaponSelectOverlay(InputHandler inputHandler)
        {
            var weaponOverlayScreen = new WeaponSelectOverlay(inputHandler, Screen.Instance, Game.Dungeon.Player);
            inputHandler.SetSpecialScreenAndHandler(weaponOverlayScreen.RenderItemSelectOverlay, weaponOverlayScreen.KeyboardEvent, weaponOverlayScreen.MouseButtonEvent);
            return new ActionResult();
        }

        public ActionResult UtilitySelectOverlay(InputHandler inputHandler)
        {
            var utilityOverlayScreen = new UtilitySelectOverlay(inputHandler, Screen.Instance, Game.Dungeon.Player);
            inputHandler.SetSpecialScreenAndHandler(utilityOverlayScreen.RenderItemSelectOverlay, utilityOverlayScreen.KeyboardEvent, utilityOverlayScreen.MouseButtonEvent);
            return new ActionResult();
        }
    }
}
