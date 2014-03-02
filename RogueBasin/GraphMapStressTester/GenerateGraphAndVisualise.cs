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

            if (visualise)
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
