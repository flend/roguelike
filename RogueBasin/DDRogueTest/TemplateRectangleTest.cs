using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;

namespace DDRogueTest
{
    [TestClass]
    public class TemplateRectangleTest
    {
        [TestMethod]
        public void TestRectangleDimensionsFromWidthHeight()
        {
            TemplateRectangle rect = new TemplateRectangle(10, 10, 20, 30);

            Assert.AreEqual(rect.Right, 29);
            Assert.AreEqual(rect.Bottom, 39);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Rectangle constructed with inappropriate arguments.")]
        public void TestCannotCreateZeroWidthRectangles()
        {
            TemplateRectangle rectZeroWidth = new TemplateRectangle(10, 10, 0, 10);
    
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Rectangle constructed with inappropriate arguments.")]
        public void TestCannotCreateZeroHeightRectangles()
        {
            TemplateRectangle rectZeroWidth = new TemplateRectangle(10, 10, 10, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Rectangle constructed with inappropriate arguments.")]
        public void TestCannotCreateNegativeDimensionRectangles()
        {
            TemplateRectangle rectZeroWidth = new TemplateRectangle(10, 10, -10, -10);
        }
    }
}
