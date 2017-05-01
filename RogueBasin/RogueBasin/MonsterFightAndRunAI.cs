﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RogueBasin
{
    public enum SimpleAIStates
    {
        Patrol,
        InvestigateSound,
        Pursuit,
        Fleeing,
        Returning
    }

    public enum PatrolType
    {
        RandomWalk,
        Static,
        Rotate,
        Waypoints
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

        public int CurrentSoundID { get; set; }

        public double CurrentSoundInterestScore { get; set; }

        public long LastCheckedSounds { get; set; }

        public int BlockedOnSoundTurns { get; set; }

        /// <summary>
        /// How many turns we can stay in the same place until we give up when chasing a sound
        /// </summary>
        const int MaxBlockOnSoundTurns = 5;

        /// <summary>
        /// Which waypoint are we headed towards next?
        /// </summary>
        public int CurrentWaypoint { get; set; }

        /// <summary>
        /// This is cached here since sounds are not nicely look-upable by Id.
        /// Serialization shouldn't be a problem but it will take a copy which is kind of horrible and might cause bugs in future
        /// </summary>
        [XmlIgnore]
        protected SoundEffect currentSound;

        

        /// <summary>
        /// Longest distance charmed creature will go away from the PC
        /// </summary>
        protected const double maxChaseDistance = 5.0;

        /// <summary>
        /// Close enough to the PC to go back to duty
        /// </summary>
        protected const double recoverDistance = 2.0;

        /// <summary>
        /// List of waypoints, may be used by the patrol AI
        /// </summary>
        protected List<Point> wayPoints = new List<Point>();

        /// <summary>
        /// For rotation patrol, how many turns since we last rotated?
        /// </summary>
        protected int rotationTurns = 0;

        /// <summary>
        /// Following waypoints up in order?
        /// </summary>
        protected bool waypointsInc = true;

        /// <summary>
        /// If heading it set to sound, don't patrol (even if you're on always patrol), this give you one chance to shoot
        /// </summary>
        protected bool headingSetToSound = false;

        public MonsterFightAndRunAI(int level) : base (level)
        {
            Initialise();
        }

        public MonsterFightAndRunAI()
        {
            Initialise();
        }

        private void Initialise()
        {
            AIState = SimpleAIStates.Patrol;
            currentTarget = null;

            CurrentSoundID = -1;

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
            //TODO: add forget mode?

            Random rand = Game.Random;

            Point startOfTurnLocation = LocationMap;

            //RESTORE STATE AFTER SAVE
            //Creature references may be circular, and will crash serialization, so an index is used instead

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

            //Restore sound
            if (CurrentSoundID == -1)
            {
                currentSound = null;
            }
            else
            {
                currentSound = Game.Dungeon.GetSoundByID(CurrentSoundID);

                if (currentSound == null)
                {
                    ResetFollowingSound();
                    AIState = SimpleAIStates.Patrol;
                    LogFile.Log.LogEntryDebug("Error restoring sound, resetting", LogDebugLevel.High);
                }
            }

            //Stunned creatures miss turns
            if (StunnedTurns > 0)
            {
                StunnedTurns--;
                LogFile.Log.LogEntryDebug(this.Representation + " is stunned for " + StunnedTurns + " more turns", LogDebugLevel.Low);

                ResetTurnsMoving();
                return;
            }

            //Reloading creatures miss turns
            if (ReloadingTurns > 0)
            {
                LogFile.Log.LogEntryDebug(this.Representation + " is reloading for " + ReloadingTurns + " more turns", LogDebugLevel.Low);
                ReloadingTurns--;

                ResetTurnsMoving();
                return;
            }

            //Calculate current FOV
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(this);

            //TEST SLEEPING CREATURES
            //Sleeping is a Creature state that is used like an AI state
            //This is OK since we exit immediately

            //Creatures which sleep until seen (i.e. for ease of processing, not game effects)
            if (Sleeping && WakesOnBeingSeen())
            {
                //Check to see if we should wake by looking for woken creatures in POV
                CreatureFOV playerFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

                //Player sees monster, wake up
                if (playerFOV.CheckTileFOV(LocationMap.x, LocationMap.y))
                {
                    Sleeping = false;
                    AIState = SimpleAIStates.Patrol;
                    LogFile.Log.LogEntryDebug(this.Representation + " spotted by player so wakes", LogDebugLevel.Low);
                }
            }

            //Sleeping creatures don't react until they see a woken creature
            if (Sleeping && WakesOnSight())
            {
                //Check to see if we should wake by looking for woken creatures in POV
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
                    //For RRL, they need to be seen within the stealth radius when sleeping
                    if (!monster.Sleeping)
                    {
                        Sleeping = false;
                        AIState = SimpleAIStates.Patrol;
                        LogFile.Log.LogEntryDebug(this.Representation + " spots awake " + monster.Representation + " and wakes", LogDebugLevel.Low);
                        break;
                    }
                }

                //Check if we can see the player
                if (PlayerCanBeSeen(currentFOV))
                {
                    //In FOV wake
                    Sleeping = false;
                    AIState = SimpleAIStates.Patrol;
                    LogFile.Log.LogEntryDebug(this.Representation + " spots player and wakes", LogDebugLevel.Low);
                }
            }

            //Sleeping creatures wake if a monster wakes within their monster stealth radius
            if (Sleeping && WakesOnMonsterStealth())
            {
                //Check to see if we should wake by looking for woken creatures in POV
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
                    //For RRL, they need to be seen within the stealth radius when sleeping
                    if (!monster.Sleeping && InMonsterStealthRadius(monster.LocationMap))
                    {
                        Sleeping = false;
                        AIState = SimpleAIStates.Patrol;
                        LogFile.Log.LogEntryDebug(this.Representation + " spots awake " + monster.Representation + " within stealth radius and wakes", LogDebugLevel.Low);
                        break;
                    }
                }
            }

            //Sleeping creatures who are activated by player proximity (within FOV)
            if (Sleeping && StealthRadius() > 0)
            {
                if (InStealthRadius(Game.Dungeon.Player.LocationMap) && PlayerCanBeSeen(currentFOV))
                {
                    Sleeping = false;
                    AIState = SimpleAIStates.Patrol;
                    LogFile.Log.LogEntryDebug(this.Representation + " spots player within stealth radius", LogDebugLevel.Low);
                }
            }

            //RETURNING - used when a charmed creature gets a long way from the PC

            if (AIState == SimpleAIStates.Returning && !Sleeping)
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
                        AIState = SimpleAIStates.Patrol;
                        LogFile.Log.LogEntryDebug(this.Representation + " close enough to PC", LogDebugLevel.Low);
                    }
                    
                    //Otherwise follow the PC back
                    FollowPC();
                //}
            }

            //PURSUIT MODES - Pursuit [active] and Fleeing [temporarily fleeing, will return to target]

            if ((AIState == SimpleAIStates.Fleeing || AIState == SimpleAIStates.Pursuit) && !Sleeping)
            {
                Monster targetMonster = currentTarget as Monster;

                //Fleeing
                //Check we have a valid target (may not after reload)
                //still required?
                if (currentTarget == null)
                {
                    AIState = SimpleAIStates.Patrol;
                }

                //Is target yet living?
                else if (currentTarget.Alive == false)
                {
                    //If not, go to non-chase state
                    AIState = SimpleAIStates.Patrol;
                }
                //Charmed creatures should not attack other charmed creatures
                else if (Charmed && targetMonster != null && targetMonster.Charmed)
                {
                    //Go to non-chase state
                    AIState = SimpleAIStates.Patrol;
                }
                //Is target on another level (i.e. has escaped down the stairs)
                else if (currentTarget.LocationLevel != this.LocationLevel)
                {
                    AIState = SimpleAIStates.Patrol;
                }
                //Have we just become charmed? Reset AI (stop chasing player)
                else if (currentTarget == Game.Dungeon.Player && Charmed)
                {
                    AIState = SimpleAIStates.Patrol;
                }
                //Have we just become passive? Reset AI (stop chasing player)
                else if (currentTarget == Game.Dungeon.Player && Passive)
                {
                    AIState = SimpleAIStates.Patrol;
                }
                //Has the player stealthed?
                else if (currentTarget == Game.Dungeon.Player && Game.Dungeon.Player.isStealthed())
                {
                    LogFile.Log.LogEntryDebug(this.Representation + " stop chasing. Player went stealthed", LogDebugLevel.Medium);
                    AIState = SimpleAIStates.Patrol;
                }
                //Have we just been attacked by a new enemy?
                else if (LastAttackedBy != null && LastAttackedBy.Alive && LastAttackedBy != currentTarget)
                {
                    //Reset the AI for now
                    AIState = SimpleAIStates.Patrol;
                }
                else
                {
                    //Otherwise continue to pursue or flee
                    ChaseCreature(currentTarget);
                }
            }

            //PATROL STATE OR INVESTIGATE STATE

            //Check states which override patrol or investigate (e.g being attacked, charmed, seeing the PC)

            if ((AIState == SimpleAIStates.Patrol || AIState == SimpleAIStates.InvestigateSound) && !Sleeping)
            {
     
                Map currentMap = Game.Dungeon.Levels[LocationLevel];
                
                //AI branches here depending on if we are charmed or passive

                if (this.Charmed)
                {
                    //Charmed - will fight for the PC
                    //Won't attack passive creatures (otherwise will de-passify them and it would be annoying)

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
                        LogFile.Log.LogEntryDebug(this.Representation + " charm chases " + closestCreature.Representation, LogDebugLevel.Low);
                        AIState = SimpleAIStates.Pursuit;
                        ChaseCreature(closestCreature);
                    }
                    else
                    {
                        //No creature to chase, go find PC
                        FollowPC();
                    }

                }
                else if(!Passive)
                {
                    //Normal fighting behaviour

                    //Optional: check next move and open any doors if possible. This gives us a chance to shoot lurking PCs
                    //Removed this while closing doors is not possible - avoids repeated abuse
                    
                    /*
                    if (CanOpenDoors() && (AIState == SimpleAIStates.Patrol || AIState == SimpleAIStates.InvestigateSound))
                    {

                        //Very simple version, monsters open any doors they are facing
                        List<Point> directedAhead = DirectionUtil.SurroundingPointsFromDirection(Heading, LocationMap, 3);

                        foreach (Point p in directedAhead)
                        {
                            MapTerrain doorTerrain = Game.Dungeon.GetTerrainAtPoint(this.LocationLevel, p);

                            if (doorTerrain == MapTerrain.ClosedDoor)
                            {
                                LogFile.Log.LogEntryDebug(this.Representation + " : door detected ahead, opening", LogDebugLevel.Medium);
                                Game.Dungeon.OpenDoor(this.LocationLevel, p);
                            }
                        }
                    }*/

                    //Find creatures & PC in FOV
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
                        if (currentFOV.CheckTileFOV(Game.Dungeon.Player.LocationMap.x, Game.Dungeon.Player.LocationMap.y) && !Game.Dungeon.Player.isStealthed())
                        {
                            monstersInFOV.Add(Game.Dungeon.Player);
                            LogFile.Log.LogEntryDebug(this.Representation + " spots " + Game.Dungeon.Player.Representation, LogDebugLevel.Low);
                        }
                    }

                    //Have we just been attacked by a new enemy? If so, respond to them
                    if (LastAttackedBy != null && LastAttackedBy.Alive && LastAttackedBy != currentTarget)
                    {
                        //Is this target within FOV? If so, attack it
                        if (monstersInFOV.Contains(LastAttackedBy))
                        {

                            LogFile.Log.LogEntryDebug(this.Representation + " changes target to " + LastAttackedBy.Representation, LogDebugLevel.Medium);
                            AIState = SimpleAIStates.Pursuit;
                            ChaseCreature(LastAttackedBy);
                        }
                        else
                        {
                            //Continue chasing whoever it was we were chasing last
                            if (currentTarget != null)
                            {
                                AIState = SimpleAIStates.Pursuit;
                                ChaseCreature(currentTarget);
                            }
                        }
                    }



                    //Check if we can see the PC and pursue them

                    //If we are not currently pursuing anything and we see the PC, pursue if seen
                    //Technically, go into pursuit mode, which may not involve actual movement
                    if ((AIState == SimpleAIStates.Patrol || AIState == SimpleAIStates.InvestigateSound) && monstersInFOV.Contains(Game.Dungeon.Player) && !Game.Dungeon.Player.isStealthed())
                    {
                        Creature closestCreature = Game.Dungeon.Player;
                        //Start chasing this creature
                        LogFile.Log.LogEntryDebug(this.Representation + " chases " + closestCreature.Representation, LogDebugLevel.Medium);
                        AIState = SimpleAIStates.Pursuit;
                        ChaseCreature(closestCreature);
                    }
                }

                //This is so we don't have to instrument each state
                if (LocationMap == startOfTurnLocation)
                {
                    ResetTurnsMoving();
                    AddTurnsInactive();
                }
                else
                {
                    ResetTurnsInactive();
                    AddTurnsMoving();
                }
            }

            //INVESTIGATE SOUNDS

            //If a new sound has happened, calculate interest
            //If high interest, make this our target
            //Continue to investigate the sound
            //If we've reached the target, return to Patrol [sightings are handled above]

            //Monster that don't pursue still have a chance to direct their FOVs at sounds

            //Sleeping monsters will still investigate large sounds - but they will sleep again if they get to the source and nothing else happens

            bool moveFollowingSound = false;

            if ((AIState == SimpleAIStates.Patrol || AIState == SimpleAIStates.InvestigateSound) && WillInvestigateSounds() )
            {
                double currentSoundInterest;

                if (CurrentSoundID == -1)
                {
                    currentSoundInterest = 0.0;
                }
                else {
                    //Interest in the last interesting sound will decay over time
                    currentSoundInterest = currentSound.DecayedInterest(CurrentSoundInterestScore, Game.Dungeon.WorldClock);
                }

                //Get sounds which have happened since we last looked (and update)
                //TODO: reset this when we leave a pursuit state - could look at very old sounds then??

                List<SoundEffect> newSounds = Game.Dungeon.GetSoundsAfterTime(LastCheckedSounds);
                LastCheckedSounds = Game.Dungeon.WorldClock;

                SoundEffect newSoundToFollow = null;
                int newSoundToFollowID = -1;
                double newSoundInterest = currentSoundInterest;

                foreach(SoundEffect soundEvent in newSounds) {
                    SoundEffect sEffect = soundEvent;

                    double newSoundScore = sEffect.DecayedMagnitude(this.LocationLevel, this.LocationMap);

                    if (newSoundScore > newSoundInterest)
                    {
                        newSoundToFollowID = sEffect.ID;
                        newSoundToFollow = sEffect;
                        newSoundInterest = newSoundScore;
                    }
                }


                //Have we found a new more interesting sound?
                //If so, follow it
                if (newSoundToFollowID != -1)
                {
                    LogFile.Log.LogEntryDebug(this.Representation + " new sound target: " + newSoundToFollow + "[ int: " + newSoundInterest + "] (old: " + currentSound + " [ int: " + currentSoundInterest + "])", LogDebugLevel.Medium);
                    Sleeping = false;

                    //Change sound
                    
                    //A sound we choose gets a boost in interest to give us a bit of hystersis
                     SetSoundToFollow(newSoundToFollow, newSoundInterest * 1.5);

                    AIState = SimpleAIStates.InvestigateSound;
                }
                else
                {
                    if (currentSoundInterest < 0.01)
                    {
                        //Sound has decayed so much it's not interesting, or we never had an interesting sound
                        ResetFollowingSound();
                        if(AIState == SimpleAIStates.InvestigateSound)
                            LogFile.Log.LogEntryDebug(this.Representation + " sound " + currentSound + " is old, resetting", LogDebugLevel.Low);
                        AIState = SimpleAIStates.Patrol;
                        

                    }
                }

                //For a new or existing sound, pursue it
                if (AIState == SimpleAIStates.InvestigateSound)
                {
                    moveFollowingSound = InvestigateSound();
                }
            }

            //If nothing else happened, do the Patrol action
            //Don't if we moved in response to a sound
            if (((AIState == SimpleAIStates.Patrol && !moveFollowingSound) || (WillAlwaysPatrol() && !headingSetToSound)) && !Sleeping)
            {
                //We haven't got anything to do and we can't see the PC
                //Do normal movement
                DoPatrol();
            }

            //Reset the skip-patrol if we looked at sound flag
            headingSetToSound = false;
        }

        /// <summary>
        /// Reset sound state. Doesn't change AI
        /// </summary>
        private void ResetFollowingSound()
        {
            CurrentSoundID = -1;
            currentSound = null;
            CurrentSoundInterestScore = 0.0;
        }

        private void SetSoundToFollow(SoundEffect sound, double interest)
        {
            CurrentSoundID = sound.ID;
            CurrentSoundInterestScore = interest;
            currentSound = sound;

            BlockedOnSoundTurns = 0;

        }

        /// <summary>
        /// Used for the InvestigateSound AI state.
        /// Approach the sound location. Other checks in ProcessTurn() will take over if we see a target to pursue
        /// Return true if we made a move (so we can't patrol this turn)
        /// </summary>
        private bool InvestigateSound()
        {
            //Check the square is pathable to
            Pathing.PathingResult pathingResult;
            Pathing.PathingPermission permission = Pathing.PathingPermission.Normal;

            if (CanOpenDoors())
                permission = Pathing.PathingPermission.IgnoreDoors;

            pathingResult = Game.Dungeon.Pathing.GetPathToPoint(this.LocationLevel, this.LocationMap, currentSound.MapLocation, PathingType(), permission, IgnoreDangerousTerrain);

            if (pathingResult.TerminallyBlocked)
            {
                //Not routeable due to doors etc.
                LogFile.Log.LogEntryDebug(this.Representation + " permanently blocked on sound " + currentSound + ". Returning to patrol", LogDebugLevel.Medium);
                ResetFollowingSound();
                AIState = SimpleAIStates.Patrol;

                return false;
            }

            if (pathingResult.MonsterFinalLocation == LocationMap)
            {
                //Temporarily blocked
                BlockedOnSoundTurns++;
                LogFile.Log.LogEntryDebug(this.Representation + " temp blocked on sound " + currentSound + "turns: " + BlockedOnSoundTurns, LogDebugLevel.Low);

                if (BlockedOnSoundTurns > MaxBlockOnSoundTurns)
                {
                    //Go back to patrol
                    LogFile.Log.LogEntryDebug(this.Representation + " blocked for too long on " + currentSound + ". Returning to patrol", LogDebugLevel.Low);
                    ResetFollowingSound();
                    AIState = SimpleAIStates.Patrol;
                    
                }

                return true;
            }

            //(if temporarily blocked will just attempt to move onto their own square)
            
            //Only creatures that pursue actually move
            if (WillPursue() && CanMove())
            {
                //For some reason they are ignoring dangerous terrain so hack here
                if (Game.Dungeon.DangerousFeatureAtLocation(new Location(this.LocationLevel, pathingResult.MonsterFinalLocation)))
                {
                    ResetFollowingSound();
                    headingSetToSound = true;
                    AIState = SimpleAIStates.Patrol;
                    return true;
                }


                SetHeadingToMapSquare(pathingResult.MonsterFinalLocation);
                MoveIntoSquare(pathingResult.MonsterFinalLocation);
            }
            else
            {
                //Creature that can't move or don't pursue reset to Patrol now, but get a chance to detect the PC in the next loop
                SetHeadingToMapSquare(pathingResult.MonsterFinalLocation);
                LogFile.Log.LogEntryDebug(this.Representation + " (non-pursue) changing heading only for sound " + currentSound, LogDebugLevel.Medium);
                ResetFollowingSound();
                headingSetToSound = true;
                AIState = SimpleAIStates.Patrol;

                return true;
            }

            //We made it? Go back to patrol
            if (pathingResult.MonsterFinalLocation == currentSound.MapLocation)
            {
                LogFile.Log.LogEntryDebug(this.Representation + " reached source of sound " + currentSound + ". Returning to patrol", LogDebugLevel.Low);
                ResetFollowingSound();
                AIState = SimpleAIStates.Patrol;
                return true;
            }

            return true;
        }

        private bool PlayerCanBeSeen(CreatureFOV currentFOV) {
            return Game.Dungeon.Player.LocationLevel == this.LocationLevel &&
                    currentFOV.CheckTileFOV(Game.Dungeon.Player.LocationMap.x, Game.Dungeon.Player.LocationMap.y) && !Game.Dungeon.Player.isStealthed();
        }

        protected void DoPatrol()
        {
            //Carry out patrol movement

            //Don't return if we can't move here (because we may still want to rotate etc.)

            switch (GetPatrolType())
            {
                case PatrolType.Static:

                    //Don't move
                    return;

                case PatrolType.Rotate:
                    {
                        //Still waiting to rotate?
                        if (rotationTurns != GetPatrolRotationSpeed())
                        {
                            rotationTurns++;
                            return;
                        }

                        //Rotate this turn
                        rotationTurns = 0;

                        Heading = DirectionUtil.RotateHeading(Heading, GetPatrolRotationAngle(), GetPatrolRotationClockwise());

                    }

                    return;


                case PatrolType.RandomWalk:
                    {

                        //Override if we can't move
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

                        validMove = Game.Dungeon.MapSquareIsWalkable(LocationLevel, newLocation) && !Game.Dungeon.DangerousFeatureAtLocation(new Location(LocationLevel, newLocation));

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
                            SetHeadingToMapSquare(newLocation);
                            MoveIntoSquare(newLocation);
                        }
                    }
                    break;

                case PatrolType.Waypoints:
                    {
                        if (Waypoints.Count == 0)
                        {
                            LogFile.Log.LogEntryDebug(this.Representation + " Can't patrol - no way points", LogDebugLevel.High);
                            return;
                        }

                        //Head towards next waypoint
                        //Check the square is pathable to
                        Pathing.PathingResult pathingResult;
                        Pathing.PathingPermission permission = Pathing.PathingPermission.Normal;

                        if(CanOpenDoors())
                            permission = Pathing.PathingPermission.IgnoreDoors;

                        pathingResult = Game.Dungeon.Pathing.GetPathToPoint(this.LocationLevel, this.LocationMap, Waypoints[CurrentWaypoint], PathingType(), permission, IgnoreDangerousTerrain);

                        if (pathingResult.TerminallyBlocked)
                        {
                            //Pick a random waypoint to continue to
                            CurrentWaypoint = Game.Random.Next(Waypoints.Count);

                            //This sometimes happen if we add walls after putting waypoints
                            LogFile.Log.LogEntryDebug(this.Representation + " unrouteable waypoint - picking new waypoint", LogDebugLevel.High);
                            return;
                        }

                        //Temporarily blocked
                        if (pathingResult.MonsterFinalLocation == this.LocationMap)
                        {
                            //Pick a random waypoint to continue to
                            CurrentWaypoint = Game.Random.Next(Waypoints.Count);
                            //Will continue there next turn

                            LogFile.Log.LogEntryDebug(this.Representation + " temporarily blocked - picking new waypoint", LogDebugLevel.Medium);
                            return;
                        }
                        
                        //Walk towards waypoint
                        SetHeadingToMapSquare(pathingResult.MonsterFinalLocation);
                        MoveIntoSquare(pathingResult.MonsterFinalLocation);

                        //We made it? Go to next waypoint
                        if (pathingResult.MonsterFinalLocation == Waypoints[CurrentWaypoint])
                        {
                            int nextWaypoint;
                            if (HasSquarePatrol())
                                nextWaypoint = (CurrentWaypoint + 1) % Waypoints.Count;
                            else
                            {
                                if (WaypointsInc)
                                {
                                    nextWaypoint = CurrentWaypoint + 1;
                                    if (nextWaypoint == Waypoints.Count)
                                    {
                                        WaypointsInc = false;
                                        nextWaypoint = CurrentWaypoint - 1;
                                    }
                                }
                                else
                                {
                                    nextWaypoint = CurrentWaypoint - 1;
                                    if (nextWaypoint < 0)
                                    {
                                        WaypointsInc = true;
                                        nextWaypoint = 1;
                                    }

                                }

                            }

                            LogFile.Log.LogEntryDebug(this.Representation + " reached waypoint " + CurrentWaypoint + ". Moving to waypoint " + nextWaypoint, LogDebugLevel.Low);
                            CurrentWaypoint = nextWaypoint;
                        }
                    }
                    return;

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
                    LogFile.Log.LogEntryDebug(this.Representation + " recovered", LogDebugLevel.Medium);
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
                            LogFile.Log.LogEntryDebug(this.Representation + " fleeing", LogDebugLevel.Medium);
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

                Pathing.PathingResult pathingResult;
                Point nextStep = LocationMap;

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
                    Pathing.PathingPermission permission = Pathing.PathingPermission.Normal;

                    if(CanOpenDoors())
                        permission = Pathing.PathingPermission.IgnoreDoors;

                    pathingResult = Game.Dungeon.Pathing.GetPathToPoint(this.LocationLevel, this.LocationMap, new Point(fleeX, fleeY), PathingType(), permission, IgnoreDangerousTerrain);

                    if (pathingResult.TerminallyBlocked)
                    {
                        counter++;
                        continue;
                    }

                    //Otherwise we found it
                    goodPath = true;
                    nextStep = pathingResult.MonsterFinalLocation;
                    break;
                } while (counter < totalFleeLoops);

                //If we found a good path, walk it
                if (goodPath)
                {
                    SetHeadingToMapSquare(nextStep);
                    MoveIntoSquare(nextStep);
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
                            LogFile.Log.LogEntryDebug(this.SingleDescription + " returns to PC", LogDebugLevel.Low);
                            AIState = SimpleAIStates.Returning;
                            FollowPC();
                        }
                    }
                }

                //Not charmed - pursue and attack
                FollowAndAttack(newTarget);
            }
        }

        /// <summary>
        /// Call before updating LocationMap. Points creature heading at location
        /// </summary>
        /// <param name="nextStep"></param>
        protected void SetHeadingToMapSquare(Point nextStep)
        {
            //If we're not actually moving anywhere, don't alter heading
            if (nextStep == this.LocationMap)
                return;

            Heading = DirectionUtil.AngleFromOriginToTarget(LocationMap, nextStep);
        }

        /// <summary>
        /// Call after updating LocationMap. Aim at the target
        /// </summary>
        protected void SetHeadingToTarget(Creature newTarget)
        {
            Heading = DirectionUtil.AngleFromOriginToTarget(this.LocationMap, newTarget.LocationMap);
        }

        protected virtual void FollowAndAttack(Creature newTarget)
        {
            //Find location of next step on the path towards target

            Pathing.PathingResult pathingResult;
            Pathing.PathingPermission permission = Pathing.PathingPermission.Normal;

            if(CanOpenDoors())
                permission = Pathing.PathingPermission.IgnoreDoors;

            pathingResult = Game.Dungeon.Pathing.GetPathToCreature(this, newTarget, PathingType(), permission, IgnoreDangerousTerrain);

            Point nextStep = pathingResult.MonsterFinalLocation;

            //We are adjacent and can attack
            if (pathingResult.MoveIsInteractionWithTarget)
            {
                //If we can attack, attack the monster or PC
                if(WillAttack()) {

                    CombatResults result;

                    if (newTarget == Game.Dungeon.Player)
                    {
                        result = AttackPlayer(newTarget as Player, false);
                    }
                    else
                    {
                        //It's a normal creature
                        result = AttackMonster(newTarget as Monster, false);
                    }

                    Screen.Instance.DrawMeleeAttack(this, newTarget, result);

                    //If we killed it, move into its square
                    if (result == CombatResults.DefenderDied &&
                        !(newTarget == Game.Dungeon.Player && Game.Dungeon.PlayerImmortal))
                    {
                        nextStep = currentTarget.LocationMap;
                    }
                }
            }

            //If we are permanently blocked, return to patrol state
            if (pathingResult.TerminallyBlocked)
            {
                LogFile.Log.LogEntryDebug(this.Representation + " permanently blocked (door), returning to patrol ", LogDebugLevel.Medium);
                AIState = SimpleAIStates.Patrol;
                return;

            }

            //Otherwise (or if the creature died), move towards it (or its corpse)
            if(WillPursue()) {
                
                //If we want to pursue, move towards the creature
                if (CanMove())
                {
                    MoveIntoSquare(nextStep);
                    SetHeadingToTarget(newTarget);
                }
            }
            else {
                //If we don't we continue our normal Patrol route
                //(we are set to Pursuit in the AI though)
                DoPatrol();
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
            Pathing.PathingPermission permission = Pathing.PathingPermission.Normal;
            if (CanOpenDoors())
                permission = Pathing.PathingPermission.IgnoreDoors;

            var pathingResult = Game.Dungeon.Pathing.GetPathToCreature(this, player, PathingType(), permission, IgnoreDangerousTerrain);

            if (pathingResult.TerminallyBlocked)
            {
                //No path
                //Need to check this really
                AIState = SimpleAIStates.Patrol;
                return;
            }

            MoveIntoSquare(pathingResult.MonsterFinalLocation);   
            SetHeadingToTarget(player);
        }
    
        /// <summary>
        /// Move into the square, opening doors as necessary
        /// </summary>
        /// <param name="nextStep"></param>
        protected void MoveIntoSquare(Point nextStep) {

            MapTerrain doorTerrain = Game.Dungeon.GetTerrainAtPoint(this.LocationLevel, nextStep);
    
            if(doorTerrain == MapTerrain.ClosedDoor) {
                Game.Dungeon.OpenDoor(new Location(this.LocationLevel, nextStep));
                LogFile.Log.LogEntryDebug(this.Representation + " opened door", LogDebugLevel.Medium);
            }

            //Apply any dangerous terrain effects
            Game.Dungeon.MonsterInteractWithActiveFeature(this, this.LocationLevel, nextStep);

            LocationMap = nextStep;
        }

        public void RecoverOnBeingHit()
        {
            if (AIState == SimpleAIStates.Fleeing &&
                Game.Random.Next(100) < GetChanceToRecoverOnBeingHit())
            {
                AIState = SimpleAIStates.Pursuit;
                LogFile.Log.LogEntryDebug(this.Representation + " recovers and returns to the fight", LogDebugLevel.Low);
            }
            if(StunnedTurns != 6)
                StunnedTurns = 0;
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
            if (currentTarget != null && creature != currentTarget)
            {
                LogFile.Log.LogEntryDebug(this.Representation + " changes target from " + currentTarget.Representation + " to " + creature.Representation, LogDebugLevel.Low);
            }
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


        /// <summary>
        /// Does the creature pursue + lock-on to other creatures?
        /// </summary>
        protected virtual bool WillPursue()
        {
            //By default creatures pursue
            return true;
        }

        /// <summary>
        /// Does the creature always do their patrol option, even if they are engaged with a target (e.g. fixed rotating turrets)?
        /// </summary>
        protected virtual bool WillAlwaysPatrol()
        {
            //By default creatures don't always patrol
            return false;
        }

        /// <summary>
        /// Does the creature has the ability to attack the PC and other creatures?
        /// </summary>
        /// <returns></returns>
        protected virtual Boolean WillAttack()
        {

            return true;
        }

        /// <summary>
        /// Override to set what patrol type (default move behaviour) this creature has
        /// </summary>
        /// <returns></returns>
        public virtual PatrolType GetPatrolType()
        {
            //RW may be safer than static as a default
            return PatrolType.RandomWalk;
        }

        /// <summary>
        /// If set to Rotate or Waypoints patrol, do we go clockwise or anti-clockwise?
        /// </summary>
        /// <returns></returns>
        public virtual bool GetPatrolRotationClockwise()
        {
            return false;
        }

        /// <summary>
        /// How many turns does it take to do one rotation
        /// </summary>
        /// <returns></returns>
        protected virtual int GetPatrolRotationSpeed()
        {
            return 1;
        }

        /// <summary>
        /// How many radians to turn in one rotation?
        /// </summary>
        /// <returns></returns>
        protected virtual double GetPatrolRotationAngle()
        {
            return Math.PI / 4;
        }

        /// <summary>
        /// Set to false to ignore sounds. Can't move already ignore sounds
        /// </summary>
        /// <returns></returns>
        protected virtual bool WillInvestigateSounds()
        {
            return true;
        }

        public List<Point> Waypoints
        {
            get
            {
                return wayPoints;
            }
            set
            {
                wayPoints = value;
            }
        }

        //For serialisation
        public int RotationTurns
        {
            get
            {
                return rotationTurns;
            }
            set
            {
                rotationTurns = value;
            }
        }

        public bool WaypointsInc
        {
            get
            {
                return waypointsInc;
            }
            set
            {
                waypointsInc = value;
            }

        }



        /// <summary>
        /// Monster can open doors, so uses with-opened-doors routing
        /// </summary>
        /// <returns></returns>
        public virtual bool CanOpenDoors() {
            return false;
        }

        /// <summary>
        /// Used for creatures which follow square patrol patterns.
        /// 
        /// For SquarePatrol use CreatureStartPosAndWaypoints()
        /// For LinearPatrol use CreatureStartPosAndWaypointsSisterRooms()
        /// </summary>
        /// <returns></returns>
        public virtual bool HasSquarePatrol()
        {
            return false;
        }

        public override bool OnPatrol()
        {
            return AIState == SimpleAIStates.Patrol;
        }

        public override bool InPursuit()
        {
            return AIState == SimpleAIStates.Pursuit;
        }
    }
}
