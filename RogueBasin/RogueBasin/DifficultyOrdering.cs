using QuickGraph;
using QuickGraph.Algorithms.TopologicalSort;
using System.Collections.Generic;

namespace RogueBasin
{
    public class DifficultyOrdering
    {
        private DirectedGraphWrapper directedGraphWrapper;
        
        public DifficultyOrdering(DirectedGraphWrapper directedGraphWrapper)
        {
            this.directedGraphWrapper = directedGraphWrapper;
        }
        
        public IEnumerable<int> GetLevelsInAscendingDifficultyOrder()
        {
            var topologicalSort = new TopologicalSortAlgorithm<int, TaggedEdge<int, string>>(directedGraphWrapper.Graph);
            topologicalSort.Compute();

            return topologicalSort.SortedVertices;
        }
    }
}
