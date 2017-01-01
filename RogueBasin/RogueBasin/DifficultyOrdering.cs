using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph.Algorithms.TopologicalSort;

namespace RogueBasin
{
    public class DifficultyOrdering
    {
        private AdjacencyGraph<int, TaggedEdge<int, string>> difficultyGraph;
        
        public DifficultyOrdering(AdjacencyGraph<int, TaggedEdge<int, string>> difficultyGraph) {
            this.difficultyGraph = difficultyGraph;
        }
        
        public IEnumerable<int> GetLevelsInAscendingDifficultyOrder()
        {
            var topologicalSort = new TopologicalSortAlgorithm<int, TaggedEdge<int, string>>(difficultyGraph);
            topologicalSort.Compute();

            return topologicalSort.SortedVertices;
        }
    }
}
