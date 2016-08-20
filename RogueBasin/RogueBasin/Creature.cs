﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Linq;

namespace RogueBasin
{
    public enum CreatureAttackType
    {
        Normal, Shotgun
    }

    /// <summary>
    /// Base class for Creatures.
    /// </summary>
    public abstract class Creature : MapObject
    {
        /// <summary>
        /// The creature's inventory
        /// </summary>
        Inventory inventory;

        /// <summary>
        /// The equipment slots the creature has
        /// </summary>
        List<EquipmentSlotInfo> equipmentSlots;

        /// <summary>
        /// Is the creature still alive?
        /// </summary>
        bool alive;

        public int UniqueID { get; set; }

        public uint TurnCount { get; set; }

        /// <summary>
        /// The creature we were last attacked by
        /// </summary>
        [XmlIgnore]
        public Creature LastAttackedBy { get; set; }

        public int LastAttackedByID { get; set; }

        /// <summary>
        /// Where the creature is facing. Set after move.
        /// Should be set on character creation (perhaps using helper function)
        /// </summary>
        public double Heading { get; set; }

        /// <summary>
        /// Sight radius
        /// </summary>
        public int SightRadius {get; set;}

        public int NormalSightRadius { get; set; }

        /// <summary>
        /// Increment each game turn for the creature's internal clock. Turn at turnClockLimit
        /// </summary>
        protected int speed = 100;

        /// <summary>
        /// Current turn clock value for the creature. When 1000 the creature takes a turn
        /// </summary>
        protected int turnClock = 0;

        /// <summary>
        /// How much the turn clock has to reach to process
        /// </summary>
        protected const int turnClockLimit = 400;

        /// <summary>
        /// Number of ticks (calls to IncrementTime from the main loop) that occur between each a creature's turns. Used to calculate durations etc.
        /// </summary>
        public const int turnTicks = 10;

        /** For a creature, set heading when it moves */
        public override Point LocationMap
        {
            get
            {
                return base.LocationMap;
            }
            set
            {
                //If we have a previous location, set the creature's heading
                
                //Handled in the AI, since it has more useful information about where to point
                /*
                if (LocationMap != null)
                {
                    this.Heading = DirectionUtil.AngleFromOriginToTarget(LocationMap, value);
                    LogFile.Log.LogEntryDebug("Creature: " + this.Representation.ToString() + " Returned direction: " + this.Heading.ToString(), LogDebugLevel.Low);
                }
                */
                base.LocationMap = value;
            }
        }

        /// <summary>
        /// A list of all the equipment slots the creature has
        /// </summary>
        public List<EquipmentSlotInfo> EquipmentSlots
        {
            get
            {
                return equipmentSlots;
            }
        }

        /// <summary>
        /// For serialization
        /// </summary>
        public int TurnClock
        {
            get
            {
                return turnClock;
            }
            set
            {
                turnClock = value;
            }
        }

        public int Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }

        /// <summary>
        /// Set to false to kill the creature
        /// </summary>
        public bool Alive
        {
            get
            {
                return alive;
            }
            set
            {
                alive = value;
            }
        }

        public void AddTurnsMoving()
        {
            TurnsMoving++;
        }

        public void ResetTurnsMoving()
        {
            TurnsMoving = 0;
        }

        public int TurnsMoving { get; private set; }
        public int TurnsInactive { get; private set; }
        public int TurnsSinceAction { get; private set; }

        public void AddTurnsInactive()
        {
            TurnsInactive++;
        }

        public void ResetTurnsInactive()
        {
            TurnsInactive = 0;
        }

        public void AddTurnsSinceAction()
        {
            TurnsSinceAction++;
        }

        public void ResetTurnsSinceAction()
        {
            TurnsSinceAction = 0;
        }

        public Creature()
        {
            alive = true;

            LastAttackedBy = null;
            LastAttackedByID = -1;

            NormalSightRadius = 5;

            inventory = new Inventory();

            equipmentSlots = new List<EquipmentSlotInfo>();

            //Used as a test for re-adding monsters
            UniqueID = 0;

            //Don't randomized the turn clock - otherwise sometimes summonded enemies get an attack in before the player
            //RandomStartTurnClock();

            //By default set a random heading. Override from outside
            SetRandomHeading();
        }

        private void SetRandomHeading()
        {
            int randX = Game.Random.Next(-1, 2);
            int randY = Game.Random.Next(-1, 2);

            Heading = DirectionUtil.DiagonalCardinalAngleFromRelativePosition(randX, randY);
        }

        public void SetHeadingToTarget(Point target)
        {
            var relativeDirection = target - this.LocationMap;
            Heading = DirectionUtil.DiagonalCardinalAngleFromRelativePosition(relativeDirection.x, relativeDirection.y);
        }

        //Creatures start with a random amount in their turn clock. This stops them all moving simultaneously (looks strange if the player is fast)
        private void RandomStartTurnClock()
        {
            turnClock = Game.Random.Next(turnClockLimit);
        }

