namespace RogueBasin
{
    public abstract class LevelBuilder
    {
        public abstract LevelInfo GenerateLevel(int levelNo);
        public abstract LevelInfo CompleteLevel();
    }
}
