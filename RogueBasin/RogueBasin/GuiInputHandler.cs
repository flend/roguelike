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

            SetupUIPanelActions();
        }

        private void SetupUIPanelActions()
        {
            var itemSelectRect = new Rectangle(new System.Drawing.Point(0, 0), new Size(Screen.Instance.ScreenWidth, Screen.Instance.ScreenHeight));
            var itemSelectAction = new UIAction(itemSelectRect, MouseButton.PrimaryButton, new Func<ActionResult>(ItemSelectOverlay));
            uiPanelActions.Add(itemSelectAction);
        }

        public ActionResult HandleMouseClick(MouseButtonEventArgs clickLocation, bool shifted)
        {
            foreach(var panelAction in uiPanelActions)
            {
                if(panelAction.uiRect.Contains(clickLocation.Position) && panelAction.mouseButton == clickLocation.Button)
                {
                    return panelAction.uiAction();
                }
            }

            return new ActionResult();
        }

        private ActionResult ItemSelectOverlay()
        {
            return gameActions.ItemSelectOverlay(inputHandler);
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