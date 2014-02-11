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
    }
}
