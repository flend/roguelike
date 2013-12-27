using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphMap;
using System.Collections.Generic;

namespace TestGraphMap
{
    [TestClass]
    public class DoorTest
    {
        [TestMethod]
        public void DoorCanBeUnlockedWithAMatchedClue()
        {
            var testDoor = new Door(new QuickGraph.TaggedEdge<int, string>(0, 1, "test"), "door1", 0, 1);
            var clue0 = new Clue(0);

            Assert.IsTrue(testDoor.CanDoorBeUnlockedWithClues(new List<Clue>(new Clue[] { clue0 })));
        }

        [TestMethod]
        public void MultipleLockDoorCanBeUnlockedWithEnoughClues()
        {
            var testDoor = new Door(new QuickGraph.TaggedEdge<int, string>(0, 1, "test"), "door1", 0, 2);
            var clue0 = new Clue(0);
            var clue1 = new Clue(0);

            Assert.IsTrue(testDoor.CanDoorBeUnlockedWithClues(new List<Clue>(new Clue[] { clue0, clue1 })));
        }

        [TestMethod]
        public void DoorCantBeUnlockedWithNotEnoughClues()
        {
            var testDoor = new Door(new QuickGraph.TaggedEdge<int, string>(0, 1, "test"), "door1", 0, 2);
            var clue0 = new Clue(0);

            Assert.IsFalse(testDoor.CanDoorBeUnlockedWithClues(new List<Clue>(new Clue[] { clue0 })));
        }

        [TestMethod]
        public void DoorCantBeUnlockedWithTheWrongClues()
        {
            var testDoor = new Door(new QuickGraph.TaggedEdge<int, string>(0, 1, "test"), "door1", 0, 1);
            var clue0 = new Clue(1);

            Assert.IsFalse(testDoor.CanDoorBeUnlockedWithClues(new List<Clue>(new Clue[] { clue0 })));
        }
    }
}
