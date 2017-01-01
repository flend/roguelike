using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System.Collections.Generic;
using System.Linq;

namespace DDRogueTest
{
    [TestClass]
    public class LevelRegisterTest
    {
        [TestMethod]
        public void SixLevelDifficultyGraphReturnedInCorrectOrder()
        {
            var register = new LevelRegister();
            register.RegisterNewLevel(new RequiredLevelInfo(LevelType.ArcologyLevel));
            register.RegisterNewLevel(new RequiredLevelInfo(LevelType.BridgeLevel));
            register.RegisterNewLevel(new RequiredLevelInfo(LevelType.CommercialLevel));
            register.RegisterNewLevel(new RequiredLevelInfo(LevelType.ComputerCoreLevel));
            register.RegisterNewLevel(new RequiredLevelInfo(LevelType.FlightDeck));
            register.RegisterNewLevel(new RequiredLevelInfo(LevelType.LowerAtriumLevel));

            register.RegisterAscendingDifficultyRelationship(0, 1);
            register.RegisterAscendingDifficultyRelationship(1, 2);
            register.RegisterAscendingDifficultyRelationship(1, 4);
            register.RegisterAscendingDifficultyRelationship(2, 3);
            register.RegisterAscendingDifficultyRelationship(3, 4);
            register.RegisterAscendingDifficultyRelationship(3, 5);
            register.RegisterAscendingDifficultyRelationship(4, 5);

            var difficultyOrdering = new DifficultyOrdering(register.DifficultyGraph.Graph);
            var orderedLevels = difficultyOrdering.GetLevelsInAscendingDifficultyOrder();

            CollectionAssert.AreEqual(new List<int> { 0, 1, 2, 3, 4, 5 }, orderedLevels.ToList());
        }
    }
}
