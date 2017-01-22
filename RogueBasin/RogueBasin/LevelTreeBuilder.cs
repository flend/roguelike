using GraphMap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public class LevelTreeBuilder
    {
        private LevelRegister levelRegister;

        private bool quickLevelGen = false;
        private int startLevel;
        private ImmutableDictionary<int, int> levelDifficulties;
        private ConnectivityMap levelLinks;

        public LevelTreeBuilder(int startLevel, LevelRegister levelRegister, bool quickLevelGen)
        {
            this.startLevel = startLevel;
            this.levelRegister = levelRegister;
            this.quickLevelGen = quickLevelGen;
        }

        private IEnumerable<LevelAndDifficulty> GetLevelDifficulties(IEnumerable<int> levelDifficultyOrder)
        {
            //This should come from which quests are chosen
            var levelsAndDifficulties = levelDifficultyOrder.Select((item, index) => new LevelAndDifficulty(item, index));
            
            return levelsAndDifficulties;
        }

        public ImmutableDictionary<int, int> LevelDifficulties
        {
            get
            {
                if (levelDifficulties == null)
                {
                    GenerateLevelLinks();
                }

                return levelDifficulties;
            }
        }

        public ConnectivityMap LevelLinks
        {
            get
            {
                if (levelLinks == null)
                {
                    GenerateLevelLinks();
                }
                return levelLinks;
            }
        }

        /// <summary>
        /// Build a level->level map showing how the levels are connected
        /// </summary>
        public ConnectivityMap GenerateLevelLinks()
        {
            levelLinks = new ConnectivityMap();

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
            
            var difficultyOrderer = new DifficultyOrdering(levelRegister.DifficultyGraph);
            var levelDifficultyOrder = difficultyOrderer.GetLevelsInAscendingDifficultyOrder();          

            //Add easiest two levels as start connection
            var medicalLevelId = levelDifficultyOrder.ElementAt(0);
            var lowerAtriumLevelId = levelDifficultyOrder.ElementAt(1);

            levelLinks.AddRoomConnection(new Connection(medicalLevelId, lowerAtriumLevelId));

            //This feels kinda misplaced

            if (!quickLevelGen)
            {
                //Create levels in order of difficulty

                //Ordered in increasing difficulty
                var levelsAndDifficultiesFull = GetLevelDifficulties(levelDifficultyOrder);
                var levelsAndDifficultiesWithoutMedicalAndLowerAtrium = levelsAndDifficultiesFull.Skip(2);
                var levelsAndDifficultiesWithoutMedicalAndLowerAtriumAndMostDifficultLevel = levelsAndDifficultiesWithoutMedicalAndLowerAtrium.SkipLast(1);

                var mostDifficultLevelAndDifficulty = levelsAndDifficultiesFull.TakeLast(1);

                //Pick terminuses (all levels except most difficult and lower atrium)
                //Note that shuffle now has an implicit toList() which stops multiple evaluations giving different results
                var terminusShuffle = levelsAndDifficultiesWithoutMedicalAndLowerAtriumAndMostDifficultLevel.Shuffle();

                var numberOfTerminii = Game.Random.Next(2) + 2;
                var subTerminusNodes = terminusShuffle.Take(numberOfTerminii);

                //Add most difficult level as terminus
                var terminusNodes = subTerminusNodes.Union(mostDifficultLevelAndDifficulty);

                //Fragile way of removing lowerAtrium
                var remainingNodes = levelsAndDifficultiesWithoutMedicalAndLowerAtrium.Except(terminusNodes);

                foreach (var level in remainingNodes)
                {
                    var parentNodes = terminusNodes.Where(parent => parent.difficulty > level.difficulty);
                    var parentNodesShuffled = parentNodes.Shuffle();
                    var numberOfParents = Math.Min(1 + Game.Random.Next(3), parentNodes.Count());

                    for (int p = 0; p < numberOfParents; p++)
                    {
                        var parentLevel = parentNodesShuffled.ElementAt(p);
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
                    levelLinks.AddRoomConnection(new Connection(lowerAtriumLevelId, level.level));
                }

                //TODO: try to balance the tree a bit, otherwise pathological situations (one long branch) are quite likely
            }

            levelDifficulties = levelDifficultyOrder.Select((l, i) => new { l, i }).ToImmutableDictionary(x => x.l, x => x.i);

            return levelLinks;
        }
    }
}
