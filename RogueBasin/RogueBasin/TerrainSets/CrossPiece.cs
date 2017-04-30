using System.Collections.Generic;
using System.Linq;

namespace RogueBasin.TerrainSets
{
    class CrossPiece
    {
        Point origin;
        int width;
        int height;
        double angleRadians;

        public CrossPiece(Point origin, int width, int height, double angleRadians)
        {
            this.width = width;
            this.height = height;
            this.origin = origin;
            this.angleRadians = angleRadians;
        }

        public IEnumerable<Point> Generate() {

            int xLeft = origin.x - width / 2;
            int yTop = origin.y - height / 2;

            List<Point> pointsToRet = new List<Point>();

            var xBar = Enumerable.Range(xLeft, width).Select(x => new Point(x, origin.y));
            var yBar = Enumerable.Range(yTop, height).Select(y => new Point(origin.x, y));

            return xBar.Concat(yBar).Select(p => Utility.RotatePoint(p, origin, angleRadians));
        }


    }
}
