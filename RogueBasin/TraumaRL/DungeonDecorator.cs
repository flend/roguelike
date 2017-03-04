using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraumaRL
{
    class DungeonDecorator
    {
        private QuestMapBuilder builder;
        private MapState mapState;

        Dictionary<LevelType, List<DecorationFeatureDetails.DecorationFeatures>> featuresByLevel;

        public DungeonDecorator(MapState state, QuestMapBuilder builder)
        {
            this.mapState = state;
            this.builder = builder;

            SetupFeatures();
        }

        private void SetupFeatures()
        {

            featuresByLevel = new Dictionary<LevelType, List<DecorationFeatureDetails.DecorationFeatures>>();

            featuresByLevel[LevelType.MedicalLevel] = new List<DecorationFeatureDetails.DecorationFeatures>
            {
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                DecorationFeatureDetails.DecorationFeatures.Bone,
                DecorationFeatureDetails.DecorationFeatures.Skeleton,
                DecorationFeatureDetails.DecorationFeatures.Instrument1,
                DecorationFeatureDetails.DecorationFeatures.Instrument2,
                DecorationFeatureDetails.DecorationFeatures.Instrument3,
                DecorationFeatureDetails.DecorationFeatures.MedicalAutomat,
                DecorationFeatureDetails.DecorationFeatures.CoffeePC,
                DecorationFeatureDetails.DecorationFeatures.DesktopPC,
                DecorationFeatureDetails.DecorationFeatures.Chair1,
                DecorationFeatureDetails.DecorationFeatures.Chair2,
                DecorationFeatureDetails.DecorationFeatures.Stool,
                DecorationFeatureDetails.DecorationFeatures.Plant1,
                DecorationFeatureDetails.DecorationFeatures.Plant2,
                DecorationFeatureDetails.DecorationFeatures.Plant3,
                DecorationFeatureDetails.DecorationFeatures.CleaningDevice,
                DecorationFeatureDetails.DecorationFeatures.WheelChair,
                DecorationFeatureDetails.DecorationFeatures.Bin
            };

            featuresByLevel[LevelType.LowerAtriumLevel] = new List<DecorationFeatureDetails.DecorationFeatures>
            {
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                DecorationFeatureDetails.DecorationFeatures.Bone,
                DecorationFeatureDetails.DecorationFeatures.Skeleton,
                DecorationFeatureDetails.DecorationFeatures.Instrument1,
                DecorationFeatureDetails.DecorationFeatures.Instrument2,
                DecorationFeatureDetails.DecorationFeatures.Instrument3,
                DecorationFeatureDetails.DecorationFeatures.Plant1,
                DecorationFeatureDetails.DecorationFeatures.Plant2,
                DecorationFeatureDetails.DecorationFeatures.Plant3,
                DecorationFeatureDetails.DecorationFeatures.Chair1,
                DecorationFeatureDetails.DecorationFeatures.Chair1,
                DecorationFeatureDetails.DecorationFeatures.Safe1,
                DecorationFeatureDetails.DecorationFeatures.Safe2,
                DecorationFeatureDetails.DecorationFeatures.Statue1,
                DecorationFeatureDetails.DecorationFeatures.Statue2,
                DecorationFeatureDetails.DecorationFeatures.Statue3,
                DecorationFeatureDetails.DecorationFeatures.Statue4,
                DecorationFeatureDetails.DecorationFeatures.AutomatMachine,
                DecorationFeatureDetails.DecorationFeatures.Bin
            };

            featuresByLevel[LevelType.ScienceLevel] = new List<DecorationFeatureDetails.DecorationFeatures>
            {
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                DecorationFeatureDetails.DecorationFeatures.Bone,
                DecorationFeatureDetails.DecorationFeatures.Skeleton,
                DecorationFeatureDetails.DecorationFeatures.CleaningDevice,
                DecorationFeatureDetails.DecorationFeatures.Instrument1,
                DecorationFeatureDetails.DecorationFeatures.Instrument2,
                DecorationFeatureDetails.DecorationFeatures.Instrument3,
                DecorationFeatureDetails.DecorationFeatures.MedicalAutomat,
                DecorationFeatureDetails.DecorationFeatures.CoffeePC,
                DecorationFeatureDetails.DecorationFeatures.DesktopPC,
                DecorationFeatureDetails.DecorationFeatures.Chair1,
                DecorationFeatureDetails.DecorationFeatures.Chair2,
                DecorationFeatureDetails.DecorationFeatures.Stool,
                DecorationFeatureDetails.DecorationFeatures.Plant1,
                DecorationFeatureDetails.DecorationFeatures.Plant2,
                DecorationFeatureDetails.DecorationFeatures.Plant3,
                DecorationFeatureDetails.DecorationFeatures.WheelChair,
                DecorationFeatureDetails.DecorationFeatures.Screen1,
                DecorationFeatureDetails.DecorationFeatures.Screen2,
                DecorationFeatureDetails.DecorationFeatures.Screen8,
                DecorationFeatureDetails.DecorationFeatures.Screen9
            };

            featuresByLevel[LevelType.StorageLevel] = new List<DecorationFeatureDetails.DecorationFeatures>
            {
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                DecorationFeatureDetails.DecorationFeatures.Bone,
                DecorationFeatureDetails.DecorationFeatures.Skeleton,
                DecorationFeatureDetails.DecorationFeatures.Instrument1,
                DecorationFeatureDetails.DecorationFeatures.Instrument2,
                DecorationFeatureDetails.DecorationFeatures.Crate,
                DecorationFeatureDetails.DecorationFeatures.Safe1,
                DecorationFeatureDetails.DecorationFeatures.Safe2,
                DecorationFeatureDetails.DecorationFeatures.Machine,
                DecorationFeatureDetails.DecorationFeatures.Machine2,
                DecorationFeatureDetails.DecorationFeatures.MachinePart1,
                DecorationFeatureDetails.DecorationFeatures.MachinePart2,
                DecorationFeatureDetails.DecorationFeatures.MachinePart3,
                DecorationFeatureDetails.DecorationFeatures.Screen1,
                DecorationFeatureDetails.DecorationFeatures.Screen2,
                DecorationFeatureDetails.DecorationFeatures.Screen3,
                DecorationFeatureDetails.DecorationFeatures.Screen4
            };

            featuresByLevel[LevelType.FlightDeck] = new List<DecorationFeatureDetails.DecorationFeatures>
                    {
                        DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                        DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                        DecorationFeatureDetails.DecorationFeatures.Bone,
                        DecorationFeatureDetails.DecorationFeatures.Skeleton,
                        DecorationFeatureDetails.DecorationFeatures.Instrument1,
                        DecorationFeatureDetails.DecorationFeatures.Instrument2,
                DecorationFeatureDetails.DecorationFeatures.Crate,
                DecorationFeatureDetails.DecorationFeatures.Machine,
                DecorationFeatureDetails.DecorationFeatures.Machine2,
                DecorationFeatureDetails.DecorationFeatures.Computer1,
                DecorationFeatureDetails.DecorationFeatures.Computer2,
                DecorationFeatureDetails.DecorationFeatures.Computer3,
                        DecorationFeatureDetails.DecorationFeatures.MachinePart1,
                DecorationFeatureDetails.DecorationFeatures.MachinePart2,
                DecorationFeatureDetails.DecorationFeatures.MachinePart3,
                        DecorationFeatureDetails.DecorationFeatures.Screen1,
                        DecorationFeatureDetails.DecorationFeatures.Screen2,
                        DecorationFeatureDetails.DecorationFeatures.Screen3,
                        DecorationFeatureDetails.DecorationFeatures.Pillar1,
                DecorationFeatureDetails.DecorationFeatures.Pillar2,
                        DecorationFeatureDetails.DecorationFeatures.Pillar3,
                        DecorationFeatureDetails.DecorationFeatures.Screen8
                    };

            featuresByLevel[LevelType.ReactorLevel] = new List<DecorationFeatureDetails.DecorationFeatures>
                    {
                        DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                        DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                        DecorationFeatureDetails.DecorationFeatures.Bone,
                        DecorationFeatureDetails.DecorationFeatures.Skeleton,
                        DecorationFeatureDetails.DecorationFeatures.Instrument1,
                        DecorationFeatureDetails.DecorationFeatures.Instrument2,
                DecorationFeatureDetails.DecorationFeatures.EggChair,
                DecorationFeatureDetails.DecorationFeatures.Machine,
                DecorationFeatureDetails.DecorationFeatures.Machine2,
                        DecorationFeatureDetails.DecorationFeatures.MachinePart1,
                DecorationFeatureDetails.DecorationFeatures.MachinePart2,
                DecorationFeatureDetails.DecorationFeatures.MachinePart3,
                DecorationFeatureDetails.DecorationFeatures.Computer1,
                DecorationFeatureDetails.DecorationFeatures.Computer2,
                DecorationFeatureDetails.DecorationFeatures.Computer3,
                        DecorationFeatureDetails.DecorationFeatures.Screen1,
                        DecorationFeatureDetails.DecorationFeatures.Screen2,
                        DecorationFeatureDetails.DecorationFeatures.Screen3,
                        DecorationFeatureDetails.DecorationFeatures.Screen4,
                        DecorationFeatureDetails.DecorationFeatures.Screen6,
                        DecorationFeatureDetails.DecorationFeatures.Screen7,
                        DecorationFeatureDetails.DecorationFeatures.Screen8
                    };

            featuresByLevel[LevelType.ArcologyLevel] = new List<DecorationFeatureDetails.DecorationFeatures>
                    {
                        DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                        DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                        DecorationFeatureDetails.DecorationFeatures.Bone,
                        DecorationFeatureDetails.DecorationFeatures.Skeleton,
                        DecorationFeatureDetails.DecorationFeatures.Instrument1,
                        DecorationFeatureDetails.DecorationFeatures.Instrument2,
                        DecorationFeatureDetails.DecorationFeatures.Egg1,
                DecorationFeatureDetails.DecorationFeatures.Egg2,
                DecorationFeatureDetails.DecorationFeatures.Egg3,
                DecorationFeatureDetails.DecorationFeatures.Spike,
                DecorationFeatureDetails.DecorationFeatures.CorpseinGoo,
                DecorationFeatureDetails.DecorationFeatures.Machine,
                DecorationFeatureDetails.DecorationFeatures.Machine2,
                        DecorationFeatureDetails.DecorationFeatures.MachinePart1,
                DecorationFeatureDetails.DecorationFeatures.MachinePart2,
                DecorationFeatureDetails.DecorationFeatures.MachinePart3
              
                    };

            featuresByLevel[LevelType.CommercialLevel] = new List<DecorationFeatureDetails.DecorationFeatures>
                    {
                        DecorationFeatureDetails.DecorationFeatures.HumanCorpse,
                        DecorationFeatureDetails.DecorationFeatures.HumanCorpse2,
                        DecorationFeatureDetails.DecorationFeatures.Bone,
                        DecorationFeatureDetails.DecorationFeatures.Skeleton,
                        DecorationFeatureDetails.DecorationFeatures.Instrument1,
                        DecorationFeatureDetails.DecorationFeatures.Instrument2,
                        DecorationFeatureDetails.DecorationFeatures.Crate,
                        DecorationFeatureDetails.DecorationFeatures.Safe1,
                DecorationFeatureDetails.DecorationFeatures.Safe2,
                        DecorationFeatureDetails.DecorationFeatures.MachinePart1,
                DecorationFeatureDetails.DecorationFeatures.MachinePart2,
                DecorationFeatureDetails.DecorationFeatures.MachinePart3,
                DecorationFeatureDetails.DecorationFeatures.ShopAutomat1,
                DecorationFeatureDetails.DecorationFeatures.ShopAutomat2,
                DecorationFeatureDetails.DecorationFeatures.Statue1,
                DecorationFeatureDetails.DecorationFeatures.Statue2,
                DecorationFeatureDetails.DecorationFeatures.Statue3,
                DecorationFeatureDetails.DecorationFeatures.Statue4,
                DecorationFeatureDetails.DecorationFeatures.AutomatMachine,
                DecorationFeatureDetails.DecorationFeatures.Plant1,
                DecorationFeatureDetails.DecorationFeatures.Plant2,
                DecorationFeatureDetails.DecorationFeatures.Plant3,
                DecorationFeatureDetails.DecorationFeatures.Pillar1,
                DecorationFeatureDetails.DecorationFeatures.Pillar2,
                        DecorationFeatureDetails.DecorationFeatures.Pillar3,
                DecorationFeatureDetails.DecorationFeatures.CleaningDevice,
                DecorationFeatureDetails.DecorationFeatures.Bin
              
                    };

            featuresByLevel[LevelType.ComputerCoreLevel] = featuresByLevel[LevelType.ReactorLevel];

            featuresByLevel[LevelType.BridgeLevel] = featuresByLevel[LevelType.FlightDeck];

        }

        public void AddDecorationFeatures()
        {
            var mapInfo = mapState.MapInfo;

            foreach (var kv in mapState.LevelGraph.LevelInfo)
            {
                var thisLevel = kv.Key;
                var thisLevelInfo = kv.Value;

                var roomsInThisLevel = mapInfo.GetRoomIndicesForLevel(thisLevel);
                roomsInThisLevel = mapInfo.FilterOutCorridors(roomsInThisLevel);

                double chanceToSkip = 0.5;
                double avConcentration = 0.1;
                double stdConcentration = 0.02;

                double featureAv = 10;
                double featureStd = 100;

                if (!featuresByLevel.ContainsKey(thisLevelInfo.LevelType))
                    continue;

                foreach (var room in roomsInThisLevel)
                {
                    //if (Gaussian.BoxMuller(0, 1) < chanceToSkip)
                    //  continue;

                    //Bias rooms towards one or two types
                    var featuresAndWeights = featuresByLevel[thisLevelInfo.LevelType].Select(f => new Tuple<int, DecorationFeatureDetails.Decoration>((int)Math.Abs(Gaussian.BoxMuller(featureAv, featureStd)), DecorationFeatureDetails.decorationFeatures[f]));

                    var thisRoom = mapInfo.Room(room);
                    var thisRoomArea = thisRoom.Room.Width * thisRoom.Room.Height;

                    var numberOfFeatures = (int)Math.Abs(Gaussian.BoxMuller(thisRoomArea * avConcentration, thisRoomArea * stdConcentration));

                    AddStandardDecorativeFeaturesToRoomUsingGrid(mapInfo, room, numberOfFeatures, featuresAndWeights);
                }
            }
        }


        private void AddStandardDecorativeFeaturesToRoomUsingGrid(MapInfo mapInfo, int roomId, int featuresToPlace, IEnumerable<Tuple<int, DecorationFeatureDetails.Decoration>> decorationDetails)
        {
            var thisRoom = mapInfo.Room(roomId);
            var floorPoints = RoomTemplateUtilities.GetGridFromRoom(thisRoom.Room, 2, 1, 0.5);
            var floorPointsToUse = floorPoints.Shuffle().Take(featuresToPlace);
            AddStandardDecorativeFeaturesToRoom(mapInfo, roomId, floorPointsToUse, decorationDetails);
        }

        private void AddStandardDecorativeFeaturesToRoom(MapInfo mapInfo, int roomId, IEnumerable<RogueBasin.Point> points, IEnumerable<Tuple<int, DecorationFeatureDetails.Decoration>> decorationDetails)
        {
            if (points.Count() == 0)
                return;

            var featuresObjectsDetails = points.Select(p => new Tuple<RogueBasin.Point, DecorationFeatureDetails.Decoration>
                (p, Utility.ChooseItemFromWeights<DecorationFeatureDetails.Decoration>(decorationDetails)));
            var featureObjectsToPlace = featuresObjectsDetails.Select(dt => new Tuple<RogueBasin.Point, Feature>
                (dt.Item1, new RogueBasin.Features.StandardDecorativeFeature(dt.Item2.representation, dt.Item2.colour, dt.Item2.isBlocking)));

            var featuresPlaced = mapInfo.Populator.AddFeaturesToRoom(mapInfo, roomId, featureObjectsToPlace);
            LogFile.Log.LogEntryDebug("Placed " + featuresPlaced + " standard decorative features in room " + roomId, LogDebugLevel.Medium);
        }
    }
}
