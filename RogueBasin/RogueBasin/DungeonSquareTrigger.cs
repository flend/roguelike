namespace RogueBasin
{
    /// <summary>
    /// Function that triggers when the PC moves into a particular square
    /// </summary>
    public abstract class DungeonSquareTrigger
    {
        public int Level { get; set; }
        public Point mapPosition { get; set; }

        /// <summary>
        /// Has the trigger been activated?
        /// Don't access this directly. Only public for serializing
        /// </summary>
        private bool triggered;
        
        public DungeonSquareTrigger()
        {
            triggered = false;
        }

        /// <summary>
        /// Run the trigger. Really responsible for its own stage and retriggering.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="mapLocation"></param>
        /// <returns></returns>
        public abstract bool CheckTrigger(int level, Point mapLocation);

        protected bool CheckLocation(int level, Point mapLocation)
        {

            //Just check location
            if (Level != level || mapLocation != mapPosition)
                return false;

            return true;
        }

        protected bool Triggered {
            get
            {
                return triggered;
            }
            set
            {
                triggered = value;
            }
        }

        public virtual bool IsTriggered()
        {
            return triggered;
        }
    }
}
