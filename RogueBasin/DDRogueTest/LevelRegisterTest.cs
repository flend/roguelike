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
            register.GetIdForLevelType(new RequiredLevelInfo(LevelType.ArcologyLevel));
            register.GetIdForLevelType(new RequiredLevelInfo(LevelType.BridgeLevel));
            register.GetIdForLevelType(new RequiredLevelInfo(LevelType.CommercialLevel));
            register.GetIdForLevelType(new RequiredLevelInfo(LevelType.ComputerCoreLevel));
            register.GetIdForLevelType(new RequiredLevelInfo(LevelType.FlightDeck));
            register.GetIdForLevelType(new RequiredLevelInfo(LevelType.LowerAtriumLevel));

            register.RegisterAscendingDifficultyRelationship(0, 1);
            register.RegisterAscendingDifficultyRelationship(1, 2);
            register.RegisterAscendingDifficultyRelationship(1, 4);
            register.RegisterAscendingDifficultyRelationship(2, 3);
            register.RegisterAscendingDifficultyRelationship(3, 4);
            register.RegisterAscendingDifficultyRelationship(3, 5);
            register.RegisterAscendingDifficultyRelationship(4, 5);

            var difficultyOrdering = new DifficultyOrdering(register.DifficultyGraph);
            var orderedLevels = difficultyOrdering.GetLevelsInAscendingDifficultyOrder();

            CollectionAssert.AreEqual(new List<int> { 0, 1, 2, 3, 4, 5 }, orderedLevels.ToList());
        }

        [TestMethod]
        public void TestLevelIdCanBeRetrievedByRequiredLevelInfo()
        {
            var register = new LevelRegister();
            register.GetIdForLevelType(new RequiredLevelInfo(LevelType.ArcologyLevel));
            register.GetIdForLevelType(new RequiredLevelInfo(LevelType.BridgeLevel));

            Assert.AreEqual(0, register.GetIdForLevelType(new RequiredLevelInfo(LevelType.ArcologyLevel)).id);
            Assert.AreEqual(1, register.GetIdForLevelType(new RequiredLevelInfo(LevelType.BridgeLevel)).id);
        }
    }
}
