using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogueBasin;

namespace DDRogueTest
{
    [TestClass]
    public class ArrayCacheTest
    {
        [TestMethod]
        public void ArrayCacheStoresAreasWithinDefaultSize()
        {
            var arrayCache = new ArrayCache<int>(10, 10);

            var areaToAdd = MakeTestArray(10, 10, 1);

            arrayCache.MergeArea(new Point(0, 0), areaToAdd, Math.Max);

            CollectionAssert.AreEqual(areaToAdd, arrayCache.GetMergedArea());
            Assert.AreEqual(arrayCache.TL, new Point(0, 0));
            Assert.AreEqual(arrayCache.BR, new Point(9, 9));
        }

        [TestMethod]
        public void ArrayCacheCanBeInitialisedToAValue()
        {
            var arrayCache = new ArrayCache<int>(10, 10, 1);

            var areaToAdd = MakeTestArray(10, 10, 0);
            var areaExpected = MakeTestArray(10, 10, 1);

            arrayCache.MergeArea(new Point(0, 0), areaToAdd, Math.Max);

            CollectionAssert.AreEqual(areaExpected, arrayCache.GetMergedArea());
        }

        [TestMethod]
        public void ArrayCacheUsesInitialisedValueWhenCacheExpands()
        {
            var arrayCache = new ArrayCache<int>(10, 10, 1);

            var areaToAdd = MakeTestArray(10, 10, 0);
            var areaToAdd2 = MakeTestArray(10, 10, 0);

            arrayCache.MergeArea(new Point(0, 0), areaToAdd, Math.Max);
            arrayCache.MergeArea(new Point(30, 30), areaToAdd2, Math.Max);

            Assert.AreEqual(arrayCache.GetMergedPoint(new Point(20, 20)), 1);
        }

        [TestMethod]
        public void CheckMergeTestsAgainstDefaultValue()
        {
            var arrayCache = new ArrayCache<int>(10, 10, 1);

            var areaToAdd = MakeTestArray(10, 10, 1);

            Assert.IsFalse(arrayCache.CheckMergeArea(new Point(0, 0), areaToAdd, MergeWithException));
        }

        [TestMethod]
        public void ArrayCacheStoresAreasWithinDefaultSizeNotOriginAligned()
        {
            var arrayCache = new ArrayCache<int>(10, 10);

            var areaToAdd = MakeTestArray(10, 10, 1);

            arrayCache.MergeArea(new Point(-10, -10), areaToAdd, Math.Max);

            CollectionAssert.AreEqual(areaToAdd, arrayCache.GetMergedArea());
            Assert.AreEqual(arrayCache.TL, new Point(-10, -10));
            Assert.AreEqual(arrayCache.BR, new Point(-1, -1));
            Assert.AreEqual(arrayCache.CacheWidth, 10);
            Assert.AreEqual(arrayCache.CacheHeight, 10);
        }

        [TestMethod]
        public void ArrayCacheStoresAreasLargerThanDefaultSize()
        {
            var arrayCache = new ArrayCache<int>(10, 10);

            var areaToAdd = MakeTestArray(20, 20, 1);

            arrayCache.MergeArea(new Point(-10, -10), areaToAdd, Math.Max);

            CollectionAssert.AreEqual(areaToAdd, arrayCache.GetMergedArea());
            Assert.AreEqual(arrayCache.TL, new Point(-10, -10));
            Assert.AreEqual(arrayCache.BR, new Point(9, 9));

            Assert.AreEqual(arrayCache.CacheWidth, 20);
            Assert.AreEqual(arrayCache.CacheHeight, 20);
        }

        [TestMethod]
        public void ArrayCacheStoresAreasLargerThanDefaultSizeAfter2Merges()
        {
            var arrayCache = new ArrayCache<int>(10, 10);

            arrayCache.MergeArea(new Point(3, 3), MakeTestArray(10, 10, 1), Math.Max);
            arrayCache.MergeArea(new Point(4, 4), MakeTestArray(10, 10, 2), Math.Max);

            Assert.AreEqual(arrayCache.TL, new Point(3, 3));
            Assert.AreEqual(arrayCache.BR, new Point(13, 13));

            Assert.AreEqual(arrayCache.CacheWidth, 11);
            Assert.AreEqual(arrayCache.CacheHeight, 11);
        }

