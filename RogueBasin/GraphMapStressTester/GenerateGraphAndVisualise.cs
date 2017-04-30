using GraphMap;
using RogueBasin;
using System;

namespace GraphMapStressTester
{
    class GenerateGraphAndVisualise
    {
        Random random;

        public GenerateGraphAndVisualise(Random rand)
        {
            this.random = rand;
        }

        public void DoLockClueStressTest(int numberOfNodes, double branchingRatio, bool visualise)
        {
            var graphGenerator = new GraphGenerator(random);

            var randomMap = graphGenerator.GenerateConnectivityMapNoCycles(numberOfNodes, branchingRatio);

            var mapModel = new MapModel(randomMap, 0);
            var doorManager = new DoorAndClueManager(mapModel);

            if (visualise)
                VisualiseConnectivityGraph(mapModel, doorManager);
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
    }
}
