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

        public void TemplatedMapTest()
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
                    StandardDungeonSetup();

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
            Game.Base.PlayMovie("qe_start", true);
            Game.Base.PlayMovie("helpkeys", true);

            if (Game.Dungeon.Player.PlayItemMovies)
            {
                Game.Base.PlayMovie("helpkeys", true);
            }
        }

        private void GenerateStoryDungeon(bool retry)
        {
            //Setup a single test level
            TraumaWorldGenerator worldGen = new TraumaWorldGenerator();

            worldGen.GenerateTraumaLevels(retry);

            Game.Dungeon.MapState = worldGen.MapState;
            Game.Dungeon.DungeonInfo.LevelNaming = worldGen.MapState.LevelGraph.LevelReadableNames.ToDictionary(kv => kv.Key, kv => kv.Value);

            LogFile.Log.LogEntryDebug("Player start: " + Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation, LogDebugLevel.High);
        }

        private void RandomSetup() {
            int seedToUse = new Random().Next();
            seedToUse = 153;
            LogFile.Log.LogEntry("Random seed: " + seedToUse);
            Game.Random = new Random(seedToUse);
        }

        GameDifficulty difficulty;
        string playerName;
        bool playItemMovies;

        private void IntroScreen()
        {
            //live
            //var gameInfo = new RogueBasin.GameIntro();

            //gameInfo.ShowIntroScreen();

            //difficulty = gameInfo.Difficulty;
            //playerName = gameInfo.PlayerName;
            //playItemMovies = gameInfo.ShowMovies;

            //dev
            difficulty = GameDifficulty.Medium;
            playerName = "Dave";
            playItemMovies = true;
        }
    
        private void StandardSystemSetup()
        {
            rb = new RogueBase();
            Game.Base = rb;
            rb.SetupSystem();

            //Minimum debug
            if(Game.Config.DebugMode)
                LogFile.Log.DebugLevel = 4;
            else
                LogFile.Log.DebugLevel = 1;
        }

        private void StandardDungeonSetup()
        {
            var dungeonInfo = new DungeonInfo();
            Game.Dungeon = new Dungeon(dungeonInfo);

            Game.Dungeon.Player.StartGameSetup();

            Game.Dungeon.Difficulty = difficulty;
            Game.Dungeon.Player.Name = playerName;
            Game.Dungeon.Player.PlayItemMovies = playItemMovies;

            Game.Dungeon.AllLocksOpen = false;

            if(Game.Config.DebugMode)
                Game.Dungeon.PlayerImmortal = true;
        }

        private void RunGame()
        {
            Game.Base.StartGame();

            rb.StartEventLoop();

            //Movies can only be shown after event loop started
            ShowIntroMovies();
            
        }
    }
}
