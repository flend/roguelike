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
        public void TemplatesCanBeAlignedToNonMatchingDoorsByRotationBaseBotDoorTargetBotDoor()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate toAlignRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");

            TemplatePositioned baseRoom = new TemplatePositioned(0, 0, 0, baseRoomTemplate, TemplateRotation.Deg0);

            TemplatePositioned alignedRoom = RoomTemplateUtilities.AlignRoomOnDoorWithRotation(toAlignRoomTemplate, baseRoom, 0, 0, 5);

            Assert.AreEqual(1, alignedRoom.X);
            Assert.AreEqual(8, alignedRoom.Y);
        }

        [TestMethod]
        public void RotatePointInRoom90Deg()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            Point doorPoint = new Point(4, 2);

            Point rotatedPoint = RoomTemplateUtilities.RotateRoomPoint(baseRoomTemplate, doorPoint, TemplateRotation.Deg90);

            Assert.AreEqual(new Point(1, 4), rotatedPoint);
        }

        [TestMethod]
        public void RotatePointInRoom180Deg()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            Point doorPoint = new Point(4, 2);

            Point rotatedPoint = RoomTemplateUtilities.RotateRoomPoint(baseRoomTemplate, doorPoint, TemplateRotation.Deg180);

            Assert.AreEqual(new Point(3, 1), rotatedPoint);
        }

        [TestMethod]
        public void RotatePointInRoom270Deg()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            Point doorPoint = new Point(4, 2);

            Point rotatedPoint = RoomTemplateUtilities.RotateRoomPoint(baseRoomTemplate, doorPoint, TemplateRotation.Deg270);

            Assert.AreEqual(new Point(2, 3), rotatedPoint);
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

        [TestMethod]
        public void RotateRoomTemplate0DegCausesNoChanges()
        {
            RoomTemplate asymmetricRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrotation.room");
            RoomTemplate rotatedTemplate = RoomTemplateUtilities.RotateRoomTemplate(asymmetricRoomTemplate, TemplateRotation.Deg0);

            Assert.AreEqual(rotatedTemplate, asymmetricRoomTemplate);
        }

        [TestMethod]
        public void RotateRoomTemplate90DegHasCorrectDoorPlacement()
        {
            RoomTemplate asymmetricRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrotation.room"); //6x7
            RoomTemplate rotatedTemplate = RoomTemplateUtilities.RotateRoomTemplate(asymmetricRoomTemplate, TemplateRotation.Deg90);

            //Door at 0,3 is rotated (and is now index 2)
            Assert.AreEqual(rotatedTemplate.PotentialDoors[2].Location, new Point(6, 3));

            Assert.AreEqual(7, rotatedTemplate.Width);
            Assert.AreEqual(6, rotatedTemplate.Height);
        }

        [TestMethod]
        public void RotateRoomTemplate180DegHasCorrectDoorPlacement()
        {
            RoomTemplate asymmetricRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrotation.room"); //6x7
            RoomTemplate rotatedTemplate = RoomTemplateUtilities.RotateRoomTemplate(asymmetricRoomTemplate, TemplateRotation.Deg180);

            //Door at 0,3 is rotated (and is now index 3)
            Assert.AreEqual(new Point(2, 6), rotatedTemplate.PotentialDoors[3].Location);

            //No change to dimensions
            Assert.AreEqual(6, rotatedTemplate.Width);
            Assert.AreEqual(7, rotatedTemplate.Height);
        }

        [TestMethod]
        public void RotateRoomTemplate270DegHasCorrectDoorPlacement()
        {
            RoomTemplate asymmetricRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testrotation.room"); //6x7
            RoomTemplate rotatedTemplate = RoomTemplateUtilities.RotateRoomTemplate(asymmetricRoomTemplate, TemplateRotation.Deg270);

            //Door at 0,3 is rotated (and is now index 1)
            Assert.AreEqual(rotatedTemplate.PotentialDoors[1].Location, new Point(0, 2));

            Assert.AreEqual(7, rotatedTemplate.Width);
            Assert.AreEqual(6, rotatedTemplate.Height);
        }

        private RoomTemplate LoadTemplateFromAssemblyFile(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream(filePath);
            return RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);
        }
    }
}
