using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;
using System;

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

        [TestMethod]
        public void TestRectangleOverlapTL()
        {
            TemplateRectangle rectBase = new TemplateRectangle(10, 10, 20, 30);
            TemplateRectangle rectTop = new TemplateRectangle(0, 0, 15, 15);

            TemplateRectangle rectOverlap = rectBase.GetOverlapRectangle(rectTop);

            Assert.AreEqual(new TemplateRectangle(10, 10, 5, 5), rectOverlap);
        }

        [TestMethod]
        public void TestRectangleOverlapBR()
        {
            TemplateRectangle rectBase = new TemplateRectangle(10, 10, 10, 10);
            TemplateRectangle rectTop = new TemplateRectangle(15, 15, 10, 10);

            TemplateRectangle rectOverlap = rectBase.GetOverlapRectangle(rectTop);

            Assert.AreEqual(new TemplateRectangle(15, 15, 5, 5), rectOverlap);
        }

        [TestMethod]
        public void TestRectangleCrossed()
        {
            TemplateRectangle rectBase = new TemplateRectangle(-10, -10, 20, 20);
            TemplateRectangle rectTop = new TemplateRectangle(-20, 0, 40, 10);

            TemplateRectangle rectOverlap = rectBase.GetOverlapRectangle(rectTop);

            Assert.AreEqual(new TemplateRectangle(-10, 0, 20, 10), rectOverlap);
        }

        [TestMethod]
        public void TestRectangleEntirelyInOtherRectangle()
        {
            TemplateRectangle rectBase = new TemplateRectangle(-10, -10, 5, 5);
            TemplateRectangle rectTop = new TemplateRectangle(-8, -8, 2, 2);

            TemplateRectangle rectOverlap = rectBase.GetOverlapRectangle(rectTop);

            Assert.AreEqual(new TemplateRectangle(-8, -8, 2, 2), rectOverlap);
        }

    }
}
