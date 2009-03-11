using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    public enum SimpleAIStates
    {
        RandomWalk,
        Pursuit
    }

    public abstract class MonsterSimpleAI : Monster
    {
        public SimpleAIStates AIState {get; set;}
        Creature currentTarget;

        public MonsterSimpleAI()
        {
            AIState = SimpleAIStates.RandomWalk;
            currentTarget = null;
        }
        /// <summary>
        /// Run the Simple AI actions
        /// </summary>
        public override void ProcessTurn()
        {
            //If in pursuit state, continue to pursue enemy until it is dead (or creature itself is killed) [no FOV used after initial target selected]
            
            //If in randomWalk state, look for new enemies in FOV.
            //Closest enemy becomes new target
            
            //If no targets, move randomly

            Random rand = Game.Random;


            if (AIState == SimpleAIStates.Pursuit)
            {
                //Pursuit state, continue chasing and attacking target

                //Check we have a valid target (may not after reload)
                if (currentTarget == null)
                {
                    AIState = SimpleAIStates.RandomWalk;
                }

                //Is target yet living?
                else if (currentTarget.Alive == false)
                {
                    //If not, go to non-chase state
                    AIState = SimpleAIStates.RandomWalk;
                }
                //Is target on another level (i.e. has escaped down the stairs)
                else if (currentTarget.LocationLevel != this.LocationLevel)
                {
                    AIState = SimpleAIStates.RandomWalk;
                }
                else
                {
                    //Otherwise continue to chase

                    ChaseCreature(currentTarget);
                }
            }
            
            if(AIState == SimpleAIStates.RandomWalk) {
                //RandomWalk state

                //Search an area of sightRadius on either side for creatures and check they are in the FOV

                Map currentMap = Game.Dungeon.Levels[LocationLevel];
                
                //Get the FOV from Dungeon (this also updates the map creature FOV state)
                TCODFov currentFOV = Game.Dungeon.CalculateCreatureFOV(this);
                //currentFOV.CalculateFOV(LocationMap.x, LocationMap.y, SightRadius);

                //Check for other creatures within this creature's FOV

                int xl = LocationMap.x - SightRadius;
                int xr = LocationMap.x + SightRadius;

                int yt = LocationMap.y - SightRadius;
                int yb = LocationMap.y + SightRadius;

                //If sight is infinite, check all the map
                if (SightRadius == 0)
                {
                    xl = 0;
                    xr = currentMap.width;
                    yt = 0;
                    yb = currentMap.height;
                }

                if (xl < 0)
                    xl = 0;
                if(xr >= currentMap.width)
                    xr = currentMap.width - 1;
                if (yt < 0)
                    yt = 0;
                if (yb >= currentMap.height)
                    yb = currentMap.height - 1;

                //List will contain monsters & player
                List<Creature> creaturesInFOV = new List<Creature>();

                foreach (Monster monster in Game.Dungeon.Monsters)
                {
                    //Same monster
                    if (monster == this)
                        continue;

                    //Not on the same level
                    if (monster.LocationLevel != this.LocationLevel)
                        continue;

                    //Not in FOV
                    if (!currentFOV.CheckTileFOV(monster.LocationMap.x, monster.LocationMap.y))
                        continue;

                    //Otherwise add to list of possible targets
                    creaturesInFOV.Add(monster);

                    LogFile.Log.LogEntryDebug(this.Representation + " spots " + monster.Representation, LogDebugLevel.Low);
                }

                //Check PC
                if (Game.Dungeon.Player.LocationLevel == this.LocationLevel)
                {
                    if (currentFOV.CheckTileFOV(Game.Dungeon.Player.LocationMap.x, Game.Dungeon.Player.LocationMap.y))
                    {
                        creaturesInFOV.Add(Game.Dungeon.Player);
                        LogFile.Log.LogEntryDebug(this.Representation + " spots " + Game.Dungeon.Player.Representation, LogDebugLevel.Low);
                    }
                }

                //If there are possible targets, find the closest and chase it
                //Otherwise continue to move randomly
                /*
                if (creaturesInFOV.Count > 0)
                {
                    
                    //Find the closest creature
                    Creature closestCreature = null;
                    double closestDistance = Double.MaxValue; //a long way

                    foreach (Creature creature in creaturesInFOV)
                    {
                        double distanceSq = Math.Pow(creature.LocationMap.x - this.LocationMap.x, 2) +
                            Math.Pow(creature.LocationMap.y - this.LocationMap.y, 2);

                        double distance = Math.Sqrt(distanceSq);

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestCreature = creature;
                        }
                    }


                    //Start chasing this creature
                    LogFile.Log.LogEntryDebug(this.Representation + " chases " + closestCreature.Representation, LogDebugLevel.Medium);
                    ChaseCreature(closestCreature);
                }*/

                //Current behaviour: only chase the PC
                if(creaturesInFOV.Contains(Game.Dungeon.Player)) {
                    Creature closestCreature = Game.Dungeon.Player;
                    //Start chasing this creature
                    LogFile.Log.LogEntryDebug(this.Representation + " chases " + closestCreature.Representation, LogDebugLevel.Low);
                    ChaseCreature(closestCreature);
                }

                else
                {
                    //Move randomly. If we walk into something attack it, but it does not become a new target

                    int direction = rand.Next(9);

                    int moveX = 0;
                    int moveY = 0;

                    moveX = direction / 3 - 1;
                    moveY = direction % 3 - 1;

                    //If we're not moving quit at this point, otherwise the target square will be the one we're in
                    if (moveX == 0 && moveY == 0)
                    {
                        return;
                    }

                    //Check this is a valid move
                    bool validMove = false;
                    Point newLocation = new Point(LocationMap.x + moveX, LocationMap.y + moveY);

                    validMove = Game.Dungeon.MapSquareCanBeEntered(LocationLevel, newLocation);

                    //Give up if this is not a valid move
                    if (!validMove)
                        return;

                    //Check if the square is occupied by a PC or monster
                    SquareContents contents = Game.Dungeon.MapSquareContents(LocationLevel, newLocation);
                    bool okToMoveIntoSquare = false;

                    if (contents.empty)
                    {
                        okToMoveIntoSquare = true;
                    }

                    if (contents.player != null)
                    {
                        //Attack the player
                        CombatResults result = AttackPlayer(contents.player);

                        if (result == CombatResults.DefenderDied)
                        {
                            //Bad news for the player here!
                            okToMoveIntoSquare = true;
                        }
                    }

                    if (contents.monster != null)
                    {
                        //Attack the monster
                        CombatResults result = AttackMonster(contents.monster);

                        if (result == CombatResults.DefenderDied)
                        {
                            okToMoveIntoSquare = true;
                        }
                    }

                    //Move if allowed
                    if (okToMoveIntoSquare)
                    {
                        LocationMap = newLocation;
                    }
                }
            }
        }

        private void ChaseCreature(Creature newTarget)
        {
            //Confirm this as current target
            currentTarget = newTarget;

            //Go into pursuit mode
            AIState = SimpleAIStates.Pursuit;

            //Find location of next step on the path towards them
            Point nextStep = Game.Dungeon.GetPathTo(this, newTarget);

            bool moveIntoSquare = true;

            //If this is the same as the target creature's location, we are adjacent and can attack
            if (nextStep.x == newTarget.LocationMap.x && nextStep.y == newTarget.LocationMap.y)
            {
                //Attack the monster
                //Ugly select here
                CombatResults result;

                if (newTarget == Game.Dungeon.Player)
                {
                    result = AttackPlayer(newTarget as Player);
                }
                else
                {
                    //It's a normal creature
                    result = AttackMonster(newTarget as Monster);
                }
                 

                //If we killed it, move into its square
                if (result != CombatResults.DefenderDied)
                {
                    moveIntoSquare = false;
                }
            }

            //Otherwise (or if the creature died), move towards it (or its corpse)
            if (moveIntoSquare)
            {
                LocationMap = nextStep;
            }
        }
        
        
        int toHitRoll;

        //Could be in Monster
        private int AttackCreatureWithModifiers(Creature player, int hitMod, int damBase, int damMod, int ACmod)
        {
            int attackToHit = hitModifier + hitMod;
            int attackDamageMod = damageModifier + damMod;

            int attackDamageBase;

            if (damBase > damageBase)
                attackDamageBase = damBase;
            else
                attackDamageBase = damageBase;

            int playerAC = player.ArmourClass() + ACmod;
            toHitRoll = Utility.d20() + attackToHit;

            if (toHitRoll >= playerAC)
            {
                //Hit - calculate damage
                int totalDamage = Utility.DamageRoll(attackDamageBase) + attackDamageMod;

                return totalDamage;
            }

            //Miss
            return 0;
        }


        public override CombatResults AttackPlayer(Player player)
        {
            //Recalculate combat stats if required
            if (this.RecalculateCombatStatsRequired)
                this.CalculateCombatStats();

            if (player.RecalculateCombatStatsRequired)
                player.CalculateCombatStats();

            //Calculate damage from a normal attack
            int damage = AttackCreatureWithModifiers(player, 0, 0, 0, 0);

            //Do we hit the player?
            if (damage > 0)
            {
                int monsterOrigHP = player.Hitpoints;

                player.Hitpoints -= damage;

                //Is the player dead, if so kill it?
                if (player.Hitpoints <= 0)
                {
                    Game.Dungeon.PlayerDeath();

                    //Debug string
                    string combatResultsMsg = "MvP ToHit: " + toHitRoll + " AC: " + player.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + player.Hitpoints + " killed";
                    Game.MessageQueue.AddMessage(combatResultsMsg);
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    return CombatResults.DefenderDied;
                }

                //Debug string
                string combatResultsMsg3 = "MvP ToHit: " + toHitRoll + " AC: " + player.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + player.Hitpoints + " injured";
                Game.MessageQueue.AddMessage(combatResultsMsg3);
                LogFile.Log.LogEntryDebug(combatResultsMsg3, LogDebugLevel.Medium);

                return CombatResults.NeitherDied;
            }

            //Miss
            string combatResultsMsg2 = "MvP ToHit: " + toHitRoll + " AC: " + player.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + player.Hitpoints + " miss";
            Game.MessageQueue.AddMessage(combatResultsMsg2);
            LogFile.Log.LogEntryDebug(combatResultsMsg2, LogDebugLevel.Medium);

            return CombatResults.NeitherDied;
        }

        public override CombatResults AttackMonster(Monster monster)
        {
            //Recalculate combat stats if required
            if (this.RecalculateCombatStatsRequired)
                this.CalculateCombatStats();

            if (monster.RecalculateCombatStatsRequired)
                monster.CalculateCombatStats();

            //Calculate damage from a normal attack
            int damage = AttackCreatureWithModifiers(monster, 0, 0, 0, 0);

            //Do we hit the player?
            if (damage > 0)
            {
                int monsterOrigHP = monster.Hitpoints;

                monster.Hitpoints -= damage;

                //Is the player dead, if so kill it?
                if (monster.Hitpoints <= 0)
                {
                    Game.Dungeon.KillMonster(monster);

                    //Debug string
                    string combatResultsMsg = "MvM ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " killed";
                    //Game.MessageQueue.AddMessage(combatResultsMsg);
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                    return CombatResults.DefenderDied;
                }

                //Debug string
                string combatResultsMsg3 = "MvM ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + monster.Hitpoints + " injured";
                //Game.MessageQueue.AddMessage(combatResultsMsg3);
                LogFile.Log.LogEntryDebug(combatResultsMsg3, LogDebugLevel.Medium);

                return CombatResults.NeitherDied;
            }

            //Miss
            string combatResultsMsg2 = "MvM ToHit: " + toHitRoll + " AC: " + monster.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monster.Hitpoints + " miss";
            //Game.MessageQueue.AddMessage(combatResultsMsg2);
            LogFile.Log.LogEntryDebug(combatResultsMsg2, LogDebugLevel.Medium);

            return CombatResults.NeitherDied;
        }
    }
}
