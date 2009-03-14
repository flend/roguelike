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

        public List<HiddenNameInfo> hiddenNameInfo;
        public List<DungeonSquareTrigger> triggers;

        public Player player;

        public long worldClock = 0;

        /// <summary>
        /// List of global events
        /// </summary>
        public List<DungeonEffect> effects;
    }
}
