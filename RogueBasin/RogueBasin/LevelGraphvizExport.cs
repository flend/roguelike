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
    public class LevelGraphvizExport
    {
        private ConnectivityMap map;
        private ImmutableDictionary<int, string> vertexMapping;

        public LevelGraphvizExport(ConnectivityMap map, ImmutableDictionary<int, string> levelNaming)
        {
            this.map = map;
            this.vertexMapping = levelNaming;
        }

        public void OutputLevelGraph(string filename)
        {
            var graphviz = new GraphvizAlgorithm<int, TaggedEdge<int, string>>(map.RoomConnectionGraph);

            graphviz.FormatVertex += graphviz_FormatVertex;

            graphviz.Generate(new FileDotEngineUndirected(), filename);
        }

        private void graphviz_FormatVertex(object sender, FormatVertexEventArgs<int> e)
        {
            //(If nothing is included here, having this here allows default behaviour for serializer (add labels))

            //Use formattor explicitally
            var vertexFormattor = e.VertexFormatter;
            int vertexNo = e.Vertex;

            string vertexLabel = vertexNo.ToString();

            if (vertexMapping.ContainsKey(vertexNo))
                vertexLabel = vertexNo + "-" + vertexMapping[vertexNo];
            
            vertexFormattor.Label = vertexLabel;
        }
    }
}
