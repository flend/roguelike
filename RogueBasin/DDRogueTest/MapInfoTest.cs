using GraphMap;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System.Collections.Generic;
using System.Linq;

namespace DDRogueTest
{
    [TestClass]
    public class MapInfoTest
    {
        [TestMethod]
        public void RoomsInDescendingDistanceFromSourceProducesOrdersByDistanceDescAndIdAscending()
        {
            var mapInfo = GetStandardMapInfo();

            var roomInOrderFromSource = mapInfo.RoomsInDescendingDistanceFromSource(5, new int[] { 1, 2, 3, 6, 7 });

            CollectionAssert.AreEqual(new List<int> { 1, 2, 7, 3, 6 }, roomInOrderFromSource.ToList());
        }

        [TestMethod]
        public void RoomsInALevelCanBeReturned()
        {
            var mapInfo = GetStandardMapInfo();

            var level1Nodes = mapInfo.GetRoomIndicesForLevel(1);

            CollectionAssert.AreEquivalent(new List<int>(new int[] { 5, 6, 7 }), level1Nodes.ToList());
        }

        [TestMethod]
        public void ConnectionsInALevelCanBeReturned()
        {
            var mapInfo = GetStandardMapInfo();

            var level1Nodes = mapInfo.GetConnectionsOnLevel(0);

            CollectionAssert.AreEquivalent(new List<Connection>(new Connection[] { new Connection(1, 2), new Connection(2, 3), new Connection(3, 5) }), level1Nodes.ToList());
        }

        [TestMethod]
        public void TheDoorOnAConnectionCanBeReturned()
        {
            var mapInfo = GetStandardMapInfo();
            var doorInfo = mapInfo.GetDoorForConnection(new Connection(2, 3));

            Assert.AreEqual(0, doorInfo.LevelNo);
            Assert.AreEqual(new Point(5, 5), doorInfo.MapLocation);
        }

        [TestMethod]
        public void CorridorsCanBeFilteredOut()
        {
            var mapInfo = GetStandardMapInfoForTemplates();
            var filteredRooms = mapInfo.FilterOutCorridors(new List<int> { 0, 1, 2 }).ToList();
            CollectionAssert.AreEqual(new List<int> { 2 }, filteredRooms);
        }

        [TestMethod]
        public void GetWalkablePointsInRoomCorrectlyRemovesFeatureAndCreatureOccupiedPoints()
        {
            var mapInfo = StandardTwoRoomOneLevelMapInfo();
            var room0Info = mapInfo.Populator.RoomInfo(0);

            var blockingFeatureToPlace = new FeatureRoomPlacement(new TestEntities.BlockingFeature(), new Point(1, 2));
            var nonBlockingFeatureToPlace = new FeatureRoomPlacement(new TestEntities.NonBlockingFeature(), new Point(2, 3));
            var monsterToPlace = new MonsterRoomPlacement(new TestEntities.TestMonster(), new Point(4, 4));
            room0Info.AddFeature(blockingFeatureToPlace);
            room0Info.AddFeature(nonBlockingFeatureToPlace);
            room0Info.AddMonster(monsterToPlace);

            var roomLevelLocation = mapInfo.Room(0).Location;

            var freeSpacesAbsolute = mapInfo.GetAllUnoccupiedRoomPoints(new List<int>(){0}, false);
            var freeSpacesRelative = freeSpacesAbsolute.Select(p => p.mapLocation - roomLevelLocation);

            Assert.IsFalse(freeSpacesRelative.Contains(new Point(0, 1)));

            Assert.IsFalse(freeSpacesRelative.Contains(new Point(1, 2)));
            Assert.IsFalse(freeSpacesRelative.Contains(new Point(4, 4)));

            Assert.IsTrue(freeSpacesRelative.Contains(new Point(2, 3)));
            Assert.IsTrue(freeSpacesRelative.Contains(new Point(1, 1)));

        }

        [TestMethod]
        public void GetWalkablePointsInRoomReturnsBoundariesIfAvailable()
        {
            var mapInfo = StandardTwoRoomOneLevelMapInfo();
            var room0Info = mapInfo.Populator.RoomInfo(0);

            var blockingFeatureToPlace = new FeatureRoomPlacement(new TestEntities.BlockingFeature(), new Point(1, 1));
            room0Info.AddFeature(blockingFeatureToPlace);

            var roomLevelLocation = mapInfo.Room(0).Location;

            var freeSpacesAbsolute = mapInfo.GetAllUnoccupiedRoomPoints(new List<int>() { 0 }, true);
            var freeSpacesRelative = freeSpacesAbsolute.Select(p => p.mapLocation - roomLevelLocation);

            Assert.AreEqual(12, freeSpacesRelative.Count());

            Assert.IsFalse(freeSpacesRelative.Contains(new Point(1, 1)));

            Assert.IsTrue(freeSpacesRelative.Contains(new Point(1, 3)));
        }

