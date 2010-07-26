using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    public enum SpecialAIType
    {
        Healer,
        Raiser,
        Summoner,
        PlayerEffecter,
        MonsterEffecter,
        PlayerCaster,
        Dragon
    };

    /// <summary>
    /// There are different types of special AI but they all use the MonsterThrowAndRun AI base.
    /// Their special action (healing, raising, summoning etc.) differs. They all have missile weapons.
    /// </summary>
    public abstract class MonsterSpecialAI : MonsterFightAndRunAI
    {
        //public SimpleAIStates AIState { get; set; }
        //protected Creature currentTarget;

        public MonsterSpecialAI()
        {
            AIState = SimpleAIStates.RandomWalk;
            currentTarget = null;
        }

        protected abstract SpecialAIType GetSpecialAIType();

        /// <summary>
        /// Only used for the effector AI
        /// </summary>
        /// <returns></returns>
        protected virtual PlayerEffect GetSpecialAIEffect() { return null; }


        protected virtual Spell GetSpecialAISpell() { return null; }

        /// <summary>
        /// Does the player resist the attack?
        /// </summary>
        /// <returns></returns>
        protected virtual bool DoPlayerResistance() { return false;  }

        protected virtual string EffectAttackString() { return ""; }

        protected abstract double GetMissileRange();

        protected abstract string GetWeaponName();

        /// <summary>
        /// Color of the projectile
        /// </summary>
        /// <returns></returns>
        protected virtual Color GetWeaponColor()
        {
            return ColorPresets.DarkGray;
        }

        protected override string HitsPlayerCombatString()
        {
            string combatStr = "";

            if (!Unique)
                combatStr = "The ";

            return combatStr + this.SingleDescription + " " + GetWeaponName() + " at you. It hits.";
        }

        protected override string MissesPlayerCombatString()
        {
            string combatStr = "";

            if (!Unique)
                combatStr = "The ";

            return combatStr + this.SingleDescription + " " + GetWeaponName() + " at you. It misses.";
        }

        protected override string HitsMonsterCombatString(Monster target)
        {
            string combatStr = "";

            if (!Unique)
                combatStr = "The ";

            return combatStr + this.SingleDescription + " " + GetWeaponName() + " at the " + target.SingleDescription + ". It hits.";
        }

        protected override string MissesMonsterCombatString(Monster target)
        {
            string combatStr = "";

            if (!Unique)
                combatStr = "The ";

            return combatStr + this.SingleDescription + " " + GetWeaponName() + " at the " + target.SingleDescription + ". It misses.";
        }

        /// <summary>
        /// Can't charge these - too complex with healing and summons and stuff
        /// </summary>
        /// <returns></returns>
        public override bool CanBeCharmed()
        {
            return false;
        }

        /// <summary>
        /// Override the following code from the hand to hand AI to give us some range and to use special abilities.
        /// This is almost identical to ThrowAndRunAI and could be combined perhaps?
        /// </summary>
        /// <param name="newTarget"></param>
        protected override void FollowAndAttack(Creature newTarget)
        {
            double range = Game.Dungeon.GetDistanceBetween(this, newTarget);

            //Back away if we are too close & can see the target
            //If we can't see the target, don't back away
            TCODFov currentFOV = Game.Dungeon.CalculateCreatureFOV(this);

            bool backAwayFromTarget = false;

            if (range < GetMissileRange() / 2.0 && currentFOV.CheckTileFOV(newTarget.LocationMap.x, newTarget.LocationMap.y))
            {
                //Check our chance to back away. For a hard/clever creature this is 100. For a stupid creature it is 0.
                 if (Game.Random.Next(100) < GetChanceToBackAway())
                 {
                     backAwayFromTarget = true;
                 }
            }

            if(backAwayFromTarget && CanMove()) {
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
                    //Get a to-flee to square adjacent to the creature
                    //Performs very badly

                    /*
                    fleeX = this.LocationMap.x + Game.Random.Next(2) - 1;
                    fleeY = this.LocationMap.y + Game.Random.Next(2) - 1;

                    if (fleeX < 0 || fleeX >= Game.Dungeon.Levels[this.LocationLevel].width)
                    {
                        LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail off map", LogDebugLevel.Low);
                        continue;
                    }

                    if (fleeY < 0 || fleeY >= Game.Dungeon.Levels[this.LocationLevel].height)
                    {
                        LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail off map", LogDebugLevel.Low);
                        continue;
                    }*/

                    //This performs badly when there are few escape options and you are close to the edge of the map
                    fleeX = Game.Random.Next(Game.Dungeon.Levels[this.LocationLevel].width);
                    fleeY = Game.Random.Next(Game.Dungeon.Levels[this.LocationLevel].height);

                    //Relax conditions if we are having a hard time
                    if (counter > relaxDirectionAt)
                        relaxDirection = true;

                    //Find the square to move to
                    nextStep = Game.Dungeon.GetPathFromCreatureToPoint(this.LocationLevel, this, new Point(fleeX, fleeY));

                    //Check the square is pathable to
                    if (nextStep.x == LocationMap.x && nextStep.y == LocationMap.y)
                    {
                        counter++;
                        //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail unpathable", LogDebugLevel.Low);
                        continue;
                    }

                    //Check that the next square is in a direction away from the attacker
                    int deltaFleeX = nextStep.x - this.LocationMap.x;
                    int deltaFleeY = nextStep.y - this.LocationMap.y;

                    if (!relaxDirection)
                    {
                        if (deltaFleeX > 0 && deltaX > 0)
                        {
                            counter++;
                            //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail direction", LogDebugLevel.Low);
                            continue;
                        }
                        if (deltaFleeX < 0 && deltaX < 0)
                        {
                            counter++;
                            //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail direction", LogDebugLevel.Low);
                            continue;
                        }
                        if (deltaFleeY > 0 && deltaY > 0)
                        {
                            counter++;
                            //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail direction", LogDebugLevel.Low);
                            continue;
                        }
                        if (deltaFleeY < 0 && deltaY < 0)
                        {
                            counter++;
                            //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail direction", LogDebugLevel.Low);
                            continue;
                        }
                    }

                    //Check the square is empty
                    bool isEnterable = Game.Dungeon.MapSquareIsWalkable(this.LocationLevel, new Point(fleeX, fleeY));
                    if (!isEnterable)
                    {
                        counter++;
                        //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail enterable", LogDebugLevel.Low);
                        continue;
                    }

                    //Check the square is empty of creatures
                    SquareContents contents = Game.Dungeon.MapSquareContents(this.LocationLevel, new Point(fleeX, fleeY));
                    if (contents.monster != null)
                    {
                        counter++;
                        //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail blocked", LogDebugLevel.Low);
                        continue;
                    }


                    //Check that the target is visible from the square we are fleeing to
                    //This may prove to be too expensive

                    TCODFov projectedFOV = Game.Dungeon.CalculateCreatureFOV(this, nextStep);

                    if (!projectedFOV.CheckTileFOV(newTarget.LocationMap.x, newTarget.LocationMap.y))
                    {
                        counter++;
                        //LogFile.Log.LogEntryDebug("MonsterThrowAndRunAI: Back away fail fov", LogDebugLevel.Low);
                        continue;
                    }

                    //Otherwise we found it
                    goodPath = true;
                    break;
                } while (counter < totalFleeLoops);

                LogFile.Log.LogEntryDebug("Back away results. Count: " + counter + " Direction at: " + relaxDirectionAt + " Total: " + totalFleeLoops, LogDebugLevel.Low);

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

                    //Missile animation
                    Screen.Instance.DrawMissileAttack(this, new Point(LocationMap.x, LocationMap.y), new Point(newTarget.LocationMap.x, newTarget.LocationMap.y), GetWeaponColor());
                }
            }
            //Not so close we want to back away
            else if (range < GetMissileRange() + 0.005)
            {
                //In range

                //Check FOV. If not in FOV, chase the player.
                if (!currentFOV.CheckTileFOV(newTarget.LocationMap.x, newTarget.LocationMap.y))
                {
                    ContinueChasing(newTarget);
                    return;
                }

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

                    //Missile animation
                    Screen.Instance.DrawMissileAttack(this, new Point(LocationMap.x, LocationMap.y), new Point(newTarget.LocationMap.x, newTarget.LocationMap.y), GetWeaponColor());
                }
            }
            //Not too close, not in range, chase the target
            else
            {
                ContinueChasing(newTarget);
            }
        }

        private void ContinueChasing(Creature newTarget)
        {
            //If not, move towards the player

            //Return if we can't move
            if (!CanMove())
                return;

            //Find location of next step on the path towards them
            Point nextStep = Game.Dungeon.GetPathTo(this, newTarget);

            bool moveIntoSquare = true;

            //If this is the same as the target creature's location, we are adjacent. Something is wrong, but attack anyway
            if (nextStep.x == newTarget.LocationMap.x && nextStep.y == newTarget.LocationMap.y)
            {
                LogFile.Log.LogEntryDebug("MonsterSpecialAI: Adjacent to target and still moving towards", LogDebugLevel.High);
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

                //Missile animation
                Screen.Instance.DrawMissileAttack(this, new Point(LocationMap.x, LocationMap.y), new Point(newTarget.LocationMap.x, newTarget.LocationMap.y), GetWeaponColor());
            }

            //Otherwise (or if the creature died), move towards it (or its corpse)
            if (moveIntoSquare)
            {
                LocationMap = nextStep;
            }
        }
        /// <summary>
        /// Chance that monster backs away from player before attacking. Default 100.
        /// </summary>
        /// <returns></returns>
        protected virtual int GetChanceToBackAway()
        {
            return 100;
        }

        private bool UseSpecialAbility()
        {
            //Check if they are going to use their special at all
            if(Game.Random.Next(100) > GetUseSpecialChance()) {
                return false;
            }

            if (GetSpecialAIType() == SpecialAIType.Healer)
            {
                //Look for injured creatures within range
                List<Monster> targetsInRange = new List<Monster>();

                foreach (Monster monster in Game.Dungeon.Monsters)
                {
                    if (this.LocationLevel != monster.LocationLevel)
                        continue;

                    //Can't heal yourself
                    if (monster == this)
                        continue;

                    //Don't healed charmed monsters either
                    if (Game.Dungeon.GetDistanceBetween(this, monster) < GetMissileRange() + 0.005
                        && !monster.Charmed)
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

            else if (GetSpecialAIType() == SpecialAIType.Raiser)
            {
                //Look for a nearby corpse
                //Look for injured creatures within range
                List<Feature> corpseInRange = new List<Feature>();

                foreach (Feature feature in Game.Dungeon.Features)
                {
                    if (this.LocationLevel != feature.LocationLevel)
                        continue;

                    if (Game.Dungeon.GetDistanceBetween(this, feature) < GetMissileRange() + 0.005)
                    {
                        if (feature is Features.Corpse)
                        {
                            corpseInRange.Add(feature);
                        }
                    }
                }

                if (corpseInRange.Count == 0)
                    return false;

                //Pick a corpse at random
                Feature actualCorpse = corpseInRange[Game.Random.Next(corpseInRange.Count)];

                //Check this square is empty
                int corpseLevel = actualCorpse.LocationLevel;
                Point corpseMap = actualCorpse.LocationMap;

                SquareContents contents = Game.Dungeon.MapSquareContents(corpseLevel, corpseMap);

                if (!contents.empty)
                    return false;

                //Raise a creature here

                //For now just raise skeletons I think we might need to make a separate AI for each raisey creature
                Game.Dungeon.Features.Remove(actualCorpse); //should have a helper for this really

                //Spawn a skelly
                bool raisedSuccess = RaiseCorpse(actualCorpse.LocationLevel, actualCorpse.LocationMap);

                if (raisedSuccess)
                {
                    Game.MessageQueue.AddMessage("The " + this.SingleDescription + " tries to raise a corpse!");
                    LogFile.Log.LogEntryDebug(this.SingleDescription + " raises corpse", LogDebugLevel.Medium);
                }
                return raisedSuccess;

            }
            //Effect on player. Know we are in range if this was called
            else if (GetSpecialAIType() == SpecialAIType.PlayerEffecter) {

                //Shouldn't happen if charmed

                LogFile.Log.LogEntryDebug(this.SingleDescription + " attempting player effect attack", LogDebugLevel.Medium);

                //Player already has this effect
                Player player = Game.Dungeon.Player;

                PlayerEffect effectToUse = GetSpecialAIEffect();

                if(effectToUse == null) {
                    LogFile.Log.LogEntryDebug(this.SingleDescription + " error getting effect", LogDebugLevel.High);
                    return false;
                }

                //Don't do it twice
                if(player.IsEffectActive(effectToUse.GetType())) {
                    return false;
                }

                string attackStr = EffectAttackString();

                Game.MessageQueue.AddMessage("The " + this.SingleDescription + " tries to " + attackStr + " you!");

                //Player resistance
                bool playerResistance = DoPlayerResistance();

                if(playerResistance == true) {
                    Game.MessageQueue.AddMessage("You resist the attack.");
                    return true;
                }

                //If failed, we add our effect
                player.AddEffect(effectToUse);

                return true;

            }
            //Spellkon player. Know we are in range if this was called
            else if (GetSpecialAIType() == SpecialAIType.PlayerCaster)
            {

                //Shouldn't happen if charmed

                LogFile.Log.LogEntryDebug(this.SingleDescription + " attempting player spell attack", LogDebugLevel.Medium);

                //Player already has this effect
                Player player = Game.Dungeon.Player;

                Spell effectToUse = GetSpecialAISpell();

                if (effectToUse == null)
                {
                    LogFile.Log.LogEntryDebug(this.SingleDescription + " error getting spell", LogDebugLevel.High);
                    return false;
                }

                string attackStr = EffectAttackString();

                Game.MessageQueue.AddMessage("The " + this.SingleDescription + " tries to " + attackStr + " you!");

                //Player resistance
                bool playerResistance = DoPlayerResistance();

                if (playerResistance == true)
                {
                    Game.MessageQueue.AddMessage("You resist the attack.");
                    return true;
                }

                //If failed, we add our effect
                effectToUse.DoSpell(player.LocationMap);

                return true;

            }
            

            //Dragon can do a variety of things
            else if (GetSpecialAIType() == SpecialAIType.Dragon)
            {
                Player player = Game.Dungeon.Player;

                //Are we injured? If so, try to heal ourselves

                if (this.Hitpoints < (int)Math.Floor(this.MaxHitpoints / 2.0))
                {
                    int oldHP = this.Hitpoints;
                    this.Hitpoints += (int)(Game.Random.Next(this.MaxHitpoints - this.Hitpoints) / 5.0);

                    //Update msg
                    Game.MessageQueue.AddMessage("The Dragon heals itself!");
                    LogFile.Log.LogEntryDebug(this.SingleDescription + " hp: " + oldHP + " -> " + this.Hitpoints, LogDebugLevel.Medium);

                    return true;
                }

                //If not, screw around with the player a bit

                //50% chance we will just attack

                if (Game.Random.Next(100) < 50)
                {
                    return false;
                }

               //Otherwise decide what we're going to do

                int taskNo = Game.Random.Next(3);

                if (taskNo == 0)
                {
                    //Player already has this effect
                    /*
                    Spell effectToUse = new Spells.Blink();

                    if (effectToUse == null)
                    {
                        LogFile.Log.LogEntryDebug(this.SingleDescription + " error getting spell", LogDebugLevel.High);
                        return false;
                    }

                    Game.MessageQueue.AddMessage("The Dragon tries to teleport you!");

                    //Player resistance
                    bool playerResistance = DoPlayerResistance();

                    if (playerResistance == true)
                    {
                        Game.MessageQueue.AddMessage("You resist the attack.");
                        return true;
                    }

                    //If failed, we add our effect
                    effectToUse.DoSpell(player.LocationMap);

                    return true;*/

                    return false;

                }
                else if (taskNo == 1)
                {
                    int duration = 2 * Creature.turnTicks + Game.Random.Next(5 * Creature.turnTicks);

                    PlayerEffects.SpeedDown speedDownEff = new RogueBasin.PlayerEffects.SpeedDown(duration, 30);

                    if (speedDownEff == null)
                    {
                        LogFile.Log.LogEntryDebug(this.SingleDescription + " error getting effect", LogDebugLevel.High);
                        return false;
                    }

                    //Don't do it twice
                    if (player.IsEffectActive(typeof(PlayerEffects.SpeedDown)))
                    {
                        return false;
                    }
                    Game.MessageQueue.AddMessage("The Dragon tries to slow you!");

                    //Player resistance
                    bool playerResistance = DoPlayerResistance();

                    if (playerResistance == true)
                    {
                        Game.MessageQueue.AddMessage("You resist the attack.");
                        return true;
                    }

                    //If failed, we add our effect
                    player.AddEffect(speedDownEff);

                }
                else
                {
                    int duration = 3 * Creature.turnTicks + Game.Random.Next(5 * Creature.turnTicks);
                    int playerSight = Game.Dungeon.Player.SightRadius;
                    int sightDown = playerSight - 1;

                    PlayerEffects.SightRadiusDown sightDownEff = new RogueBasin.PlayerEffects.SightRadiusDown(duration, sightDown);

                    if (sightDownEff == null)
                    {
                        LogFile.Log.LogEntryDebug(this.SingleDescription + " error getting effect", LogDebugLevel.High);
                        return false;
                    }

                    //Don't do it twice
                    if (player.IsEffectActive(typeof(PlayerEffects.SightRadiusDown)))
                    {
                        return false;
                    }

                    Game.MessageQueue.AddMessage("The Dragon tries to blind you!");

                    //Player resistance
                    bool playerResistance = DoPlayerResistance();

                    if (playerResistance == true)
                    {
                        Game.MessageQueue.AddMessage("You resist the attack.");
                        return true;
                    }

                    //If failed, we add our effect
                    player.AddEffect(sightDownEff);

                }

                return true;

            }
            else
            {
                //Summoner not implemented yet
                return false;
            }
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

     protected abstract int GetUseSpecialChance();
    }
}