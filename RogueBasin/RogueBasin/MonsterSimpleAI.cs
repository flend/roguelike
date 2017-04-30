﻿using System.Xml.Serialization;

namespace RogueBasin
{
    /// <summary>
    /// Simple AI. Currently no monsters implement this. So it's not fully implemented.
    /// </summary>
    public abstract class MonsterSimpleAI : Monster
    {
        public SimpleAIStates AIState {get; set;}
        [XmlIgnore]
        protected Creature currentTarget;
        public int currentTargetID = -1;

        public MonsterSimpleAI()
        {
            AIState = SimpleAIStates.Patrol;
            currentTarget = null;
        }
        
        

        /*
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

            //Restore currentTarget reference from ID, in case we have reloaded
            if (currentTargetID == -1)
            {
                currentTarget = null;
            }
            else
            {
                currentTarget = Game.Dungeon.GetCreatureByUniqueID(currentTargetID);
            }

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

                //COMMENT THIS
                //If there are possible targets, find the closest and chase it
                //Otherwise continue to move randomly
                
                //if (creaturesInFOV.Count > 0)
                //{
                    
                //    //Find the closest creature
                //    Creature closestCreature = null;
                //    double closestDistance = Double.MaxValue; //a long way

                //    foreach (Creature creature in creaturesInFOV)
                //    {
                //        double distanceSq = Math.Pow(creature.LocationMap.x - this.LocationMap.x, 2) +
                //            Math.Pow(creature.LocationMap.y - this.LocationMap.y, 2);

                //        double distance = Math.Sqrt(distanceSq);

                //        if (distance < closestDistance)
                //        {
                //            closestDistance = distance;
                //            closestCreature = creature;
                //        }
                //    }


                //    //Start chasing this creature
                //    LogFile.Log.LogEntryDebug(this.Representation + " chases " + closestCreature.Representation, LogDebugLevel.Medium);
                //    ChaseCreature(closestCreature);
                //}
                
                  //UNCOMMENT THIS
                //Current behaviour: only chase the PC
                if(creaturesInFOV.Contains(Game.Dungeon.Player)) {
                    Creature closestCreature = Game.Dungeon.Player;
                    //Start chasing this creature
                    LogFile.Log.LogEntryDebug(this.Representation + " chases " + closestCreature.Representation, LogDebugLevel.Low);
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

                    if (contents.player != null)
                    {
                        //Attack the player
                        CombatResults result = AttackPlayer(contents.player);

                        if (result == CombatResults.DefenderDied)
                        {
                            //Bad news for the player here!
                            okToMoveIntoSquare = true;
                        }

                        //Exception for immortal player
                        if (Game.Dungeon.PlayerImmortal)
                        {
                            okToMoveIntoSquare = false;
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
            currentTargetID = newTarget.UniqueID;

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

                //Exception for immortal player
                if (newTarget == Game.Dungeon.Player && Game.Dungeon.PlayerImmortal)
                {
                    moveIntoSquare = false;
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
        }*/

        
    }
}
