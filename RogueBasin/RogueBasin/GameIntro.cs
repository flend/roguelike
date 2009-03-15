using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using System.IO;

namespace RogueBasin
{
    /// <summary>
    /// Handles showing the intro screen and getting user input
    /// </summary>
    public class GameIntro
    {
        public string PlayerName { get; private set; }
        public bool ShowMovies { get; private set; }
        public GameDifficulty Difficulty { get; private set; }

        public GameIntro() {
            PlayerName = null;
        }

        /// <summary>
        /// Run the intro sequence. After this returns use properties to decide whether to load a game or start a new one
        /// </summary>
        public void ShowIntroScreen() {

            OpeningScreen();

            PlayerNameScreen();
        }

        Point preambleTL;

        private void PlayerNameScreen()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            //Draw frame
            //Why xpos 2 here?
            rootConsole.DrawFrame(1, 4, Screen.Instance.Width - 2, Screen.Instance.Height - 10, true);

            //Draw preample
            preambleTL = new Point(5, 7);

            int height;
            List<string> preamble = Utility.LoadTextFile("introPreamble", Screen.Instance.Width - 2 * preambleTL.x, out height);

            for (int i = 0; i < preamble.Count; i++)
            {
                rootConsole.PrintLineRect(preamble[i], preambleTL.x, preambleTL.y + i, Screen.Instance.Width - 2 * preambleTL.x, 1, LineAlignment.Left);
            }

            int nameYCoord = 5 + preamble.Count + 2;
            Point nameIntro = new Point(5, nameYCoord);
            do {
                PlayerName = Screen.Instance.GetUserString("Rogue name", nameIntro, 5);
                LogFile.Log.LogEntry("Player name: " + PlayerName);
            } while(PlayerName.Contains(" ") || PlayerName == "");

            //Check if this save game exists. If so we can exit now and the game will be loaded
            if (File.Exists(PlayerName + ".sav"))
            {
                return;
            }

            //Settings text
            int settingsYCoord = nameYCoord + 2;
            Point settingsTL = new Point(5, settingsYCoord);

            List<string> settingsText = Utility.LoadTextFile("introSettings", Screen.Instance.Width - 2 * preambleTL.x, out height);

            for (int i = 0; i < settingsText.Count; i++)
            {
                rootConsole.PrintLineRect(settingsText[i], settingsTL.x, settingsTL.y + i, Screen.Instance.Width - 2 * settingsTL.x, 1, LineAlignment.Left);
            }

            //Ask settings questions
            ShowMovies = Screen.Instance.YesNoQuestion("Show plot text and movies?", new Point(settingsTL.x, settingsTL.y + height + 1));

            //Ask settings questions
            Difficulty = Screen.Instance.DifficultyQuestion("Game Difficulty Easy / Medium / Hard?", new Point(settingsTL.x, settingsTL.y + height + 3));

        }

        /// <summary>
        /// Title screen. Press any key to continue
        /// </summary>

        Point titleCentre;
        Point anyKeyLocation;
        
        private void OpeningScreen()
        {
            Screen screen = Screen.Instance;

            //Get screen handle

            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw title

            titleCentre = new Point(screen.Width / 2, screen.Height / 2);
            rootConsole.PrintLineRect("Welcome to DDRogue", titleCentre.x, titleCentre.y, screen.Width, 1, LineAlignment.Center);

            //Any key to continue

            anyKeyLocation = new Point(screen.Width / 2, screen.Height - 5);
            rootConsole.PrintLineRect("Press any key to continue", anyKeyLocation.x, anyKeyLocation.y, screen.Width, 1, LineAlignment.Center);
            
            //Update screen
            Screen.Instance.FlushConsole();

            //Wait for key
            KeyPress userKey = Keyboard.WaitForKeyPress(true);
        }
    }
}
