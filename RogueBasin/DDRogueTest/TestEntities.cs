using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueBasin;

namespace DDRogueTest
{
    class TestEntities
    {
        public class BlockingFeature : Feature
        {
            public BlockingFeature()
            {
                IsBlocking = true;
            }
        }

        public class NonBlockingFeature : Feature
        {
            public NonBlockingFeature() { }
        }

        public class TestMonster : MonsterFightAndRunAI
        {
            public TestMonster() { }

            public override int DamageBase()
            {
                return 0;
            }

            protected override int ClassMaxHitpoints()
            {
                return 0;
            }

            public override int CreatureCost()
            {
                return 0;
            }

            public override Monster NewCreatureOfThisType()
            {
                return new TestMonster();
            }

            public override string GroupDescription
            {
                get { return "Test monsters"; }
            }

            public override string SingleDescription
            {
                get { return "Test monster"; }
            }
        }
    }
}
