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
            Top = 0, Left = 1, Bottom = 2, Right = 3
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
        Transparent, //default
        Floor,
        Wall,
        WallWithPossibleDoor,
        OpenWithPossibleDoor
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
        public static Dictionary<RoomTemplateTerrain, char> RoomTerrainChars { get; private set; }

        static RoomTemplateUtilities()
        {
            RoomTerrainChars = new Dictionary<RoomTemplateTerrain, char>();
            SetupRoomTerrainChars();
        }

        private static void SetupRoomTerrainChars()
        {
            RoomTerrainChars.Add(RoomTemplateTerrain.Floor, '.');
            RoomTerrainChars.Add(RoomTemplateTerrain.OpenWithPossibleDoor, '+');
            RoomTerrainChars.Add(RoomTemplateTerrain.Transparent, ' ');
            RoomTerrainChars.Add(RoomTemplateTerrain.Wall, '#');
            RoomTerrainChars.Add(RoomTemplateTerrain.WallWithPossibleDoor, '#');
        }

        public static Point GetRandomPointWithTerrain(RoomTemplate room, RoomTemplateTerrain terrainToFind)
        {
            var candidatePoints = new List<Point>();

            for (int i = 0; i < room.Width; i++)
            {
                for (int j = 0; j < room.Height; j++)
                {
                    if (room.terrainMap[i, j] == terrainToFind)
                        candidatePoints.Add(new Point(i, j));
                }
            }

            return candidatePoints[Game.Random.Next(candidatePoints.Count)];
        }

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

        /// <summary>
        /// Returns the template and the offset onto the template of the start points (where the offsets are relative to)
        /// </summary>
        public static Tuple<RoomTemplate, Point> ExpandCorridorTemplateBend(int xOffset, int yOffset, int lTransition, bool switchToHorizontal, RoomTemplate corridorTemplate)
        {
            if (corridorTemplate.Height > 1)
                throw new ApplicationException("Only corridor templates of height 1 supported");

            //I think the code is getting there for #..# but I don't want to put the time in now
            if (corridorTemplate.Width != 3)
                throw new ApplicationException("Only corridor templates of width 3 supported");

            if (Math.Abs(lTransition) < 1)
                throw new ApplicationException("transition must be at least 1");

            if(Math.Abs(xOffset) < 2 && switchToHorizontal)
                throw new ApplicationException("Need x-offset of at least 2 for horizontal corridor");

            if (Math.Abs(yOffset) < 2 && !switchToHorizontal)
                throw new ApplicationException("Need y-offset of at least 2 for vertical corridor");

            int mirroring = 0;
            RoomTemplateTerrain[,] newRoom;
            bool useEndPoint = false;

            if (xOffset < 0 && yOffset < 0)
            {
                useEndPoint = switchToHorizontal ? false : true;
            }
            if (xOffset > 0 && yOffset > 0)
            {
                useEndPoint = switchToHorizontal ? true : false;
            }
            if (xOffset < 0 && yOffset > 0)
            {
                useEndPoint = switchToHorizontal ? true : false;
            }
            if (xOffset > 0 && yOffset < 0)
            {
                useEndPoint = switchToHorizontal ? false : true;
            }

            Point startPoint;
            Point endPoint;

            if (switchToHorizontal)
            {
                if (xOffset < 0 && lTransition > 0 ||
                    xOffset > 0 && lTransition < 0)
                    throw new ApplicationException("Transition is not within corridor");

                //Horizontal
                mirroring = 2;
                int transition = lTransition > 0 ? Math.Abs(xOffset) - lTransition : -lTransition;

                if (xOffset * yOffset < 0)
                {
                    mirroring = 3;
                    transition = lTransition > 0 ? lTransition : Math.Abs(xOffset) + lTransition;
                }

                var corridorBend = GenerateBaseCorridorBend(yOffset, xOffset, transition, corridorTemplate);
                newRoom = corridorBend.Item1;
                startPoint = corridorBend.Item2;
                endPoint = corridorBend.Item3;
            }
            else
            {
                if (yOffset < 0 && lTransition > 0 ||
                    yOffset > 0 && lTransition < 0)
                    throw new ApplicationException("Transition is not within corridor");

                //Vertical
                int transition = lTransition > 0 ? lTransition : Math.Abs(yOffset) + lTransition;
                if (xOffset * yOffset < 0)
                    mirroring = 1;

                var corridorBend = GenerateBaseCorridorBend(xOffset, yOffset, transition, corridorTemplate);
                newRoom = corridorBend.Item1;
                startPoint = corridorBend.Item2;
                endPoint = corridorBend.Item3;
            }

            RoomTemplate templateToReturn = null;
            Point startPointToRet = null;
            Point endPointToRet = null;

            if (mirroring == 0)
            {
                templateToReturn = new RoomTemplate(newRoom);
                startPointToRet = startPoint;
                endPointToRet = endPoint;
            }
            //Horizontal reflection
            if (mirroring == 1)
            {
                var mirrorRoom = new RoomTemplateTerrain[newRoom.GetLength(0), newRoom.GetLength(1)];

                for (int i = 0; i < newRoom.GetLength(0); i++)
                {
                    for (int j = 0; j < newRoom.GetLength(1); j++)
                    {
                        mirrorRoom[newRoom.GetLength(0) - 1 - i, j] = newRoom[i, j];
                    }
                }
                templateToReturn = new RoomTemplate(mirrorRoom);
                startPointToRet = new Point(newRoom.GetLength(0) - 1 - startPoint.x, startPoint.y);
                endPointToRet = new Point(newRoom.GetLength(0) - 1 - endPoint.x, endPoint.y);
            }

            //X-Y mirror
            if (mirroring == 2)
            {
                var mirrorRoom = new RoomTemplateTerrain[newRoom.GetLength(1), newRoom.GetLength(0)];

                for (int i = 0; i < newRoom.GetLength(0); i++)
                {
                    for (int j = 0; j < newRoom.GetLength(1); j++)
                    {
                        mirrorRoom[newRoom.GetLength(1) - 1 - j, newRoom.GetLength(0) - 1 - i] = newRoom[i, j];
                    }
                }
                templateToReturn = new RoomTemplate(mirrorRoom);
                startPointToRet = new Point(newRoom.GetLength(1) - 1 - startPoint.y, newRoom.GetLength(0) - 1 - startPoint.x);
                endPointToRet = new Point(newRoom.GetLength(1) - 1 - endPoint.y, newRoom.GetLength(0) - 1 - endPoint.x);
            }

            //X-Y mirror, Y reflect
            if (mirroring == 3)
            {
                var mirrorRoom = new RoomTemplateTerrain[newRoom.GetLength(1), newRoom.GetLength(0)];

                for (int i = 0; i < newRoom.GetLength(0); i++)
                {
                    for (int j = 0; j < newRoom.GetLength(1); j++)
                    {
                        mirrorRoom[j, newRoom.GetLength(0) - 1 - i] = newRoom[i, j];
                    }
                }
                templateToReturn = new RoomTemplate(mirrorRoom);
                startPointToRet = new Point(startPoint.y, newRoom.GetLength(0) - 1 - startPoint.x);
                endPointToRet = new Point(endPoint.y, newRoom.GetLength(0) - 1 - endPoint.x);
            }

            if (useEndPoint)
                return new Tuple<RoomTemplate, Point>(templateToReturn, endPointToRet);
            else
                return new Tuple<RoomTemplate, Point>(templateToReturn, startPointToRet);
        }
        
        public static Tuple<RoomTemplate, Point> ExpandCorridorTemplateLShaped(int xOffset, int yOffset, bool horizontalFirst, RoomTemplate corridorTemplate)
        {
            if (corridorTemplate.Height > 1)
                throw new ApplicationException("Only corridor templates of height 1 supported");

            //I think the code is getting there for #..# but I don't want to put the time in now
            if (corridorTemplate.Width != 3)
                throw new ApplicationException("Only corridor templates of width 3 supported");

            if(Math.Abs(xOffset) < 1 || Math.Abs(yOffset) < 1)
                throw new ApplicationException("offset must be at least 1");

            int mirroring = 0;
            RoomTemplateTerrain[,] newRoom;
            bool useEndPoint = false;

            if (xOffset < 0 && yOffset < 0) {
                mirroring = horizontalFirst ? 0 : 2;
                useEndPoint = horizontalFirst ? true : false;
            }
            if (xOffset > 0 && yOffset > 0) {
                mirroring = horizontalFirst ? 2 : 0;
                useEndPoint = horizontalFirst ? true : false;
            }
            if (xOffset < 0 && yOffset > 0) {
                mirroring = horizontalFirst ? 3 : 1;
                useEndPoint = horizontalFirst ? true : false;
            }
            if (xOffset > 0 && yOffset < 0) {
                mirroring = horizontalFirst ? 1 : 3;
                useEndPoint = horizontalFirst ? true : false;
            }
            
            var roomAndStartPoint = GenerateBaseCorridorLShaped(Math.Abs(xOffset), Math.Abs(yOffset), corridorTemplate);
            newRoom = roomAndStartPoint.Item1;
            var startPoint = roomAndStartPoint.Item2;
            var endPoint = roomAndStartPoint.Item3;

            RoomTemplate mapToReturn = null;
            Point startPointMirror = null;
            Point endPointMirror = null;

            if (mirroring == 0)
            {
                mapToReturn = new RoomTemplate(newRoom);
                startPointMirror = startPoint;
                endPointMirror = endPoint;
            }
            //Horizontal reflection
            if (mirroring == 1)
            {
                var mirrorRoom = new RoomTemplateTerrain[newRoom.GetLength(0), newRoom.GetLength(1)];

                for (int i = 0; i < newRoom.GetLength(0); i++)
                {
                    for (int j = 0; j < newRoom.GetLength(1); j++)
                    {
                        mirrorRoom[newRoom.GetLength(0) - 1 - i, j] = newRoom[i, j];
                    }
                }
                mapToReturn = new RoomTemplate(mirrorRoom);
                startPointMirror = new Point(newRoom.GetLength(0) - 1 - startPoint.x, startPoint.y);
                endPointMirror = new Point(newRoom.GetLength(0) - 1 - endPoint.x, endPoint.y);
            }

            //Y=-X mirror
            if (mirroring == 2)
            {
                var mirrorRoom = new RoomTemplateTerrain[newRoom.GetLength(0), newRoom.GetLength(1)];

                for (int i = 0; i < newRoom.GetLength(0); i++)
                {
                    for (int j = 0; j < newRoom.GetLength(1); j++)
                    {
                        mirrorRoom[i, j] = newRoom[newRoom.GetLength(0) - 1 - i, newRoom.GetLength(1) - 1 - j];
                    }
                }
                mapToReturn =  new RoomTemplate(mirrorRoom);
                startPointMirror = new Point(newRoom.GetLength(0) - 1 - startPoint.x, newRoom.GetLength(1) - 1 - startPoint.y);
                endPointMirror = new Point(newRoom.GetLength(0) - 1 - endPoint.x, newRoom.GetLength(1) - 1 - endPoint.y);
            }

            //Vertical reflection
            if (mirroring == 3)
            {
                var mirrorRoom = new RoomTemplateTerrain[newRoom.GetLength(0), newRoom.GetLength(1)];

                for (int i = 0; i < newRoom.GetLength(0); i++)
                {
                    for (int j = 0; j < newRoom.GetLength(1); j++)
                    {
                        mirrorRoom[i, newRoom.GetLength(1) - 1 - j] = newRoom[i, j];
                    }
                }
                mapToReturn = new RoomTemplate(mirrorRoom);
                startPointMirror = new Point(startPoint.x, newRoom.GetLength(1) - 1 - startPoint.y);
                endPointMirror = new Point(endPoint.x, newRoom.GetLength(1) - 1 - endPoint.y);
            }

            if(useEndPoint)
                return new Tuple<RoomTemplate, Point>(mapToReturn, endPointMirror);
            else
                return new Tuple<RoomTemplate, Point>(mapToReturn, startPointMirror);
        }

        public static Tuple<RoomTemplate, Point> ExpandCorridorTemplateStraight(int offset, bool switchToHorizontal, RoomTemplate corridorTemplate)
        {
            var length = Math.Abs(offset) + 1;
            var expandedTemplate = ExpandCorridorTemplate(switchToHorizontal, length, corridorTemplate);

            Point corridorEndPoint;

            if (offset > 0)
                corridorEndPoint = new Point(0, 1);
            else
            {
                if(switchToHorizontal)
                    corridorEndPoint = new Point(-offset, 1);
                else
                    corridorEndPoint = new Point(1, -offset);
            }
                

            return new Tuple<RoomTemplate, Point>(expandedTemplate, corridorEndPoint);
        }

        private static Tuple<RoomTemplateTerrain[,], Point, Point> GenerateBaseCorridorBend(int xOffset, int yOffset, int lTransition, RoomTemplate corridorTemplate)
        {
            var absXOffset = Math.Abs(xOffset);
            var absYOffset = Math.Abs(yOffset);

            var width = absXOffset + 1 + corridorTemplate.Width - 1;
            var height = absYOffset + 1;

            var leftFromCentre = (int)Math.Floor((corridorTemplate.Width - 1) / 2.0);
            var rightFromCentre = corridorTemplate.Width - 1 - leftFromCentre;

            var openSquares = corridorTemplate.Width - 2;

            var newRoom = new RoomTemplateTerrain[width, height];

            //Down left
            for (int j = 0; j <= lTransition; j++)
            {
                for (int i = 0; i < corridorTemplate.Width; i++)
                {
                    newRoom[i, j] = corridorTemplate.terrainMap[i, 0];
                }
            }

            //Cap
            for (int i = 0; i < corridorTemplate.Width; i++)
            {
                //Use the outside character (should be solid)
                if (newRoom[i, lTransition + 1] == RoomTemplateTerrain.Transparent)
                    newRoom[i, lTransition + 1] = corridorTemplate.terrainMap[0, 0];
            }

            //Down right
            for (int j = lTransition; j <= absYOffset; j++)
            {
                for (int i = 0; i < corridorTemplate.Width; i++)
                {
                    newRoom[absXOffset + 1 - rightFromCentre + i, j] = corridorTemplate.terrainMap[i, 0];
                }
            }

            //Cap
            for (int i = 0; i < corridorTemplate.Width; i++)
            {
                //Use the outside character (should be solid)
                if (newRoom[absXOffset + 1 - rightFromCentre + i, lTransition - 1] == RoomTemplateTerrain.Transparent)
                    newRoom[absXOffset + 1 - rightFromCentre + i, lTransition - 1] = corridorTemplate.terrainMap[0, 0];
            }

            //Overlay rotated cross-corridor. Always prefer open to closed
            for (int j = 1; j <= absXOffset; j++)
            {
                for (int i = 0; i < corridorTemplate.Width; i++)
                {
                    if (newRoom[j, lTransition - 1 + i] == RoomTemplateTerrain.Transparent ||
                        corridorTemplate.terrainMap[i, 0] == RoomTemplateTerrain.Floor)
                        newRoom[j, lTransition - 1 + i] = corridorTemplate.terrainMap[i, 0];
                }
            }
            return new Tuple<RoomTemplateTerrain[,], Point, Point>(newRoom, new Point(leftFromCentre, 0), new Point(width - 1 - rightFromCentre, absYOffset));
        }

        /// <summary>
        /// Returns terrain, start of corridor, end of corridor
        /// </summary>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="corridorTemplate"></param>
        /// <returns></returns>
        private static Tuple<RoomTemplateTerrain[,], Point, Point> GenerateBaseCorridorLShaped(int xOffset, int yOffset, RoomTemplate corridorTemplate)
        {
            var absXOffset = Math.Abs(xOffset);
            var absYOffset = Math.Abs(yOffset);

            var leftFromCentre = (int)Math.Floor((corridorTemplate.Width - 1) / 2.0);
            var rightFromCentre = corridorTemplate.Width - 1 - leftFromCentre;

            var width = absXOffset + leftFromCentre + 1;
            var height = absYOffset + corridorTemplate.Width - 1;

            var openSquares = corridorTemplate.Width - 2;

            var newRoom = new RoomTemplateTerrain[width, height];

            //Down left
            for (int j = 0; j <= yOffset; j++)
            {
                for (int i = 0; i < corridorTemplate.Width; i++)
                {
                    newRoom[i, j] = corridorTemplate.terrainMap[i, 0];
                }
            }

            //Cap
            for (int i = 0; i < corridorTemplate.Width; i++)
            {
                //Use the outside character (should be solid)
                newRoom[i, yOffset + 1] = corridorTemplate.terrainMap[0, 0];
            }

            //Overlay rotated cross-corridor. Always prefer open to closed
            for (int j = 1; j <= absXOffset + 1; j++)
            {
                for (int i = 0; i < corridorTemplate.Width; i++)
                {
                    if (newRoom[j, yOffset - 1 + i] == RoomTemplateTerrain.Transparent ||
                        corridorTemplate.terrainMap[i, 0] == RoomTemplateTerrain.Floor)
                        newRoom[j, yOffset - 1 + i] = corridorTemplate.terrainMap[i, 0];
                }
            }
            return new Tuple<RoomTemplateTerrain[,], Point, Point>(newRoom, new Point(leftFromCentre, 0), new Point(absXOffset + leftFromCentre, absYOffset));
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

        public static Point RotateRoomPoint(RoomTemplate templateToRotate, Point pointToRotate, int ninetyDegreeAntiClockwiseSteps) {
            
            Point rotatedPoint = pointToRotate;

            for (int i = 0; i < ninetyDegreeAntiClockwiseSteps; i++) {

                int dimension = i % 2 == 1 ? templateToRotate.Width : templateToRotate.Height;
                rotatedPoint = RotatePointRight(rotatedPoint, dimension);
            }
           
            return rotatedPoint;
        }

        
        public static Point RotateRoomPoint(RoomTemplate templateToRotate, Point pointToRotate, TemplateRotation rotationAmount)
        {
            return RotateRoomPoint(templateToRotate, pointToRotate, (int)rotationAmount);
        }


        public static RoomTemplate RotateRoomTemplate(RoomTemplate templateToRotate, int ninetyDegreeAntiClockwiseSteps)
        {

            RoomTemplateTerrain[,] rotatedTerrain = templateToRotate.terrainMap;

            for (int i = 0; i < ninetyDegreeAntiClockwiseSteps; i++) { 
                    rotatedTerrain = RotateTerrainRight(rotatedTerrain);
            }

            return new RoomTemplate(rotatedTerrain);
        }

        public static RoomTemplate RotateRoomTemplate(RoomTemplate templateToRotate, TemplateRotation rotationAmount)
        {
            return RotateRoomTemplate(templateToRotate, (int)rotationAmount);
        }

        /** Expand a corridor template (vertically aligned) into a suitable room template */
        static public TemplatePositioned GetTemplateForCorridorBetweenPoints(Point point1, Point point2, int z, RoomTemplate corridorTemplate, int corridorRoomIndex)
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

            return new TemplatePositioned(left, top, z, expandedCorridor, corridorRoomIndex);
        }

        static public bool ArePointsOnVerticalLine(Point point1, Point point2)
        {
            return point1.x == point2.x;
        }

        static public bool ArePointsOnHorizontalLine(Point point1, Point point2)
        {
            return point1.y == point2.y;
        }

        /** Orient a corridorTemplate for a width 1 / height 1 corridor */
        static public TemplatePositioned GetTemplateForSingleSpaceCorridor(Point location, bool corridorRunVertically, int z, RoomTemplate corridorTemplate, int corridorRoomIndex)
        {
            if (corridorTemplate.Height > 1)
                throw new ApplicationException("Corridor template is too long for gap");

            bool horizontalSwitchNeeded = !corridorRunVertically;

            RoomTemplate expandedCorridor = ExpandCorridorTemplate(horizontalSwitchNeeded, 1, corridorTemplate);

            int centreOfTemplateShortAxis = corridorTemplate.Width / 2;

            int left = location.x;
            int top = location.y;

            //Find the TL for the template to be placed
            if (horizontalSwitchNeeded)
            {
                top -= centreOfTemplateShortAxis;
            }
            else
            {
                left -= centreOfTemplateShortAxis;
            }

            return new TemplatePositioned(left, top, z, expandedCorridor, corridorRoomIndex);
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

        public static RoomTemplate.DoorLocation GetOppositeDoorLocation(RoomTemplate.DoorLocation doorLocation)
        {
            switch (doorLocation)
            {
                case RoomTemplate.DoorLocation.Bottom:
                    return RoomTemplate.DoorLocation.Top;
                case RoomTemplate.DoorLocation.Top:
                    return RoomTemplate.DoorLocation.Bottom;
                case RoomTemplate.DoorLocation.Left:
                    return RoomTemplate.DoorLocation.Right;
                case RoomTemplate.DoorLocation.Right:
                    return RoomTemplate.DoorLocation.Left;
            }

            throw new ApplicationException("Unknown door location");
        }

        /// <summary>
        /// Align toAlignRoomTemplate so that a straight corridor can be drawn from baseRoom.
        /// Will rotate toAlignRoomTemplate if required
        /// </summary>
        public static Tuple<TemplatePositioned, Point> AlignRoomFacing(RoomTemplate toAlignRoomTemplate, int toAlignRoomIndex, TemplatePositioned baseRoom, int toAlignRoomDoorIndex, int baseRoomDoorIndex, int distanceApart)
        {
            Point toAlignDoorLocation = toAlignRoomTemplate.PotentialDoors[toAlignRoomDoorIndex].Location;
            Point baseDoorLocation = baseRoom.Room.PotentialDoors[baseRoomDoorIndex].Location;

            RoomTemplate.DoorLocation toAlignDoorLoc = GetDoorLocation(toAlignRoomTemplate, toAlignRoomDoorIndex);
            RoomTemplate.DoorLocation baseDoorLoc = GetDoorLocation(baseRoom.Room, baseRoomDoorIndex);

            RoomTemplate rotatedTemplate;
            Point rotatedtoAlignDoorLocation;

            //B is toAlignRoomTemplate
            //A is baseTemplate

            //Rotate 2 + (Bi - Ai) * 90 degree steps clockwise.

            int stepsToRotate = 2 + ((int)toAlignDoorLoc - (int)baseDoorLoc);
            if (stepsToRotate < 0)
                stepsToRotate += 4;
            if (stepsToRotate >= 4)
                stepsToRotate -= 4;

            rotatedTemplate = RotateRoomTemplate(toAlignRoomTemplate, stepsToRotate);
            rotatedtoAlignDoorLocation = RotateRoomPoint(toAlignRoomTemplate, toAlignDoorLocation, stepsToRotate);

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

            TemplatePositioned rotatedTemplatePosition = new TemplatePositioned(toAlignRoomPosition.x, toAlignRoomPosition.y, baseRoom.Z + 1, rotatedTemplate, toAlignRoomIndex);
            Point rotatedDoorLocation = new Point(toAlignRoomPosition.x + rotatedtoAlignDoorLocation.x, toAlignRoomPosition.y + rotatedtoAlignDoorLocation.y);

            return new Tuple<TemplatePositioned, Point>(rotatedTemplatePosition, rotatedDoorLocation);
        }

        /// <summary>
        /// Align toAlignRoomTemplate so that it matches the alignment of baseRoom and the target doors overlap
        /// </summary>
        public static Tuple<TemplatePositioned, Point> AlignRoomOverlapping(RoomTemplate toAlignRoomTemplate, int toAlignRoomIndex, TemplatePositioned baseRoom, int toAlignRoomDoorIndex, int baseRoomDoorIndex)
        {
            Point toAlignDoorLocation = toAlignRoomTemplate.PotentialDoors[toAlignRoomDoorIndex].Location;
            Point baseDoorLocation = baseRoom.Room.PotentialDoors[baseRoomDoorIndex].Location;

            RoomTemplate.DoorLocation toAlignDoorLoc = GetDoorLocation(toAlignRoomTemplate, toAlignRoomDoorIndex);
            RoomTemplate.DoorLocation baseDoorLoc = GetDoorLocation(baseRoom.Room, baseRoomDoorIndex);

            RoomTemplate rotatedTemplate;
            Point rotatedtoAlignDoorLocation;

            //B is toAlignRoomTemplate
            //A is baseTemplate

            //Rotate 2 + (Bi - Ai) * 90 degree steps clockwise.

            int stepsToRotate = ((int)toAlignDoorLoc - (int)baseDoorLoc);
            if (stepsToRotate < 0)
                stepsToRotate += 4;
            if (stepsToRotate >= 4)
                stepsToRotate -= 4;

            rotatedTemplate = RotateRoomTemplate(toAlignRoomTemplate, stepsToRotate);
            rotatedtoAlignDoorLocation = RotateRoomPoint(toAlignRoomTemplate, toAlignDoorLocation, stepsToRotate);

            int xOffset = baseDoorLocation.x - rotatedtoAlignDoorLocation.x;
            int yOffset = baseDoorLocation.y - rotatedtoAlignDoorLocation.y;

            Point toAlignRoomPosition = new Point(baseRoom.X + xOffset, baseRoom.Y + yOffset);
            
            TemplatePositioned rotatedTemplatePosition = new TemplatePositioned(toAlignRoomPosition.x, toAlignRoomPosition.y, baseRoom.Z + 1, rotatedTemplate, toAlignRoomIndex);
            Point rotatedDoorLocation = new Point(toAlignRoomPosition.x + rotatedtoAlignDoorLocation.x, toAlignRoomPosition.y + rotatedtoAlignDoorLocation.y);

            return new Tuple<TemplatePositioned, Point>(rotatedTemplatePosition, rotatedDoorLocation);
        }

        public static Tuple<Point, Point> CorridorTerminalPointsBetweenDoors(Point start, RoomTemplate.DoorLocation startDoorLoc, Point end, RoomTemplate.DoorLocation endDoorLoc)
        {
            return new Tuple<Point, Point>(GetPointOutsideDoor(start, startDoorLoc), GetPointOutsideDoor(end, endDoorLoc));
        }

        public static Point GetPointOutsideDoor(Point start, RoomTemplate.DoorLocation startDoorLoc) {
          
            switch(startDoorLoc) {
                case RoomTemplate.DoorLocation.Bottom:
                    return new Point(start.x, start.y + 1);
                case RoomTemplate.DoorLocation.Top:
                    return new Point(start.x, start.y - 1);
                case RoomTemplate.DoorLocation.Left:
                    return new Point(start.x - 1, start.y);
                case RoomTemplate.DoorLocation.Right:
                    return new Point(start.x + 1, start.y);
            }

            throw new ApplicationException("location not implemented");
        }

        public static double DistanceBetween(Point p1, Point p2)
        {
            return Math.Sqrt((p1.x - p2.x) ^ 2 + (p1.y - p2.y) ^ 2);
        }

        static public void ExportTemplateToTextFile(RoomTemplate templateToExport, string filename)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filename, false))
                {
                    for (int j = 0; j < templateToExport.Height; j++)
                    {
                        StringBuilder mapLine = new StringBuilder();

                        for (int i = 0; i < templateToExport.Width; i++)
                        {
                            //Defaults
                            char screenChar = RoomTerrainChars[templateToExport.terrainMap[i, j]];
                            mapLine.Append(screenChar);
                        }
                        writer.WriteLine(mapLine);
                    }
                }
            }
            catch (Exception e)
            {
                LogFile.Log.LogEntryDebug("Failed to write file " + e.Message, LogDebugLevel.High);
            }
        }

        public static bool CanBeConnectedWithLShapedCorridor(Point door1Coord, RoomTemplate.DoorLocation door1Loc, Point door2Coord, RoomTemplate.DoorLocation door2Loc)
        {
            //Directs and opposites can't connect

            if(door1Loc == door2Loc)
                return false;

            if(door1Loc == GetOppositeDoorLocation(door2Loc))
                return false;

            //Both doors must be in each other's mutual acceptance areas

           if(!(DoorLocationIsPossible(door1Loc, door1Coord, door2Coord) && DoorLocationIsPossible(door2Loc, door2Coord, door1Coord)))
               return false;

            //Offset in x and y must be at least 1
            if (door1Coord.x == door2Coord.x || door1Coord.y == door2Coord.y)
               return false;

           return true;
        }

        private static bool DoorLocationIsPossible(RoomTemplate.DoorLocation door1Loc, Point door1Coord, Point door2Coord)
        {
            if (door1Loc == RoomTemplate.DoorLocation.Top &&
                door2Coord.y > door1Coord.y)
                return false;

            if (door1Loc == RoomTemplate.DoorLocation.Bottom &&
                door2Coord.y < door1Coord.y)
                return false;

            if (door1Loc == RoomTemplate.DoorLocation.Left &&
                door2Coord.x > door1Coord.x)
                return false;

            if (door1Loc == RoomTemplate.DoorLocation.Right &&
                door2Coord.x < door1Coord.x)
                return false;

            return true;
        }

        public static bool CanBeConnectedWithBendCorridor(Point door1Coord, RoomTemplate.DoorLocation door1Loc, Point door2Coord, RoomTemplate.DoorLocation door2Loc)
        {
            //Door orientation must be opposite

            if (door1Loc != GetOppositeDoorLocation(door2Loc))
                return false;

            //Door 2 must be acceptable area for door1

            if (!DoorLocationIsPossible(door1Loc, door1Coord, door2Coord))
                return false;

            //There must be sufficient length to implement a bend

            if (door1Loc == RoomTemplate.DoorLocation.Left || door1Loc == RoomTemplate.DoorLocation.Right)
            {
                if (Math.Abs(door1Coord.x - door2Coord.x) < 2)
                    return false;
            }
            else {
                if (Math.Abs(door1Coord.y - door2Coord.y) < 2)
                    return false;
            }

            //There must be sufficient width to implement a bend

            if (door1Coord.x == door2Coord.x || door1Coord.y == door2Coord.y)
                return false;

            return true;

        }

        public static bool CanBeConnectedWithStraightCorridor(Point door1Coord, RoomTemplate.DoorLocation door1Loc, Point door2Coord, RoomTemplate.DoorLocation door2Loc)
        {
            //Door orientation must be opposite

            if (door1Loc != GetOppositeDoorLocation(door2Loc))
                return false;

            //Door 2 must be acceptable area for door1 (i.e. correct relative position)

            if (!DoorLocationIsPossible(door1Loc, door1Coord, door2Coord))
                return false;

            if (door1Loc == RoomTemplate.DoorLocation.Top || door1Loc == RoomTemplate.DoorLocation.Bottom)
                return ArePointsOnVerticalLine(door1Coord, door2Coord);

            //if (door1Loc == RoomTemplate.DoorLocation.Left || door1Loc == RoomTemplate.DoorLocation.Right)
            return ArePointsOnHorizontalLine(door1Coord, door2Coord);
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
