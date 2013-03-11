using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{

    /// <summary>
    /// LichAI is a hang over from DDRogue but I'm keeping around in case I want to do a forward port
    /// </summary>
    public abstract class MonsterLichAI  : Monster
    {
        public SimpleAIStates AIState { get; set; }
        protected Creature currentTarget;

        public new bool Sleeping { get; set;}

        public bool HasActivated { get; set; }

        public int MaxSummons { get; set; }

        public MonsterLichAI()
        {
            AIState = SimpleAIStates.Patrol;
            currentTarget = null;

            Sleeping = true;
            HasActivated = false;
        }


        protected abstract double GetMissileRange();

        protected abstract string GetWeaponName();
        
        /// <summary>
        /// Run the Simple AI actions
        /// </summary>
        public override void ProcessTurn()
        {
            //If in pursuit state, continue to pursue enemy until it is dead (or creature itself is killed) [no FOV used after initial target selected]

            //If in randomWalk state, look for new enemies in FOV.
            //Closest enemy becomes new target

            if (Sleeping)
                return;

            if (!HasActivated)
            {
                //Summon initial creatures
                SummonInitialCreatures();
                HasActivated = true;
            }

            //If no targets, move randomly

            //This shouldn't happen with the lich

            Random rand = Game.Random;


            if (AIState == SimpleAIStates.Pursuit)
            {
                //Pursuit state, continue chasing and attacking target

                //Check we have a valid target (may not after reload)
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
                //Is target on another level (i.e. has escaped down the stairs)
                else if (currentTarget.LocationLevel != this.LocationLevel)
                {
                    AIState = SimpleAIStates.Patrol;
                }
                else
                {
                    //Otherwise continue to chase

                    ChaseCreature(currentTarget);
                }
            }

            if (AIState == SimpleAIStates.Patrol)
            {
                //RandomWalk state

                //Search an area of sightRadius on either side for creatures and check they are in the FOV

                Map currentMap = Game.Dungeon.Levels[LocationLevel];

                //Get the FOV from Dungeon (this also updates the map creature FOV state)
                CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(this);
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
                if (xr >= currentMap.width)
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
                if (creaturesInFOV.Contains(Game.Dungeon.Player))
                {
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

        private void SummonInitialCreatures()
        {
            //Summon a few skellies and zombies
            int no = 2 + Game.Random.Next(3);

            TrySummoning(this.LocationMap);
            TrySummoning(this.LocationMap);
            TrySummoning(this.LocationMap);
            TrySummoning(this.LocationMap);
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

        private void ChaseCreature(Creature newTarget)
        {
            //Confirm this as current target
            currentTarget = newTarget;

            //Go into pursuit mode
            AIState = SimpleAIStates.Pursuit;

            //If we are in range, fire
            double range = Game.Dungeon.GetDistanceBetween(this, newTarget);

            if (range < GetMissileRange() / 2.0)
            {
                //Too close creature will try to back away
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
                    //if not, continue attacking
                    //Fire at the player
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
                }
            }

            else if (range < GetMissileRange() + 0.005)
            {
                //In preference they will use their special ability rather than fighting

                //Try to use special
                bool usingSpecial = UseSpecialAbility();

                if (!usingSpecial)
                {
                    //In range

                    //Fire at the player
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
                }
            }
            else
            {
                //If not, move towards the player

                //Find location of next step on the path towards them
                Point nextStep = Game.Dungeon.GetPathTo(this, newTarget);

                bool moveIntoSquare = true;

                //If this is the same as the target creature's location, we are adjacent. Something is wrong, but attack anyway
                if (nextStep.x == newTarget.LocationMap.x && nextStep.y == newTarget.LocationMap.y)
                {
                    LogFile.Log.LogEntry("SimpleThrowingAI: Adjacent to target and still moving towardws");
                    //Fire at the player
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
                }

                //Otherwise (or if the creature died), move towards it (or its corpse)
                if (moveIntoSquare)
                {
                    LocationMap = nextStep;
                }
            }
        }

        protected override string HitsPlayerCombatString()
        {
            return "The " + this.SingleDescription + " " + GetWeaponName() + " at you. It hits.";
        }

        protected override string MissesPlayerCombatString()
        {
            return "The " + this.SingleDescription + " " + GetWeaponName() + " at you. It misses.";
        }

        protected override string HitsMonsterCombatString(Monster target)
        {
            return "The " + this.SingleDescription + " " + GetWeaponName() + " at the " + target.SingleDescription + ". It hits.";
        }

        protected override string MissesMonsterCombatString(Monster target)
        {
            return "The " + this.SingleDescription + " " + GetWeaponName() + " at the " + target.SingleDescription + ". It misses.";
        }

        /*
        private void ChaseCreature(Creature newTarget)
        {
            //Confirm this as current target
            currentTarget = newTarget;

            //Go into pursuit mode
            AIState = SimpleAIStates.Pursuit;

            //If we are in range, fire
            double range = Game.Dungeon.GetDistanceBetween(this, newTarget);

            if (range < GetMissileRange() + 0.005)
            {
                //In preference they will use their special ability rather than fighting

                //Try to use special
                bool usingSpecial = UseSpecialAbility();

                if (!usingSpecial)
                {
                   //In range

                    //Fire at the player
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
                }
            }
            else
            {
                //If not, move towards the player

                //Find location of next step on the path towards them
                Point nextStep = Game.Dungeon.GetPathTo(this, newTarget);

                bool moveIntoSquare = true;

                //If this is the same as the target creature's location, we are adjacent. Something is wrong, but attack anyway
                if (nextStep.x == newTarget.LocationMap.x && nextStep.y == newTarget.LocationMap.y)
                {
                    LogFile.Log.LogEntry("SimpleThrowingAI: Adjacent to target and still moving towardws");
                    //Fire at the player
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
                }

                //Otherwise (or if the creature died), move towards it (or its corpse)
                if (moveIntoSquare)
                {
                    LocationMap = nextStep;
                }
            }
        }*/

        private bool TrySummoning(Point destCentre)
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            //Count the number of summons already there
            int noSummons = 0;

            foreach (Monster monster in Game.Dungeon.Monsters)
            {
                if (monster.LocationLevel == this.LocationLevel)
                    noSummons++;
            }

            //10 + lich
            //Too many summons
            if (noSummons > MaxSummons)
                return false;


            //Tries to summon

            //Find a square adjacent to player
            Point playerSq = destCentre;

            //Free square
            Point freeSq = new Point(-1, -1);
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    bool enterable = dungeon.MapSquareIsWalkable(player.LocationLevel, new Point(playerSq.x + i, playerSq.y + j));
                    SquareContents sq = dungeon.MapSquareContents(player.LocationLevel, new Point(playerSq.x + i, playerSq.y + j));

                    if (enterable && sq.empty)
                        freeSq = new Point(playerSq.x + i, playerSq.y + j);

                }
            }

            if (freeSq.x == -1 || freeSq.y == -1)
            {
                return false;
            }

            bool raisedSuccess;

            //Have a free square to summon into
            if (Game.Random.Next(10) < 5)
            {
                raisedSuccess = Game.Dungeon.AddMonsterDynamic(new Creatures.Zombie(), player.LocationLevel, freeSq);
            }
            else
                raisedSuccess = Game.Dungeon.AddMonsterDynamic(new Creatures.Skeleton(), player.LocationLevel, freeSq);

            return raisedSuccess;
        }

        private bool UseSpecialAbility()
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            //Check if they are going to use their special at all

            //Lich always try to use a special ability

            //Lich will summon up to maximum and then heal

            //Summoning

            bool summonSuccess = TrySummoning(player.LocationMap);

            if (summonSuccess)
                return true;

            //If that didn't work try healing
            bool healingSuccess = TryHealing();

            if (healingSuccess)
                return true;

            return false;
        }

        private bool TryHealing()
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = Game.Dungeon.Player;

            //Look for injured creatures within range
            List<Monster> targetsInRange = new List<Monster>();

            foreach (Monster monster in Game.Dungeon.Monsters)
            {
                if (this.LocationLevel != monster.LocationLevel)
                    continue;

                //Can't heal yourself
                if (monster == this)
                    continue;

                if (Game.Dungeon.GetDistanceBetween(this, monster) < GetMissileRange() + 0.005)
                {
                    targetsInRange.Add(monster);
                }
            }

            //See if any of them are injured
            List<Monster> injuredTargets = targetsInRange.FindAll(x => x.Hitpoints < x.MaxHitpoints);

            if (injuredTargets.Count == 0)
                return false;

            //Pick a random monster
            Monster actualTarget = injuredTargets[Game.Random.Next(injuredTargets.Count)];

            //Heal this monster
            int oldHP = actualTarget.Hitpoints;
            actualTarget.Hitpoints += (int)(Game.Random.Next(actualTarget.MaxHitpoints - actualTarget.Hitpoints) / 3.0);

            //Update msg
            Game.MessageQueue.AddMessage("The " + this.SingleDescription + " heals the " + actualTarget.SingleDescription);
            LogFile.Log.LogEntryDebug(actualTarget.SingleDescription + " hp: " + oldHP + " -> " + actualTarget.Hitpoints, LogDebugLevel.Medium);

            //We used this ability
            return true;
        }

        /// <summary>
        /// Raise a corpse. Virtual so different raises can raise different things.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="locationMap"></param>
        /// <returns></returns>
        protected virtual bool RaiseCorpse(int level, Point locationMap) {
            return false;
        }

        /*
        public override CombatResults AttackPlayer(Player player)
        {
            //Recalculate combat stats if required
            if (this.RecalculateCombatStatsRequired)
                this.CalculateCombatStats();

            if (player.RecalculateCombatStatsRequired)
                player.CalculateCombatStats();

            //Calculate damage from a normal attack
            int damage = AttackCreatureWithModifiers(player, 0, 0, 0, 0);

            //Player side
            string resultPhrase;
            if (damage > 0)
                resultPhrase = "hits";
            else
                resultPhrase = "misses";

            //Do we hit the player?
            if (damage > 0)
            {
                int monsterOrigHP = player.Hitpoints;

                player.Hitpoints -= damage;

                //Is the player dead, if so kill it?
                if (player.Hitpoints <= 0)
                {
                    Game.Dungeon.PlayerDeath("was killed by a " + this.SingleDescription);

                    //Debug string
                    string combatResultsMsg = "MvP ToHit: " + toHitRoll + " AC: " + player.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + player.Hitpoints + " killed";
                    
                    LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);


                    string playerMsg = "The " + this.SingleDescription + " " + GetWeaponName() + " at you. It " + resultPhrase + ". You die.";
                    Game.MessageQueue.AddMessage(playerMsg);

                    return CombatResults.DefenderDied;
                }

                //Debug string
                string combatResultsMsg3 = "MvP ToHit: " + toHitRoll + " AC: " + player.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + monsterOrigHP + "->" + player.Hitpoints + " injured";

                string playerMsg3 = "The " + this.SingleDescription + " " + GetWeaponName() + " at you. It " + resultPhrase + ".";
                Game.MessageQueue.AddMessage(playerMsg3);
                LogFile.Log.LogEntryDebug(combatResultsMsg3, LogDebugLevel.Medium);

                return CombatResults.NeitherDied;
            }

            //Miss
            string combatResultsMsg2 = "MvP ToHit: " + toHitRoll + " AC: " + player.ArmourClass() + " Dam: 1d" + damageBase + "+" + damageModifier + " MHP: " + player.Hitpoints + " miss";
            
            string playerMsg2 = "The " + this.SingleDescription + " " + GetWeaponName() + " at you. It " + resultPhrase + ".";
            Game.MessageQueue.AddMessage(playerMsg2);
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
        }*/
        /// <summary>
        /// On death, null our currentTarget. This is a good idea anyway, but looks like it was added for an important reason 'circular references'
        /// </summary>
        public override void NotifyMonsterDeath()
        {
            currentTarget = null;
        }

        //Do nothing
        public override void NotifyHitByCreature(Creature creature, int damage)
        {
            return;
        }

    }
}