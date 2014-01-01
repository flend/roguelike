using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System.Reflection;
using System.IO;

namespace DDRogueTest
{
    [TestClass]
    public class TemplatedMapBuilderTest
    {
        [TestMethod]
        public void TestAddTemplateOnTopWorksWithNoExistingTemplates()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplateOnTop(templatePos1);
        }

        [TestMethod]
        public void TestOverlappingSolidRoomsCantBeAdded()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(0, 0, 10, room1, TemplateRotation.Deg0, 0);
            Assert.IsFalse(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void TestCompletelyOverlappingSolidRoomsCantBeAdded()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(0, 0, 1, room1, TemplateRotation.Deg0, 0);
            Assert.IsFalse(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void TestNonOverlappingSolidRoomsCanBeAdded()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(8, 0, 10, room1, TemplateRotation.Deg0, 0);
            Assert.IsTrue(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException), "Rooms can't be placed at the same Z")]
        public void RoomsCantBePlacedAtSameZ()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testsolid1.room");

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(8, 0, 0, room1, TemplateRotation.Deg0, 0);
            Assert.IsTrue(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void RoomsCanBePlacedAtPresetZUsingAddPositionedTemplateOnTop()
        {
            //Load sample template 8x4
            RoomTemplate room1 = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testsolid1.room");

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(8, 0, 0, room1, TemplateRotation.Deg0, 0);
            Assert.IsTrue(mapGen.AddPositionedTemplateOnTop(templatePos2));
        }

        [TestMethod]
        public void RoomsWithOverlapOnTransparentBoundariesCanBeAdded()
        {
            RoomTemplate room1 = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testsolid1.room");

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(9, 0, 1, room1, TemplateRotation.Deg0, 0);
            Assert.IsTrue(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void RoomsAtHighXWhichMergeShouldntOverlap()
        {
            RoomTemplate room1 = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testsolid1.room");

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(20, 20, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(21, 21, 1, room1, TemplateRotation.Deg0, 0);
            Assert.IsFalse(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void PlacingTwoRoomsWithAConnectionCorridorCreatesCorrectConnectivityGraph()
        {
            RoomTemplate room1 = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testsolid1.room");

            TemplatedMapBuilder mapGen = new TemplatedMapBuilder();

            TemplatePositioned templatePos1 = new TemplatePositioned(20, 20, 0, room1, TemplateRotation.Deg0, 0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(21, 21, 1, room1, TemplateRotation.Deg0, 0);
            Assert.IsFalse(mapGen.AddPositionedTemplate(templatePos2));
        }

        private RoomTemplate LoadTemplateFromAssemblyFile(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream(filePath);
            return RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);
        }

    }
}