        [TestMethod]
        public void ArrayCacheMergesTwoInputAreasUsingFunction()
        {
            var arrayCache = new ArrayCache<int>(10, 10);

            arrayCache.MergeArea(new Point(3, 3), MakeTestArray(10, 10, 1), Math.Max);
            arrayCache.MergeArea(new Point(4, 4), MakeTestArray(10, 10, 2), Math.Max);

            var mergedArea = arrayCache.GetMergedArea();

            Assert.AreEqual(mergedArea[4 - arrayCache.TL.x, 4 - arrayCache.TL.x], 2);
            Assert.AreEqual(mergedArea[3 - arrayCache.TL.x, 3 - arrayCache.TL.x], 1);
        }

        [TestMethod]
        public void IfMergeThrowsExceptionCheckMergedAreaFails()
        {
            var arrayCache = new ArrayCache<int>(10, 10);

            arrayCache.MergeArea(new Point(3, 3), MakeTestArray(10, 10, 1), Math.Max);

            Assert.IsFalse(arrayCache.CheckMergeArea(new Point(12, 12), MakeTestArray(10, 10, 1), MergeWithException));
        }

        [TestMethod]
        public void IfMergeDoesNotThrowsExceptionCheckMergedAreaPasses()
        {
            var arrayCache = new ArrayCache<int>(10, 10);

            arrayCache.MergeArea(new Point(3, 3), MakeTestArray(10, 10, 1), Math.Max);

            Assert.IsTrue(arrayCache.CheckMergeArea(new Point(13, 13), MakeTestArray(10, 10, 1), MergeWithException));
        }

        [TestMethod]
        public void ArrayCacheMergesTwoInputAreasUsingDifferentFunction()
        {
            var arrayCache = new ArrayCache<int>(10, 10);

            arrayCache.MergeArea(new Point(3, 3), MakeTestArray(10, 10, 1), Math.Max);
            arrayCache.MergeArea(new Point(4, 4), MakeTestArray(10, 10, 2), Math.Min);

            var mergedArea = arrayCache.GetMergedArea();

            Assert.AreEqual(mergedArea[4 - arrayCache.TL.x, 4 - arrayCache.TL.x], 1);
            Assert.AreEqual(mergedArea[3 - arrayCache.TL.x, 3 - arrayCache.TL.x], 1);
            Assert.AreEqual(mergedArea[13 - arrayCache.TL.x, 13 - arrayCache.TL.x], 0);
        }

        [TestMethod]
        public void ArrayCacheCanBeAccessedPointByPoint()
        {
            var arrayCache = new ArrayCache<int>(10, 10);

            arrayCache.MergeArea(new Point(3, 3), MakeTestArray(10, 10, 1), Math.Max);
            arrayCache.MergeArea(new Point(4, 4), MakeTestArray(10, 10, 2), Math.Max);

            var mergedArea = arrayCache.GetMergedArea();

            Assert.AreEqual(arrayCache.GetMergedPoint(new Point(3, 3)), 1);
            Assert.AreEqual(arrayCache.GetMergedPoint(new Point(4, 4)), 2);
            Assert.AreEqual(arrayCache.GetMergedPoint(new Point(13, 13)), 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void AccessingPointsOutsideArrayCacheThrowsException()
        {
            var arrayCache = new ArrayCache<int>(10, 10);

            arrayCache.MergeArea(new Point(3, 3), MakeTestArray(10, 10, 1), Math.Max);

            arrayCache.GetMergedPoint(new Point(2, 3));
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void AccessingPointsOutsideArrayCacheThrowsExceptionBR()
        {
            var arrayCache = new ArrayCache<int>(10, 10);

            arrayCache.MergeArea(new Point(3, 3), MakeTestArray(10, 10, 1), Math.Max);

            arrayCache.GetMergedPoint(new Point(13, 13));
        }

        private int[,] MakeTestArray(int x, int y, int val)
        {
            var ret = new int[x, y];

            for (int i = 0; i < x;i++) {
                for (int j = 0; j < y; j++)
                {
                    ret[i, j] = val;
                }

            }

            return ret;
        }

        private int MergeWithException(int x, int y)
        {
            if (x + y == 2)
                throw new ApplicationException("Merge conflict");

            return x + y;
        }
    }
}
