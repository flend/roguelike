using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    public class FeaturesToCreaturesTrigger : DungeonSquareTrigger
    {
        private List<Feature> featuresToRemove;
        private Dictionary<Location, Monster> creaturesToPlace;

        public FeaturesToCreaturesTrigger(List<Feature> featuresToRemove, Dictionary<Location, Monster> creaturesToPlace)
        {
            this.featuresToRemove = featuresToRemove;
            this.creaturesToPlace = creaturesToPlace;
        }

        public override bool CheckTrigger(int level, Point mapLocation)
        {
            //Remove features
            //Do first, because we probably plan to place monsters on top of features
            foreach (var feature in featuresToRemove)
            {
                Game.Dungeon.RemoveFeature(feature);
            }

            foreach(var kv in creaturesToPlace) {
                var loc = kv.Key;
                var monster = kv.Value;
                Game.Dungeon.AddMonsterDynamic(monster, loc.Level, loc.MapCoord);
            }
            
            return true;
        }
    }
}
