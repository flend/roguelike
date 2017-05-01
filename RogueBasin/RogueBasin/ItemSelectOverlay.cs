using SdlDotNet.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    class ItemSelectOverlay
    {
        private readonly Player player;
        private readonly InputHandler inputHandler;
        private readonly Screen screen;

        public ItemSelectOverlay(InputHandler inputHandler, Screen screen, Player player)
        {
            this.player = player;
            this.inputHandler = inputHandler;
            this.screen = screen;
        }

        public void KeyboardEvent(KeyboardEventArgs keyboardArgs)
        {
            inputHandler.ClearSpecialScreenAndHandler();
        }

        public void MouseButtonEvent(MouseButtonEventArgs mouseArgs)
        {
            inputHandler.ClearSpecialScreenAndHandler();
        }

        public void RenderItemSelectOverlay()
        {
            //Draw all available ranged weapons
            var rangedWeapons = player.GetRangedWeaponsOrdered();

            var ySpacing = 30;
            var yOffset = screen.rangedWeaponUISize.Height + screen.UIScale(ySpacing);

            int counter = 0;

            foreach(var weapon in rangedWeapons)
            {
                RenderWeaponUI(weapon, new Point(0, -1 * yOffset * counter));
                counter++;
            }
        }

        private void RenderWeaponUI(Item weapon, Point yOffset)
        {
            IEquippableItem weaponE = weapon as IEquippableItem;
            RangedWeapon weaponR = weapon as RangedWeapon;

            screen.DrawUISpriteByCentre(weapon, new Point(screen.playerUI_TL.x + screen.rangedWeaponUICenter.x, screen.playerUI_TL.y + screen.rangedWeaponUICenter.y) + yOffset);

            var rangedDamage = player.ScaleRangedDamage(weaponE, weaponE.DamageBase());

            //Draw bullets
            double weaponAmmoRatio = weaponE.RemainingAmmo() / (double)weaponE.MaxAmmo();
            var ammoBarTL = screen.playerUI_TL + screen.ammoBarOffset_TL + yOffset;
            screen.DrawGraduatedBarVertical("ui_bullet", weaponAmmoRatio, new Rectangle(ammoBarTL.ToPoint(), screen.ammoBarArea), 0.1);

            //Ranged Damage base

            var rangedStr = "DMG: " + rangedDamage;
            screen.DrawSmallUIText(rangedStr, screen.playerUI_TL + screen.playerRangedTextOffset + yOffset, LineAlignment.Center, screen.statsColor);

            //Help

            var rangedHelp = "(F)";
            screen.DrawText(rangedHelp, screen.playerUI_TL + screen.rangedHelpOffset + yOffset, LineAlignment.Center, screen.statsColor);
        }
    }
}
