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
    [System.Xml.Serialization.XmlInclude(typeof(Triggers.LichBattle))]
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
    public abstract class DungeonSquareTrigger
    {
        public int Level { get; set; }
        public Point mapPosition { get; set; }

        
        public DungeonSquareTrigger()
        {

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
    }
}
