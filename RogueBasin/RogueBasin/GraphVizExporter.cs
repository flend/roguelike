using GraphMap;
using QuickGraph;
using QuickGraph.Graphviz;
using System;

namespace RogueBasin
{
    static public class GraphVizExporter
    {
        public static void OutputDirectedGraph(DirectedGraphWrapper graph, string filename)
        {
            var graphviz = new GraphvizAlgorithm<int, TaggedEdge<int, string>>(graph.Graph);

            graphviz.FormatVertex += graphviz_FormatVertex;
            graphviz.FormatEdge += graphviz_FormatEdge;

            graphviz.Generate(new FileDotEngine(), filename);
        }

        private static void graphviz_FormatEdge(object sender, FormatEdgeEventArgs<int, TaggedEdge<int, string>> e)
        {
            //Take tag from TaggedEdge and add as label on edge in serialized view

            TaggedEdge<int, string> edge = e.Edge;
            var edgeFormattor = e.EdgeFormatter;

            string edgeTag = edge.Tag;
            edgeFormattor.Label = new QuickGraph.Graphviz.Dot.GraphvizEdgeLabel();
            edgeFormattor.Label.Value = edgeTag;

        }

        private static void graphviz_FormatVertex(object sender, FormatVertexEventArgs<int> e)
        {
            //(If nothing is included here, having this here allows default behaviour for serializer (add labels))

            //Use formattor explicitally
            var vertexFormattor = e.VertexFormatter;
            int vertexNo = e.Vertex;

            vertexFormattor.Label = String.Format("{0}", vertexNo);
        }
    }
}
