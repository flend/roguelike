using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System.Reflection;
using System.IO;

namespace DDRogueTest
{
    [TestClass]
    public class RoomFillingTest
    {
        [TestMethod]
        public void EntirelyWalkableRoomIsConnected()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testfilling_walkable.room");
            var filler = new RoomFilling(roomTemplate);

            Assert.IsTrue(filler.Connected);
        }

        [TestMethod]
        public void EntirelyUnWalkableRoomIsConnected()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testfilling_notwalkable.room");
            var filler = new RoomFilling(roomTemplate);

            Assert.IsTrue(filler.Connected);
        }

        [TestMethod]
        public void RoomWithBarrierDividingWalkableAreasIsNotConnected()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testfilling_divided.room");
            var filler = new RoomFilling(roomTemplate);

            Assert.IsFalse(filler.Connected);
        }

        [TestMethod]
        public void RoomWithPartialBarrierDividingWalkableAreasIsConnected()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testfilling_dividedgap.room");
            var filler = new RoomFilling(roomTemplate);

            Assert.IsTrue(filler.Connected);
        }

        [TestMethod]
        public void RoomWithPartialVerticalBarrierDividingWalkableAreasIsConnected()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testfilling_dividedgap_vert.room");
            var filler = new RoomFilling(roomTemplate);

            Assert.IsTrue(filler.Connected);
        }

        [TestMethod]
        public void RoomWithDiagonalGapConnectingWalkableAreasIsConnected()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testfilling_diagonalconnected.room");
            var filler = new RoomFilling(roomTemplate);

            Assert.IsTrue(filler.Connected);
        }

        private RoomTemplate LoadTemplateFromAssemblyFile(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream(filePath);
            return RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);
        }
    }
}
