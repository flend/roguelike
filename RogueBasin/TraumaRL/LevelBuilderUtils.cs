using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TraumaRL
{
    class LevelBuilderUtils
    {
        public void AddCorridorsBetweenOpenDoors(TemplatedMapGenerator templatedGenerator, int totalExtraConnections, List<RoomTemplate> corridorsToUse)
        {
            var extraConnections = 0;

            var allDoors = templatedGenerator.PotentialDoors;

            //Find all possible doors matches that aren't in the same room
            var allBendDoorPossibilities = from d1 in allDoors
                                           from d2 in allDoors
                                           where RoomTemplateUtilities.CanBeConnectedWithBendCorridor(d1.MapCoords, d1.DoorLocation, d2.MapCoords, d2.DoorLocation)
                                                 && d1.OwnerRoomIndex != d2.OwnerRoomIndex
                                           select new { origin = d1, target = d2 };

            var allLDoorPossibilities = from d1 in allDoors
                                        from d2 in allDoors
                                        where RoomTemplateUtilities.CanBeConnectedWithLShapedCorridor(d1.MapCoords, d1.DoorLocation, d2.MapCoords, d2.DoorLocation)
                                              && d1.OwnerRoomIndex != d2.OwnerRoomIndex
                                        select new { origin = d1, target = d2 };

            var allStraightDoorPossibilities = from d1 in allDoors
                                               from d2 in allDoors
                                               where RoomTemplateUtilities.CanBeConnectedWithStraightCorridor(d1.MapCoords, d1.DoorLocation, d2.MapCoords, d2.DoorLocation)
                                                     && d1.OwnerRoomIndex != d2.OwnerRoomIndex
                                               select new { origin = d1, target = d2 };

            var allOverlappingDoorPossibilities = from d1 in allDoors
                                                  from d2 in allDoors
                                                  where d1.MapCoords == d2.MapCoords
                                                        && d1.OwnerRoomIndex != d2.OwnerRoomIndex
                                                  select new { origin = d1, target = d2 };


            //Materialize for speed

            var allMatchingDoorPossibilities = allBendDoorPossibilities.Union(allLDoorPossibilities).Union(allOverlappingDoorPossibilities).Union(allStraightDoorPossibilities).ToList();
            //var allMatchingDoorPossibilities = allLDoorPossibilities;
            //var allMatchingDoorPossibilities = allBendDoorPossibilities;

            var shuffleMatchingDoors = allMatchingDoorPossibilities.Shuffle(Game.Random);

            for (int i = 0; i < allMatchingDoorPossibilities.Count; i++)
            {
                //Try a random combination to see if it works
                var doorsToTry = shuffleMatchingDoors.ElementAt(i);

                LogFile.Log.LogEntryDebug("Trying door " + doorsToTry.origin.MapCoords + " to " + doorsToTry.target.MapCoords, LogDebugLevel.Medium);

                bool success = templatedGenerator.JoinDoorsWithCorridor(doorsToTry.origin, doorsToTry.target, corridorsToUse.RandomElement());
                if (success)
                    extraConnections++;

                if (extraConnections > totalExtraConnections)
                    break;
            }

            //Previous code (was super-slow!)
            //while (allMatchingDoorPossibilities.Any() && extraConnections < totalExtraConnections)
            //In any case, remove this attempt
            //var doorsToTry = allMatchingDoorPossibilities.ElementAt(Game.Random.Next(allMatchingDoorPossibilities.Count()));
            //allMatchingDoorPossibilities = allMatchingDoorPossibilities.Except(Enumerable.Repeat(doorsToTry, 1)); //order n - making it slow?

        }

        public int PlaceOriginRoom(TemplatedMapGenerator templatedGenerator, RoomTemplate roomToPlace)
        {
            return templatedGenerator.PlaceRoomTemplateAtPosition(roomToPlace, new RogueBasin.Point(0, 0));
        }

        public int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, RoomTemplate roomToPlace, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength)
        {
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, roomToPlace, corridorToPlace, minCorridorLength, maxCorridorLength, null);
        }

        public int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, List<Tuple<int, RoomTemplate>> roomToPlaceWithWeights, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength)
        {
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, roomToPlaceWithWeights, corridorToPlace, minCorridorLength, maxCorridorLength, null);
        }

        public int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, RoomTemplate roomToPlace, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength, Func<int> doorPicker)
        {
            return PlaceRandomConnectedRooms(templatedGenerator, roomsToPlace, new List<Tuple<int, RoomTemplate>> { new Tuple<int, RoomTemplate>(1, roomToPlace) }, corridorToPlace, minCorridorLength, maxCorridorLength, doorPicker);
        }


        /// <summary>
        /// Failure mode is placing fewer rooms than requested
        /// </summary>
        public int PlaceRandomConnectedRooms(TemplatedMapGenerator templatedGenerator, int roomsToPlace, IEnumerable<Tuple<int, RoomTemplate>> roomsToPlaceWithWeights, RoomTemplate corridorToPlace, int minCorridorLength, int maxCorridorLength, Func<int> doorPicker)
        {
            int roomsPlaced = 0;
            int attempts = 0;

            //This uses random distances and their might be collisions so we should avoid infinite loops
            int maxAttempts = roomsToPlace * 5;

            //Terminate when all rooms placed or no more potential door sites
            do
            {
                //Find a random potential door and try to grow a random room off this

                //Find room using weights
                var roomToPlace = Utility.ChooseItemFromWeights<RoomTemplate>(roomsToPlaceWithWeights);

                //Use a random door, or the function passed in
                int randomNewDoorIndex;
                if (doorPicker == null)
                {
                    //Random door
                    randomNewDoorIndex = Game.Random.Next(roomToPlace.PotentialDoors.Count);
                }
                else
                    randomNewDoorIndex = doorPicker();

                int corridorLength = Game.Random.Next(minCorridorLength, maxCorridorLength);

                try
                {
                    templatedGenerator.PlaceRoomTemplateAlignedWithExistingDoor(roomToPlace, corridorToPlace, RandomDoor(templatedGenerator), randomNewDoorIndex,
                    corridorLength);

                    roomsPlaced++;
                }
                catch (ApplicationException) { }

                attempts++;

            } while (roomsPlaced < roomsToPlace && attempts < maxAttempts && templatedGenerator.HaveRemainingPotentialDoors());
            return roomsPlaced;
        }

        public Connection AddRoomToRandomOpenDoor(TemplatedMapGenerator gen, RoomTemplate templateToPlace, RoomTemplate corridorTemplate, int distanceFromDoor)
        {
            var doorsToTry = gen.PotentialDoors.Shuffle();

            foreach (var door in doorsToTry)
            {
                try
                {
                    return gen.PlaceRoomTemplateAlignedWithExistingDoor(templateToPlace, corridorTemplate, RandomDoor(gen), 0, distanceFromDoor);
                }
                catch (ApplicationException)
                {
                    //No good, continue
                }
            }

            throw new ApplicationException("No applicable doors left");
        }

        public List<Tuple<int, Connection>> AddConnectionsToOtherLevels(ConnectivityMap levelLinks, int levelNo, LevelInfo medicalInfo, RoomTemplate corridor1, RoomTemplate elevatorVault, TemplatedMapGenerator templateGenerator)
        {
            var otherLevelConnections = levelLinks.GetAllConnections().Where(c => c.IncludesVertex(levelNo)).Select(c => c.Source == levelNo ? c.Target : c.Source);
            var connectionsToReturn = new List<Tuple<int, Connection>>();

            foreach (var otherLevel in otherLevelConnections)
            {
                var connectingRoom = AddRoomToRandomOpenDoor(templateGenerator, elevatorVault, corridor1, 3);
                connectionsToReturn.Add(new Tuple<int, Connection>(otherLevel, connectingRoom));
            }

            return connectionsToReturn;
        }

        public List<Connection> AddReplaceableVaults(TemplatedMapGenerator templateGenerator, RoomTemplate corridor1, RoomTemplate placeHolderVault, int maxPlaceHolders)
        {
            return AddReplaceableVaults(templateGenerator, corridor1, new List<RoomTemplate> { placeHolderVault }, maxPlaceHolders);
        }

        public List<Connection> AddReplaceableVaults(TemplatedMapGenerator templateGenerator, RoomTemplate corridor1, List<RoomTemplate> placeHolderVault, int maxPlaceHolders)
        {
            var vaultsToReturn = new List<Connection>();
            int cargoTotalPlaceHolders = 0;
            do
            {
                var placeHolderRoom = AddRoomToRandomOpenDoor(templateGenerator, placeHolderVault.RandomElement(), corridor1, 3);
                if (placeHolderRoom != null)
                {
                    vaultsToReturn.Add(placeHolderRoom);
                    cargoTotalPlaceHolders++;
                }
                else
                    break;
            } while (cargoTotalPlaceHolders < maxPlaceHolders);
            return vaultsToReturn;
        }

        public DoorInfo RandomDoor(TemplatedMapGenerator generator)
        {
            return generator.PotentialDoors[Game.Random.Next(generator.PotentialDoors.Count())];
        }

        public void GenerateLargeRooms(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate largeRoom1 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.largeconnectingvault1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate largeRoom2 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.largeconnectingvault2.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate corridor1 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>>();

            for (int i = 0; i < 10; i++)
            {
                allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(2, RoomTemplateUtilities.BuildRandomRectangularRoom(6, 14, 6, 14, 4, 4)));
            }

            allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom1));
            allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom2));

            PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 0, 0);
        }

        public void GenerateClosePackedSquareRooms(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate largeRoom = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.square_4way3.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate smallRoom = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.square_4way2.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate tinyRoom = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.square_4way.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate corridor1 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>>();

            //allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom1));
            //allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom2));

            int numberOfLargeRooms = (int)Math.Ceiling(numberOfRandomRooms / 2.0);
            int numberOfMediumRooms = (int)Math.Ceiling(numberOfRandomRooms / 6.0);
            int numberOfSmallRooms = numberOfRandomRooms - numberOfLargeRooms - numberOfMediumRooms;

            PlaceRandomConnectedRooms(templateGenerator, numberOfLargeRooms, largeRoom, corridor1, 0, 0);
            PlaceRandomConnectedRooms(templateGenerator, numberOfMediumRooms, smallRoom, corridor1, 0, 0);
            PlaceRandomConnectedRooms(templateGenerator, numberOfSmallRooms, tinyRoom, corridor1, 0, 0);
        }

        public void GenerateClosePackedSquareRooms2(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate largeRoom = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.square_4way3.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate largeRoom2 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.square_2way3.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate smallRoom = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.square_4way2.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate tinyRoom = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.square_4way.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate corridor1 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>>();

            //allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom1));
            //allRoomsToPlace.Add(new Tuple<int, RoomTemplate>(4, largeRoom2));

            int numberOfLargeRooms = (int)Math.Ceiling(numberOfRandomRooms / 2.0);
            int numberOfMediumRooms = (int)Math.Ceiling(numberOfRandomRooms / 6.0);
            int numberOfSmallRooms = numberOfRandomRooms - numberOfLargeRooms - numberOfMediumRooms;

            PlaceRandomConnectedRooms(templateGenerator, numberOfLargeRooms, largeRoom, corridor1, 0, 0);
            PlaceRandomConnectedRooms(templateGenerator, numberOfLargeRooms, largeRoom2, corridor1, 0, 0);
            PlaceRandomConnectedRooms(templateGenerator, numberOfMediumRooms, smallRoom, corridor1, 0, 0);
            PlaceRandomConnectedRooms(templateGenerator, numberOfSmallRooms, tinyRoom, corridor1, 0, 0);
        }

        public void BuildTXShapedRooms(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate lshapeRoom = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.lshape2.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate lshapeAsymmetric = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.lshape_asymmetric2.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate tshape = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.tshape1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate xshape = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.xshape1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate corridor1 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>> { 
                new Tuple<int, RoomTemplate>(100, lshapeRoom),
                new Tuple<int, RoomTemplate>(100, lshapeAsymmetric),
                new Tuple<int, RoomTemplate>(100, tshape),
                new Tuple<int, RoomTemplate>(100, xshape) };

            PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 4, 6);
        }

        public void BuildTXShapedRoomsBig(TemplatedMapGenerator templateGenerator, int numberOfRandomRooms)
        {
            RoomTemplate lshapeRoom = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.lshape3.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate lshapeAsymmetric = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.lshape_asymmetric3.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate tshape = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.tshape2.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate xshape = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.xshape2.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate corridor1 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            var allRoomsToPlace = new List<Tuple<int, RoomTemplate>> { 
                new Tuple<int, RoomTemplate>(100, lshapeRoom),
                new Tuple<int, RoomTemplate>(100, lshapeAsymmetric),
                new Tuple<int, RoomTemplate>(100, tshape),
                new Tuple<int, RoomTemplate>(100, xshape) };

            PlaceRandomConnectedRooms(templateGenerator, numberOfRandomRooms, allRoomsToPlace, corridor1, 4, 6);
        }

        public void AddStandardPlaceholderVaults(LevelInfo medicalInfo, TemplatedMapGenerator templateGenerator, int maxPlaceHolders)
        {
            RoomTemplate armory1 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.armory1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate armory2 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.armory2.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate armory3 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.armory3.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
            RoomTemplate corridor1 = new RoomTemplateLoader("RogueBasin.bin.Debug.vaults.corridortemplate3x1.room", StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();

            medicalInfo.ReplaceableVaultConnections.AddRange(
                AddReplaceableVaults(templateGenerator, corridor1, new List<RoomTemplate> { armory1, armory2, armory3 }, maxPlaceHolders));
        }

        public void ReplaceAllsDoorsWithFloor(LevelInfo levelInfo)
        {
            levelInfo.LevelGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);
            //Remove doors
            levelInfo.LevelGenerator.ReplaceConnectedDoorsWithTerrain(RoomTemplateTerrain.Floor);
        }

        public void ReplaceUnconnectedDoorsWithWalls(LevelInfo levelInfo)
        {
            levelInfo.LevelGenerator.ReplaceUnconnectedDoorsWithTerrain(RoomTemplateTerrain.Wall);
        }
    }
}
