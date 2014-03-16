using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RogueBasin
{
    public enum LogDebugLevel
    {
        High = 1, Medium = 2, Low = 3, Profiling = 4
    }

    public sealed class LogFile
    {
        static LogFile instance = null;

        string logFilename;
        StreamWriter logFile;

        bool logFileWorking = false;

        int debugLevel = 0;

        LogFile()
        {
            //Create logfile name
            logFilename = "logs/roguelog_" + LogTime(DateTime.Now) + ".txt";

            try
            {
                //Open logfile
                Directory.CreateDirectory("logs");
                logFile = new StreamWriter(logFilename);
                logFileWorking = true;
            }
            catch (Exception e)
            {
                throw new ApplicationException("Couldn't start log file: " + e.Message);
            }
        }

        /// <summary>
        /// Debug entry for the log. Will only be displayed if the log verbosity is at least level debugLevel
        /// </summary>
        /// <param name="entry"></param>
        public void LogEntryDebug(string entry, LogDebugLevel entryLevel)
        {
            if (debugLevel >= (int)entryLevel)
                LogEntry(entry);
        }

        /// <summary>
        /// Entry for the log
        /// </summary>
        /// <param name="entry"></param>
        public void LogEntry(string entry)
        {
            if (!logFileWorking)
            {
                //Fail silently
                return;
            }

            //Make entry
            string datedEntry = LogTime(DateTime.Now) + ": " + entry;
            //Add to file
            try
            {
                //In case we log again after closing
                if(logFile == null)
                    logFile = new StreamWriter(logFilename, true, Encoding.Default);

                logFile.WriteLine(datedEntry);
                logFile.Flush(); //debug only
            }
            catch (Exception)
            {
                //Fail silently
            }

            //To debug console
            //debug only
            //Screen.Instance.ConsoleLine(datedEntry);
        }

        public void Close()
        {
            if(logFile != null)
                logFile.Close();
            logFile = null;
        }

        //Produce save dateTime string for filenames
        string LogTime(DateTime dateTime)
        {
            string ret = dateTime.Year.ToString("0000") + "-" + dateTime.Month.ToString("00") + "-" + dateTime.Day.ToString("00") + "_" + dateTime.Hour.ToString("00") + "-" + dateTime.Minute.ToString("00") + "-" + dateTime.Second.ToString("00");
            return ret;
        }

        ~LogFile()
        {
            //Close logfile
            if(logFile != null)
                logFile.Close();
        }

        public static LogFile Log
        {
            get
            {
                if (instance == null)
                {
                    instance = new LogFile();
                }
                return instance;
            }
        }

        /// <summary>
        /// Set the level of debugging info to include
        /// </summary>
        public int DebugLevel
        {
            set
            {
                debugLevel = value;
            }
            get
            {
                return debugLevel;
            }
        }
    }
}