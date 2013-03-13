using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin
{
    public class SerializableMap
    {
        public MapSquare[] mapSquares;
        public Point PCStartLocation;

        public int width;
        public int height;

        public double LightLevel;

        /// <summary>
        /// Are we guaranteed to be connected?
        /// </summary>
        public bool GuaranteedConnected { get; set; }

        /// <summary>
        /// For serialization only
        /// </summary>
        public SerializableMap()
        {
        }

        /// <summary>
        /// Make serializable map from map
        /// </summary>
        /// <param name="original"></param>
        public SerializableMap(Map original)
        {
            this.PCStartLocation = original.PCStartLocation;
            this.width = original.width;
            this.height = original.height;
            this.LightLevel = original.LightLevel;
            this.GuaranteedConnected = original.GuaranteedConnected;

            //Need to make mapSquares 1 dimensional

            mapSquares = new MapSquare[width * height];

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    mapSquares[j * width + i] = original.mapSquares[i, j];
                }
            }
        }

        /// <summary>
        /// Get proper map back
        /// </summary>
        /// <returns></returns>
        public Map MapFromSerializableMap()
        {
            Map newMap = new Map(width, height);
            newMap.PCStartLocation = this.PCStartLocation;
            newMap.LightLevel = this.LightLevel;
            newMap.GuaranteedConnected = this.GuaranteedConnected;

            //Make mapSquares
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    newMap.mapSquares[i, j] = this.mapSquares[j * width + i];
                }
            }

            return newMap;
        }
    }

    /// <summary>
    /// All the members of dungeon that we need to serialize to make a save game
    /// </summary>
    public class SaveGameInfo
    {
        public List<SerializableMap> levels;
        public List<Monster> monsters;
        public List<Item> items;
        public List<Feature> features;

        public List<SpecialMove> specialMoves;
        public List<Spell> spells;

        public List<HiddenNameInfo> hiddenNameInfo;
        public List<DungeonSquareTrigger> triggers;

        public List<string> messageLog;

        public Player player;

        public GameDifficulty difficulty;

        public long worldClock = 0;
        public int dateCounter = 0;

        public int nextUniqueID;
        public int nextUniqueSoundID;

        /// <summary>
        /// List of global events
        /// </summary>
        public List<KeyValuePair<long, SoundEffect>> effects;

        public DungeonInfo dungeonInfo;
    }
}
