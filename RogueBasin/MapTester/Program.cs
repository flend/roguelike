﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueBasin;
using GraphMap;

namespace MapTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var tester = new MapTester();
            
            //Choose manual test
            tester.TemplatedMapTest();
        }
    }

    class MapTester {

        RogueBase rb;

        public void TemplatedMapTest()
        {
            StandardGameSetup();

            //int seedToUse = 150;
            //Game.Random = new Random(seedToUse);
            Game.Random = new Random();

            bool testGraphics = false;
            bool multiLevelDungeon = false;
            bool storyDungeon = true;

            if (testGraphics)
            {
                TestGraphics();
            }
            else
            {
                
                if (multiLevelDungeon)
                    GenerateMultiLevelDungeon();
                else if(storyDungeon)
                    GenerateStoryDungeon();
                else {
                    //Setup a single test level
                    MapGeneratorTemplated templateGen = new MapGeneratorTemplated();

                    //Map templateMap = templateGen.GenerateMap2();
                    Map templateMap = templateGen.GenerateMapBranchRooms();

                    int levelNo = Game.Dungeon.AddMap(templateMap);

                    LogFile.Log.LogEntryDebug("Player start: " + Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation, LogDebugLevel.High);

                    //Extract connectivity map
                    var graphModel = new MapModel(templateGen.ConnectivityMap, 0);
                    VisualiseConnectivityGraph(graphModel);
                }
            }
            RunGame();
        }

        private void GenerateMultiLevelDungeon()
        {
            //Setup a single test level
            MapGeneratorTemplated templateGen = new MapGeneratorTemplated();

            //templateGen.GenerateMultiLevelDungeon();
            var mapInfo = templateGen.GenerateDungeonWithReplacedVaults();
            
            LogFile.Log.LogEntryDebug("Player start: " + Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation, LogDebugLevel.High);

            //Extract connectivity map
            VisualiseConnectivityGraph(mapInfo.Model);
        }

        private void GenerateStoryDungeon()
        {
            //Setup a single test level
            MapGeneratorTemplated templateGen = new MapGeneratorTemplated();

            //templateGen.GenerateMultiLevelDungeon();

            LogFile.Log.LogEntryDebug("Player start: " + Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation, LogDebugLevel.High);

            //Extract connectivity map
            //VisualiseConnectivityGraph(mapInfo.Model);
        }

        private void TestGraphics()
        {
            //Setup a single test level
            MapGeneratorTemplated templateGen = new MapGeneratorTemplated();

            var mapInfo = templateGen.GenerateTestGraphicsDungeon();
        }

        private void VisualiseConnectivityGraph(MapModel graphModel)
        {
            var visualiser = new DoorClueGraphvizExport(graphModel);
            visualiser.OutputFullGraph("bsptree-full");
            visualiser.OutputClueDoorGraph("bsptree-door");
            visualiser.OutputDoorDependencyGraph("bsptree-dep");

            try
            {
                var graphVizLocation = Game.Config.Entries[Config.GraphVizLocation];

                GraphVizUtils.RunGraphVizPNG(graphVizLocation, "bsptree-full");
                GraphVizUtils.DisplayPNGInChildWindow("bsptree-full");
                GraphVizUtils.RunGraphVizPNG(graphVizLocation, "bsptree-door");
                GraphVizUtils.DisplayPNGInChildWindow("bsptree-door");
                GraphVizUtils.RunGraphVizPNG(graphVizLocation, "bsptree-dep");
                GraphVizUtils.DisplayPNGInChildWindow("bsptree-dep");
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Can't find graphViz in config file", LogDebugLevel.High);
            }
        }

        private void StandardGameSetup()
        {
            rb = new RogueBase();
            rb.SetupSystem();

            Game.Dungeon = new Dungeon();

            Game.Dungeon.Player.LocationLevel = 0;

            Screen.Instance.SeeAllMonsters = true;
            Screen.Instance.SeeAllMap = true;
        }

        private void RunGame()
        {
            Game.Dungeon.Player.LocationMap = Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation;

            rb.StartEventLoop();

            //rb.AdvanceDungeonToNextPlayerTick(false);
        }
    }
}
