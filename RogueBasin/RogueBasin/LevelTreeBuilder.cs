using GraphMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public class LevelTreeBuilder
    {
        public const int medicalLevel = 0;
        public const int lowerAtriumLevel = 1;
        public const int scienceLevel = 2;
        public const int storageLevel = 3;
        public const int flightDeck = 4;
        public const int reactorLevel = 5;
        public const int arcologyLevel = 6;
        public const int commercialLevel = 7;
        public const int computerCoreLevel = 8;
        public const int bridgeLevel = 9;

        private LevelRegister levelRegister;

        private bool quickLevelGen = false;

        public LevelTreeBuilder(LevelRegister levelRegister, bool quickLevelGen)
        {
            this.levelRegister = levelRegister;
            this.quickLevelGen = quickLevelGen;
        }

        private IEnumerable<LevelAndDifficulty> GetLevelDifficulties()
        {
            //This should come from which quests are chosen
            var levelsAndDifficulties = new List<LevelAndDifficulty> {
                    new LevelAndDifficulty(flightDeck, 9),
                    new LevelAndDifficulty(bridgeLevel, 8),
                    new LevelAndDifficulty(reactorLevel, 7),
                    new LevelAndDifficulty(computerCoreLevel, 6),
                    new LevelAndDifficulty(arcologyLevel, 5),
                    new LevelAndDifficulty(scienceLevel, 4),
                    new LevelAndDifficulty(storageLevel, 3),
                    new LevelAndDifficulty(commercialLevel, 2),
                    new LevelAndDifficulty(lowerAtriumLevel, 1),
                    new LevelAndDifficulty(medicalLevel, 0)
                };

            return levelsAndDifficulties;
        }

        /// <summary>
        /// Build a level->level map showing how the levels are connected
        /// </summary>
        public ConnectivityMap GenerateLevelLinks()
        {
            var levelLinks = new ConnectivityMap();

            //Order of the main quest (in future, this will be generic)

            //Escape pod (flight deck) [requires active self-destruct]
            //Activate self-destruct (bridge) [requires enable self-destruct]
            //Enable self-destruct (reactor) [requires computer cores destroyed]
            //Destroy computer cores (computer-core) [no pre-requisite]
            //Bridge lock (any level place captain's cabin) [no pre-requisite]
            //Computer core lock (arcology) [no pre-requisite]
            //Arcology lock (any level - place bioware) [no pre-requisite]
            //Arcology lock (any level) [antennae disabled]
            //Antennae (science / storage) [no pre-requisite]

            //Lower Atrium (may be out-of-sequence)
            //Medical

            //Level order (last to first)

            //flight deck
            //bridge
            //reactor
            //computer-core
            //arcology
            //science
            //storage

            //lower atrium
            //medical

            //non-difficulty sequenced:

            //commercial (close to atrium)

            //Player starts in medical which links to the lower atrium
            levelLinks.AddRoomConnection(new Connection(medicalLevel, lowerAtriumLevel));

            //This feels kinda misplaced

            if (!quickLevelGen)
            {
                //Create levels in order of difficulty
                //NOTE THIS ALGORITHM REQUIRES THAT GetLevelDifficulties gives back unique difficulties in lower-is-easier form! TODO: fix
                var levelsAndDifficultiesFull = GetLevelDifficulties();
                var levelsAndDifficultiesAscending = levelsAndDifficultiesFull.Except(EnumerableEx.Return(new LevelAndDifficulty(medicalLevel, 0)));

                var maxDifficulty = levelsAndDifficultiesAscending.Max(l => l.difficulty);
                var levelsAndDifficulties = levelsAndDifficultiesAscending.Select(l => new LevelAndDifficulty(l.level, maxDifficulty - l.difficulty));

                //Pick terminuses (all levels except most difficult and lower atrium)
                //Note that shuffle now has an implicit toList() which stops multiple evaluations giving different results
                var terminusShuffle = levelsAndDifficulties.Skip(1).Take(7).Shuffle();

                var numberOfTerminii = Game.Random.Next(2) + 2;
                var subTerminusNodes = terminusShuffle.Take(numberOfTerminii);

                //Add most difficult level as terminus
                var terminusNodes = subTerminusNodes.Union(EnumerableEx.Return(levelsAndDifficulties.ElementAt(0)));

                //Fragile way of removing lowerAtrium
                var remainingNodes = levelsAndDifficulties.Except(terminusNodes).Except(EnumerableEx.Return(new LevelAndDifficulty(lowerAtriumLevel, maxDifficulty - 1)));

                foreach (var level in remainingNodes)
                {
                    var parentNodes = terminusNodes.Where(parent => parent.difficulty < level.difficulty).Shuffle();
                    var numberOfParents = Math.Min(Game.Random.Next(3), parentNodes.Count());

                    for (int p = 0; p < numberOfParents; p++)
                    {
                        var parentLevel = parentNodes.ElementAt(p);
                        //Pick a parent from current terminusNodes, which is less difficult
                        levelLinks.AddRoomConnection(new Connection(level.level, parentLevel.level));
                        //Remove parent from terminii
                        terminusNodes = terminusNodes.Except(EnumerableEx.Return(parentLevel));
                    }

                    //Add this level

                    terminusNodes = terminusNodes.Union(EnumerableEx.Return(level));
                }

                //Connect all terminii to lower atrium
                foreach (var level in terminusNodes)
                {
                    levelLinks.AddRoomConnection(new Connection(lowerAtriumLevel, level.level));
                }

                //TODO: try to balance the tree a bit, otherwise pathological situations (one long branch) are quite likely
            }

            return levelLinks;
        }
    }
}
