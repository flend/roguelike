using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using System.Xml.Serialization;

namespace RogueBasin
{
    public enum SimpleAIStates
    {
        RandomWalk,
        Pursuit,
        Fleeing,
        Returning
    }

    /// <summary>
    /// Base AI for all creatures.
    /// Fighting creatures can use this class, other more complex classes inherit off it.
    /// ProcessTurn() is currently used by all inherited classes
    /// </summary>
    public abstract class MonsterFightAndRunAI : Monster
    {
        public SimpleAIStates AIState {get; set;}
        [XmlIgnore]
        protected Creature currentTarget;
        public int currentTargetID = -1;
        protected int lastHitpoints;

        /// <summary>
        /// Longest distance charmed creature will go away from the PC
        /// </summary>
        protected const double maxChaseDistance = 5.0;

        /// <summary>
        /// Close enough to the PC to go back to duty
        /// </summary>
        protected const double recoverDistance = 2.0;

        public MonsterFightAndRunAI()
        {
            AIState = SimpleAIStates.RandomWalk;
            currentTarget = null;

            lastHitpoints = MaxHitpoints;
        }

        double GetDistance(Creature creature1, Creature creature2)
        {
            double distanceSq = Math.Pow(creature1.LocationMap.x - creature2.LocationMap.x, 2) +
                                    Math.Pow(creature1.LocationMap.y - creature2.LocationMap.y, 2);

            double distance = Math.Sqrt(distanceSq);

            return distance;
        }

        /// <summary>
        /// Main loop called on each turn when we move
        /// </summary>
        public override void ProcessTurn()
        {
            //If in pursuit state, continue to pursue enemy until it is dead (or creature itself is killed) [no FOV used after initial target selected]
            
            //If in randomWalk state, look for new enemies in FOV.
            //Closest enemy becomes new target
            
            //If no targets, move randomly

            Random rand = Game.Random;

            //Restore currentTarget reference from ID, in case we have reloaded
            if (currentTargetID == -1)
            {
                currentTarget = null;
            }
            else
            {
                currentTarget = Game.Dungeon.GetCreatureByUniqueID(currentTargetID);
            }

            //Restore lastAttackedByFromID
            if (LastAttackedByID == -1)
            {
                LastAttackedBy = null;
            }
            else
            {
                LastAttackedBy = Game.Dungeon.GetCreatureByUniqueID(LastAttackedByID);
            }

            //Creatures which sleep until seen (i.e. for ease of processing, not game effects)
            if (Sleeping && WakesOnBeingSeen())
            {
                //Check to see if we should wake by looking for woken creatures in POV
                //(when we drop through currentFOV may be unnecessarily recalculated)

                TCODFov currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

                //Player sees up, wake up
                if (currentFOV.CheckTileFOV(LocationMap.x, LocationMap.y))
                {
                    Sleeping = false;
                    AIState = SimpleAIStates.RandomWalk;
                    LogFile.Log.LogEntryDebug(this.Representation + " spotted by player so wakes", LogDebugLevel.Low);
                }
            }

            //Sleeping creatures don't react until woken
            if (Sleeping && WakesOnSight())
            {
                //Check to see if we should wake by looking for woken creatures in POV
                //(when we drop through currentFOV may be unnecessarily recalculated)

                TCODFov currentFOV = Game.Dungeon.CalculateCreatureFOV(this);

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

                    //Otherwise in FOV
                    //Check if it's awake. If so, wake up and stop
                    if (!monster.Sleeping)
                    {
                        Sleeping = false;
                        AIState = SimpleAIStates.RandomWalk;
                        LogFile.Log.LogEntryDebug(this.Representation + " spots awake " + monster.Representation + " and wakes", LogDebugLevel.Low);
                        break;
                    }
                }

                //Check the player
                if (Game.Dungeon.Player.LocationLevel == this.LocationLevel &&
                    currentFOV.CheckTileFOV(Game.Dungeon.Player.LocationMap.x, Game.Dungeon.Player.LocationMap.y))
                {
                    //In FOV wake
                    Sleeping = false;
                    AIState = SimpleAIStates.RandomWalk;
                    LogFile.Log.LogEntryDebug(this.Representation + " spots player and wakes", LogDebugLevel.Low);
                }
            }

            //If we're still sleeping then skip this go
            if (Sleeping)
                return;

