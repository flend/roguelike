using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace RogueBasin
{
    public class RoomTemplate
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        
        public RoomTemplateTerrain[,] terrainMap;

        public RoomTemplate(RoomTemplateTerrain[,] terrain)
        {
            this.terrainMap = (RoomTemplateTerrain[,])terrain.Clone();
            Width = terrainMap.GetLength(0);
            Height = terrainMap.GetLength(1);
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            RoomTemplate p = obj as RoomTemplate;
            if ((System.Object)p == null)
            {
                return false;
            }

            return IsTerrainTheSame(p);
        }

        public bool Equals(RoomTemplate p)
        {
            if ((object)p == null)
            {
                return false;
            }

            return IsTerrainTheSame(p);
        }

        private bool IsTerrainTheSame(RoomTemplate p)
        {
            if(p.Width != this.Width)
                return false;

            if(p.Height != this.Height)
                return false;

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (this.terrainMap[i, j] != p.terrainMap[i, j])
                        return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hashCount = 0;

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (terrainMap[i, j] == RoomTemplateTerrain.Wall)
                        hashCount++;
                }
            }

            return Width ^ (hashCount + Height);
        }
    }

    /** Types of terrain possible for an abstract room template */
    public enum RoomTemplateTerrain
    {
        Floor,
        Wall,
        WallWithPossibleDoor,
        OpenWithPossibleDoor,
        Transparent
    }

    /** Mapping for templates. Could be loaded from disk */
    public static class StandardTemplateMapping
    {
        public static readonly Dictionary<char, RoomTemplateTerrain> terrainMapping;

        static StandardTemplateMapping()
        {
            terrainMapping = new Dictionary<char, RoomTemplateTerrain>();

            terrainMapping[' '] = RoomTemplateTerrain.Transparent;
            terrainMapping['#'] = RoomTemplateTerrain.Wall;
            terrainMapping['.'] = RoomTemplateTerrain.Floor;
            terrainMapping['+'] = RoomTemplateTerrain.OpenWithPossibleDoor;
            terrainMapping['-'] = RoomTemplateTerrain.WallWithPossibleDoor;
        }

    }

    public static class RoomTemplateUtilities
    {
        /** Stretches a corridor template into a full sized corridor of length.
         *  Template must be n x 1 (1 row deep).*/
        static public RoomTemplate ExpandCorridorTemplate(bool switchToHorizontal, int length, RoomTemplate corridorTemplate)
        {
            if (corridorTemplate.Height > 1)
                throw new ApplicationException("Only corridor templates of height 1 supported");

            RoomTemplateTerrain[,] newRoom;

            if (switchToHorizontal)
            {
                newRoom = new RoomTemplateTerrain[length, corridorTemplate.Width];
                for (int j = 0; j < length; j++)
                {
                    for (int i = 0; i < corridorTemplate.Width; i++)
                    {
                        newRoom[j, i] = corridorTemplate.terrainMap[i, 0];
                    }
                }
            }
            else
            {
                newRoom = new RoomTemplateTerrain[corridorTemplate.Width, length];
                for (int j = 0; j < length; j++)
                {
                    for (int i = 0; i < corridorTemplate.Width; i++)
                    {
                        newRoom[i, j] = corridorTemplate.terrainMap[i, 0];
                    }
                }
            }

            return new RoomTemplate(newRoom);
        }

        /** Expand a corridor template (vertically aligned) into a suitable room template */
        static public TemplatePositioned GetTemplateForCorridorBetweenPoints(Point point1, Point point2, int z, RoomTemplate corridorTemplate)
        {
            if (!((point1.x == point2.x) || (point1.y == point2.y)))
            {
                throw new ApplicationException("Corridors must be straight");
            }

            bool horizontalSwitchNeeded = false;
            int length;

            if (point1.y == point2.y)
            {
                horizontalSwitchNeeded = true;
                length = Math.Abs(point1.x - point2.x) + 1;
            }
            else
            {
                length = Math.Abs(point1.y - point2.y) + 1;
            }

            RoomTemplate expandedCorridor = ExpandCorridorTemplate(horizontalSwitchNeeded, length, corridorTemplate);

            int centreOfTemplateShortAxis = corridorTemplate.Width / 2;

            int left = Math.Min(point1.x, point2.x);
            int top = Math.Min(point1.y, point2.y);

            //Find the TL for the template to be placed
            if (horizontalSwitchNeeded)
            {
                top -= centreOfTemplateShortAxis;
            }
            else
            {
                left -= centreOfTemplateShortAxis;
            }

            return new TemplatePositioned(left, top, z, expandedCorridor, TemplateRotation.Deg0);
        }
    }

    /** Loads a room / vault from disk and returns as a usuable object */
    public class RoomTemplateLoader
    {

        /** Loads template from a file stream. Throws exception on failure */
        public static RoomTemplate LoadTemplateFromFile(Stream fileStream, Dictionary<char, RoomTemplateTerrain> terrainMapping)
        {
            StreamReader reader = new StreamReader(fileStream);
            string thisLine;

            List<string> mapRows = new List<string>();

            while ((thisLine = reader.ReadLine()) != null)
            {
                mapRows.Add(thisLine);
            }

            //Calculate dimensions
            int width = 0;
            int height = 0;

            foreach (string mapRow in mapRows)
            {
                if (mapRow.Length > width)
                    width = mapRow.Length;

                height++;
            }

            if (width == 0)
            {
                LogFile.Log.LogEntry("No data in room template file stream");
                throw new ApplicationException("No data in room template file - width is 0");
            }

            //Build a 2d representation of the room

            RoomTemplateTerrain[,] roomMap = new RoomTemplateTerrain[width, height];

            for (int y = 0; y < mapRows.Count; y++)
            {
                int x;
                for (x = 0; x < mapRows[y].Length; x++)
                {
                    char inputTerrain = mapRows[y][x];

                    if (!terrainMapping.ContainsKey(inputTerrain))
                    {
                        LogFile.Log.LogEntryDebug("No mapping for char : " + inputTerrain + " in file", LogDebugLevel.High);
                        roomMap[x, y] = RoomTemplateTerrain.Transparent;
                    }

                    roomMap[x, y] = terrainMapping[inputTerrain];
                }

                //Fill all rows to width length
                for (; x < width; x++)
                {
                    roomMap[x, y] = RoomTemplateTerrain.Transparent;
                }
            }

            return new RoomTemplate(roomMap);
        }

        /** Loads template from manifest resource file. Throws exception on failure */
        public static RoomTemplate LoadTemplateFromFile(string filenameRoot, Dictionary<char, RoomTemplateTerrain> terrainMapping)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();

            /*
            MessageBox.Show("Showing all embedded resource names");

            string[] names = _assembly.GetManifestResourceNames();
            foreach (string name in names)
                MessageBox.Show(name);
            */

            string[] names = _assembly.GetManifestResourceNames();

            string filename = filenameRoot;
            Stream _fileStream = _assembly.GetManifestResourceStream(filename);

            LogFile.Log.LogEntry("Loading room template from file: " + filename);

            return LoadTemplateFromFile(_fileStream, terrainMapping);
        }
    }
}
