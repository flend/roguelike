using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;

namespace DDRogueTest
{
    [TestClass]
    public class TemplatePositionedTest
    {
        [TestMethod]
        public void PotentialDoorsShouldBeAvailableFromPositionedTemplate()
        {
            RoomTemplate room1 = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.test4doors.room");

            TemplatePositioned templatePos1 = new TemplatePositioned(11, 12, 0, room1, 0);
            //Doors at: (3,0),(0,1),(7,1),(3,3)
            List <Point> outputList = new List<Point>();
            outputList.Add(new Point(14, 12));
            outputList.Add(new Point(11, 13));
            outputList.Add(new Point(18, 13));
            outputList.Add(new Point(14, 15));

            List<Point> doorList = templatePos1.PotentialDoors.ToList();

            CollectionAssert.AreEqual(doorList, outputList);
        }

        private RoomTemplate LoadTemplateFromAssemblyFile(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream(filePath);
            return RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);
        }
    }
}
