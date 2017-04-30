using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System.Collections.Generic;
using System.Linq;

namespace DDRogueTest
{
    [TestClass]
    public class DifficultyOrderingTest
    {
        [TestMethod]
        public void TwoLevelsReturnedInCorrectOrder()
        {
            var dag = new DirectedGraphWrapper();
            dag.AddSourceDestEdge(1, 2);

            var difficultyOrdering = new DifficultyOrdering(dag);
            var orderedLevels = difficultyOrdering.GetLevelsInAscendingDifficultyOrder();

            CollectionAssert.AreEqual(new List<int>{1, 2}, orderedLevels.ToList());
        }

        [TestMethod]
        public void SixEdgeUnambiguousExampleReturnedInCorrectOrder()
        {
            var dag = new DirectedGraphWrapper();
            dag.AddSourceDestEdge(0, 1);
            dag.AddSourceDestEdge(1, 2);
            dag.AddSourceDestEdge(1, 4);
            dag.AddSourceDestEdge(2, 3);
            dag.AddSourceDestEdge(3, 4);
            dag.AddSourceDestEdge(3, 5);
            dag.AddSourceDestEdge(4, 5);

            var difficultyOrdering = new DifficultyOrdering(dag);
            var orderedLevels = difficultyOrdering.GetLevelsInAscendingDifficultyOrder();

            CollectionAssert.AreEqual(new List<int> { 0, 1, 2, 3, 4, 5 }, orderedLevels.ToList());
        }

        [TestMethod]
        public void SixEdgeAmbiguousExampleReturnedInSetOrder()
        {
            var dag = new DirectedGraphWrapper();
            dag.AddSourceDestEdge(0, 1);
            dag.AddSourceDestEdge(1, 2);
            dag.AddSourceDestEdge(1, 4);
            dag.AddSourceDestEdge(2, 3);
            dag.AddSourceDestEdge(3, 5);
            dag.AddSourceDestEdge(4, 5);

            var difficultyOrdering = new DifficultyOrdering(dag);
            var orderedLevels = difficultyOrdering.GetLevelsInAscendingDifficultyOrder();

            CollectionAssert.AreEqual(new List<int> { 0, 1, 4, 2, 3, 5 }, orderedLevels.ToList());
        }
    }
}
