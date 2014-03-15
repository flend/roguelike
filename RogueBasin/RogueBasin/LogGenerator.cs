using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RogueBasin
{
    public enum LogType {
        Elevator, QuestArbitrary, SimpleLockedDoor, GoodyDoor
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
        Dictionary<LogType, Dictionary<string, List<LogExtract>>> logDatabase = new Dictionary<LogType, Dictionary<string, List<LogExtract>>>();
        Dictionary<string, LogExtract> logDatabaseByFilename = new Dictionary<string, LogExtract>();

        List<string> femaleFirst = new List<string>();
        List<string> maleFirst = new List<string>();
        List<string> allLast = new List<string>();

        List<string> femaleFirstMaster = new List<string>();
        List<string> maleFirstMaster = new List<string>();
        List<string> allLastMaster = new List<string>();

        Dictionary<string, LogType> logPrefixMapping = new Dictionary<string, LogType>() {
            { "el", LogType.Elevator },
            { "qe", LogType.QuestArbitrary },
            { "dr", LogType.SimpleLockedDoor },
            { "gds", LogType.GoodyDoor }
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
                        allLastMaster.Add(thisLine);
                    }
                }

                filename = "RogueBasin.bin.Debug.logdata.male_first.txt";
                _fileStream = _assembly.GetManifestResourceStream(filename);

                using (StreamReader reader = new StreamReader(_fileStream))
                {
                    string thisLine;
                    while ((thisLine = reader.ReadLine()) != null)
                    {
                        maleFirstMaster.Add(thisLine);
                    }
                }

                filename = "RogueBasin.bin.Debug.logdata.female_first.txt";
                _fileStream = _assembly.GetManifestResourceStream(filename);

                using (StreamReader reader = new StreamReader(_fileStream))
                {
                    string thisLine;
                    while ((thisLine = reader.ReadLine()) != null)
                    {
                        femaleFirstMaster.Add(thisLine);
                    }
                }

                //We maintain a subset of names so that it seems that we have an actual crew
                int subsetNumber = 10;
                femaleFirst = femaleFirstMaster.RandomElements(subsetNumber);
                maleFirst = maleFirstMaster.RandomElements(subsetNumber);
                allLast = allLastMaster.RandomElements(subsetNumber);
            }
            catch (Exception)
            {
                //Ignore it
            }
        }

        private void LoadLogDatabase()
        {
            foreach (var e in Enum.GetValues(typeof(LogType)).Cast<LogType>())
            {
                logDatabase[e] = new Dictionary<string, List<LogExtract>>();
            }

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
                    var logShortfilenamewithsuffix = logShortfilename;

                    int logNo = 0;
                    var numberSuffix = int.TryParse(logShortfilename.Substring(logShortfilename.Count() - 1), out logNo);

                    if (numberSuffix)
                        logShortfilename = logShortfilename.Substring(0, logShortfilename.Count() - 1);

                    var logPrefix = logShortfilename.Substring(0, logShortfilename.IndexOf('_'));
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

                    if (!logDatabase[logType].ContainsKey(logShortfilename))
                        logDatabase[logType][logShortfilename] = new List<LogExtract>();
                    logDatabase[logType][logShortfilename].Add(logExtract);
                    logDatabaseByFilename.Add(logShortfilenamewithsuffix, logExtract);
                }
                catch (Exception)
                {
                    //Ignore it
                    continue;
                }
            }
        }
        
        public LogEntry GenerateDoorLogEntry(string doorId, int levelForDoor)
        {
            var entry = new LogEntry();

            entry.title = GenerateRandomTitle();

            var randomLog = logDatabase[LogType.SimpleLockedDoor].RandomElement();
            var substitutedLog = randomLog.Value.First();
            List<string> logEntryLines = substitutedLog.lines;

            try
            {
                logEntryLines = ApplyStandardSubstitutions(logEntryLines);

                logEntryLines = ApplySubstitutions(logEntryLines, new Dictionary<string, string> {
                { "<level>", Game.Dungeon.DungeonInfo.LevelNaming[levelForDoor] },
                { "<idtype>", doorId }
            });

            }
            catch (Exception)
            {
             //Not to worry
            }

            entry.lines = logEntryLines;

            return entry;
        }

        public LogEntry GenerateGoodyRoomLogEntry(string doorId, int levelForDoor, List<Item> itemsInRoom)
        {
            var entry = new LogEntry();

            entry.title = GenerateRandomTitle();

            var randomLog = logDatabase[LogType.GoodyDoor].RandomElement();
            var substitutedLog = randomLog.Value.First();
            List<string> logEntryLines = substitutedLog.lines;

            try
            {
                logEntryLines = ApplyStandardSubstitutions(logEntryLines);

                logEntryLines = ApplySubstitutions(logEntryLines, new Dictionary<string, string> {
                { "<doorlevel>", Game.Dungeon.DungeonInfo.LevelNaming[levelForDoor] },
                { "<idtype>", doorId },
                { "<item>", itemsInRoom.RandomElement().SingleItemDescription }
            });

            }
            catch (Exception)
            {
                //Not to worry
            }

            entry.lines = logEntryLines;

            return entry;
        }

        public List<LogEntry> GenerateCoupledDoorLogEntry(string doorId, int levelForDoor, int levelForClue)
        {
            
            //Ensure we have 2 coupled entries
            var randomLog = logDatabase[LogType.SimpleLockedDoor].Where(kv => kv.Value.Count() == 2).RandomElement();
            
            var firstLog = randomLog.Value.ElementAt(0);
            var secondLog = randomLog.Value.ElementAt(1);

            var firstLogName = allLast.RandomElement();
            var secondLogName = allLast.RandomElement();

            var firstReturnLog = new LogEntry();
            firstReturnLog.title = GenerateRandomTitle(firstLogName);

            var secondReturnLog = new LogEntry();
            secondReturnLog.title = GenerateRandomTitle(secondLogName);

            var firstlogEntryLines = firstLog.lines;

            try
            {
                firstlogEntryLines = ApplySubstitutions(firstlogEntryLines, new Dictionary<string, string> {
                { "<doorlevel>", Game.Dungeon.DungeonInfo.LevelNaming[levelForDoor] },
                { "<cluelevel>", Game.Dungeon.DungeonInfo.LevelNaming[levelForClue] },
                { "<lastname>", secondLogName },
                { "<idtype>", doorId }
            });

            }
            catch (Exception)
            {
                //Not to worry
            }

            firstReturnLog.lines = firstlogEntryLines;

            var secondlogEntryLines = secondLog.lines;

            try
            {
                secondlogEntryLines = ApplySubstitutions(secondlogEntryLines, new Dictionary<string, string> {
                { "<doorlevel>", Game.Dungeon.DungeonInfo.LevelNaming[levelForDoor] },
                { "<cluelevel>", Game.Dungeon.DungeonInfo.LevelNaming[levelForClue] },
                { "<lastname>", firstLogName },
                { "<idtype>", doorId }
            });

            }
            catch (Exception)
            {
                //Not to worry
            }

            firstReturnLog.lines = firstlogEntryLines;
            secondReturnLog.lines = secondlogEntryLines;

            return new List<LogEntry>{ firstReturnLog, secondReturnLog};
        }

        public LogEntry GenerateElevatorLogEntry(int sourceLevel, int targetLevel)
        {
            var entry = new LogEntry();

            entry.title = GenerateRandomTitle();

            var randomLog = logDatabase[LogType.Elevator].RandomElement();
            var substitutedLog = randomLog.Value.First();
            List<string> logEntryLines = substitutedLog.lines;

            try {
                logEntryLines = ApplySubstitutions(logEntryLines, new Dictionary<string, string> {
                { "<source>", Game.Dungeon.DungeonInfo.LevelNaming[sourceLevel] },
                { "<target>", Game.Dungeon.DungeonInfo.LevelNaming[targetLevel] }
            });

            } catch(Exception) {
                logEntryLines = ApplySubstitutions(logEntryLines, new Dictionary<string, string> {
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

        public List<string> ApplyStandardSubstitutions(IEnumerable<string> log)
        {
            return ApplySubstitutions(log, new Dictionary<string, string> {
                { "<lastname>", allLast.RandomElement() },
                { "<femalefirst>", femaleFirst.RandomElement() },
                { "<malefirst>", maleFirst.RandomElement() }
            });
        }

        public List<string> ApplySubstitutions(IEnumerable<string> log, Dictionary<string, string> subtitutions)
        {
            var listToReturn = new List<string>();

            foreach (var str in log)
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

            if (Game.Random.Next(2) > 0)
                firstName = maleFirst.RandomElement();
            else
                firstName = femaleFirst.RandomElement();

            string surname = allLast.RandomElement();

            return GenerateRandomTitle(firstName, surname);
        }

        public string GenerateRandomTitle(string surname)
        {
            string firstName;

            if (Game.Random.Next(2) > 0)
                firstName = maleFirst.RandomElement();
            else
                firstName = femaleFirst.RandomElement();

            return GenerateRandomTitle(firstName, surname);
        }

        public string GenerateRandomTitle(string firstName, string lastName)
        {
            string prefix = "--.--";
            string suffix = "--.--";

            string randomDate = "" + Game.Random.Next(10) + Game.Random.Next(10) + Game.Random.Next(10) + "." + Game.Random.Next(10) + Game.Random.Next(10);

            var fullString = prefix + lastName + " " + firstName + " DX:" + randomDate + suffix;

            var centering = (40 - fullString.Count()) / 2;

            var fullStringWithCentering = new String(' ', centering) + fullString;

            return fullStringWithCentering;
        }

    }
}
