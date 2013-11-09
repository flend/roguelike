using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System.Reflection;
using System.IO;

namespace DDRogueTest
{
    [TestClass]
    public class MapGeneratorTemplatedTest
    {
        [TestMethod]
        public void TestOverlappingSolidRoomsCantBeAdded()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            MapGeneratorTemplated mapGen = new MapGeneratorTemplated();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(0, 0, 10, room1, TemplateRotation.Deg0);
            Assert.IsFalse(mapGen.AddPositionedTemplate(templatePos2));
        }

        [TestMethod]
        public void TestNonOverlappingSolidRoomsCanBeAdded()
        {
            //Load sample template 8x4
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream("DDRogueTest.testdata.vaults.testsolid1.room");
            RoomTemplate room1 = RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);

            MapGeneratorTemplated mapGen = new MapGeneratorTemplated();

            TemplatePositioned templatePos1 = new TemplatePositioned(0, 0, 0, room1, TemplateRotation.Deg0);
            mapGen.AddPositionedTemplate(templatePos1);

            TemplatePositioned templatePos2 = new TemplatePositioned(8, 0, 10, room1, TemplateRotation.Deg0);
            Assert.IsTrue(mapGen.AddPositionedTemplate(templatePos2));
        }
    }
}
