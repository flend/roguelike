using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RogueBasin
{
    public sealed class LogFile
    {
        static LogFile instance = null;

        string logFilename;
        StreamWriter logFile;

        bool logFileWorking = false;

        LogFile()
        {
            //Create logfile name


            logFilename = "roguelog_" + LogTime(DateTime.Now) + ".txt";

            try
            {
                //Open logfile
                logFile = new StreamWriter(logFilename);
                logFile.Close();
                logFileWorking = true;
            }
            catch (Exception e)
            {
                throw new ApplicationException("Couldn't start log file: " + e.Message);
            }
        }

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
                logFile = new StreamWriter(logFilename, true);
                logFile.WriteLine(datedEntry);
                logFile.Close();
            }
            catch (Exception)
            {
                //Fail silently
            }

            //To debug console
            Screen.Instance.ConsoleLine(datedEntry);
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
    }
}