        [TestMethod]
        public void GetWalkablePointsInRoomReturnsAllWalkableSquareIfNoBoundariesAvailable()
        {
            var mapInfo = SmallTwoRoomOneLevelMapInfo();
            var room0Info = mapInfo.Populator.RoomInfo(0);

            FeatureRoomPlacement blockingFeatureToPlace = new FeatureRoomPlacement(new TestEntities.BlockingFeature(), new Point(1, 1));
            room0Info.AddFeature(blockingFeatureToPlace);
            blockingFeatureToPlace = new FeatureRoomPlacement(new TestEntities.BlockingFeature(), new Point(2, 1));
            room0Info.AddFeature(blockingFeatureToPlace);
            blockingFeatureToPlace = new FeatureRoomPlacement(new TestEntities.BlockingFeature(), new Point(3, 1));
            room0Info.AddFeature(blockingFeatureToPlace);

            blockingFeatureToPlace = new FeatureRoomPlacement(new TestEntities.BlockingFeature(), new Point(3, 2));
            room0Info.AddFeature(blockingFeatureToPlace);

            blockingFeatureToPlace = new FeatureRoomPlacement(new TestEntities.BlockingFeature(), new Point(1, 3));
            room0Info.AddFeature(blockingFeatureToPlace);
            blockingFeatureToPlace = new FeatureRoomPlacement(new TestEntities.BlockingFeature(), new Point(2, 3));
            room0Info.AddFeature(blockingFeatureToPlace);
            blockingFeatureToPlace = new FeatureRoomPlacement(new TestEntities.BlockingFeature(), new Point(3, 3));
            room0Info.AddFeature(blockingFeatureToPlace);

            var roomLevelLocation = mapInfo.Room(0).Location;

            var freeSpacesAbsolute = mapInfo.GetAllUnoccupiedRoomPoints(new List<int>() { 0 }, true);
            var freeSpacesRelative = freeSpacesAbsolute.Select(p => p.mapLocation - roomLevelLocation).ToList();

            CollectionAssert.AreEquivalent(new List<Point>() { new Point(1,2), new Point(2,2)}, freeSpacesRelative);
        }

        [TestMethod]
        public void GetFreePointsToPlaceCreatureInRoomCorrectlyRemovesFeatureAndCreatureOccupiedPoints()
        {
            var mapInfo = StandardTwoRoomOneLevelMapInfo();
            var room0Info = mapInfo.Populator.RoomInfo(0);

            var blockingFeatureToPlace = new FeatureRoomPlacement(new TestEntities.BlockingFeature(), new Point(1, 1));
            var nonBlockingFeatureToPlace = new FeatureRoomPlacement(new TestEntities.NonBlockingFeature(), new Point(2, 2));
            var monsterToPlace = new MonsterRoomPlacement(new TestEntities.TestMonster(), new Point(3, 3));
            room0Info.AddFeature(blockingFeatureToPlace);
            room0Info.AddFeature(nonBlockingFeatureToPlace);
            room0Info.AddMonster(monsterToPlace);

            var roomLevelLocation = mapInfo.Room(0).Location;

            var freeSpacesAbsolute = mapInfo.GetUnoccupiedPointsInRoom(0);
            var freeSpacesRelative = freeSpacesAbsolute.Select(p => p - roomLevelLocation);

            Assert.IsFalse(freeSpacesRelative.Contains(new Point(1, 1)));
            Assert.IsFalse(freeSpacesRelative.Contains(new Point(3, 3)));

            Assert.IsTrue(freeSpacesRelative.Contains(new Point(2, 2)));
        }

        [TestMethod]
        public void GetOccupiedPointsInRoomRemovesFeatureAndCreatureOccupiedPoints()
        {
            var mapInfo = StandardTwoRoomOneLevelMapInfo();
            var room0Info = mapInfo.Populator.RoomInfo(0);

            var blockingFeatureToPlace = new FeatureRoomPlacement(new TestEntities.BlockingFeature(), new Point(1, 2));
            var nonBlockingFeatureToPlace = new FeatureRoomPlacement(new TestEntities.NonBlockingFeature(), new Point(2, 1));
            var monsterToPlace = new MonsterRoomPlacement(new TestEntities.TestMonster(), new Point(3, 4));
            room0Info.AddFeature(blockingFeatureToPlace);
            room0Info.AddFeature(nonBlockingFeatureToPlace);
            room0Info.AddMonster(monsterToPlace);

            var roomLevelLocation = mapInfo.Room(0).Location;

            var occupiedSpacesAbsolute = mapInfo.GetOccupiedPointsInRoom(0);
            var occupiedSpacesRelative = occupiedSpacesAbsolute.Select(p => p - roomLevelLocation);

            Assert.IsTrue(occupiedSpacesRelative.Contains(new Point(1, 2)));
            Assert.IsTrue(occupiedSpacesRelative.Contains(new Point(3, 4)));

            Assert.IsFalse(occupiedSpacesRelative.Contains(new Point(2, 1)));
        }


