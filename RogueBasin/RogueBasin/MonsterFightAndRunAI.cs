using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    public enum SimpleAIStates
    {
        RandomWalk,
        Pursuit,
        Fleeing
    }

    /// <summary>
    /// Simple AI runs when it is down to a certain number of HP
    /// </summary>
    public abstract class MonsterFightAndRunAI : Monster
    {
        public SimpleAIStates AIState {get; set;}
        Creature currentTarget;
        int lastHitpoints;

        public MonsterFightAndRunAI()
        {
            AIState = SimpleAIStates.RandomWalk;
            currentTarget = null;

            lastHitpoints = ClassMaxHitpoints();
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
            
            if (AIState == SimpleAIStates.Fleeing || AIState == SimpleAIStates.Pursuit)
            {

                //Fleeing
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
                    //Otherwise continue to flee

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

                //COMMENT THIS
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
                
                  //UNCOMMENT THIS
                //Current behaviour: only chase the PC
                if(creaturesInFOV.Contains(Game.Dungeon.Player)) {
                    Creature closestCreature = Game.Dungeon.Player;
                    //Start chasing this creature
                    LogFile.Log.LogEntryDebug(this.Representation + " chases " + closestCreature.Representation, LogDebugLevel.Low);
                    AIState = SimpleAIStates.Pursuit;
                    ChaseCreature(closestCreature);
                 //END COMMENTING
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
            //AIState = SimpleAIStates.Pursuit;

            //If the creature is badly damaged they may flee
            int maxHitPointsWillFlee = GetMaxHPWillFlee();
            int chanceToRecover = GetChanceToRecover(); // out of 100
            int chanceToFlee = GetChanceToFlee(); // out of 100

            //Are we fleeing already
            if (AIState == SimpleAIStates.Fleeing)
            {
                //Do we recover?
                if (Game.Random.Next(100) < chanceToRecover)
                {
                    AIState = SimpleAIStates.Pursuit;
                    LogFile.Log.LogEntryDebug(this.SingleDescription + " recovered", LogDebugLevel.Medium);
                }
            }
            else
            {
                //Check if we want to flee. Only recheck after we've been injured again
                if (Hitpoints <= maxHitPointsWillFlee && Hitpoints < lastHitpoints)
                {
                    if (Game.Random.Next(100) < chanceToFlee)
                    {
                        AIState = SimpleAIStates.Fleeing;
                        LogFile.Log.LogEntryDebug(this.SingleDescription + " fleeing", LogDebugLevel.Medium);
                    }
                }
            }

            lastHitpoints = Hitpoints;

            if (AIState == SimpleAIStates.Fleeing)
            {
                //Flee code, same as ThrowAndRunAI
                int deltaX = newTarget.LocationMap.x - this.LocationMap.x;
                int deltaY = newTarget.LocationMap.y - this.LocationMap.y;

                //Find a point in the dungeon to flee to
                int fleeX = 0;
                int fleeY = 0;

                int counter = 0;

                bool relaxDirection = false;
                bool goodPath = false;

                Point nextStep = new Point(0, 0);

                int totalFleeLoops = GetTotalFleeLoops();
                int relaxDirectionAt = RelaxDirectionAt();

                do
                {
                    fleeX = Game.Random.Next(Game.Dungeon.Levels[this.LocationLevel].width);
                    fleeY = Game.Random.Next(Game.Dungeon.Levels[this.LocationLevel].height);

                    //Relax conditions if we are having a hard time
                    if (counter > relaxDirectionAt)
                        relaxDirection = true;

                    //Check these are in the direction away from the attacker
                    int deltaFleeX = fleeX - this.LocationMap.x;
                    int deltaFleeY = fleeY - this.LocationMap.y;

                    if (!relaxDirection)
                    {
                        if (deltaFleeX > 0 && deltaX > 0)
                        {
                            counter++;
                            continue;
                        }
                        if (deltaFleeX < 0 && deltaX < 0)
                        {
                            counter++;
                            continue;
                        }
                        if (deltaFleeY > 0 && deltaY > 0)
                        {
                            counter++;
                            continue;
                        }
                        if (deltaFleeY < 0 && deltaY < 0)
                        {
                            counter++;
                            continue;
                        }
                    }

                    //Check the square is empty
                    bool isEnterable = Game.Dungeon.MapSquareCanBeEntered(this.LocationLevel, new Point(fleeX, fleeY));
                    if (!isEnterable)
                    {
                        counter++;
                        continue;
                    }

                    //Check the square is empty of creatures
                    SquareContents contents = Game.Dungeon.MapSquareContents(this.LocationLevel, new Point(fleeX, fleeY));
                    if (contents.monster != null)
                    {
                        counter++;
                        continue;
                    }

                    //Check the square is pathable to
                    nextStep = Game.Dungeon.GetPathFromCreatureToPoint(this.LocationLevel, this, new Point(fleeX, fleeY));

                    if (nextStep.x == -1 && nextStep.y == -1)
                    {
                        counter++;
                        continue;
                    }

                    //Otherwise we found it
                    goodPath = true;
                    break;


                } while (counter < totalFleeLoops);

                //If we found a good path, walk it
                if (goodPath)
                {
                    LocationMap = nextStep;
                }
                else
                {
                    //No good place to flee, attack instead
                    //Copied from below

                    //Find location of next step on the path towards them
                    nextStep = Game.Dungeon.GetPathTo(this, newTarget);

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

            }
            else
            {
                //Persui and attack

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
        }

        /// <summary>
        /// out of 100, recover back to persuit when fleeing
        /// </summary>
        /// <returns></returns>
        protected virtual int GetChanceToRecover()
        {
            return 0;
        }
        /// <summary>
        /// out of 100, chance to flee when below flee hp
        /// </summary>
        /// <returns></returns>
        protected virtual int GetChanceToFlee()
        {
            return 0;
        }

        /// <summary>
        /// max hitpoint when will start thinking about fleeing
        /// </summary>
        /// <returns></returns>
        protected virtual int GetMaxHPWillFlee()
        {
            return 0;
        }

        /// <summary>
        /// Flee ai cleverness. 10 loops performs pretty well, much higher is infallable
        /// </summary>
        /// <returns></returns>
        protected virtual int GetTotalFleeLoops() { return 10; }

        /// <summary>
        /// Relax the requirement to flee in a direction away from the player at this loop. Very low makes the ai more stupid. Very high makes it more likely to fail completely.
        /// </summary>
        /// <returns></returns>
        protected virtual int RelaxDirectionAt() { return 0; }

    }
}
