namespace RogueBasin
{
    /// <summary>
    /// Base class for SpecialMoves. Shouldn't be instantiated. Instantiate a child.
    /// Contains checks for whether we know a move or not.
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.StunBox))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.WallPush))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.ChargeAttack))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.WallVault))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.VaultBackstab))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.OpenSpaceAttack))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.Evade))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.MultiAttack))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.BurstOfSpeed))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.CloseQuarters))]
    [System.Xml.Serialization.XmlInclude(typeof(SpecialMoves.WallLeap))]
    public abstract class SpecialMove
    {
        /// <summary>
        /// Has the player learnt this move yet?
        /// </summary>
        public bool Known { get; set; }

        public SpecialMove() {
           Known = false;
        }

        /// <summary>
        /// Check the player's last action, does it fit with the move?
        /// Return code: true is fits, false if it caused a failure.
        /// We run this function twice if it fails at first. This is because a move may cause a failure at second stage, but be valid as a new first stage. The first run clears sta
        /// </summary>
        /// <param name="isMove"></param>
        /// <param name="isAttack"></param>
        /// <param name="locationAfterMove"></param>
        public virtual bool CheckAction(bool isMove, Point locationAfterMove, bool otherMoveSuccessful) {
            return true;
        }

        /// <summary>
        /// Is the move complete and will fire?
        /// </summary>
        /// <returns></returns>
        public virtual bool MoveComplete() { return false; }

        /// <summary>
        /// Carry out the move (instead of normal move / attack)
        /// If noMove is set, only attack, don't move
        /// </summary>
        public virtual void DoMove(Point locationAfterMove, bool noMove) { }

        /// <summary>
        /// Clear the counter - used when another move has triggered
        /// </summary>
        public virtual void ClearMove() { }

        /// <summary>
        /// Return a 4 character abbrevation for the move
        /// </summary>
        /// <returns></returns>
        public virtual string Abbreviation() { return ""; }

        /// <summary>
        /// Return the root of the movie for this special move
        /// </summary>
        public virtual string MovieRoot() { return ""; }

        /// <summary>
        /// Return a presentable string for the name
        /// </summary>
        public virtual string MoveName() { return ""; }

        /// <summary>
        /// Total stages in move
        /// </summary>
        /// <returns></returns>
        public virtual int TotalStages() { return 0; }

        /// <summary>
        /// Current stage in move
        /// </summary>
        /// <returns></returns>
        public virtual int CurrentStage() { return 0; }

        /// <summary>
        /// Does the move give an attack on a monster which wouldn't normally be attacked by this move? e.g. Multi, Open ground
        /// </summary>
        /// <returns></returns>
        public virtual bool AddsAttack() { return false; }

        /// <summary>
        /// Special moves that cause movement different to just the direction pressed, e.g. WallLeap
        /// </summary>
        /// <returns></returns>
        public virtual bool CausesMovement() { return false; }

        /// <summary>
        /// If the move gives an added attack and it happened this turn, return true.
        /// </summary>
        /// <returns></returns>
        public virtual bool AttackIsOn() { return false; }

        /// <summary>
        /// Does the move start with an attack? If so, we need to check if bonus attacks start it
        /// </summary>
        /// <returns></returns>
        public virtual bool StartsWithAttack()
        {
            return false;
        }

        /// <summary>
        /// For moves that cause movement, return the effective player move
        /// e.g. for WallLeap the effective move is an attack from the direction of the player to the monster
        /// Could potentially make this a return value of DoMove
        /// </summary>
        /// <returns></returns>
        public virtual Point RelativeMoveAfterMovement()
        {
            LogFile.Log.LogEntryDebug("RelativeMoveAfterMovement() called on a non-movement special move", LogDebugLevel.High);
            return new Point(0,0);
        }

        /// <summary>
        /// For moves that cause an additional attack, what is the relative move from the player's current position which would cause that attack normally?
        /// Used to trigger later moves
        /// </summary>
        /// <returns></returns>
        public virtual Point RelativeAttackVector()
        {
            LogFile.Log.LogEntryDebug("RelativeAttackVector() called on a non-attack special move", LogDebugLevel.High);
            return new Point(0, 0);
        }

        /// <summary>
        /// Checks if the move gets cancelled by the death of the creature
        /// </summary>
        public virtual void CheckForMonsterDeath()
        {

        }

        /// <summary>
        /// Moves which don't happen if another special move was successful (only CQ for now)
        /// </summary>
        /// <returns></returns>
        public virtual bool NotSimultaneous()
        {
            return false;
        }

        /// <summary>
        /// Helper function that determines the number of cardinal directions for a close quarters attack
        /// </summary>
        /// <returns></returns>
        protected int FindNumberOfCardinals(Creature creature)
        {
            //Are 3 cardinal directions around the monster unwalkable?
            Point monsterLoc = creature.LocationMap;

            int noCardinals = 0;

            Dungeon dungeon = Game.Dungeon;

            if (!dungeon.MapSquareIsWalkable(creature.LocationLevel, new Point(monsterLoc.x - 1, monsterLoc.y)))
                noCardinals++;

            if (!dungeon.MapSquareIsWalkable(creature.LocationLevel, new Point(monsterLoc.x + 1, monsterLoc.y)))
                noCardinals++;

            if (!dungeon.MapSquareIsWalkable(creature.LocationLevel, new Point(monsterLoc.x, monsterLoc.y + 1)))
                noCardinals++;

            if (!dungeon.MapSquareIsWalkable(creature.LocationLevel, new Point(monsterLoc.x, monsterLoc.y - 1)))
                noCardinals++;

            return noCardinals;
        }

        /// <summary>
        /// Current stage in move
        /// </summary>
        /// <returns></returns>
        public abstract int GetRequiredCombat();
    }
}
