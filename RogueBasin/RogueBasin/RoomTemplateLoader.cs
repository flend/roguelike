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
        public class PotentialDoor
        {
            public Point Location { get; private set; }

            public PotentialDoor(Point location)
            {
                this.Location = location;
            }
        }

        public enum DoorLocation
        {
            Top, Bottom, Left, Right
        }


        public int Width { get; private set; }
        public int Height { get; private set; }
        
        public RoomTemplateTerrain[,] terrainMap;

        /// <summary>
        /// Get the potential doors. Indexed in by row then column
        /// </summary>
        public List<PotentialDoor> PotentialDoors
        {
            get
            {
                //Calculate (could cache)
                List<PotentialDoor> doors = new List<PotentialDoor>();

                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        if (terrainMap[j, i] == RoomTemplateTerrain.OpenWithPossibleDoor ||
                            terrainMap[j, i] == RoomTemplateTerrain.WallWithPossibleDoor)
                        {
                            doors.Add(new PotentialDoor(new Point(j, i)));
                        }
                    }
                }
                return doors;
            }
        }

        public RoomTemplate(RoomTemplateTerrain[,] terrain)
        {
            SetMapRelatedMembers(terrain);
        }

        private void SetMapRelatedMembers(RoomTemplateTerrain[,] terrain)
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
            terrainMapping['-'] = RoomTemplateTerrain.OpenWithPossibleDoor;
            terrainMapping['+'] = RoomTemplateTerrain.WallWithPossibleDoor;
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

        private static RoomTemplateTerrain[,] RotateTerrainRight(RoomTemplateTerrain[,] matrix)
        {
            int w = matrix.GetLength(0);
            int h = matrix.GetLength(1);
            RoomTemplateTerrain[,] ret = new RoomTemplateTerrain[h, w];
            for (int i = 0; i < w; ++i)
            {
                for (int j = h - 1; j >= 0; j--)
                {
                    ret[h - 1 - j, i] = matrix[i, j];
                }
            }
            return ret;
        }

        private static Point RotatePointRight(Point input, int height)
        {
            return new Point(height - 1 - input.y, input.x);
        }

        public static Point RotateRoomPoint(RoomTemplate templateToRotate, Point pointToRotate, TemplateRotation rotationAmount)
        {
            
            if (rotationAmount == TemplateRotation.Deg0)
                return pointToRotate;

            Point rotatedPoint;

            if (rotationAmount == TemplateRotation.Deg90)
            {
                rotatedPoint = RotatePointRight(pointToRotate, templateToRotate.Height);
            }
            else if (rotationAmount == TemplateRotation.Deg180)
            {
                rotatedPoint = RotatePointRight(pointToRotate, templateToRotate.Height);
                rotatedPoint = RotatePointRight(rotatedPoint, templateToRotate.Width);
            }
            else
            {
                //270
                rotatedPoint = RotatePointRight(pointToRotate, templateToRotate.Height);
                rotatedPoint = RotatePointRight(rotatedPoint, templateToRotate.Width);
                rotatedPoint = RotatePointRight(rotatedPoint, templateToRotate.Height);
            }

            return rotatedPoint;
        }

        public static RoomTemplate RotateRoomTemplate(RoomTemplate templateToRotate, TemplateRotation rotationAmount)
        {

            if (rotationAmount == TemplateRotation.Deg0)
                return templateToRotate;

            RoomTemplateTerrain[,] rotatedTerrain;

            if (rotationAmount == TemplateRotation.Deg90)
            {
                rotatedTerrain = RotateTerrainRight(templateToRotate.terrainMap);
            }
            else if (rotationAmount == TemplateRotation.Deg180)
            {
                RoomTemplateTerrain[,] oneRotation = RotateTerrainRight(templateToRotate.terrainMap);
                rotatedTerrain = RotateTerrainRight(oneRotation);
            }
            else
            {
                //270
                rotatedTerrain = RotateTerrainRight(templateToRotate.terrainMap);
                rotatedTerrain = RotateTerrainRight(rotatedTerrain);
                rotatedTerrain = RotateTerrainRight(rotatedTerrain);
            }

            return new RoomTemplate(rotatedTerrain);
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

        public static RoomTemplate.DoorLocation GetDoorLocation(RoomTemplate roomTemplate, int doorIndex)
        {
            if (roomTemplate.PotentialDoors.Count <= doorIndex)
                throw new Exception("Door index higher than available doors");

            Point doorLocation = roomTemplate.PotentialDoors[doorIndex].Location;

            if (doorLocation.y == 0)
            {
                return RoomTemplate.DoorLocation.Top;
            }
            else if (doorLocation.y == roomTemplate.Height - 1)
            {
                return RoomTemplate.DoorLocation.Bottom;
            }
            else if (doorLocation.x == 0)
            {
                return RoomTemplate.DoorLocation.Left;
            }
            else if (doorLocation.x == roomTemplate.Width - 1)
            {
                return RoomTemplate.DoorLocation.Right;
            }
            else
            {
                throw new ApplicationException("Door is not on circumference of room, can't cope");
            }
        }


        /// <summary>
        /// Align toAlignRoomTemplate so that a straight corridor can be drawn from baseRoom.
        /// Will rotate toAlignRoomTemplate if required
        /// </summary>
        public static Tuple<TemplatePositioned, Point> AlignRoomOnDoor(RoomTemplate toAlignRoomTemplate, TemplatePositioned baseRoom, int toAlignRoomDoorIndex, int baseRoomDoorIndex, int distanceApart)
        {
            Point toAlignDoorLocation = toAlignRoomTemplate.PotentialDoors[toAlignRoomDoorIndex].Location;
            Point baseDoorLocation = baseRoom.Room.PotentialDoors[baseRoomDoorIndex].Location;

            RoomTemplate.DoorLocation toAlignDoorLoc = GetDoorLocation(toAlignRoomTemplate, toAlignRoomDoorIndex);
            RoomTemplate.DoorLocation baseDoorLoc = GetDoorLocation(baseRoom.Room, baseRoomDoorIndex);

            RoomTemplate rotatedTemplate;
            Point rotatedtoAlignDoorLocation;

            if (baseDoorLoc == RoomTemplate.DoorLocation.Top)
            {
                if (toAlignDoorLoc == RoomTemplate.DoorLocation.Top)
                {
                    rotatedTemplate = RotateRoomTemplate(toAlignRoomTemplate, TemplateRotation.Deg180);
                    rotatedtoAlignDoorLocation = RotateRoomPoint(toAlignRoomTemplate, toAlignDoorLocation, TemplateRotation.Deg180);
                }
                else if (toAlignDoorLoc == RoomTemplate.DoorLocation.Left)
                {
                    rotatedTemplate = RotateRoomTemplate(toAlignRoomTemplate, TemplateRotation.Deg270);
                    rotatedtoAlignDoorLocation = RotateRoomPoint(toAlignRoomTemplate, toAlignDoorLocation, TemplateRotation.Deg270);
                }
                else if (toAlignDoorLoc == RoomTemplate.DoorLocation.Right)
                {
                    rotatedTemplate = RotateRoomTemplate(toAlignRoomTemplate, TemplateRotation.Deg90);
                    rotatedtoAlignDoorLocation = RotateRoomPoint(toAlignRoomTemplate, toAlignDoorLocation, TemplateRotation.Deg90);
                }
                else
                {
                    rotatedTemplate = toAlignRoomTemplate;
                    rotatedtoAlignDoorLocation = toAlignDoorLocation;
                }
            }
            else if (baseDoorLoc == RoomTemplate.DoorLocation.Right)
            {
                if (toAlignDoorLoc == RoomTemplate.DoorLocation.Top)
                {
                    rotatedTemplate = RotateRoomTemplate(toAlignRoomTemplate, TemplateRotation.Deg270);
                    rotatedtoAlignDoorLocation = RotateRoomPoint(toAlignRoomTemplate, toAlignDoorLocation, TemplateRotation.Deg270);
                }
                else if (toAlignDoorLoc == RoomTemplate.DoorLocation.Left)
                {
                    rotatedTemplate = toAlignRoomTemplate;
                    rotatedtoAlignDoorLocation = toAlignDoorLocation;
                }
                else if (toAlignDoorLoc == RoomTemplate.DoorLocation.Right)
                {
                    rotatedTemplate = RotateRoomTemplate(toAlignRoomTemplate, TemplateRotation.Deg180);
                    rotatedtoAlignDoorLocation = RotateRoomPoint(toAlignRoomTemplate, toAlignDoorLocation, TemplateRotation.Deg180);
                }
                else
                {
                    rotatedTemplate = RotateRoomTemplate(toAlignRoomTemplate, TemplateRotation.Deg90);
                    rotatedtoAlignDoorLocation = RotateRoomPoint(toAlignRoomTemplate, toAlignDoorLocation, TemplateRotation.Deg90);
                }
            }
            else if (baseDoorLoc == RoomTemplate.DoorLocation.Left)
            {
                if (toAlignDoorLoc == RoomTemplate.DoorLocation.Top)
                {
                    rotatedTemplate = RotateRoomTemplate(toAlignRoomTemplate, TemplateRotation.Deg90);
                    rotatedtoAlignDoorLocation = RotateRoomPoint(toAlignRoomTemplate, toAlignDoorLocation, TemplateRotation.Deg90);
                }
                else if (toAlignDoorLoc == RoomTemplate.DoorLocation.Left)
                {
                    rotatedTemplate = RotateRoomTemplate(toAlignRoomTemplate, TemplateRotation.Deg180);
                    rotatedtoAlignDoorLocation = RotateRoomPoint(toAlignRoomTemplate, toAlignDoorLocation, TemplateRotation.Deg180);
                }
                else if (toAlignDoorLoc == RoomTemplate.DoorLocation.Right)
                {
                    rotatedTemplate = toAlignRoomTemplate;
                    rotatedtoAlignDoorLocation = toAlignDoorLocation;
                }
                else
                {
                    rotatedTemplate = RotateRoomTemplate(toAlignRoomTemplate, TemplateRotation.Deg270);
                    rotatedtoAlignDoorLocation = RotateRoomPoint(toAlignRoomTemplate, toAlignDoorLocation, TemplateRotation.Deg270);
                }
            }
            else
            {
                //Bottom

                if (toAlignDoorLoc == RoomTemplate.DoorLocation.Top)
                {
                    rotatedTemplate = toAlignRoomTemplate;
                    rotatedtoAlignDoorLocation = toAlignDoorLocation;
                }
                else if (toAlignDoorLoc == RoomTemplate.DoorLocation.Left)
                {
                    rotatedTemplate = RotateRoomTemplate(toAlignRoomTemplate, TemplateRotation.Deg90);
                    rotatedtoAlignDoorLocation = RotateRoomPoint(toAlignRoomTemplate, toAlignDoorLocation, TemplateRotation.Deg90);
                }
                else if (toAlignDoorLoc == RoomTemplate.DoorLocation.Right)
                {
                    rotatedTemplate = RotateRoomTemplate(toAlignRoomTemplate, TemplateRotation.Deg270);
                    rotatedtoAlignDoorLocation = RotateRoomPoint(toAlignRoomTemplate, toAlignDoorLocation, TemplateRotation.Deg270);
                }
                else
                {
                    rotatedTemplate = RotateRoomTemplate(toAlignRoomTemplate, TemplateRotation.Deg180);
                    rotatedtoAlignDoorLocation = RotateRoomPoint(toAlignRoomTemplate, toAlignDoorLocation, TemplateRotation.Deg180);
                }
            }

            int xOffset = baseDoorLocation.x - rotatedtoAlignDoorLocation.x;
            int yOffset = baseDoorLocation.y - rotatedtoAlignDoorLocation.y;

            Point toAlignRoomPosition;

            if (baseDoorLoc == RoomTemplate.DoorLocation.Bottom)
            {
                //Vertical alignment
                toAlignRoomPosition = new Point(baseRoom.X + xOffset, baseRoom.Y + baseRoom.Room.Height + distanceApart - 1);
            }
            else if (baseDoorLoc == RoomTemplate.DoorLocation.Top)
            {
                toAlignRoomPosition = new Point(baseRoom.X + xOffset, baseRoom.Y - distanceApart - (rotatedTemplate.Height - 1));
            }
            else if (baseDoorLoc == RoomTemplate.DoorLocation.Right)
            {
                //Horizontal alignment
                toAlignRoomPosition = new Point(baseRoom.X + baseRoom.Room.Width - 1 + distanceApart, baseRoom.Y + yOffset);
            }
            else
            {
                toAlignRoomPosition = new Point(baseRoom.X - distanceApart - (rotatedTemplate.Width - 1), baseRoom.Y + yOffset);
            }

            TemplatePositioned rotatedTemplatePosition = new TemplatePositioned(toAlignRoomPosition.x, toAlignRoomPosition.y, baseRoom.Z + 1, rotatedTemplate, TemplateRotation.Deg0);
            Point rotatedDoorLocation = new Point(toAlignRoomPosition.x + rotatedtoAlignDoorLocation.x, toAlignRoomPosition.y + rotatedtoAlignDoorLocation.y);

            return new Tuple<TemplatePositioned, Point>(rotatedTemplatePosition, rotatedDoorLocation);
        }

        public static Tuple<Point, Point> CorridorTerminalPointsBetweenDoors(Point start, Point end)
        {
            Point corridorStart;
            Point corridorEnd;

            if (start.x != end.x && start.y != end.y)
                throw new ArgumentException("Start and end must be in cardinal direction");

            if (start.x == end.x)
            {
                //vertical
                if (start.y > end.y)
                {
                    corridorStart = new Point(start.x, start.y - 1);
                    corridorEnd = new Point(end.x, end.y + 1);
                }
                else
                {
                    corridorStart = new Point(start.x, start.y + 1);
                    corridorEnd = new Point(end.x, end.y - 1);
                }
            }
            else {
                //horizontal
                if(start.x > end.x) {
                    corridorStart = new Point(start.x - 1, start.y);
                    corridorEnd = new Point(end.x + 1, start.y);
                }
                else {
                    corridorStart = new Point(start.x + 1, start.y);
                    corridorEnd = new Point(end.x - 1, start.y);
                }
            }

            return new Tuple<Point, Point>(corridorStart, corridorEnd);
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
            List<RoomTemplate.PotentialDoor> potentialDoors = new List<RoomTemplate.PotentialDoor>();

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
