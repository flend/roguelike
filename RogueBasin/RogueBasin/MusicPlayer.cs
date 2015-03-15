using SdlDotNet.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    public class MusicPlayer
    {
        static MusicPlayer instance;
        bool playing = false;
        public bool Initialised { get; private set; }

        protected MusicPlayer() {
            Music bgMusic = new Music("music/hitman.mp3");
            SdlDotNet.Audio.MusicPlayer.Volume = 30;
            SdlDotNet.Audio.MusicPlayer.Load(bgMusic);
        }

        public static MusicPlayer Instance()
        { 
            if(instance == null)
                instance = new MusicPlayer();

            return instance;
        }

        internal void ToggleMusic()
        {

            try
            {
                if (!Initialised)
                {
                    SdlDotNet.Audio.MusicPlayer.Play();
                    Initialised = true;
                }

                if (!playing)
                {
                    SdlDotNet.Audio.MusicPlayer.Resume();
                    playing = true;
                }
                else
                {
                    SdlDotNet.Audio.MusicPlayer.Pause();
                    playing = false;
                }
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntryDebug("Failed to play music " + ex.Message, LogDebugLevel.High);
            }
        }
    }
}
