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

        private Dictionary<Type, Rectangle> panelLocations = new Dictionary<Type, Rectangle>();

        public ItemSelectOverlay(InputHandler inputHandler, Screen screen, Player player)
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
                    var weaponType = kv.Key;
                    var rectangle = kv.Value;

                    if(rectangle.Contains(mouseArgs.Position))
                    {
                        Game.Dungeon.Player.EquipInventoryItemType(weaponType);
                        return new SpecialScreenActionResult(true);
                    }
                }
            }

            return new SpecialScreenActionResult(false);
        }

        public void RenderItemSelectOverlay()
        {
            //Draw all available ranged weapons
            var rangedWeapons = player.GetRangedWeaponsOrdered();
            var equippedRangedWeapon = EnumerableEx.Return(player.GetEquippedRangedWeapon() as RangedWeapon);
            var rangedWeaponsExceptEquipped = rangedWeapons.Except(equippedRangedWeapon);
            var rangedWeaponsStartingWithEquipped = equippedRangedWeapon.Concat(rangedWeaponsExceptEquipped);

            var ySpacing = 30;
            var xSpacing = 30;
            var yOffset = screen.rangedWeaponUISize.Height + screen.UIScale(ySpacing);
            var xOffset = screen.rangedWeaponUISize.Width + screen.UIScale(xSpacing);

            var bottomPanelCentre = screen.playerUI_TL + screen.rangedWeaponUICenter;

            var yPanelsMax = Math.Floor((double)(bottomPanelCentre.y - screen.rangedWeaponUISize.Height / 2) / (screen.rangedWeaponUISize.Height + ySpacing)) + 1;
            var xPanelsMax = Math.Floor((double)(screen.ScreenWidth - bottomPanelCentre.x - screen.rangedWeaponUISize.Width / 2) / (screen.rangedWeaponUISize.Width + xSpacing)) + 1;

            int xCoord = 0;
            int yCoord = 0;

            foreach(var weapon in rangedWeaponsStartingWithEquipped)
            {
                var panelOffset = new Point(xOffset * xCoord, -1 * yOffset * yCoord);
                var panelCentre = new Point(screen.playerUI_TL.x + screen.rangedWeaponUICenter.x, screen.playerUI_TL.y + screen.rangedWeaponUICenter.y) + panelOffset;
                var panelTL = panelCentre - new Point(screen.rangedWeaponUISize.Width / 2, screen.rangedWeaponUISize.Height / 2);
            
                var panelRectange = new Rectangle(panelTL.ToPoint(), screen.rangedWeaponUISize);

                panelLocations[weapon.GetType()] = panelRectange;

                RenderWeaponUI(weapon, panelOffset);
                yCoord++;
                if(yCoord >= yPanelsMax)
                {
                    xCoord++;
                    yCoord = 0;
                }

                if(xCoord >= xPanelsMax)
                {
                    LogFile.Log.LogEntryDebug("Too many weapons to be displayed on screen", LogDebugLevel.Medium);
                    break;
                }
            }
        }

        private void RenderWeaponUI(Item weapon, Point offset)
        {
            IEquippableItem weaponE = weapon as IEquippableItem;
            RangedWeapon weaponR = weapon as RangedWeapon;

            var rangedWeaponUICentre = new Point(screen.playerUI_TL.x + screen.rangedWeaponUICenter.x, screen.playerUI_TL.y + screen.rangedWeaponUICenter.y) + offset;
            screen.DrawUISpriteByCentre("rangedweaponui", rangedWeaponUICentre);
            screen.DrawUISpriteByCentre(weapon, new Point(screen.playerUI_TL.x + screen.rangedWeaponUICenter.x, screen.playerUI_TL.y + screen.rangedWeaponUICenter.y) + offset);

            var rangedDamage = player.ScaleRangedDamage(weaponE, weaponE.DamageBase());

            //Draw bullets
            double weaponAmmoRatio = weaponE.RemainingAmmo() / (double)weaponE.MaxAmmo();
            var ammoBarTL = screen.playerUI_TL + screen.ammoBarOffset_TL + offset;
            screen.DrawGraduatedBarVertical("ui_bullet", weaponAmmoRatio, new Rectangle(ammoBarTL.ToPoint(), screen.ammoBarArea), 0.1);

            //Ranged Damage base

            var rangedStr = "DMG: " + rangedDamage;
            screen.DrawSmallUIText(rangedStr, screen.playerUI_TL + screen.playerRangedTextOffset + offset, LineAlignment.Center, screen.statsColor);

            //Index

            var indexStr = "(" + weaponR.Index().ToString() + ")";
            screen.DrawText(indexStr, screen.playerUI_TL + screen.rangedIndexOffset + offset, LineAlignment.Center, screen.statsColor);

        }
    }
}
