using System;

namespace RogueBasin
{
    public class GuiInputHandler
    {
        private readonly PlayerActions playerActions;
        private readonly GameActions gameActions;
        private readonly SystemActions systemActions;
        private readonly InputHandler inputHandler;

        public GuiInputHandler(InputHandler inputHandler, PlayerActions playerActions, GameActions gameActions, SystemActions systemActions)
        {
            this.inputHandler = inputHandler;
            this.playerActions = playerActions;
            this.gameActions = gameActions;
            this.systemActions = systemActions;
        }

        public ActionResult PrimaryMouseClick(Point clickLocation, bool shifted)
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