            //If a charmed creature is returning to the PC, continue to do
            if (AIState == SimpleAIStates.Returning)
            {
                //Don't stop on an attack otherwise charmed creatures will be frozen in front of missile troops

                //We have been attacked by someone new
                //if (LastAttackedBy != null && LastAttackedBy.Alive)
                //{
                    //Reset the AI, will drop through and chase the nearest target
                //    AIState = SimpleAIStates.RandomWalk;
                //}
                //else {

                    //Are we close enough to the PC?
                    double distance = GetDistance(this, Game.Dungeon.Player);

                    if (distance <= recoverDistance)
                    {
                        //Reset AI and fall through
                        AIState = SimpleAIStates.RandomWalk;
                        LogFile.Log.LogEntryDebug(this.Representation + " close enough to PC", LogDebugLevel.Medium);
                    }
                    
                    //Otherwise follow the PC back
                    FollowPC();
                //}
            }

            if (AIState == SimpleAIStates.Fleeing || AIState == SimpleAIStates.Pursuit)
            {
                Monster targetMonster = currentTarget as Monster;

                //Fleeing
                //Check we have a valid target (may not after reload)
                //still required?
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
                //Charmed creatures should not attack other charmed creatures
                else if (Charmed && targetMonster != null && targetMonster.Charmed)
                {
                    //Go to non-chase state
                    AIState = SimpleAIStates.RandomWalk;
                }
                //Is target on another level (i.e. has escaped down the stairs)
                else if (currentTarget.LocationLevel != this.LocationLevel)
                {
                    AIState = SimpleAIStates.RandomWalk;
                }
                //Have we just become charmed? Reset AI (stop chasing player)
                else if (currentTarget == Game.Dungeon.Player && Charmed)
                {
                    AIState = SimpleAIStates.RandomWalk;
                }
                //Have we just become passive? Reset AI (stop chasing player)
                else if (currentTarget == Game.Dungeon.Player && Passive)
                {
                    AIState = SimpleAIStates.RandomWalk;
                }
                //Have we just been attacked by a new enemy?
                else if (LastAttackedBy != null && LastAttackedBy.Alive && LastAttackedBy != currentTarget)
                {
                    //Reset the AI for now
                    AIState = SimpleAIStates.RandomWalk;
                }
                else
                {
                    //Otherwise continue to pursue or flee
                    ChaseCreature(currentTarget);
                    return;
                }
            }
            
            //Initial state. Could be drop through from above

            if(AIState == SimpleAIStates.RandomWalk) {
                //RandomWalk state

                //Search an area of sightRadius on either side for creatures and check they are in the FOV

                Map currentMap = Game.Dungeon.Levels[LocationLevel];
                
                

                //currentFOV.CalculateFOV(LocationMap.x, LocationMap.y, SightRadius);

                //Check for other creatures within this creature's FOV
                /*
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
                */

                //AI branches here depending on if we are charmed or passive

                if (this.Charmed)
                {
                    //Charmed - will fight for the PC
                    //Won't attack passive creatures (otherwise will de-passify them and it would be annoying)

                    //Look for creatures in FOV
                    TCODFov currentFOV = Game.Dungeon.CalculateCreatureFOV(this);

                    //List will contain monsters & player
                    List<Monster> monstersInFOV = new List<Monster>();

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
                        monstersInFOV.Add(monster);

                        LogFile.Log.LogEntryDebug(this.Representation + " spots " + monster.Representation, LogDebugLevel.Low);
                    }

                    //Look for creatures which aren't passive or charmed
                    List<Monster> notPassiveTargets = monstersInFOV.FindAll(x => !x.Passive);
                    List<Monster> notCharmedOrPassiveTargets = notPassiveTargets.FindAll(x => !x.Charmed);
                    
                    //Go chase a not-passive, not-charmed creature
                    if (notCharmedOrPassiveTargets.Count > 0)
                    {
                        //Find the closest creature
                        Monster closestCreature = null;
                        double closestDistance = Double.MaxValue; //a long way

                        foreach (Monster creature in notCharmedOrPassiveTargets)
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
                        LogFile.Log.LogEntryDebug(this.Representation + " charm chases " + closestCreature.Representation, LogDebugLevel.Medium);
                        AIState = SimpleAIStates.Pursuit;
                        ChaseCreature(closestCreature);
                    }
                    else
                    {
                        //No creature to chase, go find PC
                        FollowPC();
                    }

                }
                else if (Passive)
                {
                    //Passive - Won't attack the PC
                    MoveRandomSquareNoAttack();
                }
                else
                {
                    //Normal fighting behaviour

                    //Find creatures & PC in FOV
                    TCODFov currentFOV = Game.Dungeon.CalculateCreatureFOV(this);

                    List<Creature> monstersInFOV = new List<Creature>();

                    foreach (Creature monster in Game.Dungeon.Monsters)
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
                        monstersInFOV.Add(monster);

                        LogFile.Log.LogEntryDebug(this.Representation + " spots " + monster.Representation, LogDebugLevel.Low);
                    }

