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

            var doorAndClueTester = new DoorAndObjectiveGenerator(random);

            var mapModel = new MapModel(randomMap, 0);
            if (visualise)
                VisualiseConnectivityGraph(mapModel);

            doorAndClueTester.AddDoorsAndObjectives(mapModel, numberOfDoors, numberOfCluesPerDoor);

            if (visualise)
                VisualiseConnectivityGraphWithDoors(mapModel);

            var mapTester = new GraphSolver(mapModel);

            return mapTester.MapCanBeSolved();
        }

        private void VisualiseConnectivityGraph(MapModel graphModel)
        {
            var visualiser = new DoorClueGraphvizExport(graphModel);
            visualiser.OutputFullGraph("bsptree-full");
            GraphVizUtils.RunGraphVizPNG("bsptree-full");
            GraphVizUtils.DisplayPNGInChildWindow("bsptree-full");
        }

        private void VisualiseConnectivityGraphWithDoors(MapModel graphModel)
        {
            var visualiser = new DoorClueGraphvizExport(graphModel);
            visualiser.OutputClueDoorGraph("bsptree-door");
            visualiser.OutputDoorDependencyGraph("bsptree-dep");
            GraphVizUtils.RunGraphVizPNG("bsptree-door");
            GraphVizUtils.DisplayPNGInChildWindow("bsptree-door");
            GraphVizUtils.RunGraphVizPNG("bsptree-dep");
            GraphVizUtils.DisplayPNGInChildWindow("bsptree-dep");
        }
    }
}
