using SdlDotNet.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    class MenuScreen
    {
        int MenuPosition { get; set; }
        static readonly int maxMenuPosition = 4;

        Color titleColor = System.Drawing.Color.Khaki;
        Color textColor = System.Drawing.Color.FromArgb(255, 108, 215, 224);
        Color optionColor = System.Drawing.Color.Khaki;
        Color optionBackgroundColor = System.Drawing.Color.Black;

        int centreX;
        int optionsTextY;
        int optionsOffset;

        Action exitFunction;

        public MenuScreen(Action exitFunction) {
            this.exitFunction = exitFunction;
        }

        public void DrawMenuScreen()
        {
            var s = Screen.Instance;

            var centreXOffset = s.ScreenWidth / 4;
            var centreYOffset = s.ScreenHeight / 4;

            var graphicsY = centreYOffset * 2 - centreYOffset / 2;

            centreX = s.ScreenWidth / 2;

            var titleY = centreYOffset;
            s.DrawText("RoyaLe!", new Point(centreX, titleY), LineAlignment.Center, 40, titleColor);

            var introTextY = centreYOffset + centreYOffset / 2;
            var titleLineOffset = s.ScreenHeight / 32;

            s.DrawLargeText("Welcome to the RoyaLe!", new Point(centreX, introTextY + titleLineOffset), LineAlignment.Center, textColor);
            s.DrawLargeText("Choose your fate:", new Point(centreX, introTextY + 2 * titleLineOffset), LineAlignment.Center, textColor);

            optionsTextY = (centreYOffset * 2);
            optionsOffset = titleLineOffset;

            //Start game

            DrawOption("Start Game", 0);

            //Game mode

            var modeText = Game.Dungeon.FunMode ? "Game Mode: [FUN] Life's a laugh and death's a joke, it's true." :
                "Game Mode: [ROGUELIKE]: I learnt to Crawl before I could walk.";

            DrawOption(modeText, 1);
            
            //Sound

            var soundStr = "Sound: " + (Game.Base.PlaySounds ? "On" : "Off");
            DrawOption(soundStr, 2);
        
            //Music

            var musicStr = "Music: " + (Game.Base.PlayMusic ? "On" : "Off");
            DrawOption(musicStr, 3);

            //Quit

            var quitStr = "Quit Game";
            DrawOption(quitStr, 4);
        }

        private Color ForegroundMenu(int menuOption) {
            if (menuOption == MenuPosition)
                return optionBackgroundColor;
            else
                return optionColor;
        }

        private Color BackgroundMenu(int menuOption)
        {
            if (menuOption == MenuPosition)
                return optionColor;
            else
                return optionBackgroundColor;
        }

        private void DrawOption(string menuOption, int menuIndex)
        {
            Screen.Instance.DrawText(menuOption, MenuPoint(menuIndex), LineAlignment.Center, ForegroundMenu(menuIndex), BackgroundMenu(menuIndex));
        }

        private Point MenuPoint(int menuOption)
        {
            return new Point(centreX, optionsTextY + menuOption * optionsOffset);
        }

        public void SelectMenuOptionUp() {
            MenuPosition--;
            if(MenuPosition < 0)
                MenuPosition = maxMenuPosition;
        }

        public void SelectMenuOptionDown()
        {
            MenuPosition++;
            if (MenuPosition > maxMenuPosition)
                MenuPosition = 0;
        }

        public void MenuScreenKeyboardHandler(KeyboardEventArgs args)
        {
            if (args.Key == Key.UpArrow || args.Key == Key.Keypad8)
            {
                SelectMenuOptionUp();
                return;
            }
            if (args.Key == Key.DownArrow || args.Key == Key.Keypad2)
            {
                SelectMenuOptionDown();
                return;
            }
            if (args.Key == Key.Return || args.Key == Key.KeypadEnter)
            {
                switch (MenuPosition)
                {
                    case 0:
                        Game.Base.ClearSpecialScreenAndHandler();
                        exitFunction();
                        return;
                    case 1:
                        Game.Dungeon.FunMode = !Game.Dungeon.FunMode;
                        return;
                    case 2:
                        Game.Base.ToggleSounds();
                        return;
                    case 3:
                        Game.Base.ToggleMusic();
                        break;
                    case 4:
                        Game.Base.QuitImmediately();
                        break;
                }
            }
        }
    }
}
