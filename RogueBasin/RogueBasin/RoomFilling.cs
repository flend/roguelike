using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueBasin
{
    public class RoomFilling
    {
        RoomTemplate template;
        PathingMap thisMap;
        HashSet<Point> foundPathablePoints;
        bool result;

        public RoomFilling(RoomTemplate template)
        {
            this.template = template;

            BuildPathableMap();
        }

        private void BuildPathableMap()
        {
            thisMap = new PathingMap(template.Width, template.Height);

            for (int i = 0; i < template.Width; i++)
            {
                for (int j = 0; j < template.Height; j++)
                {
                    thisMap.setCell(i, j,
                        RoomTemplateTerrainWalkable.terrainWalkable[template.terrainMap[i, j]] ? PathingTerrain.Walkable : PathingTerrain.Unwalkable);
                }
            }
        }

        public void SetSquareUnwalkable(Point p)
        {
            if (p.x < 0 || p.y < 0 || p.x >= thisMap.Width || p.y >= thisMap.Height)
                throw new ApplicationException("Point off template.");

            thisMap.setCell(p.x, p.y, PathingTerrain.Unwalkable);
        }

        public bool SetSquareUnWalkableIfMaintainsConnectivity(Point p)
        {
            if (p.x < 0 || p.y < 0 || p.x >= thisMap.Width || p.y >= thisMap.Height)
                throw new ApplicationException("Point off template.");

            //It's not safe to set the squares at the edge of the template as unwalkable since they may link to other rooms
            //(and the fill algorithm won't work on them)
            if (p.x == 0 || p.y == 0 || p.x == thisMap.Width - 1 || p.y == thisMap.Height - 1)
                return false;

            var oldTerrain = thisMap.getCell(p);
            thisMap.setCell(p, PathingTerrain.Unwalkable);

            var isConnected = IsConnected();

            if (!isConnected)
                thisMap.setCell(p, oldTerrain);

            return isConnected;
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

        private bool CountsAsWalkable(PathingTerrain terrain) {
            return terrain == PathingTerrain.Walkable || terrain == PathingTerrain.ClosedDoor;
        }

    }
}
