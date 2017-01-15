using GraphMap;
using System;
using System.Collections;
using System.Collections.Generic;
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

                //NOTE THIS ALGORITHM REQUIRES THAT GetLevelDifficulties gives back unique difficulties in lower-is-easier form! TODO: fix
                var levelsAndDifficultiesFull = GetLevelDifficulties(levelDifficultyOrder);
                var levelsAndDifficultiesAscending = levelsAndDifficultiesFull.Except(EnumerableEx.Return(new LevelAndDifficulty(medicalLevelId, 0)));

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
                var remainingNodes = levelsAndDifficulties.Except(terminusNodes).Except(EnumerableEx.Return(new LevelAndDifficulty(lowerAtriumLevelId, maxDifficulty - 1)));

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
                    levelLinks.AddRoomConnection(new Connection(lowerAtriumLevelId, level.level));
                }

                //TODO: try to balance the tree a bit, otherwise pathological situations (one long branch) are quite likely
            }

            return levelLinks;
        }
    }
}
