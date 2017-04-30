using SdlDotNet.Audio;
using System;

namespace RogueBasin
{
    public class MusicPlayer
    {
        static MusicPlayer instance;
        bool playing = false;
        public bool Initialised { get; private set; }

        protected MusicPlayer() {

            SdlDotNet.Audio.MusicPlayer.EnableMusicFinishedCallback();
            SetupTune();
        }

        public static MusicPlayer Instance()
        { 
            if(instance == null)
                instance = new MusicPlayer();

            return instance;
        }

        internal void SetupTune()
        {
            Music bgMusic = new Music("music/hitman.ogg");
            SdlDotNet.Audio.MusicPlayer.Volume = 30;
            SdlDotNet.Audio.MusicPlayer.Load(bgMusic);
            SdlDotNet.Audio.MusicPlayer.Stop();
        }

        internal void Play()
        {

            try
            {
                if (!Initialised)
                {
                    SdlDotNet.Audio.MusicPlayer.Play(true);
                    Initialised = true;
                }

                if (!playing)
                {
                    SdlDotNet.Audio.MusicPlayer.Resume();
                    playing = true;
                }
                
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntryDebug("Failed to play music " + ex.Message, LogDebugLevel.High);
            }
        }

        internal void Stop()
        {
            try
            {

                SdlDotNet.Audio.MusicPlayer.Pause();
                playing = false;
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntryDebug("Failed to play music " + ex.Message, LogDebugLevel.High);
            }
        }
    }
}
