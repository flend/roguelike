using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{

    enum FillingTerrain
    {
        Walkable,
        Unwalkable,
        MustBeWalkable,
        ClosedDoor
    }

    class FillingMap : FieldMap<FillingTerrain>
    {

        public FillingMap(int width, int height)
            : base(width, height)
        {

        }
    };

    public class RoomFilling
    {
        RoomTemplate template;
        FillingMap thisMap;
        HashSet<Point> foundPathablePoints;
        bool result;

        public RoomFilling(RoomTemplate template)
        {
            this.template = template;

            BuildPathableMap();
        }

        private void BuildPathableMap()
        {
            thisMap = new FillingMap(template.Width, template.Height);

            for (int i = 0; i < template.Width; i++)
            {
                for (int j = 0; j < template.Height; j++)
                {
                    thisMap.setCell(i, j,
                        RoomTemplateTerrainWalkable.terrainWalkable[template.terrainMap[i, j]] ? FillingTerrain.Walkable : FillingTerrain.Unwalkable);
                }
            }
        }

        public void SetSquareUnwalkable(Point p)
        {
            if (p.x < 0 || p.y < 0 || p.x >= thisMap.Width || p.y >= thisMap.Height)
                throw new ApplicationException("Point off template.");

            thisMap.setCell(p.x, p.y, FillingTerrain.Unwalkable);
        }

        public bool SetSquareUnWalkableIfMaintainsConnectivity(Point p)
        {
            if (p.x < 0 || p.y < 0 || p.x >= thisMap.Width || p.y >= thisMap.Height)
                throw new ApplicationException("Point off template.");

            //It's not safe to set the squares at the edge of the template as unwalkable since they may link to other rooms
            //(and the fill algorithm won't work on them)
            if (p.x == 0 || p.y == 0 || p.x == thisMap.Width - 1 || p.y == thisMap.Height - 1)
                return false;

            //Can't fill a cell that we have specified as unfillable
            if (thisMap.getCell(p) == FillingTerrain.MustBeWalkable)
                return false;

            var oldTerrain = thisMap.getCell(p);
            thisMap.setCell(p, FillingTerrain.Unwalkable);

            var isConnected = IsConnected();

            if (!isConnected)
                thisMap.setCell(p, oldTerrain);

            return isConnected;
        }

        /// <summary>
        /// For squares that must be accessible (e.g. items or activateable features)
        /// </summary>
        /// <param name="p"></param>
        public void SetSquareAsUnfillableMustBeConnected(Point p)
        {
            if (p.x < 0 || p.y < 0 || p.x >= thisMap.Width || p.y >= thisMap.Height)
                throw new ApplicationException("Point off template.");

            thisMap.setCell(p.x, p.y, FillingTerrain.MustBeWalkable);
        }

        public bool Connected
        {
            get
            {
                return IsConnected();
            }
        }

        public bool IsConnected()
        {
            HashSet<Point> allPathablePoints = new HashSet<Point>();
            foundPathablePoints = new HashSet<Point>();

            for (int i = 0; i < thisMap.Width; i++)
            {
                for (int j = 0; j < thisMap.Height; j++)
                {
                    if(CountsAsWalkable(thisMap.getCell(i, j)))
                        allPathablePoints.Add(new Point(i, j));
                }
            }

            if(allPathablePoints.Count() == 0)
                return true;

            Stack<Point> workNodes = new Stack<Point>();

            workNodes.Push(allPathablePoints.First());

            while (workNodes.Count() > 0)
            {
                var thisNode = workNodes.Pop();
                if (CountsAsWalkable(thisMap.getCell(thisNode.x, thisNode.y)))
                {
                    foundPathablePoints.Add(thisNode);
                    var neighbours = Get4WayNeighbours(thisNode);
                    foreach (var p in neighbours)
                        workNodes.Push(p);
                }
            }

            if (allPathablePoints.Intersect(foundPathablePoints).Count() == allPathablePoints.Count())
                return true;

            return false;
        }

        private List<Point> Get8WayNeighbours(Point origin)
        {
            var toRet = new List<Point>();

            toRet.AddRange(ReturnNeighbourIfValid(origin + new Point(-1, -1)));
            toRet.AddRange(ReturnNeighbourIfValid(origin + new Point(-1, 0)));
            toRet.AddRange(ReturnNeighbourIfValid(origin + new Point(-1, 1)));
            toRet.AddRange(ReturnNeighbourIfValid(origin + new Point(0, -1)));
            toRet.AddRange(ReturnNeighbourIfValid(origin + new Point(0, 1)));
            toRet.AddRange(ReturnNeighbourIfValid(origin + new Point(1, -1)));
            toRet.AddRange(ReturnNeighbourIfValid(origin + new Point(1, 0)));
            toRet.AddRange(ReturnNeighbourIfValid(origin + new Point(1, 1)));

            return toRet;
        }

        private List<Point> Get4WayNeighbours(Point origin)
        {
            var toRet = new List<Point>();

            toRet.AddRange(ReturnNeighbourIfValid(origin + new Point(-1, 0)));
            toRet.AddRange(ReturnNeighbourIfValid(origin + new Point(0, -1)));
            toRet.AddRange(ReturnNeighbourIfValid(origin + new Point(0, 1)));
            toRet.AddRange(ReturnNeighbourIfValid(origin + new Point(1, 0)));

            return toRet;
        }

        private IEnumerable<Point> ReturnNeighbourIfValid(Point p)
        {
            if (p.x >= 0 && p.y >= 0 && p.x < thisMap.Width && p.y < thisMap.Height &&
                !foundPathablePoints.Contains(p))
                return new List<Point> { p };
            return new List<Point>();
        }

        private bool CountsAsWalkable(FillingTerrain terrain)
        {
            return terrain == FillingTerrain.Walkable || terrain == FillingTerrain.MustBeWalkable || terrain == FillingTerrain.ClosedDoor;
        }

    }
}
