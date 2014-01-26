using System;
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

            //Setup a single test level
            MapGeneratorTemplated templateGen = new MapGeneratorTemplated();
            //Map templateMap = templateGen.GenerateMap2();
            Map templateMap = templateGen.GenerateMapBranchRooms();
            
            int levelNo = Game.Dungeon.AddMap(templateMap);

            LogFile.Log.LogEntryDebug("Player start: " + Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation, LogDebugLevel.High);

            //Extract connectivity map
            var graphModel = new MapModel(templateGen.ConnectivityMap, 0);
            VisualiseConnectivityGraph(graphModel);

            RunGame();
        }

        private void VisualiseConnectivityGraph(MapModel graphModel)
        {
            var visualiser = new DoorClueGraphvizExport(graphModel);
            visualiser.OutputFullGraph("bsptree-full");
            visualiser.OutputClueDoorGraph("bsptree-door");
            visualiser.OutputDoorDependencyGraph("bsptree-dep");
            GraphVizUtils.RunGraphVizPNG("bsptree-full");
            GraphVizUtils.DisplayPNGInChildWindow("bsptree-full");
            GraphVizUtils.RunGraphVizPNG("bsptree-door");
            GraphVizUtils.DisplayPNGInChildWindow("bsptree-door");
            GraphVizUtils.RunGraphVizPNG("bsptree-dep");
            GraphVizUtils.DisplayPNGInChildWindow("bsptree-dep");
        }

        private void StandardGameSetup()
        {
            rb = new RogueBase();
            rb.SetupSystem();

            Game.Dungeon = new Dungeon();

            Game.Dungeon.Player.LocationLevel = 0;
        }

        private void RunGame()
        {
            Game.Dungeon.Player.LocationMap = Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation;

            Game.Dungeon.RecalculateWalkable();
            Game.Dungeon.RefreshAllLevelPathing();

            rb.MainLoop(false);
        }
    }
}
