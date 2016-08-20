using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDRogueTest
{
    [TestClass]
    public class DirectionUtilTest
    {

        [TestMethod]
        public void AngleFromOriginToTargetWorksForNWQuadrant()
        {
            Assert.AreEqual(3 * Math.PI / 4, DirectionUtil.DiagonalCardinalAngleFromRelativePosition(-1, 1), 0.001);
        }

        [TestMethod]
        public void DiagonalCardinalAngleFromRelativePositionWorksForNE1Square() {

            Assert.AreEqual(-Math.PI / 4, DirectionUtil.DiagonalCardinalAngleFromRelativePosition(1, -1), 0.001);
        }

        [TestMethod]
        public void DiagonalCardinalAngleFromRelativePositionWorksForSW1Square()
        {

            Assert.AreEqual(3 * Math.PI / 4, DirectionUtil.DiagonalCardinalAngleFromRelativePosition(-1, 1), 0.001);
        }


        [TestMethod]
        public void DiagonalCardinalAngleFromRelativePositionWorksForNW1Square()
        {

            Assert.AreEqual(-3 * Math.PI / 4, DirectionUtil.DiagonalCardinalAngleFromRelativePosition(-1, -1), 0.001);
        }

        [TestMethod]
        public void DiagonalCardinalAngleFromRelativePositionWorksForSE1Square()
        {

            Assert.AreEqual(Math.PI / 4, DirectionUtil.DiagonalCardinalAngleFromRelativePosition(1, 1), 0.001);
        }

        [TestMethod]
        public void DiagonalCardinalAngleFromRelativePositionWorksForSE4Squares()
        {

            Assert.AreEqual(Math.PI / 4, DirectionUtil.DiagonalCardinalAngleFromRelativePosition(4, 4), 0.001);
        }

    }
}
