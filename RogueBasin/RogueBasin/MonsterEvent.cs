namespace RogueBasin
{
    public class MonsterEvent
    {
        public enum MonsterEventType {
            MonsterAttacksPlayer,
            MonsterSeenByPlayer,
            MonsterWokenByPlayer
        }

        public Monster Monster;
        public MonsterEventType EventType;

        public MonsterEvent(MonsterEventType eventType, Monster monster)
        {
            Monster = monster;
            EventType = eventType;
        }
    }
}
