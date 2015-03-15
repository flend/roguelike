using SdlDotNet.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    public class SoundPlayer
    {
        static SoundPlayer instance;
        List<string> soundsToPlay = new List<string>();

        Dictionary<string, Sound> soundCache = new Dictionary<string, Sound>();

        protected SoundPlayer() {  }

        public static SoundPlayer Instance() { 
            if(instance == null)
                instance = new SoundPlayer();

            return instance;
        }

        public void EnqueueSound(string sound)
        {
            string path = "sounds/" + sound + ".ogg";
            soundsToPlay.Add(path);
        }

        public void PlaySounds()
        {
            var distinctSounds = soundsToPlay.Distinct();
            foreach (string sound in distinctSounds)
            {
                try
                {
                    Sound soundObj;
                    soundCache.TryGetValue(sound, out soundObj);

                    if (soundObj == null)
                    {
                        soundObj = new Sound(sound);
                        soundCache[sound] = soundObj;
                    }
                    soundObj.Play();
                }
                catch (Exception ex)
                {
                    LogFile.Log.LogEntryDebug("Failed to play sound " + sound + " : " + ex.Message, LogDebugLevel.High);
                }
            }
            soundsToPlay.Clear();
        }
    }
}
