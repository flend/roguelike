using System.Collections.Generic;

namespace RogueBasin.Triggers
{
    public class FeaturesToCreaturesTrigger : DungeonSquareTrigger
    {
        private readonly List<Feature> featuresToRemove;
        private readonly Dictionary<Point, Monster> creaturesToPlace;
        private readonly int roomId;

        public FeaturesToCreaturesTrigger(List<Feature> featuresToRemove, Dictionary<Point, Monster> creaturesToPlace, int roomId)
        {
            this.featuresToRemove = featuresToRemove;
            this.creaturesToPlace = creaturesToPlace;
            this.roomId = roomId;
        }

        public override bool CheckTrigger(int level, Point mapLocation)
        {
            if(IsTriggered())
            {
                return false;
            }

            Triggered = true;

            //Remove features
            //Do first, because we probably plan to place monsters on top of features
            foreach (var feature in featuresToRemove)
            {
                Game.Dungeon.RemoveFeature(feature);
            }

            foreach (var kv in creaturesToPlace) {
                var pointRelativeToRoom = kv.Key;
                var monster = kv.Value;

                var locationAbsolute = Game.Dungeon.MapState.MapInfo.RelativeRoomPointToLocation(roomId, pointRelativeToRoom);

                bool addSuccess = Game.Dungeon.AddMonsterDynamic(monster, locationAbsolute);

                if (addSuccess)
                {
                    //Point at the trigger
                    monster.SetHeadingToTarget(this.mapPosition);
                }
            }

            return true;
        }
    }
}
