using GraphMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueBasin;

namespace GraphMapStressTester
{
    class GenerateDoorAndObjectiveTestAndVisualise
    {
        Random random;

        public GenerateDoorAndObjectiveTestAndVisualise(Random rand)
        {
            this.random = rand;
        }

        public bool DoLockClueStressTest(int numberOfNodes, double branchingRatio, int numberOfDoors, int numberOfCluesPerDoor, bool visualise)
        {
            var graphGenerator = new GraphGenerator(random);

            var randomMap = graphGenerator.GenerateConnectivityMapNoCycles(numberOfNodes, branchingRatio);

            var mapModel = new MapModel(randomMap, 0);
            var doorAndClueManager = new DoorAndClueManager(mapModel);
            var doorAndClueTester = new DoorAndObjectiveGenerator(mapModel, doorAndClueManager, random);
            if (visualise)
                VisualiseConnectivityGraph(mapModel, doorAndClueManager);

            doorAndClueTester.AddDoorsAndObjectives(mapModel, numberOfDoors, numberOfCluesPerDoor);

            if (visualise)
                VisualiseConnectivityGraphWithDoors(mapModel, doorAndClueManager);

            var mapTester = new GraphSolver(mapModel, doorAndClueManager);

            return mapTester.MapCanBeSolved();
        }

        private void VisualiseConnectivityGraph(MapModel graphModel, DoorAndClueManager doorAndClueManager)
        {
            var visualiser = new DoorClueGraphvizExport(graphModel, doorAndClueManager);
            visualiser.OutputFullGraph("bsptree-full");
            try
            {
                var graphVizLocation = Game.Config.Entries[Config.GraphVizLocation];
                GraphVizUtils.RunGraphVizPDF(graphVizLocation, "bsptree-full");
                GraphVizUtils.DisplayPNGInChildWindow("bsptree-full");
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Can't find graphViz in config file", LogDebugLevel.High);
            }
        }

        private void VisualiseConnectivityGraphWithDoors(MapModel graphModel, DoorAndClueManager doorAndClueManager)
        {
            var visualiser = new DoorClueGraphvizExport(graphModel, doorAndClueManager);
            visualiser.OutputClueDoorGraph("bsptree-door");
            visualiser.OutputDoorDependencyGraph("bsptree-dep");
            try
            {
                var graphVizLocation = Game.Config.Entries[Config.GraphVizLocation];
                GraphVizUtils.RunGraphVizPDF(graphVizLocation, "bsptree-door");
                GraphVizUtils.DisplayPNGInChildWindow("bsptree-door");
                GraphVizUtils.RunGraphVizPDF(graphVizLocation, "bsptree-dep");
                GraphVizUtils.DisplayPNGInChildWindow("bsptree-dep");
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Can't find graphViz in config file", LogDebugLevel.High);
            }
        }
    }
}
