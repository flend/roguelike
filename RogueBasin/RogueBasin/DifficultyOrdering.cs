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
