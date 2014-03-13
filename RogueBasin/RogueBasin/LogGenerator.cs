using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RogueBasin
{
    public enum LogType {
        Elevator, QuestArbitrary
    }

    public class LogExtract {
        public List<string> lines = new List<string>();
        public LogType logType;
    }

    public class LogEntry
    {
        public List<string> lines = new List<string>();
        public string title;
    }

    public class LogGenerator
    {
        List<LogExtract> logDatabase = new List<LogExtract>();
        Dictionary<string, LogExtract> logDatabaseByFilename = new Dictionary<string, LogExtract>();

        List<string> femaleFirst = new List<string>();
        List<string> maleFirst = new List<string>();
        List<string> allLast = new List<string>();

        Dictionary<string, LogType> logPrefixMapping = new Dictionary<string, LogType>() {
            { "el", LogType.Elevator },
            { "qe", LogType.QuestArbitrary }
        };

        public LogGenerator()
        {
            LoadLogDatabase();
            LoadNameDatabase();
        }

        private void LoadNameDatabase()
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            
            try
            {
                string filename = "RogueBasin.bin.Debug.logdata.all_last.txt";
                Stream _fileStream = _assembly.GetManifestResourceStream(filename);
                
                using (StreamReader reader = new StreamReader(_fileStream))
                {
                    string thisLine;
                    while ((thisLine = reader.ReadLine()) != null)
                    {
                        allLast.Add(thisLine);
                    }
                }

                filename = "RogueBasin.bin.Debug.logdata.male_first.txt";
                _fileStream = _assembly.GetManifestResourceStream(filename);

                using (StreamReader reader = new StreamReader(_fileStream))
                {
                    string thisLine;
                    while ((thisLine = reader.ReadLine()) != null)
                    {
                        maleFirst.Add(thisLine);
                    }
                }

                filename = "RogueBasin.bin.Debug.logdata.female_first.txt";
                _fileStream = _assembly.GetManifestResourceStream(filename);

                using (StreamReader reader = new StreamReader(_fileStream))
                {
                    string thisLine;
                    while ((thisLine = reader.ReadLine()) != null)
                    {
                        femaleFirst.Add(thisLine);
                    }
                }

            }
            catch (Exception)
            {
                //Ignore it
            }
        }

        private void LoadLogDatabase()
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();

            string[] names = _assembly.GetManifestResourceNames();

            var logEntries = names.Where(str => str.Contains("RogueBasin.bin.Debug.logentries"));

            foreach (var filename in logEntries)
            {
                
                try
                {
                    Stream _fileStream = _assembly.GetManifestResourceStream(filename);
                    var logFilenameWithoutSuffix = filename.Substring(0, filename.LastIndexOf('.'));
                    var logShortfilename = logFilenameWithoutSuffix.Substring(logFilenameWithoutSuffix.LastIndexOf('.') + 1);
                    var logPrefix = logShortfilename.Substring(0, 2);
                    var logType = logPrefixMapping[logPrefix];
                    
                    var logExtract = new LogExtract();

                    using (StreamReader reader = new StreamReader(_fileStream))
                    {

                        string thisLine;
                        while ((thisLine = reader.ReadLine()) != null)
                        {
                            logExtract.lines.Add(thisLine);
                        }
                    }

                    logExtract.logType = logType;

                    logDatabase.Add(logExtract);
                    logDatabaseByFilename.Add(logShortfilename, logExtract);
                }
                catch (Exception)
                {
                    //Ignore it
                    continue;
                }
            }
        }

        public LogEntry GenerateElevatorLogEntry(int sourceLevel, int targetLevel)
        {
            var entry = new LogEntry();

            entry.title = GenerateRandomTitle();

            var randomLog = logDatabase.Where(le => le.logType == LogType.Elevator).RandomElement();
            var substitutedLog = randomLog;
            List<string> logEntryLines;

            try {
                logEntryLines = ApplySubstitutions(substitutedLog, new Dictionary<string, string> {
                { "<source>", Game.Dungeon.DungeonInfo.LevelNaming[sourceLevel] },
                { "<target>", Game.Dungeon.DungeonInfo.LevelNaming[targetLevel] }
            });

            } catch(Exception) {
                logEntryLines = ApplySubstitutions(substitutedLog, new Dictionary<string, string> {
                { "<source>", "" },
                { "<target>", "" }
            });
            }

            entry.lines = logEntryLines;

            return entry;
        }

        public LogEntry GenerateArbitaryLogEntry(string logname)
        {
            var entry = new LogEntry();

            entry.title = GenerateRandomTitle();

            var logByName = logDatabaseByFilename[logname];
            
            entry.lines = logByName.lines;

            return entry;
        }

        public List<string> ApplySubstitutions(LogExtract log, Dictionary<string, string> subtitutions)
        {
            var listToReturn = new List<string>();

            foreach (var str in log.lines)
            {
                string subbedStr = str;
                foreach (var kv in subtitutions)
                {
                    subbedStr = subbedStr.Replace(kv.Key, kv.Value);
                }

                listToReturn.Add(subbedStr);
            }

            return listToReturn;
        }

        public string GenerateRandomTitle()
        {
            string firstName;

            if (Game.Random.Next(1) > 0)
                firstName = maleFirst.RandomElement();
            else
                firstName = femaleFirst.RandomElement();

            string surname = allLast.RandomElement();

            //string result = System.Globalization.TextInfo.ToTitleCase(firstName);

            string prefix = "--.--";
            string suffix = "--.--";

            string randomDate = "" + Game.Random.Next(10) + Game.Random.Next(10) + Game.Random.Next(10) + "." + Game.Random.Next(10) + Game.Random.Next(10);

            var fullString = prefix + surname + " " + firstName + " DX:" + randomDate + suffix;

            return fullString;
        }

    }
}
