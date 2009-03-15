using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

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

        public static Dictionary<MapTerrain, Color> TerrainColors { get; private set; }

        public static Dictionary<GameDifficulty, string> GameDifficultyString { get; private set; }

        static StringEquivalent()
        {
            EquipmentSlots = new Dictionary<EquipmentSlot, string>();
            SetupEquipmentSlots();

            TerrainChars = new Dictionary<MapTerrain, char>();
            SetupTerrainChars();

            TerrainColors = new Dictionary<MapTerrain, Color>();
            SetupTerrainColors();

            GameDifficultyString = new Dictionary<GameDifficulty, string>();
            GameDifficultyString.Add(GameDifficulty.Easy, "Easy");
            GameDifficultyString.Add(GameDifficulty.Medium, "Medium");
            GameDifficultyString.Add(GameDifficulty.Hard, "Hard");
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
            TerrainChars.Add(MapTerrain.Grass, ',');
            TerrainChars.Add(MapTerrain.River, '=');
            TerrainChars.Add(MapTerrain.Trees, '*');
            TerrainChars.Add(MapTerrain.Road, '-');
            TerrainChars.Add(MapTerrain.Mountains, '^');
        }

        private static void SetupTerrainColors()
        {
            TerrainColors.Add(MapTerrain.Empty, ColorPresets.White);
            TerrainColors.Add(MapTerrain.Wall, ColorPresets.DarkSlateGray);
            TerrainColors.Add(MapTerrain.Corridor, ColorPresets.Gray);
            TerrainColors.Add(MapTerrain.Void, ColorPresets.Black);
            TerrainColors.Add(MapTerrain.ClosedDoor, ColorPresets.DarkGoldenrod);
            TerrainColors.Add(MapTerrain.OpenDoor, ColorPresets.DarkGoldenrod);
            TerrainColors.Add(MapTerrain.Flooded, ColorPresets.Blue);
            TerrainColors.Add(MapTerrain.Grass, ColorPresets.Green);
            TerrainColors.Add(MapTerrain.River, ColorPresets.Blue);
            TerrainColors.Add(MapTerrain.Trees, ColorPresets.DarkGreen);
            TerrainColors.Add(MapTerrain.Road, ColorPresets.DarkGoldenrod);
            TerrainColors.Add(MapTerrain.Mountains, ColorPresets.Gray);
        }

        private static void SetupEquipmentSlots()
        {
            EquipmentSlots.Add(EquipmentSlot.Body, "Body");
            EquipmentSlots.Add(EquipmentSlot.RightHand, "Right Hand");
        }
    }
}
