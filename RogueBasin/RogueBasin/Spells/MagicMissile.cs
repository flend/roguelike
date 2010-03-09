using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Spells
{
    public class MagicMissile : Spell
    {
        public override bool DoSpell(Point target)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //Check the target is within FOV
            
            //Get the FOV from Dungeon (this also updates the map creature FOV state)
            TCODFov currentFOV = Game.Dungeon.CalculateCreatureFOV(player);

            //Is the target in FOV
            if (!currentFOV.CheckTileFOV(target.x, target.y))
            {
                LogFile.Log.LogEntryDebug("Target out of FOV", LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Can't target out of sight.");

                return false;
            }

            //Check there is a monster at target
            SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, target);

            //Is there no monster here? If so, then attack it
            if (squareContents.monster != null)
            {
                LogFile.Log.LogEntryDebug("Firing magic missile", LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Magic Missile!");
                player.AttackMonster(squareContents.monster);

                //Subtract MP

                return true;
            }

            Game.MessageQueue.AddMessage("No target for magic missile.");
            LogFile.Log.LogEntryDebug("No monster to target for Magic Missile", LogDebugLevel.Medium);
            return false;
        }

        public override int MPCost()
        {
            return 2;
        }

        public override bool NeedsTarget()
        {
            return true;
        }

        public override string SpellName()
        {
            return "Magic Missile";
        }

        public override string Abbreviation()
        {
            return "MM";
        }
    }
}
