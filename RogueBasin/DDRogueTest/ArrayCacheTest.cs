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
            var arrayCache = new ArrayCache<int>(new Point(0, 0), 10, 10);

            var areaToAdd = MakeTestArray(10, 10, 1);

            arrayCache.MergeArea(new Point(0, 0), areaToAdd, Math.Max);

            CollectionAssert.AreEqual(areaToAdd, arrayCache.GetMergedArea());
            Assert.AreEqual(arrayCache.TL, new Point(0, 0));
            Assert.AreEqual(arrayCache.BR, new Point(9, 9));
        }

        [TestMethod]
        public void ArrayCacheStoresAreasWithinDefaultSizeNotOriginAligned()
        {
            var arrayCache = new ArrayCache<int>(new Point(0, 0), 10, 10);

            var areaToAdd = MakeTestArray(10, 10, 1);

            arrayCache.MergeArea(new Point(-10, -10), areaToAdd, Math.Max);

            CollectionAssert.AreEqual(areaToAdd, arrayCache.GetMergedArea());
            Assert.AreEqual(arrayCache.TL, new Point(-10, -10));
            Assert.AreEqual(arrayCache.BR, new Point(-1, -1));
        }

        [TestMethod]
        public void ArrayCacheStoresAreasLargerThanDefaultSize()
        {
            var arrayCache = new ArrayCache<int>(new Point(0, 0), 10, 10);

            var areaToAdd = MakeTestArray(20, 20, 1);

            arrayCache.MergeArea(new Point(-10, -10), areaToAdd, Math.Max);

            CollectionAssert.AreEqual(areaToAdd, arrayCache.GetMergedArea());
            Assert.AreEqual(arrayCache.TL, new Point(-10, -10));
            Assert.AreEqual(arrayCache.BR, new Point(9, 9));
        }

        [TestMethod]
        public void ArrayCacheMergesTwoInputAreasUsingFunction()
        {
            var arrayCache = new ArrayCache<int>(new Point(0, 0), 10, 10);

            arrayCache.MergeArea(new Point(3, 3), MakeTestArray(10, 10, 1), Math.Max);
            arrayCache.MergeArea(new Point(4, 4), MakeTestArray(10, 10, 2), Math.Max);

            var mergedArea = arrayCache.GetMergedArea();

            Assert.AreEqual(mergedArea[4 - arrayCache.TL.x, 4 - arrayCache.TL.x], 2);
            Assert.AreEqual(mergedArea[3 - arrayCache.TL.x, 3 - arrayCache.TL.x], 1);
        }

        [TestMethod]
        public void ArrayCacheMergesTwoInputAreasUsingDifferentFunction()
        {
            var arrayCache = new ArrayCache<int>(new Point(0, 0), 10, 10);

            arrayCache.MergeArea(new Point(3, 3), MakeTestArray(10, 10, 1), Math.Max);
            arrayCache.MergeArea(new Point(4, 4), MakeTestArray(10, 10, 2), Math.Min);

            var mergedArea = arrayCache.GetMergedArea();

            Assert.AreEqual(mergedArea[4 - arrayCache.TL.x, 4 - arrayCache.TL.x], 1);
            Assert.AreEqual(mergedArea[3 - arrayCache.TL.x, 3 - arrayCache.TL.x], 1);
            Assert.AreEqual(mergedArea[13 - arrayCache.TL.x, 13 - arrayCache.TL.x], 0);
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
    }
}
