using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraumaRL
{

    class TraumaRunner
    {
        RogueBase rb;

        public void SetupWorldAndRunGame()
        {
            RandomSetup();

            StandardSystemSetup();

            IntroScreen();

            //For testing
            bool retry = false;
            bool failFast = false;

            //For release
            do
            {
                try
                {
                    rb.SetupDungeon();

                    GenerateStoryDungeon(retry);

                    break;
                }
                catch (Exception ex)
                {
                    retry = false;
                    LogFile.Log.LogEntryDebug("Failed to create dungeon : " + ex.Message + "\n" + ex.StackTrace, LogDebugLevel.High);
                    if (failFast)
                    {
                        throw ex;
                    }
                }
            } while (true);

            //Run game         
            RunGame();
        }

        private void ShowIntroMovies()
        {
            Game.Base.SystemActions.PlayMovie("qe_start", true);
            Game.Base.SystemActions.PlayMovie("helpkeys", true);

            if (Game.Dungeon.Player.PlayItemMovies)
            {
                Game.Base.SystemActions.PlayMovie("helpkeys", true);
            }
        }

        private void GenerateStoryDungeon(bool retry)
        {
            TraumaWorldGenerator worldGen = new TraumaWorldGenerator();

            worldGen.GenerateTraumaLevels(retry);
        }

        private void RandomSetup()
        {
            int seedToUse = new Random().Next();
            seedToUse = 153;
            LogFile.Log.LogEntry("Random seed: " + seedToUse);
            Game.Random = new Random(seedToUse);
        }

        private void IntroScreen()
        {
 
        }
    
        private void StandardSystemSetup()
        {
            rb = new RogueBase();
            Game.Base = rb;
            rb.SetupSystem();
            rb.SetupDungeon();
            rb.Initialise();

            //Minimum debug
            if(Game.Config.DebugMode)
                LogFile.Log.DebugLevel = 2;
            else
                LogFile.Log.DebugLevel = 1;
        }

        private void RunGame()
        {
            Game.Base.StartGame();

            Game.Base.Events.StartEventLoop();

            //Movies can only be shown after event loop started
            ShowIntroMovies();
            
        }
    }
}
