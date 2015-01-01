using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphMap;

namespace DDRogueTest
{
    [TestClass]
    public class ConnectionTest
    {
        [TestMethod]
        public void TestConnectionIndependentSourceTargetEquality()
        {
            var connection1 = new Connection(1, 2);
            var connection2 = new Connection(1, 2);

            Assert.IsTrue(connection1 == connection2);
        }

        [TestMethod]
        public void TestConnectionUndirectedEquality()
        {
            var connection1 = new Connection(1, 2);
            var connection2 = new Connection(2, 1);

            Assert.IsTrue(connection1 == connection2);
        }

        [TestMethod]
        public void TestConnectionUndirectedFunctionEquality()
        {
            var connection1 = new Connection(1, 2);
            var connection2 = new Connection(2, 1);

            Assert.IsTrue(connection1.Equals(connection2));
        }

        [TestMethod]
        public void TestConnectionFunctionEquality()
        {
            var connection1 = new Connection(1, 2);
            var connection2 = new Connection(1, 2);

            Assert.IsTrue(connection1.Equals(connection2));
        }
    }
}
