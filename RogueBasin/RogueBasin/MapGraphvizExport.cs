using System;
using QuickGraph;
using QuickGraph.Graphviz;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using RogueBasin;

namespace GraphMap
{
    public class MapGraphvizExport
    {
        private MapModel model;
        private MapInfo mapInfo;
        private DoorAndClueManager doorAndClueManager;

        public MapGraphvizExport(MapInfo mapInfo, DoorAndClueManager doorAndClueManager)
        {
            this.model = mapInfo.Model;
            this.mapInfo = mapInfo;
            this.doorAndClueManager = doorAndClueManager;
        }

        /// <summary>
        /// Visualise the reduced graph, including door and clue locations (though clues will be at the wrapped-up vertex)
        /// </summary>
        /// <param name="filename"></param>
        public void OutputClueDoorGraph(string filename)
        {
            //Visualise the reduced graph, including door and clue locations
            var graphviz = new GraphvizAlgorithm<int, TaggedEdge<int, string>>(model.GraphNoCycles.mapNoCycles);

            graphviz.FormatVertex += graphviz_FormatVertex;
            //_FormatVertex gives all clues, but DoorAndClueManager can't distinguish log clues from item clues
            graphviz.FormatEdge += graphviz_FormatEdge;

            graphviz.Generate(new FileDotEngineUndirected(), filename);

            //TODO: Visualise the full graph, including door and clue locations

            // "C:\Program Files (x86)\Graphviz 2.28\bin\dot.exe" -Tbmp -ograph.bmp graph.dot
        }

        /// <summary>
        /// Visualise the full graph, with items from the dungeon
        /// </summary>
        /// <param name="filename"></param>
        public void OutputFullGraph(string filename)
        {
            //Visualise the reduced graph, including door and clue locations
            var graphviz = new GraphvizAlgorithm<int, TaggedEdge<int, string>>(model.BaseGraph);

            graphviz.FormatVertex += graphviz_FormatVertex_Placement;
            graphviz.FormatEdge += graphviz_FormatEdge;

            graphviz.Generate(new FileDotEngineUndirected(), filename);

            //TODO: Visualise the full graph, including door and clue locations

            // "C:\Program Files (x86)\Graphviz 2.28\bin\dot.exe" -Tbmp -ograph.bmp graph.dot
        }

        /// <summary>
        /// Output the door dependency graph
        /// </summary>
        /// <param name="filename"></param>
        public void OutputDoorDependencyGraph(string filename)
        {
            var graphviz = new GraphvizAlgorithm<int, Edge<int>>(doorAndClueManager.DoorDependencyGraph);

            graphviz.FormatVertex += graphviz_FormatDoorVertex;

            graphviz.Generate(new FileDotEngine(), filename);

            //TODO: Visualise the full graph, including door and clue locations

            // "C:\Program Files (x86)\Graphviz 2.28\bin\dot.exe" -Tbmp -ograph.bmp graph.dot
        }

        private void graphviz_FormatDoorVertex(object sender, FormatVertexEventArgs<int> e)
        {
            //Output door id
            var vertexFormattor = e.VertexFormatter;
            int vertexNo = e.Vertex;

            var door = doorAndClueManager.GetLockIdByIndex(vertexNo);

            vertexFormattor.Label = door;
        }
        
        private void graphviz_FormatEdge(object sender, FormatEdgeEventArgs<int, TaggedEdge<int, string>> e)
        {
            //Take tag from TaggedEdge and add as label on edge in serialized view

            TaggedEdge<int, string> edge = e.Edge;
            var edgeFormattor = e.EdgeFormatter;

            //If there is a door on this edge, override with this tag

            var doorHere = doorAndClueManager.GetDoorsForEdge(edge);

            string edgeTag = edge.Tag;

            if (doorHere.Count() > 0)
            {
                edgeTag = "";
                foreach (var door in doorHere)
                    edgeTag += door.Id + "+";
            }
            if(edgeTag.Length > 0)
                edgeTag = edgeTag.Substring(0, edgeTag.Length - 1);
                        
            edgeFormattor.Label = new QuickGraph.Graphviz.Dot.GraphvizEdgeLabel();
            edgeFormattor.Label.Value = edgeTag;

        }

        private void graphviz_FormatVertex(object sender, FormatVertexEventArgs<int> e)
        {
            //(If nothing is included here, having this here allows default behaviour for serializer (add labels))

            //Use formattor explicitally
            var vertexFormattor = e.VertexFormatter;
            int vertexNo = e.Vertex;

            string vertexLabel = vertexNo.ToString();

            //If there is a clue here, append clue
            var clues = doorAndClueManager.GetObjectiveAndClueIdsAtVertex(vertexNo);

            foreach (var clue in clues)
            {
                vertexLabel += "\\n";
                vertexLabel += clue;
            }

            vertexFormattor.Label = vertexLabel;
        }

        private void graphviz_FormatVertex_Placement(object sender, FormatVertexEventArgs<int> e)
        {
            //(If nothing is included here, having this here allows default behaviour for serializer (add labels))

            //Use formattor explicitally
            var vertexFormattor = e.VertexFormatter;
            int vertexNo = e.Vertex;

            string vertexLabel = vertexNo.ToString();


            foreach (var itemPlacement in mapInfo.Populator.RoomInfo(vertexNo).Items)
            {
                if (itemPlacement.item.QuestId != "")
                {
                    vertexLabel += "\\n";
                    vertexLabel += itemPlacement.item.QuestId;
                }
            }

            foreach (var featurePlacement in mapInfo.Populator.RoomInfo(vertexNo).Features)
            {
                if (featurePlacement.feature.QuestId != "")
                {
                    vertexLabel += "\\n";
                    vertexLabel += featurePlacement.feature.QuestId;
                }
            }

            foreach (var creaturePlacement in mapInfo.Populator.RoomInfo(vertexNo).Monsters)
            {
                if (creaturePlacement.monster.QuestId != "")
                {
                    vertexLabel += "\\n";
                    vertexLabel += creaturePlacement.monster.QuestId;
                }
            }

            vertexFormattor.Label = vertexLabel;
        }

        private void graphviz_FormatVertex_NoAnnotation(object sender, FormatVertexEventArgs<int> e)
        {
            //(If nothing is included here, having this here allows default behaviour for serializer (add labels))

            //Use formattor explicitally
            var vertexFormattor = e.VertexFormatter;
            int vertexNo = e.Vertex;

            string vertexLabel = vertexNo.ToString();

            vertexFormattor.Label = vertexLabel;
        }
    }
}
