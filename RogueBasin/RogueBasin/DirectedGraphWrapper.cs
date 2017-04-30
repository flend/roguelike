using QuickGraph;

namespace RogueBasin
{
    public class DirectedGraphWrapper
    {
        private AdjacencyGraph<int, TaggedEdge<int, string>> directedGraph = new AdjacencyGraph<int, TaggedEdge<int, string>>();

        public DirectedGraphWrapper()
        {

        }

        public void AddSourceDestEdge(int srcVertex, int destVertex, string tag)
        {
            TaggedEdge<int, string> possibleEdge = null;

            directedGraph.TryGetEdge(srcVertex, destVertex, out possibleEdge);

            if (possibleEdge == null)
                directedGraph.AddVerticesAndEdge(new TaggedEdge<int, string>(srcVertex, destVertex, tag));
        }

        public void AddSourceDestEdge(int srcVertex, int destVertex)
        {
            AddSourceDestEdge(srcVertex, destVertex, "");
        }


        public AdjacencyGraph<int, TaggedEdge<int, string>> Graph
        {
            get
            {
                return directedGraph;
            }
        }
    }
}
