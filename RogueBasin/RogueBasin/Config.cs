using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RogueBasin
{
    public class Config
    {
        public Dictionary<string, string> Entries { get; private set; }

        public Config(string filename)
        {
            Entries = new Dictionary<string, string>();

            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    LogFile.Log.LogEntryDebug("Loading config file: " + filename, LogDebugLevel.Medium);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] words = line.Split('=');
                        Entries.Add(words[0], words[1]);
                        LogFile.Log.LogEntryDebug("Adding config property: " + words[0] + ":" + words[1], LogDebugLevel.Low);
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntryDebug("Can't open config file: " + filename + " error: " + ex.Message, LogDebugLevel.High);
            }
        }
    }
}
