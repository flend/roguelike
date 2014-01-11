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
        public void RDHorizontalLShapedCorridorsCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedcorridorldr.room");

            //+x +y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateLShaped(5, 3, false, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "rdh-l-corridor.txt");

            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
            Assert.AreEqual(new Point(1, 0), expandedTemplate.Item2);
        }

        [TestMethod]
        public void RDVerticalLShapedCorridorsCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedcorridorlrd.room");

            //+x +y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateLShaped(6, 4, true, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "rdv-l-corridor.txt");

            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
            Assert.AreEqual(new Point(0, 1), expandedTemplate.Item2);
        }

        [TestMethod]
        public void LUHorizontalLShapedCorridorsCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedcorridorlrd.room");

            //-x -y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateLShaped(-6, -4, false, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "rdh-l-corridor.txt");

            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
            Assert.AreEqual(expandedTemplate.Item2, new Point(6, 5));
        }

        [TestMethod]
        public void LUVerticalLShapedCorridorsCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedcorridorldr.room");

            //-x -y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateLShaped(-5, -3, true, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "rdv-l-corridor.txt");

            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
            Assert.AreEqual(expandedTemplate.Item2, new Point(6, 3));
        }

        [TestMethod]
        public void LDHorizontalLShapedCorridorsCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedcorridorlld.room");

            //-x +y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateLShaped(-6, 4, true, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "rdh-l-corridor.txt");

            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
            Assert.AreEqual(expandedTemplate.Item2, new Point(7, 1));
        }

        [TestMethod]
        public void LDVerticalLShapedCorridorsCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedcorridorldl.room");

            //-x +y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateLShaped(-5, 3, false, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "rdv-l-corridor.txt");

            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
            Assert.AreEqual(expandedTemplate.Item2, new Point(5, 0));
        }

        [TestMethod]
        public void RUHorizontalLShapedCorridorsCanBeExpandedCorrectly()
        {
            var corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedcorridorldl.room");

            //+x -y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateLShaped(5, -3, true, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "rdh-l-corridor.txt");

            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
            Assert.AreEqual(expandedTemplate.Item2, new Point(0, 3));
        }

        [TestMethod]
        public void RUVerticalLShapedCorridorsCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedcorridorlld.room");

            //+x -y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateLShaped(6, -4, false, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "rdv-l-corridor.txt");

            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
            Assert.AreEqual(expandedTemplate.Item2, new Point(1, 5));
        }

        [TestMethod]
        public void TopRoomCanConnectToRightCorridorAtLowerYLowerX()
        {
            Point door1Coord = new Point(0, 0);
            RoomTemplate.DoorLocation door1Loc = RoomTemplate.DoorLocation.Top;

            Point door2Coord = new Point(-5, -1);
            RoomTemplate.DoorLocation door2Loc = RoomTemplate.DoorLocation.Right;

            Assert.IsTrue(RoomTemplateUtilities.CanBeConnectedWithLShapedCorridor(door1Coord, door1Loc, door2Coord, door2Loc));
        }

        [TestMethod]
        public void TopRoomCannotConnectToRightCorridorAtLowerYGreaterX()
        {
            Point door1Coord = new Point(0, 0);
            RoomTemplate.DoorLocation door1Loc = RoomTemplate.DoorLocation.Top;

            Point door2Coord = new Point(5, -1);
            RoomTemplate.DoorLocation door2Loc = RoomTemplate.DoorLocation.Right;

            Assert.IsFalse(RoomTemplateUtilities.CanBeConnectedWithLShapedCorridor(door1Coord, door1Loc, door2Coord, door2Loc));
        }

        [TestMethod]
        public void TopRoomCannotConnectToRightCorridorAtGreaterY()
        {
            Point door1Coord = new Point(0, 0);
            RoomTemplate.DoorLocation door1Loc = RoomTemplate.DoorLocation.Top;

            Point door2Coord = new Point(5, 1);
            RoomTemplate.DoorLocation door2Loc = RoomTemplate.DoorLocation.Right;

            Assert.IsFalse(RoomTemplateUtilities.CanBeConnectedWithLShapedCorridor(door1Coord, door1Loc, door2Coord, door2Loc));
        }

        [TestMethod]
        public void TopRoomCanConnectToLeftCorridorAtLowerYGreaterX()
        {
            Point door1Coord = new Point(0, 0);
            RoomTemplate.DoorLocation door1Loc = RoomTemplate.DoorLocation.Top;

            Point door2Coord = new Point(5, -1);
            RoomTemplate.DoorLocation door2Loc = RoomTemplate.DoorLocation.Left;

            Assert.IsTrue(RoomTemplateUtilities.CanBeConnectedWithLShapedCorridor(door1Coord, door1Loc, door2Coord, door2Loc));
        }

        [TestMethod]
        public void TopDoorCanConnectToBottomDoorAtLowerYWithBendCorridor()
        {
            Point door1Coord = new Point(0, 0);
            RoomTemplate.DoorLocation door1Loc = RoomTemplate.DoorLocation.Top;

            Point door2Coord = new Point(2, -1);
            RoomTemplate.DoorLocation door2Loc = RoomTemplate.DoorLocation.Bottom;

            Assert.IsTrue(RoomTemplateUtilities.CanBeConnectedWithBendCorridor(door1Coord, door1Loc, door2Coord, door2Loc));
        }

        [TestMethod]
        public void TopDoorCannotConnectToBottomDoorAtHigherYWithBendCorridor()
        {
            Point door1Coord = new Point(0, 0);
            RoomTemplate.DoorLocation door1Loc = RoomTemplate.DoorLocation.Top;

            Point door2Coord = new Point(2, 1);
            RoomTemplate.DoorLocation door2Loc = RoomTemplate.DoorLocation.Bottom;

            Assert.IsFalse(RoomTemplateUtilities.CanBeConnectedWithBendCorridor(door1Coord, door1Loc, door2Coord, door2Loc));
        }

        [TestMethod]
        public void TopDoorCannotConnectToLeftDoorWithBendCorridor()
        {
            Point door1Coord = new Point(0, 0);
            RoomTemplate.DoorLocation door1Loc = RoomTemplate.DoorLocation.Top;

            Point door2Coord = new Point(2, 1);
            RoomTemplate.DoorLocation door2Loc = RoomTemplate.DoorLocation.Bottom;

            Assert.IsFalse(RoomTemplateUtilities.CanBeConnectedWithBendCorridor(door1Coord, door1Loc, door2Coord, door2Loc));
        }

        [TestMethod]
        public void VerticalLSharedCorridorsCanBeExpandedCorrectlyReversed()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedlvertical1.room");

            //-x -y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateBend(-4, -6, -4, false, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "vertical-l-corridor-rev.txt");

            Assert.AreEqual(expandedTemplate.Item2, new Point(5, 6));
            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
        }

        [TestMethod]
        public void VerticalLSharedCorridorsCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedlvertical1.room");

            //+x +y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateBend(4, 6, 2, false, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "vertical-l-corridor.txt");

            Assert.AreEqual(expandedTemplate.Item2, new Point(1, 0));
            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
        }

        [TestMethod]
        public void Offset1VerticalLSharedCorridorsCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedlvertical2.room");

            //+x +y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateBend(1, 3, 1, false, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "vertical-l-corridor-1.txt");

            Assert.AreEqual(expandedTemplate.Item2, new Point(1, 0));
            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
        }

        [TestMethod]
        public void HorizontalLSharedCorridorsCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedlhorizontal1.room");

            //+x +y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateBend(7, 5, 5, true, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "horizontal-l-corridor.txt");

            Assert.AreEqual(expandedTemplate.Item2, new Point(0, 1));
            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
        }

        [TestMethod]
        public void Offset1HorizontalLSharedCorridorsCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedlhorizontal2.room");

            //+x +y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateBend(11, 1, 6, true, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "horizontal-l-corridor-1.txt");

            Assert.AreEqual(expandedTemplate.Item2, new Point(0, 1));
            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
        }

        [TestMethod]
        public void Transition1HorizontalLSharedCorridorsCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedlhorizontal3.room");

            //+x +y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateBend(2, 3, 1, true, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "horizontal-l-corridor-t1.txt");

            Assert.AreEqual(expandedTemplate.Item2, new Point(0, 1));
            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
        }

        [TestMethod]
        public void HorizontalLSharedCorridorsCanBeExpandedCorrectlyReversed()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedlhorizontal1.room");

            //-x -y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateBend(-7, -5, -2, true, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "horizontal-l-corridor-rev.txt");

            Assert.AreEqual(expandedTemplate.Item2, new Point(7, 6));
            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
        }

        [TestMethod]
        public void NegativeHorizontalLSharedCorridorsCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.negativeexpandedlhorizontal1.room");

            //+x -y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateBend(7, -5, 5, true, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "negative-horizontal-l-corridor.txt");

            Assert.AreEqual(expandedTemplate.Item2, new Point(0, 6));
            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
        }

        [TestMethod]
        public void NegativeHorizontalLSharedCorridorsCanBeExpandedCorrectlyReversed()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.negativeexpandedlhorizontal1.room");

            //-x +y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateBend(-7, 5, -2, true, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "negative-horizontal-l-corridor-rev.txt");

            Assert.AreEqual(expandedTemplate.Item2, new Point(7, 1));
            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
        }

        [TestMethod]
        public void NegativeVerticalLSharedCorridorsCanBeExpandedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.negativeexpandedlvertical1.room");

            //-x +y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateBend(-4, 6, 2, false, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "negative-vertical-l-corridor.txt");

            Assert.AreEqual(expandedTemplate.Item2, new Point(5, 0));
            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
        }

        [TestMethod]
        public void NegativeVerticalLSharedCorridorsCanBeExpandedCorrectlyReversed()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.negativeexpandedlvertical1.room");

            //+x -y
            var expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplateBend(4, -6, -4, false, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate.Item1, "negative-vertical-l-corridor-rev.txt");

            Assert.AreEqual(expandedTemplate.Item2, new Point(1, 6));
            Assert.AreEqual(expandedTemplate.Item1, correctOutput);
        }

        [TestMethod]
        public void CorridorTemplatesCanBeExpandedAndSwitchedToHorizontalCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            RoomTemplate correctOutput = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.expandedcorridorhoriz1.room");

            RoomTemplate expandedTemplate = RoomTemplateUtilities.ExpandCorridorTemplate(true, 2, corridorTemplate);
            RoomTemplateUtilities.ExportTemplateToTextFile(expandedTemplate, "expanded-horizontal-corridor.txt");

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
            TemplatePositioned positionedTemplate = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 0), new Point(5, 1), 0, corridorTemplate, 0);
        }

        [TestMethod]
        public void HorizontalCorridorTemplatesShouldBeOffsetInPlacement() {

            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 0), new Point(5, 0), 0, corridorTemplate, 0);

            Assert.AreEqual(positionedTemplate.X, 0);
            Assert.AreEqual(positionedTemplate.Y, -1);
        }

        [TestMethod]
        public void VerticalCorridorTemplatesShouldBeOffsetInPlacement()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 0), new Point(0, 5), 0, corridorTemplate, 0);

            Assert.AreEqual(positionedTemplate.X, -1);
            Assert.AreEqual(positionedTemplate.Y, 0);
        }

        [TestMethod]
        public void SingleSpaceVerticalCorridorsArePositionedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate = RoomTemplateUtilities.GetTemplateForSingleSpaceCorridor(new Point(0, 0), true, 0, corridorTemplate, 0);

            Assert.AreEqual(positionedTemplate.X, -1);
            Assert.AreEqual(positionedTemplate.Y, 0);

            Assert.AreEqual(positionedTemplate.Room.Height, 1);
        }

        [TestMethod]
        public void PointsOnAVerticalLineAreIdentifiedAsSuch()
        {
            Assert.IsTrue(RoomTemplateUtilities.ArePointsOnVerticalLine(new Point(4, 3), new Point(4, 6)));
        }

        [TestMethod]
        public void PointsNotOnAVerticalLineAreIdentifiedAsSuch()
        {
            Assert.IsFalse(RoomTemplateUtilities.ArePointsOnVerticalLine(new Point(4, 1), new Point(5, 1)));
        }

        [TestMethod]
        public void SingleSpaceHorizontalCorridorsArePositionedCorrectly()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate = RoomTemplateUtilities.GetTemplateForSingleSpaceCorridor(new Point(0, 0), false, 0, corridorTemplate, 0);

            Assert.AreEqual(positionedTemplate.X, 0);
            Assert.AreEqual(positionedTemplate.Y, -1);

            Assert.AreEqual(positionedTemplate.Room.Width, 1);
        }

        [TestMethod]
        public void HorizontalCorridorsCanBeMadeRegardlessOfPointOrder()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate1 = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 0), new Point(5, 0), 0, corridorTemplate, 0);
            TemplatePositioned positionedTemplate2 = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(5, 0), new Point(0, 0), 0, corridorTemplate, 0);

            Assert.AreEqual(positionedTemplate1.X, positionedTemplate2.X);
            Assert.AreEqual(positionedTemplate1.Y, positionedTemplate2.Y);
            Assert.AreEqual(positionedTemplate1.Room, positionedTemplate2.Room);
        }

        [TestMethod]
        public void VerticalCorridorsCanBeMadeRegardlessOfPointOrder()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate1 = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 0), new Point(0, 5), 0, corridorTemplate, 0);
            TemplatePositioned positionedTemplate2 = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 5), new Point(0, 0), 0, corridorTemplate, 0);

            Assert.AreEqual(positionedTemplate1.X, positionedTemplate2.X);
            Assert.AreEqual(positionedTemplate1.Y, positionedTemplate2.Y);
            Assert.AreEqual(positionedTemplate1.Room, positionedTemplate2.Room);
        }

        [TestMethod]
        public void VerticalCorridorTemplatesShouldBeCorrectlySized()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 0), new Point(0, 5), 0, corridorTemplate, 0);

            Assert.AreEqual(positionedTemplate.Room.Width, 3);
            Assert.AreEqual(positionedTemplate.Room.Height, 6);
        }

        [TestMethod]
        public void HorizontalCorridorTemplatesShouldBeCorrectlySized()
        {
            RoomTemplate corridorTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testcorridor1.room"); //3x1
            TemplatePositioned positionedTemplate = RoomTemplateUtilities.GetTemplateForCorridorBetweenPoints(new Point(0, 0), new Point(5, 0), 0, corridorTemplate, 0);

            Assert.AreEqual(positionedTemplate.Room.Width, 6);
            Assert.AreEqual(positionedTemplate.Room.Height, 3);
        }

        [TestMethod]
        public void TemplatesCanBeAlignedToMatchingDoorsWithoutRotationBaseBottomDoorTargetTopDoor()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate toAlignRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");

            TemplatePositioned baseRoom = new TemplatePositioned(0, 0, 0, baseRoomTemplate, TemplateRotation.Deg0, 0);

            TemplatePositioned alignedRoom = RoomTemplateUtilities.AlignRoomOnDoor(toAlignRoomTemplate, 0, baseRoom, 0, 0, 5).Item1;

            Assert.AreEqual(alignedRoom.X, 3);
            Assert.AreEqual(alignedRoom.Y, 8);
        }

        [TestMethod]
        public void TemplatesCanBeAlignedToMatchingDoorsWithoutRotationBaseTopDoorTargetBottomDoor()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");
            RoomTemplate toAlignRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");

            TemplatePositioned baseRoom = new TemplatePositioned(0, 0, 0, baseRoomTemplate, TemplateRotation.Deg0, 0);

            TemplatePositioned alignedRoom = RoomTemplateUtilities.AlignRoomOnDoor(toAlignRoomTemplate, 0, baseRoom, 0, 0, 5).Item1;

            Assert.AreEqual(-3, alignedRoom.X);
            Assert.AreEqual(-8, alignedRoom.Y);
        }

        [TestMethod]
        public void TemplatesCanBeAlignedToMatchingDoorsWithoutRotationBaseLeftDoorTargetRightDoor()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom3.room");
            RoomTemplate toAlignRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom4.room");

            TemplatePositioned baseRoom = new TemplatePositioned(0, 0, 0, baseRoomTemplate, TemplateRotation.Deg0, 0);

            TemplatePositioned alignedRoom = RoomTemplateUtilities.AlignRoomOnDoor(toAlignRoomTemplate, 0, baseRoom, 0, 0, 5).Item1;

            Assert.AreEqual(-12, alignedRoom.X);
            Assert.AreEqual(1, alignedRoom.Y);
        }

        [TestMethod]
        public void TemplatesCanBeAlignedToMatchingDoorsWithRotationBaseBottomDoorTargetRightDoorWithMultipleDoors()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.test4doors.room");
            RoomTemplate toAlignRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.test4doors.room");

            TemplatePositioned baseRoom = new TemplatePositioned(-31, -8, 0, baseRoomTemplate, TemplateRotation.Deg0, 0);

            var alignedRoomAndDoor = RoomTemplateUtilities.AlignRoomOnDoor(toAlignRoomTemplate, 0, baseRoom, 2, 3, 1);
            TemplatePositioned alignedRoom = alignedRoomAndDoor.Item1;
            Point alignedDoor = alignedRoomAndDoor.Item2;

            Assert.AreEqual(-29, alignedRoom.X);
            Assert.AreEqual(-4, alignedRoom.Y);

            Assert.AreEqual(-28, alignedDoor.x);
            Assert.AreEqual(-4, alignedDoor.y);
        }

        [TestMethod]
        public void TemplatesCanBeAlignedToMatchingDoorsWithoutRotationBaseRightDoorTargetLeftDoor()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom4.room");
            RoomTemplate toAlignRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom3.room");

            TemplatePositioned baseRoom = new TemplatePositioned(0, 0, 0, baseRoomTemplate, TemplateRotation.Deg0, 0);

            TemplatePositioned alignedRoom = RoomTemplateUtilities.AlignRoomOnDoor(toAlignRoomTemplate, 0, baseRoom, 0, 0, 5).Item1;

            Assert.AreEqual(12, alignedRoom.X);
            Assert.AreEqual(-1, alignedRoom.Y);
        }

        [TestMethod]
        public void TemplatesCanBeAlignedToNonMatchingDoorsByRotationBaseBotDoorTargetBotDoor()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");
            RoomTemplate toAlignRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");

            TemplatePositioned baseRoom = new TemplatePositioned(0, 0, 0, baseRoomTemplate, TemplateRotation.Deg0, 0);

            TemplatePositioned alignedRoom = RoomTemplateUtilities.AlignRoomOnDoor(toAlignRoomTemplate, 0, baseRoom, 0, 0, 5).Item1;

            Assert.AreEqual(1, alignedRoom.X);
            Assert.AreEqual(8, alignedRoom.Y);
        }

        [TestMethod]
        public void TemplatesCanBeAlignedToNonMatchingDoorsByRotationBaseLeftDoorTargetBotDoor()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom3.room");
            RoomTemplate toAlignRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");

            TemplatePositioned baseRoom = new TemplatePositioned(0, 0, 0, baseRoomTemplate, TemplateRotation.Deg0, 0);

            TemplatePositioned alignedRoom = RoomTemplateUtilities.AlignRoomOnDoor(toAlignRoomTemplate, 0, baseRoom, 0, 0, 5).Item1;

            Assert.AreEqual(-8, alignedRoom.X);
            Assert.AreEqual(-1, alignedRoom.Y);
        }

        [TestMethod]
        public void TemplatesCanBeAlignedToNonMatchingDoorsByRotationBaseRightDoorTargetTopDoor()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom4.room");
            RoomTemplate toAlignRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");

            TemplatePositioned baseRoom = new TemplatePositioned(0, 0, 0, baseRoomTemplate, TemplateRotation.Deg0, 0);

            TemplatePositioned alignedRoom = RoomTemplateUtilities.AlignRoomOnDoor(toAlignRoomTemplate, 0, baseRoom, 0, 0, 5).Item1;

            Assert.AreEqual(12, alignedRoom.X);
            Assert.AreEqual(-5, alignedRoom.Y);
        }

        [TestMethod]
        public void TemplatesAlignedToNonMatchingDoorsByRotationReturnCorrectRotatedDoorPosition()
        {
            RoomTemplate baseRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom4.room");
            RoomTemplate toAlignRoomTemplate = LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom2.room");

            TemplatePositioned baseRoom = new TemplatePositioned(0, 0, 0, baseRoomTemplate, TemplateRotation.Deg0, 0);

            Point alignedDoor = RoomTemplateUtilities.AlignRoomOnDoor(toAlignRoomTemplate, 0, baseRoom, 0, 0, 5).Item2;

            Assert.AreEqual(12, alignedDoor.x);
            Assert.AreEqual(1, alignedDoor.y);
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

        [TestMethod]
        public void CorridorsBetweenVerticallyTopBottomAlignedPointShouldStart1SquareIn()
        {
            Tuple<Point, Point> corridorPoints = RoomTemplateUtilities.CorridorTerminalPointsBetweenDoors(
                new Point(5, 5), RoomTemplate.DoorLocation.Bottom, new Point(5, 10), RoomTemplate.DoorLocation.Top);

            Assert.AreEqual(new Point(5, 6), corridorPoints.Item1);
            Assert.AreEqual(new Point(5, 9), corridorPoints.Item2);
        }

        [TestMethod]
        public void CorridorsBetweenVerticallyBottomTopAlignedPointShouldStart1SquareIn()
        {
            Tuple<Point, Point> corridorPoints = RoomTemplateUtilities.CorridorTerminalPointsBetweenDoors(
                new Point(0, -5), RoomTemplate.DoorLocation.Top, new Point(0, -10), RoomTemplate.DoorLocation.Bottom);

            Assert.AreEqual(new Point(0, -6), corridorPoints.Item1);
            Assert.AreEqual(new Point(0, -9), corridorPoints.Item2);
        }

        [TestMethod]
        public void CorridorsBetweenHorizontallyLeftRightAlignedPointShouldStart1SquareIn()
        {
            Tuple<Point, Point> corridorPoints = RoomTemplateUtilities.CorridorTerminalPointsBetweenDoors(
                new Point(15, 15), RoomTemplate.DoorLocation.Right, new Point(25, 15), RoomTemplate.DoorLocation.Left);

            Assert.AreEqual(new Point(16, 15), corridorPoints.Item1);
            Assert.AreEqual(new Point(24, 15), corridorPoints.Item2);
        }

        [TestMethod]
        public void CorridorsBetweenHorizontallyRightLeftAlignedPointShouldStart1SquareIn()
        {
            Tuple<Point, Point> corridorPoints = RoomTemplateUtilities.CorridorTerminalPointsBetweenDoors(
                new Point(-15, -15), RoomTemplate.DoorLocation.Left, new Point(-25, -15), RoomTemplate.DoorLocation.Right);

            Assert.AreEqual(new Point(-16, -15), corridorPoints.Item1);
            Assert.AreEqual(new Point(-24, -15), corridorPoints.Item2);
        }

        [TestMethod]
        public void CorridorsBetweenHorizontallyRightLeftNonAlignedPointShouldStart1SquareIn()
        {
            Tuple<Point, Point> corridorPoints = RoomTemplateUtilities.CorridorTerminalPointsBetweenDoors(
                new Point(-8, -9), RoomTemplate.DoorLocation.Right, new Point(0, 2), RoomTemplate.DoorLocation.Left);

            Assert.AreEqual(new Point(-7, -9), corridorPoints.Item1);
            Assert.AreEqual(new Point(-1, 2), corridorPoints.Item2);
        }

        [TestMethod]
        public void CorridorsBetweeVerticallyTopBottomNonAlignedPointShouldStart1SquareIn()
        {
            Tuple<Point, Point> corridorPoints = RoomTemplateUtilities.CorridorTerminalPointsBetweenDoors(
                new Point(-8, -9), RoomTemplate.DoorLocation.Bottom, new Point(0, 2), RoomTemplate.DoorLocation.Top);

            Assert.AreEqual(new Point(-8, -8), corridorPoints.Item1);
            Assert.AreEqual(new Point(0, 1), corridorPoints.Item2);
        }

        private RoomTemplate LoadTemplateFromAssemblyFile(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream(filePath);
            return RoomTemplateLoader.LoadTemplateFromFile(roomFileStream, StandardTemplateMapping.terrainMapping);
        }
    }
}
