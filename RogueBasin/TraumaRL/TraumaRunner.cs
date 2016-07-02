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
            Game.Dungeon.DungeonInfo.LevelNaming = worldGen.MapState.LevelNames.ToDictionary(kv => kv.Key, kv => kv.Value);
            
            LogFile.Log.LogEntryDebug("Player start: " + Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation, LogDebugLevel.High);

            VisualiseConnectivityGraph(worldGen.MapState.MapInfo.Model, worldGen.MapState.DoorAndClueManager);
            VisualiseLevelConnectivityGraph(worldGen.MapState.MapInfo.Model, worldGen.MapState.DoorAndClueManager, worldGen.MapState.LevelNames);
        }

        private void VisualiseConnectivityGraph(MapModel graphModel, DoorAndClueManager doorAndClueManager)
        {
            var visualiser = new DoorClueGraphvizExport(graphModel, doorAndClueManager);
            visualiser.OutputFullGraph("bsptree-full");
            visualiser.OutputClueDoorGraph("bsptree-door");
            visualiser.OutputDoorDependencyGraph("bsptree-dep");

            if (Game.Config.DisplayGraphs) {
                try
                {
                    var graphVizLocation = Game.Config.Entries[Config.GraphVizLocation];

                    GraphVizUtils.RunGraphVizPNG(graphVizLocation, "bsptree-full");
                    GraphVizUtils.RunGraphVizPNG(graphVizLocation, "bsptree-door");
                    GraphVizUtils.RunGraphVizPNG(graphVizLocation, "bsptree-dep");

                    GraphVizUtils.DisplayPNGInChildWindow("bsptree-full");
                    GraphVizUtils.DisplayPNGInChildWindow("bsptree-door");
                    GraphVizUtils.DisplayPNGInChildWindow("bsptree-dep");
                }
                catch (Exception)
                {
                    LogFile.Log.LogEntryDebug("Can't find graphViz in config file", LogDebugLevel.High);
                }
            }   
        }

        private void VisualiseLevelConnectivityGraph(MapModel graphModel, DoorAndClueManager manager, ImmutableDictionary<int, string> levelNaming)
        {
            var visualiser = new DoorClueGraphvizExport(graphModel, manager, levelNaming);
            visualiser.OutputClueDoorGraph("levellinks-full");
            if (Game.Config.DebugMode)
            {
                if (Game.Config.DisplayGraphs)
                {
                    try
                    {
                        var graphVizLocation = Game.Config.Entries[Config.GraphVizLocation];

                        GraphVizUtils.RunGraphVizPNG(graphVizLocation, "levellinks-full");
                        GraphVizUtils.DisplayPNGInChildWindow("levellinks-full");
                    }
                    catch (Exception)
                    {
                        LogFile.Log.LogEntryDebug("Can't find graphViz in config file", LogDebugLevel.High);
                    }
                }
            }
        }

        private void RandomSetup() {
            int seedToUse = new Random().Next();
            //int seedToUse = 153;
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
            Game.Dungeon.Player.LocationLevel = 0;
            Game.Dungeon.Player.LocationMap = Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation;

            Game.Base.StartGame();

            rb.StartEventLoop();

            //Movies can only be shown after event loop started
            ShowIntroMovies();
            
        }
    }
}
