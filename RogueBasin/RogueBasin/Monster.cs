using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    /// <summary>
    /// What happened during a combat
    /// </summary>
    public enum CombatResults
    {
        AttackerDied, DefenderDied, BothDied, NeitherDied, DefenderDamaged, DefenderUnhurt
    }

    /// <summary>
    /// Simplest class for a Monster.
    /// Subclasses will have things like better AI.
    /// Real monsters will inherit off whichever subclass they like
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Bat))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.BlackUnicorn))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.BlackUnicornUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Demon))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.DragonUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Drainer))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Faerie))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.FaerieUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.FerretUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.GoblinWitchdoctorUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Imp))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Maleficarum))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.MaleficarumUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Meddler))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.NecromancerUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Nymph))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Ogre))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.OgreUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.OrcShamanUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Overlord))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.OverlordUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Peon))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Pixie))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.PixieUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.RatUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.SkeletonUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.SpiderUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Uruk))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.UrukUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Whipper))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Rat))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Goblin))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.GoblinWitchdoctor))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Necromancer))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Orc))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.OrcShaman))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Skeleton))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Spider))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Zombie))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Ferret))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Bugbear))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Ghoul))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.SkeletalArcher))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.GhoulUnique))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Mushroom))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Statue))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Drone))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.RotatingTurret))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.PatrolBot))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.PatrolBotArea))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.AlertBot))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.PerimeterBot))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Swarmer))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.ComputerNode))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.RollingBomb))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.Juggernaut))]
    [System.Xml.Serialization.XmlInclude(typeof(Creatures.CombatBot))]
    public abstract class Monster : Creature, ITurnAI
    {
        /// <summary>
        /// Effects current active on this monster
        /// </summary>
        public List<MonsterEffect> effects { get; private set; }



        public Monster(int level)
        {
            Level = level;
            Initalise();
        }

        public Monster()
        {
            Initalise();
        }

        private void Initalise()
        {
            effects = new List<MonsterEffect>();

            //Set up attributes from class start values
            //maxHitpoints = Game.Dungeon.LevelScalingCalculation(ClassMaxHitpoints(), Level);

            maxHitpoints = ClassMaxHitpoints();

            hitpoints = maxHitpoints;
            //LogFile.Log.LogEntryDebug("Setting monster " + SingleDescription + " HP to " + hitpoints + " on construction", LogDebugLevel.Medium);

            SightRadius = NormalSightRadius;

            WasSummoned = false;
            //Calculate our combat stats
            CalculateCombatStats();

            Charmed = false;
            Passive = false;
            UnpassifyOnAttacked = true;
            WakesOnAttacked = true;

            Sleeping = true;

            Unique = false;

            //Most monsters are sensible
            IgnoreDangerousTerrain = false;
        }

        /// <summary>
        /// Used for spawning
        /// </summary>
        /// <returns></returns>
        public abstract Monster NewCreatureOfThisType();

        public bool Unique { get; set; }

        /// <summary>
        /// Current hitpoints
        /// </summary>
        int hitpoints;

        /// <summary>
        /// Maximum hitpoints
        /// </summary>
        int maxHitpoints;

        /// <summary>
        /// The monster is stunned for this many turns, and will miss this many AI turns
        /// </summary>
        public int StunnedTurns
        {
            get;
            set;
        }

        /// <summary>
        /// The monster is reloading for this many turns, and will miss this many AI turns
        /// </summary>
        public int ReloadingTurns
        {
            get;
            set;
        }

        public int Hitpoints
        {
            get
            {
                return hitpoints;
            }
            set
            {
                //LogFile.Log.LogEntryDebug("Setting monster " + SingleDescription + " HP to " + hitpoints, LogDebugLevel.Medium);
                hitpoints = value;
            }
        }

        public int MaxHitpoints
        {
            get
            {
                return maxHitpoints;
            }
            set
            {
                maxHitpoints = value;
            }
        }

        public int Level { get; set; }

        /// <summary>
        /// Special effect that means the monster fights for the player. Could be a real effect in future, and use this as an accessor
        /// </summary>
        public bool Charmed { get; set; }

        /// <summary>
        /// Monster won't attack the player
        /// </summary>
        public bool Passive { get; set; }

        /// <summary>
        /// Monster will go non-passive if attacked
        /// </summary>
        public bool UnpassifyOnAttacked { get; set; }

        /// <summary>
        /// Monster will wake if attacked when sleeping
        /// </summary>
        public bool WakesOnAttacked { get; set; }

        /// <summary>
        /// Is the creature currently asleep. Defaults to true to avoid creatures wandering around the dungeon without a player.
        /// </summary>
        public bool Sleeping { get; set; }

        /// <summary>
        /// The monster will ignore dangerous terrain and just take the hit
        /// </summary>
        public bool IgnoreDangerousTerrain { get; set; }

        /// <summary>
        /// Player armour class. Auto-calculated so not serialized
        /// </summary>
        protected int armourClass;

        /// <summary>
        /// Player damage base. Auto-calculated so not serialized
        /// </summary>
        protected int damageBase;

        /// <summary>
        /// Player damage modifier. Auto-calculated so not serialized
        /// </summary>
        protected double damageModifier;

        /// <summary>
        /// Player damage modifier. Auto-calculated so not serialized
        /// </summary>
        protected int hitModifier;

        /// <summary>
        /// Get the max hitpoints for this class of creature
        /// </summary>
        /// <returns></returns>
        abstract protected int ClassMaxHitpoints();


        /// <summary>
        /// Will the creature wake on sight of the player or another wakened creature?
        /// Override as false for classic sleep-until-attacked behaviour
        /// </summary>
        /// <returns></returns>
        virtual protected bool WakesOnSight()
        {
            return false;
        }

        /// <summary>
        /// Monster isn't processed until it's been seen by the player
        /// Used so that monsters don't wander around until necessary
        /// Override as false for wake-on-sight or wake-on-attacked behaviour
        /// </summary>
        /// <returns></returns>
        virtual protected bool WakesOnBeingSeen()
        {
            return false;
        }

        /// <summary>
        /// Wakes on seeing an awake creature within this radius
        /// </summary>
        /// <returns></returns>
        virtual protected bool WakesOnMonsterStealth()
        {
            return true;
        }

        /// <summary>
        /// For a sleeping creature, if the player is within this radius
        /// and they are visible, the creature will wake
        /// This range is for points on a line that can go diagonally, including the origin and dest
        /// </summary>
        /// <returns></returns>
        virtual protected int StealthRadius()
        {
            return 4;
        }

        /// <summary>
        /// If we see an awake monster within this range, stop sleeping
        /// </summary>
        /// <returns></returns>
        virtual protected int MonsterStealthRadius()
        {
            return 3;
        }

        /// <summary>
        /// Called when the creature is killed. Can be used to drop treasure. Not used at the moment.
        /// </summary>
        /// <returns></returns>
        virtual public void InventoryDrop() { }

        /// <summary>
        /// Was the creature summoned or raised?
        /// </summary>
        public bool WasSummoned { get; set; }

        /// <summary>
        /// Run the creature's action AI
        /// </summary>
        public virtual void ProcessTurn()
        {
            //Base monster classes just sit still
        }

        /// <summary>
        /// Set creature's status to charmed. Check if it worked at a higher level
        /// </summary>
        public void CharmCreature()
        {
            LogFile.Log.LogEntryDebug(this.SingleDescription + "charmed", LogDebugLevel.Medium);
            Charmed = true;
        }
        /// <summary>
        /// Set creature's status to uncharmed (normal). Check if it worked at a higher level
        /// </summary>
        public void UncharmCreature()
        {
            LogFile.Log.LogEntryDebug(this.SingleDescription + "uncharmed", LogDebugLevel.Medium);
            Charmed = false;
        }

        /// <summary>
        /// Set creature's status to passive. Check if it worked at a higher level
        /// </summary>
        public void PassifyCreature()
        {
            LogFile.Log.LogEntryDebug(this.SingleDescription + "passified", LogDebugLevel.Medium);
            Passive = true;
        }

        /// <summary>
        /// Set creature's status to non-passive (normal). Check if it worked at a higher level
        /// </summary>
        public void UnpassifyCreature()
        {
            LogFile.Log.LogEntryDebug(this.SingleDescription + "unpassified", LogDebugLevel.Medium);
            Passive = false;
        }

        public bool InStealthRadius(Point sq)
        {
            int range = Utility.GetPathDistanceBetween(this, sq);

            if (range <= StealthRadius())
                return true;

            return false;
        }

        public bool InMonsterStealthRadius(Point sq)
        {
            int range = Utility.GetPathDistanceBetween(this, sq);

            if (range <= MonsterStealthRadius())
                return true;

            return false;
        }

        public void CalculateCombatStats()
        {
            //Get defaults from class
            armourClass = ArmourClass();
            damageBase = DamageBase();
            damageModifier = DamageModifier();
            hitModifier = HitModifier();
            speed = BaseSpeed();

            //Check equipped items - unlikely to be implemented

            foreach (Item item in Inventory.Items)
            {
                if (!item.IsEquipped)
                    continue;

                IEquippableItem equipItem = item as IEquippableItem;

                //Error if non-equippable item is equipped
                if (equipItem == null)
                {
                    LogFile.Log.LogEntry("Item " + item.SingleItemDescription + " is non-equippable but is equipped!");
                    //Just skip
                    continue;
                }

                armourClass += equipItem.ArmourClassModifier();
                damageModifier += equipItem.DamageModifier();
                hitModifier += equipItem.HitModifier();

                if (equipItem.DamageBase() > damageBase)
                {
                    damageBase = equipItem.DamageBase();
                }
            }

            //Check effects

            ApplyEffects();
        }

        /// <summary>
        /// As part of CalculateCombatStats, go through the current effects
        /// find the most-good and most-bad stat modifiers and apply them (only)
        /// </summary>
        private void ApplyEffects()
        {
            int maxDamage = 0;
            int minDamage = 0;

            int maxHit = 0;
            int minHit = 0;

            int maxAC = 0;
            int minAC = 0;

            int maxSpeed = 0;
            int minSpeed = 0;

            //Only the greatest magnitude (+ or -) effects have an effect
            foreach (MonsterEffect effect in effects)
            {
                if (effect.ArmourClassModifier() > maxAC)
                    maxAC = effect.ArmourClassModifier();

                if (effect.ArmourClassModifier() < minAC)
                    minAC = effect.ArmourClassModifier();

                if (effect.HitModifier() > maxHit)
                    maxHit = effect.HitModifier();

                if (effect.HitModifier() < minHit)
                    minHit = effect.HitModifier();

                if (effect.SpeedModifier() > maxSpeed)
                    maxSpeed = effect.SpeedModifier();

                if (effect.SpeedModifier() < minSpeed)
                    minSpeed = effect.SpeedModifier();

                if (effect.DamageModifier() > maxDamage)
                    maxDamage = (int)effect.DamageModifier();

                if (effect.DamageModifier() < minDamage)
                    minDamage = (int)effect.DamageModifier();
            }

            damageModifier += maxDamage;
            damageModifier += minDamage;

            Speed += maxSpeed;
            Speed += minSpeed;

            hitModifier += maxHit;
            hitModifier += minHit;

            armourClass += maxAC;
            armourClass += minAC;
        }

        /// <summary>
        /// Run an effect on the monster. Calls the effect's onStart and adds it to the current effects queue
        /// </summary>
        /// <param name="effect"></param>
        internal void AddEffect(MonsterEffect effect)
        {
            effects.Add(effect);

            effect.OnStart(this);

            CalculateCombatStats();
        }

        /// <summary>
        /// Remove all existing effects
        /// </summary>
        public void RemoveAllEffects()
        {
            //Increment time on events and remove finished ones
            List<MonsterEffect> finishedEffects = new List<MonsterEffect>();

            foreach (MonsterEffect effect in effects)
            {
                if (!effect.HasEnded())
                    effect.OnEnd(this);

                finishedEffects.Add(effect);
            }

            //Remove finished effects
            foreach (MonsterEffect effect in finishedEffects)
            {
                effects.Remove(effect);
            }

            CalculateCombatStats();
        }

        /// <summary>
        /// Increment time on all monster events. Events that expire will run their onExit() routines and then delete themselves from the list
        /// </summary>
        internal void IncrementEventTime()
        {
            //Increment time on events and remove finished ones
            List<MonsterEffect> finishedEffects = new List<MonsterEffect>();

            bool eventEnded = false;

            foreach (MonsterEffect effect in effects)
            {
                effect.IncrementTime(this);

                if (effect.HasEnded())
                {
                    finishedEffects.Add(effect);
                    eventEnded = true;
                }
            }

            //Remove finished effects
            foreach (MonsterEffect effect in finishedEffects)
            {
                effects.Remove(effect);
            }

            //Recalculate stats if an event ended
            if(eventEnded)
                CalculateCombatStats();
        }

        /// <summary>
        /// Can this creature move?
        /// </summary>
        public virtual bool CanMove() {
            return true;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public abstract string SingleDescription {get;}

        /// <summary>
        /// Rats
        /// </summary>
        public abstract string GroupDescription { get; }

        /// <summary>
        /// Increment time on our events then use the base class to increment time on the monster's turn counter
        /// </summary>
        /// <returns></returns>
        internal override bool IncrementTurnTime()
        {
            IncrementEventTime();

            return base.IncrementTurnTime();
        }

        protected int toHitRoll;

        internal int ScaleRangedDamage(int damageBase)
        {
            return Game.Dungeon.LevelScalingCalculation(damageBase, Level);
        }

        internal int ScaleMeleeDamage(int damageBase)
        {
            return Game.Dungeon.LevelScalingCalculation(damageBase, Level);
        }

        protected virtual int AttackCreatureWithModifiers(Creature creature, int hitMod, int damBase, int damMod, int ACmod)
        {

            //Scale based on level
            return ScaleRangedDamage(damageBase);

            /*
            int attackToHit = hitModifier + hitMod;
            int attackDamageMod = damageModifier + damMod;

            int attackDamageBase;

            if (damBase > damageBase)
                attackDamageBase = damBase;
            else
                attackDamageBase = damageBase;

            int targetAC = creature.ArmourClass() + ACmod;
            toHitRoll = Utility.d20() + attackToHit;

            if (toHitRoll >= targetAC)
            {
                //Hit - calculate damage
                int totalDamage = Utility.DamageRoll(attackDamageBase) + attackDamageMod;

                return totalDamage;
            }

            //Miss
            return 0;*/


        }

        protected abstract string HitsPlayerCombatString();

        protected abstract string MissesPlayerCombatString();

        protected abstract string HitsMonsterCombatString(Monster target);

        protected abstract string MissesMonsterCombatString(Monster target);

        /// <summary>
        /// Can the creature be charmed by the charm power? Put in to exclude classes of monsters by the AI they have. Some AIs don't have charm and passive rules
        /// </summary>
        public virtual bool CanBeCharmed() {
            return false;
        }

        /// <summary>
        /// Can the creature be passified by the charm power? Put in to exclude classes of monsters by the AI they have. Some AIs don't have charm and passive rules
        /// </summary>
        public virtual bool CanBePassified()
        {
            return false;
        }

        public virtual CombatResults AttackPlayer(Player player, bool ranged)
        {
            StandardPreCombat();

            if (player.RecalculateCombatStatsRequired)
                player.CalculateCombatStats();

            //Do attack
            if (this.AttackType == CreatureAttackType.Shotgun)
            {
                StandardShotGunAttack(player);
            }
            else
            {
                int damage = AttackCreatureWithModifiers(player, 0, 0, 0, 0);
                string combatResultsMsg = "MvP (" + this.Representation + ") Normal. Dam: " + damage;
                LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

                AttackAudio(ranged);

                player.NotifyMonsterEvent(new MonsterEvent(MonsterEvent.MonsterEventType.MonsterAttacksPlayer, this));

                return player.ApplyCombatDamageToPlayer(this, damage, ranged);
            }

            return CombatResults.NeitherDied;
        }

        private void AttackAudio(bool ranged)
        {
            if (!ranged)
            {
                if (this is Creatures.Psycho)
                {
                    SoundPlayer.Instance().EnqueueSound("chainsaw");
                }
                else
                {
                    SoundPlayer.Instance().EnqueueSound("punch");
                }
            }
            SoundPlayer.Instance().EnqueueSound("gunshot");
        }

        public virtual CombatResults AttackMonster(Monster monster, bool ranged)
        {
            StandardPreCombat();

            if (monster.RecalculateCombatStatsRequired)
                monster.CalculateCombatStats();
            
            //Do attack
            int damage = 0;
            if (this.AttackType == CreatureAttackType.Shotgun)
            {
                //Will call ApplyDamageToMonster on all monsters hit
                StandardShotGunAttack(monster);
                return CombatResults.NeitherDied; //can't tell in this case
            }
            else
            {
                damage = AttackCreatureWithModifiers(monster, 0, 0, 0, 0);
                if (!ranged)
                    SoundPlayer.Instance().EnqueueSound("punch");
                SoundPlayer.Instance().EnqueueSound("gunshot");

                return Game.Dungeon.Combat.ApplyDamageToMonster(this, monster, damage);
            }
        }

        private void StandardShotGunAttack(Creature target)
        {
            var scaledDamage = ScaleRangedDamage(this.DamageBase());
            Game.Dungeon.FireShotgunWeapon(this, target.LocationMap, scaledDamage, 0.0, Math.PI / 8, scaledDamage / 10, scaledDamage / 10);
            SoundPlayer.Instance().EnqueueSound("shotgun");
        }

        public int GetScaledDamage()
        {
            return ScaleRangedDamage(DamageBase());
        }

        private void StandardPreCombat()
        {
            if (this.RecalculateCombatStatsRequired)
                this.CalculateCombatStats();

            if (this.ReloadTurnsRequired() > 0)
            {
                ReloadingTurns = ReloadTurnsRequired();
            }
        }

        /// <summary>
        /// Apply stun damage (miss n-turns) to monster. All stun attacks are routed through here
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="stunTurns"></param>
        /// <returns></returns>
        public CombatResults ApplyStunDamageToMonster(Creature attackingCreature, int stunTurns)
        {
            //Wake monster up etc.
            AIForMonsterIsAttacked(this, attackingCreature);

            int monsterOrigStunTurns = this.StunnedTurns;

            //Do we hit the monster?
            if (stunTurns > 0)
            {
                this.StunnedTurns += stunTurns;

                //Notify the creature that it has taken damage
                //It may activate a special ability or stop running away etc.
                this.NotifyHitByCreature(this, 0);

                //Message string
                string playerMsg2 = "";
                if (!this.Unique)
                    playerMsg2 += "The ";
                playerMsg2 += this.SingleDescription + " is stunned!";
                Game.MessageQueue.AddMessage(playerMsg2);

                string debugMsg2 = "MStun: " + monsterOrigStunTurns + "->" + this.StunnedTurns;
                LogFile.Log.LogEntryDebug(debugMsg2, LogDebugLevel.Medium);

                return CombatResults.NeitherDied;
            }

            //Miss

            string playerMsg3 = "";
            if (!this.Unique)
                playerMsg3 += "The ";
            playerMsg3 += this.SingleDescription + " shrugs off the attack.";
            Game.MessageQueue.AddMessage(playerMsg3);
            string debugMsg3 = "MStun: " + monsterOrigStunTurns + "->" + this.StunnedTurns;
            LogFile.Log.LogEntryDebug(debugMsg3, LogDebugLevel.Medium);

            return CombatResults.NeitherDied;

        }

        /// <summary>
        /// Monster has been attacked. Wake it up etc.
        /// </summary>
        /// <param name="monster"></param>
        public void AIForMonsterIsAttacked(Monster monster, Creature attackingMonster)
        {
            //Set the attacked by marker
            if (attackingMonster != null)
            {
                monster.LastAttackedBy = attackingMonster;
                monster.LastAttackedByID = attackingMonster.UniqueID;
            }

            //Was this a passive creature? It loses that flag
            if (monster.Passive && monster.UnpassifyOnAttacked)
                monster.UnpassifyCreature();

            //Was this a sleeping creature? It loses that flag
            if (monster.Sleeping && monster.WakesOnAttacked)
            {
                monster.WakeCreature();

                //All wake on sight creatures should be awake at this point. If it's a non-wake-on-sight tell the player it wakes
                Game.MessageQueue.AddMessage("The " + monster.SingleDescription + " wakes up!");
                LogFile.Log.LogEntryDebug(monster.Representation + " wakes on attack", LogDebugLevel.Low);
            }

            //Notify the creature that it has been hit
            if (attackingMonster != null)
            {
                monster.NotifyAttackByCreature(attackingMonster);
            }
        }

        /// <summary>
        /// Calculate the derived (used by other functions) sight radius based on the player's NormalSightRadius and the light level of the dungeon level the monster is on
        /// </summary>
        public void CalculateSightRadius()
        {
            //Set vision
            //double sightRatio = NormalSightRadius / 5.0;
            //SightRadius = (int)Math.Ceiling(Game.Dungeon.Levels[LocationLevel].LightLevel * sightRatio);

            //For royaleRL we support the player having infinite site radius but not the monsters
            //if (SightRadius == 0)
            //{
            //This scaling is confusing!
            SightRadius = NormalSightRadius;
            //}
        }

        /// <summary>
        /// Creature cost for level gen
        /// </summary>
        /// <returns></returns>
        abstract public int CreatureCost();

        /// <summary>
        /// Creature level
        /// </summary>
        /// <returns></returns>
        public virtual int CreatureLevel() { return Level; }

        /// <summary>
        /// We have be attacked (but not necessarily damaged by) creature
        /// </summary>
        /// <param name="creature"></param>
        abstract public void NotifyAttackByCreature(Creature creature);

        /// <summary>
        /// We have been hit by creature
        /// </summary>
        /// <param name="creature"></param>
        abstract public void NotifyHitByCreature(Creature creature, int damage);

        /// <summary>
        /// We have been killed. Do special game effects. Do some clean up of internal structure
        /// </summary>
        /// <param name="creature"></param>
        abstract public void NotifyMonsterDeath();

        /// <summary>
        /// The monster's combat challenge rating
        /// </summary>
        /// <returns></returns>
        public virtual int GetCombatXP() { return 0; }

        /// <summary>
        /// The monster's magic challenge rating
        /// </summary>
        /// <returns></returns>
        public virtual int GetMagicXP() { return 0; }

        /// <summary>
        /// The monster's magic resistance
        /// </summary>
        /// <returns></returns>
        virtual public int GetMagicRes() { return 0; }
        
        /// <summary>
        /// The monster's charm resistance
        /// </summary>
        /// <returns></returns>
        virtual public int GetCharmRes() { return 0; }

        /// <summary>
        /// Wake a creature (remove sleeping flag)
        /// </summary>
        internal void WakeCreature()
        {
            Sleeping = false;
        }

        /// <summary>
        /// Are we on patrol (i.e. not attacking?) ? Highlight on the map
        /// </summary>
        /// <returns></returns>
        virtual public bool OnPatrol()
        {
            return false;
        }

        /// <summary>
        /// Are we on patrol (i.e. not attacking?) ? Highlight on the map
        /// </summary>
        /// <returns></returns>
        virtual public bool InPursuit()
        {
            return false;
        }

        public virtual bool ShowHeading() {
            return true;
        }

        /// <summary>
        /// Do on Killed special effects
        /// </summary>
        internal virtual void OnKilledSpecialEffects()
        {
            return;
        }

        internal virtual int ReloadTurnsRequired()
        {
            return 0;
        }

        internal virtual System.Drawing.Color GetCorpseRepresentationColour()
        {
            return System.Drawing.Color.Gainsboro;
        }

        internal virtual char GetCorpseRepresentation()
        {
            return (char)498;
        }

        public virtual int DropChance()
        {
            return 10;
        }

        public virtual Feature GenerateCorpse()
        {
            return new Features.Corpse(GetCorpseRepresentation(), GetCorpseRepresentationColour());
        }

        public virtual Pathing.PathingType PathingType()
        {
            return Pathing.PathingType.Normal;
        }
    }
}
