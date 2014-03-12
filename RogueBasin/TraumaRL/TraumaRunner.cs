using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
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
            StandardGameSetup();

            GenerateStoryDungeon();

            RunGame();
        }

        private void GenerateStoryDungeon()
        {
            //Setup a single test level
            TraumaWorldGenerator templateGen = new TraumaWorldGenerator();

            var mapInfo = templateGen.GenerateDungeonWithStory();

            LogFile.Log.LogEntryDebug("Player start: " + Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation, LogDebugLevel.High);

            VisualiseConnectivityGraph(mapInfo.Model);

            VisualiseLevelConnectivityGraph(new MapModel(templateGen.LevelLinks, 0), TraumaWorldGenerator.LevelNaming);
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

        private void VisualiseLevelConnectivityGraph(MapModel graphModel, Dictionary<int, string> levelNaming)
        {
            var visualiser = new DoorClueGraphvizExport(graphModel, levelNaming);
            visualiser.OutputFullGraph("levellinks-full");
            GraphVizUtils.RunGraphVizPNG("levellinks-full");
            GraphVizUtils.DisplayPNGInChildWindow("levellinks-full");
        }

        private void StandardGameSetup()
        {

            //int seedToUse = 150;
            //Game.Random = new Random(seedToUse);
            Game.Random = new Random();

            rb = new RogueBase();
            rb.SetupSystem();

            var dungeonInfo = new DungeonInfo();
            dungeonInfo.LevelNaming = TraumaWorldGenerator.LevelNaming;
            Game.Dungeon = new Dungeon(dungeonInfo);

            Game.Dungeon.Player.StartGameSetup();

            Screen.Instance.SeeAllMonsters = true;
            Screen.Instance.SeeAllMap = true;
        }

        private void RunGame()
        {
            Game.Dungeon.Player.LocationLevel = 0;
            Game.Dungeon.Player.LocationMap = Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel].PCStartLocation;

            rb.MainLoop(false);
        }
    }
}
