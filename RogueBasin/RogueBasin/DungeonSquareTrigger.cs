using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Function that triggers when the PC moves into a particular square
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.DungeonEntranceTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.HelpFriend))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.SeeCorpses))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.SpotFriend))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TreasureRoom))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TrainAthleticsTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TerrainFlipTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TrainCharmTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TrainCombatTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TrainRestTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TrainMagicTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TrainLeaveSchoolTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TrainMagicLibraryTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TrainGeographyLibraryTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TrainMasterTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.TownToWilderness))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.ApproachingTheDragon))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.BackToSchool))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.PrinceInABox))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.SchoolEntryTrigger))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.DockDoor))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.LeaveByDock))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.FirstLevelEntry))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.Mission3Entry))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.Mission2Entry))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.Mission1Entry))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.Mission4Entry))]
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.Mission5Entry))]


    public abstract class DungeonSquareTrigger
    {
        public int Level { get; set; }
        public Point mapPosition { get; set; }

        /// <summary>
        /// Has the trigger been activated?
        /// Don't access this directly. Only public for serializing
        /// </summary>
        public bool triggered;
        
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

        /// <summary>
        /// Get: Check if this trigger or ANY OTHER TRIGGER OF THE SAME TYPE has been triggered
        /// Set: Set THIS TRIGGER triggered
        /// </summary>
        /// <returns></returns>
        protected bool Triggered {
            get
            {
                return Game.Dungeon.CheckGlobalTrigger(this.GetType());
            }
            set
            {
                triggered = value;
            }
        }

        /// <summary>
        /// Has THIS trigger been activated. Can be overridden.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsTriggered()
        {
            return triggered;
        }
    }
}
