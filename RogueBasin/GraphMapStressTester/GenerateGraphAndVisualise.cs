using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphMapStressTester
{
    class GenerateGraphAndVisualise
    {
        public GenerateGraphAndVisualise()
        {

        }

        public void DoLockClueStressTest(int numberOfNodes, double branchingRatio)
        {
            var graphGenerator = new GraphGenerator(Game.Random);

            var randomMap = graphGenerator.GenerateConnectivityMapNoCycles(numberOfNodes, branchingRatio);

            var mapModel = new MapModel(randomMap, 0);
            VisualiseConnectivityGraph(mapModel);
        }

        private void VisualiseConnectivityGraph(MapModel graphModel)
        {
            var visualiser = new DoorClueGraphvizExport(graphModel);
            visualiser.OutputFullGraph("bsptree-full");
            GraphVizUtils.RunGraphVizPNG("bsptree-full");
            GraphVizUtils.DisplayPNGInChildWindow("bsptree-full");
        }
    }
}
