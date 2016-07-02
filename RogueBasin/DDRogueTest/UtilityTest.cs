using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System.Collections.Generic;
using System.Linq;

namespace DDRogueTest
{
    [TestClass]
    public class UtilityTest
    {
        [TestMethod]

        public void QuickPermReturnsCorrectly()
        {
            var list = new List<int> { 1, 2, 3 };
            var outputList = Utility.GetPermutations<int>(list, 2);

            var output1 = outputList.ElementAt(0).ToList();
            var output2 = outputList.ElementAt(1).ToList();
            var output3 = outputList.ElementAt(2).ToList();

            Assert.AreEqual(3, outputList.Count());

            CollectionAssert.AreEquivalent(new List<int>{1, 2}, output1);
            CollectionAssert.AreEquivalent(new List<int>{1, 3}, output2);
            CollectionAssert.AreEquivalent(new List<int>{2, 3}, output3);
        }

        public static IEnumerable<Tuple<int, int>> CartesianExclusiveCombinations(IEnumerable<int> list)
        {
            return list.SelectMany(k => list.SelectMany(ik => ik != k ? new List<Tuple<int, int>> { new Tuple<int, int>(k, ik) } : new List<Tuple<int, int>>()));
        }
        [TestMethod]
        public void GetPointsOnLineReturnsCorrectPointsForNegativeX()
        {
            Point start = new Point(5, 10);
            Point end = new Point(0, 10);

            var path = Utility.GetPointsOnLine(start, end).ToList();
            var expectedPath = new List<Point>(new Point[] {
                new Point(5, 10),
                new Point(4, 10),
                new Point(3, 10),
                new Point(2, 10),
                new Point(1, 10),
                new Point(0, 10)
            });

            CollectionAssert.AreEqual(expectedPath, path);
        }

        [TestMethod]
        public void GetPointsOnLineReturnsCorrectPointsForPositiveX()
        {
            Point start = new Point(0, 10);
            Point end = new Point(5, 10);

            var path = Utility.GetPointsOnLine(start, end).ToList();
            var expectedPath = new List<Point>(new Point[] {
                new Point(0, 10),
                new Point(1, 10),
                new Point(2, 10),
                new Point(3, 10),
                new Point(4, 10),
                new Point(5, 10)
            });

            CollectionAssert.AreEqual(expectedPath, path);
        }

        [TestMethod]
        public void GetPointsOnLineReturnsCorrectPointsForDiagonalPos()
        {
            Point start = new Point(0, 0);
            Point end = new Point(4, 4);

            var path = Utility.GetPointsOnLine(start, end).ToList();
            var expectedPath = new List<Point>(new Point[] {
                new Point(0, 0),
                new Point(1, 1),
                new Point(2, 2),
                new Point(3, 3),
                new Point(4, 4)
            });

            CollectionAssert.AreEqual(expectedPath, path);
        }

        [TestMethod]
        public void GetPointsOnLineReturnsCorrectPointsForDiagonalNeg()
        {
            Point start = new Point(0, 0);
            Point end = new Point(-4, -4);

            var path = Utility.GetPointsOnLine(start, end).ToList();
            var expectedPath = new List<Point>(new Point[] {
                new Point(0, 0),
                new Point(-1, -1),
                new Point(-2, -2),
                new Point(-3, -3),
                new Point(-4, -4)
            });

            CollectionAssert.AreEqual(expectedPath, path);
        }

        [TestMethod]
        public void GetPointsOnLineReturnsCorrectPointsForSameInputs()
        {
            Point start = new Point(0, 0);
            Point end = new Point(0, 0);

            var path = Utility.GetPointsOnLine(start, end).ToList();
            var expectedPath = new List<Point>(new Point[] {
                new Point(0, 0)
            });

            CollectionAssert.AreEqual(expectedPath, path);
        }

        [TestMethod]
        public void RepeatToLengthHandlesMultipleRepeats()
        {
            List<int> initialList = new List<int>() { 1, 2 };
            var repeatedList = initialList.RepeatToLength(6).ToList();

            CollectionAssert.AreEqual(new List<int>() { 1, 2, 1, 2, 1, 2 }, repeatedList);
        }

        [TestMethod]
        public void RepeatToLengthHandlesPartialRepeats()
        {
            List<int> initialList = new List<int>() { 1, 2 };
            var repeatedList = initialList.RepeatToLength(7).ToList();

            CollectionAssert.AreEqual(new List<int>() { 1, 2, 1, 2, 1, 2, 1 }, repeatedList);
        }

        [TestMethod]
        public void RepeatToLengthHandlesShorteningLists()
        {
            List<int> initialList = new List<int>() { 1, 2 };
            var repeatedList = initialList.RepeatToLength(1).ToList();

            CollectionAssert.AreEqual(new List<int>() { 1 }, repeatedList);
        }
    }
}