        [TestMethod]
        public void RoomsCanBeRetrievedByIndex()
        {
            var newTemplate = new TemplatePositioned(9, 9, 0, null, 100);
            var mapInfoBuilder = new MapInfoBuilder();

            var templateList = new List<TemplatePositioned>();
            templateList.Add(newTemplate);

            var map = new ConnectivityMap();
            map.AddRoomConnection(new Connection(100, 101));

            mapInfoBuilder.AddConstructedLevel(0, map, templateList, new Dictionary<Connection, Point>(), 100);

            var mapInfo = new MapInfo(mapInfoBuilder, new MapPopulator());

            Assert.AreEqual(new Point(9, 9), mapInfo.Room(100).Location);
        }

        [TestMethod]
        public void RoomsInCollapsedCyclesAreRepeatedToEquateObjectDistribution()
        {
            var mapInfo = MapInfoWithCycle();

            var roomsWithRepeatsForCycles = mapInfo.RepeatRoomNodesByNumberOfRoomsInCollapsedCycles(new List<int> { 1, 2 }).ToList();
            var expectedRoomsWithRepeats = new List<int> { 1, 2, 2, 2 };

            CollectionAssert.AreEquivalent(expectedRoomsWithRepeats, roomsWithRepeatsForCycles);
        }

        [TestMethod]
        public void CyclesOnSeparateLevelsCanBeReturned()
        {
            var builder = new MapInfoBuilder();

            var l1ConnectivityMap = new ConnectivityMap();

            //Cycle in level 1
            l1ConnectivityMap.AddRoomConnection(1, 2);
            l1ConnectivityMap.AddRoomConnection(2, 3);
            l1ConnectivityMap.AddRoomConnection(3, 1);

            var l1RoomList = new List<TemplatePositioned>();
            var room1 = new TemplatePositioned(1, 1, 0, null, 1);
            l1RoomList.Add(room1);
            l1RoomList.Add(new TemplatePositioned(1, 1, 0, null, 2));
            l1RoomList.Add(new TemplatePositioned(1, 1, 0, null, 3));

            var l2ConnectivityMap = new ConnectivityMap();

            //Cycle in level 2
            l2ConnectivityMap.AddRoomConnection(5, 6);
            l2ConnectivityMap.AddRoomConnection(6, 7);
            l2ConnectivityMap.AddRoomConnection(7, 5);

            var l2RoomList = new List<TemplatePositioned>();
            var room5 = new TemplatePositioned(1, 1, 0, null, 5);
            l2RoomList.Add(room5);
            l2RoomList.Add(new TemplatePositioned(1, 1, 0, null, 6));
            l2RoomList.Add(new TemplatePositioned(1, 1, 0, null, 7));

            builder.AddConstructedLevel(0, l1ConnectivityMap, l1RoomList, new Dictionary<Connection, Point>(), 1);
            builder.AddConstructedLevel(1, l2ConnectivityMap, l2RoomList, new Dictionary<Connection, Point>(), new Connection(3, 5));

            var mapInfo = new MapInfo(builder, new MapPopulator());

            var cyclesOnLevel0 = mapInfo.GetCyclesOnLevel(0).ToList();
            Assert.AreEqual(1, cyclesOnLevel0.Count());
            CollectionAssert.AreEquivalent(cyclesOnLevel0[0], new List<Connection>{
                new Connection(1, 2),
                new Connection(2, 3),
                new Connection(3, 1)
            });

            var cyclesOnLevel1 = mapInfo.GetCyclesOnLevel(1).ToList();
            Assert.AreEqual(1, cyclesOnLevel1.Count());
            CollectionAssert.AreEquivalent(cyclesOnLevel1[0], new List<Connection>{
                new Connection(5, 6),
                new Connection(6, 7),
                new Connection(7, 5)
            });

        }


        private MapInfo GetStandardMapInfoForTemplates()
        {
            var builder = new MapInfoBuilder();

            var l1ConnectivityMap = new ConnectivityMap();
            l1ConnectivityMap.AddRoomConnection(1, 2);
            l1ConnectivityMap.AddRoomConnection(2, 3);

            var corridor1 = new RoomTemplateTerrain[1, 3];
            var corridor2 = new RoomTemplateTerrain[3, 1];
            var room1 = new RoomTemplateTerrain[4, 4];

            var l1RoomList = new List<TemplatePositioned>();
            l1RoomList.Add(new TemplatePositioned(1, 1, 0, new RoomTemplate(corridor1), 0));
            l1RoomList.Add(new TemplatePositioned(1, 1, 0, new RoomTemplate(corridor2), 1));
            l1RoomList.Add(new TemplatePositioned(1, 1, 0, new RoomTemplate(room1), 2));

            var l1DoorDict = new Dictionary<Connection, Point>();
            l1DoorDict.Add(new Connection(2, 3), new Point(5, 5));

            builder.AddConstructedLevel(0, l1ConnectivityMap, l1RoomList, l1DoorDict, 1);

            return new MapInfo(builder, new MapPopulator());
        }