                    if (Game.Dungeon.Player.LocationLevel == this.LocationLevel)
                    {
                        if (currentFOV.CheckTileFOV(Game.Dungeon.Player.LocationMap.x, Game.Dungeon.Player.LocationMap.y))
                        {
                            monstersInFOV.Add(Game.Dungeon.Player);
                            LogFile.Log.LogEntryDebug(this.Representation + " spots " + Game.Dungeon.Player.Representation, LogDebugLevel.Low);
                        }
                    }

                    //Have we just been attacked by a new enemy?
                    if (LastAttackedBy != null && LastAttackedBy.Alive && LastAttackedBy != currentTarget)
                    {
                        //Is this target within FOV? If so, attack it
                        if(monstersInFOV.Contains(LastAttackedBy)) {

                            LogFile.Log.LogEntryDebug(this.Representation + " changes target to " + LastAttackedBy.Representation, LogDebugLevel.Medium);
                            AIState = SimpleAIStates.Pursuit;
                            ChaseCreature(LastAttackedBy);
                            return;
                        }
                        else {
                            //Continue chasing whoever it was we were chasing last
                            if (currentTarget != null)
                            {
                                AIState = SimpleAIStates.Pursuit;
                                ChaseCreature(currentTarget);
                            }
                                return;
                        }
                    }
                    
                    //The next bit could be tidied up now

                    //Attack PC if seen
                    if (monstersInFOV.Contains(Game.Dungeon.Player))
                    {
                        Creature closestCreature = Game.Dungeon.Player;
                        //Start chasing this creature
                        LogFile.Log.LogEntryDebug(this.Representation + " chases " + closestCreature.Representation, LogDebugLevel.Medium);
                        AIState = SimpleAIStates.Pursuit;
                        ChaseCreature(closestCreature);
                    }
                    else
                    {
                        //Otherwise just move at random
                        MoveRandomSquareNoAttack();
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
                }
                