        /// <summary>
        /// Increment the internal turn timer and resets if over boundary. Return true if a turn should be had.
        /// </summary>
        internal virtual bool IncrementTurnTime()
        {
            turnClock += speed;

            if (turnClock >= turnClockLimit)
            {
                turnClock -= turnClockLimit;

                TurnCount++;
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Pick up an item and put it in the inventory if possible
        /// </summary>
        /// <param name="itemToPickUp"></param>
        /// <returns>False if the item won't fit in the inventory for some reason</returns>
        public virtual bool PickUpItem(Item itemToPickUp)
        {
            //Set the item as found
            itemToPickUp.IsFound = true;

            if(Inventory.ContainsItemOfType(itemToPickUp.GetType()) && itemToPickUp.DoNotPickupDuplicates()) {
                LogFile.Log.LogEntryDebug("Won't pick up duplicate " + itemToPickUp.SingleItemDescription, LogDebugLevel.Medium);
                Game.MessageQueue.AddMessage("Already have one of those!");
                return false;
            }
            else {
                inventory.AddItem(itemToPickUp);
            }

            return true;
        }

        /// <summary>
        /// Drop an item. Sets the item position to the position of this character
        /// </summary>
        /// <param name="itemToDrop"></param>
        /// <returns></returns>
        public virtual bool DropItem(Item itemToDrop)
        {
            inventory.RemoveItem(itemToDrop);

            ProcessDroppedItem(itemToDrop);

            return true;
        }

        private void ProcessDroppedItem(Item itemToDrop)
        {
            ProcessDroppedItem(itemToDrop, this.LocationLevel, this.LocationMap);
        }

        private void ProcessDroppedItem(Item itemToDrop, int locationLevel, Point locationMap)
        {
            itemToDrop.LocationLevel = locationLevel;
            itemToDrop.LocationMap = locationMap;
            itemToDrop.InInventory = false;

            itemToDrop.OnDrop(this);
        }



        /// <summary>
        /// Drop all inventory (perhaps you died). Currently drops all items on the floor, could extend by looking at surrounding squares. Use the dig algorithm
        /// </summary>
        /// <param name="itemToDrop"></param>
        /// <returns></returns>
        public virtual bool DropAllItems()
        {
            foreach (Item item in inventory.Items)
            {
                ProcessDroppedItem(item);
            }

            inventory.RemoveAllItems();

            return true;
        }

        /// <summary>
        /// Drop all inventory (perhaps you died). Currently drops all items on the floor, could extend by looking at surrounding squares. Use the dig algorithm
        /// </summary>
        /// <param name="itemToDrop"></param>
        /// <returns></returns>
        public virtual bool DropAllItems(int level)
        {
            var numItemsToDrop = inventory.Items.Count;

            var completelyFreePoints = Game.Dungeon.GetWalkableAdjacentSquaresFreeOfCreaturesAndItems(LocationLevel, LocationMap)
                .Union(new List<Point>{ LocationMap });

            var partiallyFreePoints = Game.Dungeon.GetWalkableAdjacentSquaresFreeOfCreatures(LocationLevel, LocationMap)
                .Union(new List<Point> { LocationMap });

            var pointsToUse = numItemsToDrop <= completelyFreePoints.Count() ?
                completelyFreePoints : partiallyFreePoints;

            var pointsLeft = pointsToUse.ToList();

            foreach (Item item in inventory.Items)
            {
                var randomPoint = pointsLeft.RandomElement();
                pointsLeft.Remove(randomPoint);
                ProcessDroppedItem(item, level, randomPoint);

                if(pointsLeft.Count() == 0)
                    pointsLeft = pointsToUse.Select(x => x).ToList();
            }

            inventory.RemoveAllItems();

            return true;
        }


        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public virtual int ArmourClass() { return 0; }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public virtual int HitModifier() { return 0; }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public abstract int DamageBase();

        /// <summary>
        /// Creature damage modifier.  Set by type of creature.
        /// </summary>
        public virtual double DamageModifier() { return 0; }

        /// <summary>
        /// What FOV we have in addition to the tcod
        /// </summary>
        public virtual CreatureFOV.CreatureFOVType FOVType() { 
            return CreatureFOV.CreatureFOVType.Base;
        }

        /// <summary>
        /// Creature base speed
        /// </summary>
        /// <returns></returns>
        public virtual int BaseSpeed() { return 100; }

        /// <summary>
        /// An effect with combat stat changes has expired, so they need to be recalculated
        /// </summary>
        public bool RecalculateCombatStatsRequired { get; set; }

        /// <summary>
        /// List of inventory items
        /// </summary>
        /// <returns></returns>
        public List<Item> GetInventoryItems()
        {
            return inventory.Items;
        }

        public CreatureAttackType AttackType
        {
            get {
                return GetCreatureAttackType();
            }
        }

        protected virtual CreatureAttackType GetCreatureAttackType()
        {
            return CreatureAttackType.Normal;
        }


        /// <summary>
        /// Inventory - possibly make this protected at some point?
        /// </summary>
        public Inventory Inventory
        {
            get
            {
                return inventory;
            }
            //For serialization
            set
            {
                inventory = value;
            }
        }

        public virtual string QuestId
        {
            get { return ""; }
        }
    }
}
