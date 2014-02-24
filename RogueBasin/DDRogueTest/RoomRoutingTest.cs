using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System.Reflection;
using System.IO;

namespace DDRogueTest
{
    [TestClass]
    public class RoomRoutingTest
    {
        [TestMethod]
        public void TestPlacingABlockInCentreOfLargeRoomAllowsPathing()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrouting.room"); //8x4

            var roomRouting = new RoomRouting(roomTemplate);

            Assert.IsTrue(roomRouting.SetSquareUnwalkableIfDoorPathingIsPreserved(new Point(3, 2)));
        }

        [TestMethod]
        public void PlacingABlockOffSideOfTemplateFails()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrouting.room"); //8x4

            var roomRouting = new RoomRouting(roomTemplate);

            Assert.IsFalse(roomRouting.SetSquareUnwalkableIfDoorPathingIsPreserved(new Point(-1, -1)));
        }

        [TestMethod]
        public void CompletelyBlockingOffADoorwayFails()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrouting.room"); //8x4

            var roomRouting = new RoomRouting(roomTemplate);

            roomRouting.SetSquareUnwalkableIfDoorPathingIsPreserved(new Point(1, 1));
            roomRouting.SetSquareUnwalkableIfDoorPathingIsPreserved(new Point(1, 2)); 
            Assert.IsFalse(roomRouting.SetSquareUnwalkableIfDoorPathingIsPreserved(new Point(1, 3)));
        }

        [TestMethod]
        public void DividingRoomIntoTwoBreakingSomeConnectionsButLeavingAllDoorsWithOneRouteFails()
        {
            RoomTemplate roomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrouting.room"); //8x4

            var roomRouting = new RoomRouting(roomTemplate);

            roomRouting.SetSquareUnwalkableIfDoorPathingIsPreserved(new Point(4, 1));
            roomRouting.SetSquareUnwalkableIfDoorPathingIsPreserved(new Point(4, 2));
            roomRouting.SetSquareUnwalkableIfDoorPathingIsPreserved(new Point(3, 2));
            roomRouting.SetSquareUnwalkableIfDoorPathingIsPreserved(new Point(2, 2));
            Assert.IsFalse(roomRouting.SetSquareUnwalkableIfDoorPathingIsPreserved(new Point(2, 3)));
        }

        private RoomTemplate LoadTemplateFromAssemblyFile(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream(filePath);
            return RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);
        }

    }
}
