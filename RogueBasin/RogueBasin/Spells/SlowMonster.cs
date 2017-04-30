namespace RogueBasin.Spells
{
    public class SlowMonster : Spell
    {
        public override bool DoSpell(Point target)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //Check the target is within FOV
            
            //Get the FOV from Dungeon (this also updates the map creature FOV state)
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(player);

            //Is the target in FOV
            if (!currentFOV.CheckTileFOV(target.x, target.y))
            {
                LogFile.Log.LogEntryDebug("Target out of FOV", LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Target is out of sight.");

                return false;
            }

            //Check there is a monster at target
            SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, target);

            //Is there no monster here? If so, then slow it
            if (squareContents.monster != null)
            {
                Monster targetM = squareContents.monster;


                LogFile.Log.LogEntryDebug("Slowing " + targetM.SingleDescription, LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Slow Monster!");

                //Check magic resistance
                bool monsterResisted = CheckMagicResistance(targetM);
                if (monsterResisted)
                    return true;

                //Add the slow monster effect
                int duration = 5 * Creature.turnTicks + Game.Random.Next(10 * Creature.turnTicks);
                targetM.AddEffect(new MonsterEffects.SlowDown(duration, targetM.Speed / 2));

                return true;
            }

            Game.MessageQueue.AddMessage("No target for slow monster.");
            LogFile.Log.LogEntryDebug("No monster to target for Slow Monster", LogDebugLevel.Medium);
            return false;
        }

        public override int MPCost()
        {
            return 4;
        }

        public override int GetRange()
        {
            return 6;
        }

        public override bool NeedsTarget()
        {
            return true;
        }

        public override string SpellName()
        {
            return "Slow Monster";
        }

        public override string Abbreviation()
        {
            return "SM";
        }

        internal override int GetRequiredMagic()
        {
            return 70;
        }

        internal override string MovieRoot()
        {
            return "spellslowmonster";
        }
    }
}
