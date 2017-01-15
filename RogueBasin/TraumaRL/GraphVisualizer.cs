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
    static class GraphVisualizer
    {
        static public void VisualiseClueDoorGraph(MapInfo mapInfo, DoorAndClueManager doorAndClueManager, string prefix)
        {
            var visualiser = new MapGraphvizExport(mapInfo, doorAndClueManager);
            visualiser.OutputClueDoorGraph(prefix + "-door");
            visualiser.OutputDoorDependencyGraph(prefix + "-dep");

            var graphVizLocation = Game.Config.Entries[Config.GraphVizLocation];

            try
            {
                if (Game.Config.SaveGraphs || Game.Config.DisplayGraphs)
                {
                    GraphVizUtils.RunGraphVizPDF(graphVizLocation, prefix + "-door");
                    GraphVizUtils.RunGraphVizPDF(graphVizLocation, prefix + "-dep");
                }

                if (Game.Config.DisplayGraphs)
                {
                    GraphVizUtils.DisplayPNGInChildWindow(prefix + "-door");
                    GraphVizUtils.DisplayPNGInChildWindow(prefix + "-dep");
                }
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Can't find graphViz in config file", LogDebugLevel.High);
            }
        }

        static public void VisualiseLevelConnectivityGraph(ConnectivityMap map, ImmutableDictionary<int, string> levelNaming)
        {
            var visualiser = new LevelGraphvizExport(map, levelNaming);
            visualiser.OutputLevelGraph("levellinks-full");
            if (Game.Config.SaveGraphs || Game.Config.DisplayGraphs)
            {
                try
                {
                    var graphVizLocation = Game.Config.Entries[Config.GraphVizLocation];

                    GraphVizUtils.RunGraphVizPDF(graphVizLocation, "levellinks-full");

                    if (Game.Config.DisplayGraphs)
                    {
                        GraphVizUtils.DisplayPNGInChildWindow("levellinks-full");
                    }
                }
                catch (Exception)
                {
                    LogFile.Log.LogEntryDebug("Can't find graphViz in config file", LogDebugLevel.High);
                }
            }
        }

        static public void VisualiseDirectedGraph(DirectedGraphWrapper graphWrapper, string filename)
        {
            GraphVizExporter.OutputDirectedGraph(graphWrapper, filename);
            if (Game.Config.SaveGraphs || Game.Config.DisplayGraphs)
            {
                try
                {
                    var graphVizLocation = Game.Config.Entries[Config.GraphVizLocation];

                    GraphVizUtils.RunGraphVizPDF(graphVizLocation, filename);

                    if (Game.Config.DisplayGraphs)
                    {
                        GraphVizUtils.DisplayPNGInChildWindow(filename);
                    }
                }
                catch (Exception)
                {
                    LogFile.Log.LogEntryDebug("Can't find graphViz in config file", LogDebugLevel.High);
                }
            }
        }

        static public void VisualiseFullMapGraph(MapInfo mapInfo, DoorAndClueManager doorAndClueManager, string prefix)
        {
            var visualiser = new MapGraphvizExport(mapInfo, doorAndClueManager);
            visualiser.OutputFullGraph(prefix + "-full");

            var graphVizLocation = Game.Config.Entries[Config.GraphVizLocation];

            try
            {
                if (Game.Config.SaveGraphs || Game.Config.DisplayGraphs)
                {
                    GraphVizUtils.RunGraphVizPDF(graphVizLocation, prefix + "-full");
                }

                if (Game.Config.DisplayGraphs)
                {
                    GraphVizUtils.DisplayPNGInChildWindow(prefix + "-full");
                }
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Can't find graphViz in config file", LogDebugLevel.High);
            }
        }

    }
}
