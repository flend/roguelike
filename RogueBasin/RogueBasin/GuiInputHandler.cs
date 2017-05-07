using SdlDotNet.Input;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RogueBasin
{
    public class UIAction
    {
        public readonly Rectangle uiRect;
        public readonly MouseButton mouseButton;
        public readonly Func<ActionResult> uiAction;

        public UIAction(Rectangle uiRect, MouseButton mouseButton, Func<ActionResult> uiAction)
        {
            this.uiRect = uiRect;
            this.mouseButton = mouseButton;
            this.uiAction = uiAction;
        }
    }

    public class GuiInputHandler
    {
        private readonly PlayerActions playerActions;
        private readonly GameActions gameActions;
        private readonly SystemActions systemActions;
        private readonly InputHandler inputHandler;

        private List<UIAction> uiPanelActions = new List<UIAction>();

        public GuiInputHandler(InputHandler inputHandler, PlayerActions playerActions, GameActions gameActions, SystemActions systemActions)
        {
            this.inputHandler = inputHandler;
            this.playerActions = playerActions;
            this.gameActions = gameActions;
            this.systemActions = systemActions;
        }

        private void SetupUIPanelActions()
        {
            var weaponSelectAction = new UIAction(Screen.Instance.rangedWeaponUIRectAbs, MouseButton.PrimaryButton, new Func<ActionResult>(WeaponSelectOverlay));
            uiPanelActions.Add(weaponSelectAction);

            var utilitySelectAction = new UIAction(Screen.Instance.utilityUIRectAbs, MouseButton.PrimaryButton, new Func<ActionResult>(UtilitySelectOverlay));
            uiPanelActions.Add(utilitySelectAction);
        }

        public ActionResult HandleMouseClick(MouseButtonEventArgs clickLocation, bool shifted)
        {
            //Delayed until the screen has had time to render once and calculate UI rects
            if (uiPanelActions.Count == 0)
            {
                SetupUIPanelActions();
            }

            foreach(var panelAction in uiPanelActions)
            {
                if(panelAction.uiRect.Contains(clickLocation.Position) && panelAction.mouseButton == clickLocation.Button)
                {
                    return panelAction.uiAction();
                }
            }

            return new ActionResult();
        }

        private ActionResult WeaponSelectOverlay()
        {
            return gameActions.WeaponSelectOverlay(inputHandler);
        }

        private ActionResult UtilitySelectOverlay()
        {
            return gameActions.UtilitySelectOverlay(inputHandler);
        }

        public bool PositionInUI(Point point)
        {
            if(point.y >= Screen.Instance.playerTextUI_TL.y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}