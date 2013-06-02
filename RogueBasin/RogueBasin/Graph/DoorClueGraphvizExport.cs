using System;
using QuickGraph;
using QuickGraph.Graphviz;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace graphtestc
{
    class DoorClueGraphvizExport
    {
        private MapModel model;

        public DoorClueGraphvizExport(MapModel m)
        {
            this.model = m;
        }

        /// <summary>
        /// Output the map graph, with doors & clues
        /// </summary>
        /// <param name="filename"></param>
        public void OutputUndirectedGraph(string filename)
        {

            //Visualise the reduced graph, including door and clue locations
            var graphviz = new GraphvizAlgorithm<int, TaggedEdge<int, string>>(model.GraphNoCycles);

            graphviz.FormatVertex += graphviz_FormatVertex;
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
            //Visualise the reduced graph, including door and clue locations
            var graphviz = new GraphvizAlgorithm<int, Edge<int>>(model.DoorDependencyGraph);

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

            var door = model.GetDoorByIndex(vertexNo);

            vertexFormattor.Label = door.Id;
        }

        private void graphviz_FormatDoorEdge(object sender, FormatEdgeEventArgs<int, TaggedEdge<int, string>> e)
        {
            //Take tag from TaggedEdge and add as label on edge in serialized view

            TaggedEdge<int, string> edge = e.Edge;
            var edgeFormattor = e.EdgeFormatter;

            //If there is a door on this edge, override with this tag

            MapModel.Door doorHere = model.GetDoorForEdge(edge);

            string edgeTag = edge.Tag;

            if (doorHere != null)
                edgeTag = doorHere.Id;

            edgeFormattor.Label = new QuickGraph.Graphviz.Dot.GraphvizEdgeLabel();
            edgeFormattor.Label.Value = edgeTag;

        }

        private void graphviz_FormatEdge(object sender, FormatEdgeEventArgs<int, TaggedEdge<int, string>> e)
        {
            //Take tag from TaggedEdge and add as label on edge in serialized view

            TaggedEdge<int, string> edge = e.Edge;
            var edgeFormattor = e.EdgeFormatter;

            //If there is a door on this edge, override with this tag

            MapModel.Door doorHere = model.GetDoorForEdge(edge);

            string edgeTag = edge.Tag;

            if (doorHere != null)
                edgeTag = doorHere.Id;
                        
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
            var clues = model.GetClueIdForVertex(vertexNo);

            foreach (var clue in clues)
            {
                vertexLabel += "\\n";
                vertexLabel += clue;
            }

            vertexFormattor.Label = vertexLabel;
        }
    }
}
