using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System.Reflection;
using System.IO;

namespace DDRogueTest
{
    [TestClass]
    public class RoomTemplateUtilitiesTest
    {
        [TestMethod]
        public void CorridorTemplatesCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedcorridor1.room");

            RoomTemplate expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplate(false, 2, corridorTemplate);

            Assert.AreEqual(expandedTemplate, correctOutput);
        }

        [TestMethod]
        public void CorridorTemplatesCanBeExpandedAndSwitchedToHorizontalCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedcorridorhoriz1.room");

            RoomTemplate expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplate(true, 2, corridorTemplate);

            Assert.AreEqual(expandedTemplate, correctOutput);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException), "Corridors must be of length 1")]
        public void CorridorsTemplatesMustBeHeightOne()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testtransparent1.room"); //8x4

            RoomTemplateUtilities.ExpandCorridorTemplate(false, 2, corridorTemplate);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void CorridorsMustBeStraight()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 0), new Point(5, 1), 0, corridorTemplate);
        }

        [TestMethod]
        public void HorizontalCorridorTemplatesShouldBeOffsetInPlacement() {

            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 0), new Point(5, 0), 0, corridorTemplate);

            Assert.AreEqual(positionedTemplate.X, 0);
            Assert.AreEqual(positionedTemplate.Y, -1);
        }

        [TestMethod]
        public void VerticalCorridorTemplatesShouldBeOffsetInPlacement()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 0), new Point(0, 5), 0, corridorTemplate);

            Assert.AreEqual(positionedTemplate.X, -1);
            Assert.AreEqual(positionedTemplate.Y, 0);
        }

        [TestMethod]
        public void HorizontalCorridorsCanBeMadeRegardlessOfPointOrder()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate1 = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 0), new Point(5, 0), 0, corridorTemplate);
            TemplatePositioned positionedTemplate2 = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(5, 0), new Point(0, 0), 0, corridorTemplate);

            Assert.AreEqual(positionedTemplate1.X, positionedTemplate2.X);
            Assert.AreEqual(positionedTemplate1.Y, positionedTemplate2.Y);
            Assert.AreEqual(positionedTemplate1.Room, positionedTemplate2.Room);
        }

        [TestMethod]
        public void VerticalCorridorsCanBeMadeRegardlessOfPointOrder()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate1 = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 0), new Point(0, 5), 0, corridorTemplate);
            TemplatePositioned positionedTemplate2 = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 5), new Point(0, 0), 0, corridorTemplate);

            Assert.AreEqual(positionedTemplate1.X, positionedTemplate2.X);
            Assert.AreEqual(positionedTemplate1.Y, positionedTemplate2.Y);
            Assert.AreEqual(positionedTemplate1.Room, positionedTemplate2.Room);
        }

        [TestMethod]
        public void VerticalCorridorTemplatesShouldBeCorrectlySized()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 0), new Point(0, 5), 0, corridorTemplate);

            Assert.AreEqual(positionedTemplate.Room.Width, 3);
            Assert.AreEqual(positionedTemplate.Room.Height, 6);
        }

        [TestMethod]
        public void HorizontalCorridorTemplatesShouldBeCorrectlySized()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 0), new Point(5, 0), 0, corridorTemplate);

            Assert.AreEqual(positionedTemplate.Room.Width, 6);
            Assert.AreEqual(positionedTemplate.Room.Height, 3);
        }

        [TestMethod]
        public void TemplatesCanBeAlignedToMatchingDoorsWithoutRotationBaseBottomDoorTargetTopDoor()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate toAlignRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");

            TemplatePositioned baseRoom = new TemplatePositioned(0, 0, 0, baseRoomTemplate, TemplateRotation.Deg0);

            TemplatePositioned alignedRoom = RoomTemplateUtilities.AlignRoomOnDoor(toAlignRoomTemplate, baseRoom, 0, 0, 5);

            Assert.AreEqual(alignedRoom.X, 3);
            Assert.AreEqual(alignedRoom.Y, 8);
        }

        [TestMethod]
        public void TemplatesCanBeAlignedToMatchingDoorsWithoutRotationBaseTopDoorTargetBottomDoor()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate toAlignRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");

            TemplatePositioned baseRoom = new TemplatePositioned(0, 0, 0, baseRoomTemplate, TemplateRotation.Deg0);

            TemplatePositioned alignedRoom = RoomTemplateUtilities.AlignRoomOnDoor(toAlignRoomTemplate, baseRoom, 0, 0, 5);

            Assert.AreEqual(-3, alignedRoom.X);
            Assert.AreEqual(-8, alignedRoom.Y);
        }

        [TestMethod]
        public void TemplatesCanBeAlignedToMatchingDoorsWithoutRotationBaseLeftDoorTargetRightDoor()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom3.room");
            RoomTemplate toAlignRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom4.room");

            TemplatePositioned baseRoom = new TemplatePositioned(0, 0, 0, baseRoomTemplate, TemplateRotation.Deg0);

            TemplatePositioned alignedRoom = RoomTemplateUtilities.AlignRoomOnDoor(toAlignRoomTemplate, baseRoom, 0, 0, 5);

            Assert.AreEqual(-12, alignedRoom.X);
            Assert.AreEqual(1, alignedRoom.Y);
        }

        [TestMethod]
        public void TemplatesCanBeAlignedToMatchingDoorsWithoutRotationBaseRightDoorTargetLeftDoor()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom4.room");
            RoomTemplate toAlignRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom3.room");

            TemplatePositioned baseRoom = new TemplatePositioned(0, 0, 0, baseRoomTemplate, TemplateRotation.Deg0);

            TemplatePositioned alignedRoom = RoomTemplateUtilities.AlignRoomOnDoor(toAlignRoomTemplate, baseRoom, 0, 0, 5);

            Assert.AreEqual(12, alignedRoom.X);
            Assert.AreEqual(-1, alignedRoom.Y);
        }

        [TestMethod]
        public void GetLocationOfDoorsOnCircumferenceOfRoom()
        {
            RoomTemplate fourDoorRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.test4doors.room");

            Assert.AreEqual(RoomTemplateUtilities.GetDoorLocation(fourDoorRoomTemplate, 0), RoomTemplate.DoorLocation.Top);
            Assert.AreEqual(RoomTemplateUtilities.GetDoorLocation(fourDoorRoomTemplate, 1), RoomTemplate.DoorLocation.Left);
            Assert.AreEqual(RoomTemplateUtilities.GetDoorLocation(fourDoorRoomTemplate, 2), RoomTemplate.DoorLocation.Right);
            Assert.AreEqual(RoomTemplateUtilities.GetDoorLocation(fourDoorRoomTemplate, 3), RoomTemplate.DoorLocation.Bottom);
        }

        private RoomTemplate LoadTemplateFromAssemblyFile(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream(filePath);
            return RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);
        }
    }
}
