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
        public void RoomWithDiagonalGapConnectingWalkableAreasNotConnected()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testfilling_diagonalconnected.room");
            var filler = new RoomFilling(roomTemplate);

            Assert.IsFalse(filler.Connected);
        }

        [TestMethod]
        public void TestPlacingABlockInCentreOfLargeRoomAllowsPathing()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrouting.room"); //8x4

            var filler = new RoomFilling(roomTemplate);

            filler.SetSquareUnwalkable(new Point(3, 2));
            Assert.IsTrue(filler.Connected);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void PlacingABlockOffSideOfTemplateFails()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrouting.room"); //8x4

            var filler = new RoomFilling(roomTemplate);

            filler.SetSquareUnwalkable(new Point(-1, -11));
            Assert.IsTrue(filler.Connected);
        }

        [TestMethod]
        public void CompletelyBlockingOffADoorwayFails()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrouting.room"); //8x4

            var filler = new RoomFilling(roomTemplate);

            filler.SetSquareUnwalkable(new Point(1, 1));
            filler.SetSquareUnwalkable(new Point(1, 2));
            filler.SetSquareUnwalkable(new Point(1, 3));
            Assert.IsFalse(filler.Connected);
        }

        [TestMethod]
        public void CompletelyBlockingDoorwayUsingCheckAndSetFunctionFails()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrouting.room"); //8x4

            var filler = new RoomFilling(roomTemplate);

            filler.SetSquareUnWalkableIfMaintainsConnectivity(new Point(1, 1));
            
            Assert.IsFalse(filler.SetSquareUnWalkableIfMaintainsConnectivity(new Point(1, 2)));
        }

        [TestMethod]
        public void DividingRoomIntoTwoBreakingSomeConnectionsButLeavingAllDoorsWithOneRouteFails()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrouting.room"); //8x4

            var filler = new RoomFilling(roomTemplate);

            filler.SetSquareUnwalkable(new Point(4, 1));
            filler.SetSquareUnwalkable(new Point(4, 2));
            filler.SetSquareUnwalkable(new Point(3, 2));
            filler.SetSquareUnwalkable(new Point(2, 2));
            filler.SetSquareUnwalkable(new Point(2, 1));

            Assert.IsFalse(filler.Connected);
        }

        [TestMethod]
        public void BlockingOffADoorByReplacementFailsHorizontal()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrouting.room"); //8x4

            var filler = new RoomFilling(roomTemplate);

            Assert.IsFalse(filler.SetSquareUnWalkableIfMaintainsConnectivity(new Point(3, 4)));
        }

        [TestMethod]
        public void BlockingOffADoorByReplacementFailsVertical()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrouting.room"); //8x4

            var filler = new RoomFilling(roomTemplate);

            Assert.IsFalse(filler.SetSquareUnWalkableIfMaintainsConnectivity(new Point(0, 2)));
        }


        private RoomTemplate LoadTemplateFromAssemblyFile(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream(filePath);
            return RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);
        }
    }
}
