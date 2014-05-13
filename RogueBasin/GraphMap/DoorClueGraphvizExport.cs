using System;
using QuickGraph;
using QuickGraph.Graphviz;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphMap
{
    public class DoorClueGraphvizExport
    {
        private MapModel model;
        private Dictionary<int, string> vertexMapping = new Dictionary<int,string>();

        public DoorClueGraphvizExport(MapModel m)
        {
            this.model = m;
        }

        public DoorClueGraphvizExport(MapModel m, Dictionary<int, string> vertexMapping)
        {
            this.model = m;
            this.vertexMapping = vertexMapping;
        }

        /// <summary>
        /// Visualise the cucle-reduced graph, including door and clue locations
        /// </summary>
        /// <param name="filename"></param>
        public void OutputClueDoorGraph(string filename)
        {
            //Visualise the reduced graph, including door and clue locations
            var graphviz = new GraphvizAlgorithm<int, TaggedEdge<int, string>>(model.GraphNoCycles.mapNoCycles);

            graphviz.FormatVertex += graphviz_FormatVertex;
            graphviz.FormatEdge += graphviz_FormatEdge;

            graphviz.Generate(new FileDotEngineUndirected(), filename);

            //TODO: Visualise the full graph, including door and clue locations

            // "C:\Program Files (x86)\Graphviz 2.28\bin\dot.exe" -Tbmp -ograph.bmp graph.dot
        }

        /// <summary>
        /// Visualise the full graph, including door and clue locations (though clues will be at the wrapped-up vertex)
        /// </summary>
        /// <param name="filename"></param>
        public void OutputFullGraph(string filename)
        {
            //Visualise the reduced graph, including door and clue locations
            var graphviz = new GraphvizAlgorithm<int, TaggedEdge<int, string>>(model.BaseGraph);

            graphviz.FormatVertex += graphviz_FormatVertex_NoAnnotation;

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
            var graphviz = new GraphvizAlgorithm<int, Edge<int>>(model.DoorAndClueManager.DoorDependencyGraph);

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

            var door = model.DoorAndClueManager.GetLockIdByIndex(vertexNo);

            vertexFormattor.Label = door;
        }

        private void graphviz_FormatDoorEdge(object sender, FormatEdgeEventArgs<int, TaggedEdge<int, string>> e)
        {
            //Take tag from TaggedEdge and add as label on edge in serialized view

            TaggedEdge<int, string> edge = e.Edge;
            var edgeFormattor = e.EdgeFormatter;

            //If there is a door on this edge, override with this tag

            var doorHere = model.DoorAndClueManager.GetDoorsForEdge(edge);

            string edgeTag = edge.Tag;

            if (doorHere.Count() > 0)
            {
                edgeTag = "";
                foreach(var door in doorHere)
                    edgeTag += door.Id + "+";
            }
                

            edgeFormattor.Label = new QuickGraph.Graphviz.Dot.GraphvizEdgeLabel();
            edgeFormattor.Label.Value = edgeTag;

        }

        private void graphviz_FormatEdge(object sender, FormatEdgeEventArgs<int, TaggedEdge<int, string>> e)
        {
            //Take tag from TaggedEdge and add as label on edge in serialized view

            TaggedEdge<int, string> edge = e.Edge;
            var edgeFormattor = e.EdgeFormatter;

            //If there is a door on this edge, override with this tag

            var doorHere = model.DoorAndClueManager.GetDoorsForEdge(edge);

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

            if (vertexMapping.ContainsKey(vertexNo))
                vertexLabel = vertexMapping[vertexNo];

            //If there is a clue here, append clue
            var clues = model.DoorAndClueManager.GetObjectiveAndClueIdsAtVertex(vertexNo);

            foreach (var clue in clues)
            {
                vertexLabel += "\\n";
                vertexLabel += clue;
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