                  //UNCOMMENT THIS
                //Current behaviour: only chase the PC
                if(creaturesInFOV.Contains(Game.Dungeon.Player)) {
                    Creature closestCreature = Game.Dungeon.Player;
                    //Start chasing this creature
                    LogFile.Log.LogEntryDebug(this.Representation + " chases " + closestCreature.Representation, LogDebugLevel.Low);
                    AIState = SimpleAIStates.Pursuit;
                    ChaseCreature(closestCreature);
                 //END COMMENTING
                }*/


            }
        }

        protected void MoveRandomSquareNoAttack()
        {
            //Move randomly.

            //Return if we can't move
            if (!CanMove())
                return;

            int direction = Game.Random.Next(9);

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

            validMove = Game.Dungeon.MapSquareIsWalkable(LocationLevel, newLocation);

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

            //Move if allowed
            if (okToMoveIntoSquare)
            {
                LocationMap = newLocation;
            }
        }

        private void ChaseCreature(Creature newTarget)
        {
            //Confirm this as current target
            currentTarget = newTarget;
            currentTargetID = newTarget.UniqueID;

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
                //Only not-charmed creatures will flee

                if (!Charmed)
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
                    bool isEnterable = Game.Dungeon.MapSquareIsWalkable(this.LocationLevel, new Point(fleeX, fleeY));
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
                    FollowAndAttack(newTarget);
                }

            }
            //Not fleeing
            else
            {
                //If charmed creatures get too far away chasing they will come back
                if (Charmed)
                {
                    //Calculate distance between PC and creature
                    if (Game.Dungeon.Player.LocationLevel == this.LocationLevel)
                    {

                        double distanceSq = Math.Pow(Game.Dungeon.Player.LocationMap.x - this.LocationMap.x, 2) +
                                    Math.Pow(Game.Dungeon.Player.LocationMap.y - this.LocationMap.y, 2);
                        double distance = Math.Sqrt(distanceSq);

                        if (distance > maxChaseDistance)
                        {
                            LogFile.Log.LogEntryDebug(this.SingleDescription + " returns to PC", LogDebugLevel.Medium);
                            AIState = SimpleAIStates.Returning;
                            FollowPC();
                        }
                    }
                }

                //Not charmed - pursue and attack
                FollowAndAttack(newTarget);
            }
        }

        protected virtual void FollowAndAttack(Creature newTarget)
        {
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

                Screen.Instance.DrawMeleeAttack(this, newTarget);

                //If we killed it, move into its square
                if (result != CombatResults.DefenderDied)
                {
                    moveIntoSquare = false;
                }

                //Exception for immortal player
                if (newTarget == Game.Dungeon.Player && Game.Dungeon.PlayerImmortal)
                {
                    moveIntoSquare = false;
                }
            }

            //Otherwise (or if the creature died), move towards it (or its corpse)
            if (moveIntoSquare && CanMove())
            {
                LocationMap = nextStep;
            }
        }

        /// <summary>
        /// Follow the PC but don't attack him - used for charmed creatures
        /// </summary>
        void FollowPC()
        {
            Player player = Game.Dungeon.Player;

            //If we can't move, don't follow
            if (!CanMove())
                return;

            //Find location of next step on the path towards them
            Point nextStep = Game.Dungeon.GetPathTo(this, player);

            bool moveIntoSquare = true;

            //Check we don't walk on top of him
            if (nextStep.x == player.LocationMap.x && nextStep.y == player.LocationMap.y)
            {
                moveIntoSquare = false;
            }

            //Move into square - seems low level but GetPathTo return no move if not possible
            if (moveIntoSquare)
            {
                LocationMap = nextStep;
            }
        }
    

        public void RecoverOnBeingHit()
        {
            if (AIState == SimpleAIStates.Fleeing &&
                Game.Random.Next(100) < GetChanceToRecoverOnBeingHit())
            {
                AIState = SimpleAIStates.Pursuit;
                LogFile.Log.LogEntry(this.SingleDescription + "recovers and returns to the fight");
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
        /// out of 100, recover back to persuit when fleeing
        /// </summary>
        /// <returns></returns>
        protected virtual int GetChanceToRecoverOnBeingHit()
        {
            return 50;
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

        protected override string HitsPlayerCombatString()
        {
            string combatStr = "";

            if (!Unique)
                combatStr = "The ";

            return combatStr + this.SingleDescription + " hits you.";
        }

        protected override string MissesPlayerCombatString()
        {
            string combatStr = "";

            if (!Unique)
                combatStr = "The ";

            return combatStr + this.SingleDescription + " misses you.";
        }

        protected override string HitsMonsterCombatString(Monster target)
        {
            string combatStr = "";

            if (!Unique)
                combatStr = "The ";

            return combatStr + this.SingleDescription + " hits the " + target.SingleDescription + ".";
        }

        protected override string MissesMonsterCombatString(Monster target)
        {
            string combatStr = "";

            if (!Unique)
                combatStr = "The ";

            return combatStr + this.SingleDescription + " misses the " + target.SingleDescription + ".";
        }

        public override bool CanBeCharmed()
        {
            return true;
        }

        public override bool CanBePassified()
        {
            return true;
        }

        /// <summary>
        /// A creature has attacked us (possibly from out of our view range). Don't just sit there passively
        /// </summary>
        public override void NotifyAttackByCreature(Creature creature)
        {
            AIState = SimpleAIStates.Pursuit;
            currentTarget = creature;
            currentTargetID = creature.UniqueID;
        }

        /// <summary>
        /// We've been hit. Have a chance of recovering if we are fleeing
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="damage"></param>
        public override void NotifyHitByCreature(Creature creature, int damage)
        {
            RecoverOnBeingHit();
        }

        /// <summary>
        /// On death, null our currentTarget. This is a good idea anyway, but looks like it was added for an important reason 'circular references'
        /// </summary>
        public override void NotifyMonsterDeath()
        {
            currentTarget = null;
            currentTargetID = -1;
        }
    }
}