        private MapInfo MapInfoWithCycle()
        {
            var builder = new MapInfoBuilder();
            var l1ConnectivityMap = new ConnectivityMap();
            l1ConnectivityMap.AddRoomConnection(1, 2);
            l1ConnectivityMap.AddRoomConnection(2, 3);
            l1ConnectivityMap.AddRoomConnection(3, 4);
            l1ConnectivityMap.AddRoomConnection(4, 2);

            builder.AddConstructedLevel(0, l1ConnectivityMap, new List<TemplatePositioned>(), new Dictionary<Connection, Point>(), 1);

            return new MapInfo(builder, new MapPopulator());
        }

        private MapInfo GetStandardMapInfo()
        {
            var builder = new MapInfoBuilder();

            var l1ConnectivityMap = new ConnectivityMap();
            l1ConnectivityMap.AddRoomConnection(1, 2);
            l1ConnectivityMap.AddRoomConnection(2, 3);

            var l1RoomList = new List<TemplatePositioned>();
            var room1 = new TemplatePositioned(1, 1, 0, null, 1);
            l1RoomList.Add(room1);
            l1RoomList.Add(new TemplatePositioned(1, 1, 0, null, 2));
            l1RoomList.Add(new TemplatePositioned(1, 1, 0, null, 3));

            var l2ConnectivityMap = new ConnectivityMap();
            l2ConnectivityMap.AddRoomConnection(5, 6);
            l2ConnectivityMap.AddRoomConnection(6, 7);

            var l2RoomList = new List<TemplatePositioned>();
            var room5 = new TemplatePositioned(1, 1, 0, null, 5);
            l2RoomList.Add(room5);
            l2RoomList.Add(new TemplatePositioned(1, 1, 0, null, 6));
            l2RoomList.Add(new TemplatePositioned(1, 1, 0, null, 7));

            var l1DoorDict = new Dictionary<Connection, Point>();
            l1DoorDict.Add(new Connection(2, 3), new Point(5, 5));

            var l2DoorDict = new Dictionary<Connection, Point>();
            l2DoorDict.Add(new Connection(5, 6), new Point(8, 8));

            builder.AddConstructedLevel(0, l1ConnectivityMap, l1RoomList, l1DoorDict, 1);
            builder.AddConstructedLevel(1, l2ConnectivityMap, l2RoomList, l2DoorDict, new Connection(3, 5));

            return new MapInfo(builder, new MapPopulator());
        }

        private MapInfo StandardTwoRoomOneLevelMapInfo()
        {
            var builder = new MapInfoBuilder();
            
            RoomTemplate baseRoomTemplate = TestUtilities.LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom3.room");
            RoomTemplate toAlignRoomTemplate = TestUtilities.LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.testalignmentroom1.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);
            
            var startRoomId = mapGen.PlaceRoomTemplateAtPosition(baseRoomTemplate, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(toAlignRoomTemplate, null, mapGen.PotentialDoors[0], 0, 0);

            builder.AddConstructedLevel(0, mapGen.ConnectivityMap, mapGen.GetRoomTemplatesInWorldCoords(), mapGen.GetDoorsInMapCoords(), startRoomId);

            return new MapInfo(builder, new MapPopulator());
        }

        private MapInfo SmallTwoRoomOneLevelMapInfo()
        {
            var builder = new MapInfoBuilder();

            RoomTemplate baseRoomTemplate = TestUtilities.LoadTemplateFromAssemblyFile("DDRogueTest.testdata.vaults.smallroom.room");

            TemplatedMapBuilder mapBuilder = new TemplatedMapBuilder();
            TemplatedMapGenerator mapGen = new TemplatedMapGenerator(mapBuilder);

            var startRoomId = mapGen.PlaceRoomTemplateAtPosition(baseRoomTemplate, new Point(0, 0));
            mapGen.PlaceRoomTemplateAlignedWithExistingDoor(baseRoomTemplate, null, mapGen.PotentialDoors[0], 0, 0);

            builder.AddConstructedLevel(0, mapGen.ConnectivityMap, mapGen.GetRoomTemplatesInWorldCoords(), mapGen.GetDoorsInMapCoords(), startRoomId);

            return new MapInfo(builder, new MapPopulator());
        }

    }
}
