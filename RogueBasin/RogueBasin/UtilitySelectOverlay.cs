using SdlDotNet.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    class UtilitySelectOverlay
    {
        private readonly Player player;
        private readonly InputHandler inputHandler;
        private readonly Screen screen;

        private Dictionary<Type, Rectangle> panelLocations = new Dictionary<Type, Rectangle>();

        public UtilitySelectOverlay(InputHandler inputHandler, Screen screen, Player player)
        {
            this.player = player;
            this.inputHandler = inputHandler;
            this.screen = screen;
        }

        public SpecialScreenActionResult KeyboardEvent(KeyboardEventArgs keyboardArgs)
        {
            return new SpecialScreenActionResult(true);
        }

        public SpecialScreenActionResult MouseButtonEvent(MouseButtonEventArgs mouseArgs)
        {
            if (mouseArgs.Button == MouseButton.PrimaryButton)
            {
                foreach(var kv in panelLocations)
                {
                    var itemType = kv.Key;
                    var rectangle = kv.Value;

                    if(rectangle.Contains(mouseArgs.Position))
                    {
                        player.EquipInventoryItemType(itemType);
                        return new SpecialScreenActionResult(true);
                    }
                }
            }

            return new SpecialScreenActionResult(false);
        }

        public void RenderItemSelectOverlay()
        {
            var utilityItems = player.GetDistinctUtilityItemsOrdered();
            var equippedUtilityItem = EnumerableEx.Return(player.GetEquippedUtility() as Item);
            var utilityItemsExceptEquipped = utilityItems.Except(equippedUtilityItem);
            var utilityItemsStartingWithEquipped = equippedUtilityItem.Concat(utilityItemsExceptEquipped);

            var ySpacing = 30;
            var xSpacing = 30;
            var yOffset = screen.utilityUISize.Height + screen.UIScale(ySpacing);
            var xOffset = screen.utilityUISize.Width + screen.UIScale(xSpacing);

            var bottomPanelCentre = new Point(screen.playerUIRect.Location) + screen.utilityUICenter;

            var yPanelsMax = Math.Floor((double)(bottomPanelCentre.y - screen.utilityUISize.Height / 2) / (screen.utilityUISize.Height + ySpacing)) + 1;
            var xPanelsMax = Math.Floor((double)(screen.ScreenWidth - bottomPanelCentre.x - screen.utilityUISize.Width / 2) / (screen.utilityUISize.Width + xSpacing)) + 1;

            int xCoord = 0;
            int yCoord = 0;

            foreach(var utility in utilityItemsStartingWithEquipped)
            {
                var panelOffset = new Point(xOffset * xCoord, -1 * yOffset * yCoord);
                var playerUITL = screen.playerUIRect.TopLeft();
                var panelCentre = playerUITL + screen.utilityUICenter + panelOffset;
                var panelTL = panelCentre - new Point(screen.utilityUISize.Width / 2, screen.utilityUISize.Height / 2);
            
                var panelRectange = new Rectangle(panelTL.ToPoint(), screen.utilityUISize);

                panelLocations[utility.GetType()] = panelRectange;

                RenderUtilityUI(utility, panelOffset);
                yCoord++;
                if(yCoord >= yPanelsMax)
                {
                    xCoord++;
                    yCoord = 0;
                }

                if(xCoord >= xPanelsMax)
                {
                    LogFile.Log.LogEntryDebug("Too many utilities to be displayed on screen", LogDebugLevel.Medium);
                    break;
                }
            }
        }

        private void RenderUtilityUI(Item utility, Point offset)
        {
            var utilityUICentre = screen.playerUIRect.TopLeft() + screen.utilityUICenter + offset;
            var utilityUIBackgroundCentre = screen.playerUIRect.TopLeft() + screen.utilityUICenter + screen.utilityUIBackgroundOffset + offset;
            screen.DrawUISpriteByCentre("utilityui", utilityUIBackgroundCentre);
            screen.DrawUISpriteByCentre(utility, utilityUICentre);

            //Draw no of items
            double itemNoRatio = Math.Min(player.GetNoItemsOfSameType(utility) / (double)10.0, 1.0);
            screen.DrawGraduatedBarVertical("ui_bullet", itemNoRatio, new Rectangle((screen.playerUIRect.TopLeft() + screen.itemNoBarOffset_TL + offset).ToPoint(), screen.utilityNumBarArea), 0.1);
        }
    }
}
