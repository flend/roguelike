using System;
using System.Collections.Generic;

namespace RogueBasin
{

    public class SystemActions
    {
        InputHandler inputHandler;
        
        public void SetInputHandler(InputHandler inputHandler)
        {
            this.inputHandler = inputHandler;
        }

        public void MusicStart()
        {
            Game.MessageQueue.AddMessage("Music on");
            MusicPlayer.Instance().Play();
            Game.Base.PlayMusic = true;
        }

        public void MusicStop()
        {
            Game.MessageQueue.AddMessage("Music off");
            MusicPlayer.Instance().Stop();
            Game.Base.PlayMusic = false;
        }


        public void ToggleSounds()
        {
            if (Game.Base.PlaySounds)
            {
                SoundsOff();
            }
            else
            {
                SoundsOn();
            }
        }

        private void SoundsOn()
        {
            Game.MessageQueue.AddMessage("Sounds on");
            Game.Base.PlaySounds = true;
        }

        private void SoundsOff()
        {
            Game.MessageQueue.AddMessage("Sounds off");
            Game.Base.PlaySounds = false;
        }

        public void ToggleMusic()
        {
            if (Game.Base.PlayMusic)
            {
                MusicStop();
            }
            else
            {
                MusicStart();
            }
        }

        public void PlayMovie(string filename, bool keypressBetweenFrames)
        {
            Game.Base.InputHandler.SetInputState(InputHandler.InputState.MovieDisplay);
            Screen.Instance.EnqueueMovie(filename);
            Screen.Instance.NeedsUpdate = true;
        }

        public void PlayMovie(Movie movie, bool keypressBetweenFrames)
        {
            Game.Base.InputHandler.SetInputState(InputHandler.InputState.MovieDisplay);
            Screen.Instance.EnqueueMovie(movie);
            Screen.Instance.NeedsUpdate = true;
        }


        public void PlayLog(LogEntry logEntry)
        {
            try
            {
                var movieFrames = new List<MovieFrame>();

                var logFrame = new MovieFrame();
                var allLines = new List<string>();
                allLines.Add(logEntry.title);
                allLines.AddRange(logEntry.lines);
                logFrame.ScanLines = allLines;

                movieFrames.Add(logFrame);
                var movie = new Movie(movieFrames);

                PlayMovie(movie, true);
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Failed to play movie from frames " + ex.Message);
            }
        }

        public void DoEndOfGame(bool lived, bool won, bool quit)
        {
            Screen.Instance.EndOfGameWon = won;
            Screen.Instance.EndOfGameQuit = quit;

            Game.Base.GameStarted = false;

            inputHandler.SetSpecialScreenAndHandler(Screen.Instance.EndOfGameScreen, inputHandler.EndOfGameSelectionKeyHandler);
        }

        public void QuitImmediately()
        {
            Game.Dungeon.RunMainLoop = false;
            Game.Base.Events.QuitApplication();
        }

        public void RestartGameAfterDeath()
        {
            Game.Base.SetupGameWithNewDungeon();
        }
    }
}
