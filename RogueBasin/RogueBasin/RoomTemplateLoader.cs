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
