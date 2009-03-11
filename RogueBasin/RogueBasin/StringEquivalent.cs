using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    /// <summary>
    /// Retreives string equivalents for various enums
    /// </summary>
    static class StringEquivalent
    {
        /// <summary>
        /// Mapping of equipment slots to descriptions
        /// </summary>
        public static Dictionary<EquipmentSlot, string> EquipmentSlots { get; private set; }

        /// <summary>
        /// Mapping of terrain to ASCII characters
        /// </summary>
        public static Dictionary<MapTerrain, char> TerrainChars { get; private set; }

        static StringEquivalent()
        {
            EquipmentSlots = new Dictionary<EquipmentSlot, string>();
            SetupEquipmentSlots();

            TerrainChars = new Dictionary<MapTerrain, char>();
            SetupTerrainChars();
        }

        private static void SetupTerrainChars()
        {
            TerrainChars.Add(MapTerrain.Empty, '.');
            TerrainChars.Add(MapTerrain.Wall, '#');
            TerrainChars.Add(MapTerrain.Corridor, '|');
            TerrainChars.Add(MapTerrain.Void, ' ');
            TerrainChars.Add(MapTerrain.ClosedDoor, '+');
            TerrainChars.Add(MapTerrain.OpenDoor, '/');
            TerrainChars.Add(MapTerrain.Flooded, '=');
        }

        private static void SetupEquipmentSlots()
        {
            EquipmentSlots.Add(EquipmentSlot.Body, "Body");
            EquipmentSlots.Add(EquipmentSlot.RightHand, "Right Hand");
        }
    }
}
