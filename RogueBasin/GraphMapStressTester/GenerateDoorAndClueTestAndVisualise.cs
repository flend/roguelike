﻿using GraphMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueBasin;

namespace GraphMapStressTester
{
    class GenerateDoorAndClueTestAndVisualise
    {
        Random random;

        public GenerateDoorAndClueTestAndVisualise(Random rand)
        {
            this.random = rand;
        }

        public bool DoLockClueStressTest(int numberOfNodes, double branchingRatio, int numberOfDoors, int numberOfCluesPerDoor, bool visualise)
        {
            var graphGenerator = new GraphGenerator(random);

            var randomMap = graphGenerator.GenerateConnectivityMapNoCycles(numberOfNodes, branchingRatio);

            var doorAndClueTester = new DoorAndClueGenerator(random);

            var mapModel = new MapModel(randomMap, 0);
            if (visualise)
                VisualiseConnectivityGraph(mapModel);

            doorAndClueTester.AddDoorsAndClues(mapModel, numberOfDoors, numberOfCluesPerDoor);

            if (visualise)
                VisualiseConnectivityGraphWithDoors(mapModel);

            var mapTester = new GraphSolver(mapModel);

            return mapTester.MapCanBeSolved();
        }

        private void VisualiseConnectivityGraph(MapModel graphModel)
        {
            var visualiser = new DoorClueGraphvizExport(graphModel);
            visualiser.OutputFullGraph("bsptree-full");
            try
            {
                var graphVizLocation = Game.Config.Entries[Config.GraphVizLocation];
                GraphVizUtils.RunGraphVizPNG(graphVizLocation, "bsptree-full");
                GraphVizUtils.DisplayPNGInChildWindow("bsptree-full");
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Can't find graphViz in config file", LogDebugLevel.High);
            }
        }

        private void VisualiseConnectivityGraphWithDoors(MapModel graphModel)
        {
            var visualiser = new DoorClueGraphvizExport(graphModel);
            visualiser.OutputClueDoorGraph("bsptree-door");
            visualiser.OutputDoorDependencyGraph("bsptree-dep");
            try
            {
                var graphVizLocation = Game.Config.Entries[Config.GraphVizLocation];
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
    }
}
