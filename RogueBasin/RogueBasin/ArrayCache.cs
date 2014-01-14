﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    
    public class ArrayCache<T>
    {
        private T[,] arrayCache;
        
        /// <summary>
        /// TL extent of the cache
        /// </summary>
        Point cacheTL;

        /// <summary>
        /// TL extent of added areas
        /// </summary>
        Point realTL;

        /// <summary>
        /// BR extent of added areas
        /// </summary>
        Point realBR;

        public ArrayCache(Point cacheTL, int sizeX, int sizeY)
        {
            arrayCache = new T[sizeX, sizeY];
            this.cacheTL = cacheTL;
        }

        public int CacheWidth
        {
            get
            {
                return arrayCache.GetLength(0);
            }
        }

        public int CacheHeight
        {
            get
            {
                return arrayCache.GetLength(1);
            }
        }

        public int Width
        {
            get
            {
                return realBR.x - realTL.x + 1;
            }
        }

        public int Height {
            get {
                return realBR.y - realTL.y + 1;
            }
        }

        public Point TL {
            get {
                return realTL;
            }
        }

        public Point BR {
            get {
                return realBR;
            }
        }

        //Merge in areaToAdd at areaTL, using the mergeArea function
        public void MergeArea(Point areaTL, T[,] areaToAdd, Func<T, T, T> mergeArea) {

            //Check extent

            int cacheRight = cacheTL.x + CacheWidth - 1;
            int cacheBot = cacheTL.y + CacheHeight - 1;

            int left = Math.Min(areaTL.x, cacheTL.x);
            int top = Math.Min(areaTL.y, cacheTL.y);
            int bottom = Math.Max(cacheBot, areaTL.y + areaToAdd.GetLength(1));
            int right = Math.Max(cacheRight, areaTL.x + areaToAdd.GetLength(0));

            if (left < cacheTL.x || right > cacheRight
                || bottom > cacheBot || top < cacheTL.y)
            {
                //Area is too big, we need to resize
                T[,] newCache = new T[right - left + 1, bottom - top + 1];
                Point newTL = new Point(left, top);

                int offsetX = cacheTL.x - left;
                int offsetY = cacheTL.y - top;

                //Copy from the old array into the new array
                for (int i = 0; i < arrayCache.GetLength(0); i++)
                {
                    for (int j = 0; j < arrayCache.GetLength(1); j++)
                    {
                        newCache[offsetX + i, offsetY + j] = arrayCache[i, j];
                    }
                }

                //Adopt the new, larger cache area
                arrayCache = newCache;
                cacheTL = newTL;
            }

            //Merge new area into the old area, using the passed in function
            for (int i = 0; i < areaToAdd.GetLength(0); i++)
            {
                for (int j = 0; j < areaToAdd.GetLength(1); j++)
                {
                    arrayCache[areaTL.x - cacheTL.x + i, areaTL.y - cacheTL.y + j] = 
                        mergeArea(arrayCache[areaTL.x - cacheTL.x + i, areaTL.y - cacheTL.y + j], areaToAdd[i, j]);
                }
            }

            if (realTL == null)
                realTL = areaTL;
            if (realBR == null)
                realBR = new Point(areaTL.x + areaToAdd.GetLength(0) - 1, areaTL.y + areaToAdd.GetLength(1) - 1);

            //Update maximum extents (inclusive coordinates)
            int rleft = Math.Min(areaTL.x, realTL.x);
            int rtop = Math.Min(areaTL.y, realTL.y);
            int rbottom = Math.Max(realBR.y, areaTL.y + areaToAdd.GetLength(1) - 1);
            int rright = Math.Max(realBR.x, areaTL.x + areaToAdd.GetLength(0) - 1);

            realTL = new Point(rleft, rtop);
            realBR = new Point(rright, rbottom);
        }

        /// <summary>
        /// Return the real area occupied by merged input areas (not the whole cached areas)
        /// </summary>
        /// <returns></returns>
        public T[,] GetMergedArea()
        {
            int left = realTL.x - cacheTL.x;
            int top = realTL.y - cacheTL.y;
            int right = realBR.x - cacheTL.x;
            int bottom = realBR.y - cacheTL.y;

            var mergedArea = new T[realBR.x - realTL.x + 1, realBR.y - realTL.y + 1];

            for (int i = left; i <= right; i++)
            {
                for (int j = top; j <= bottom; j++)
                {
                    mergedArea[i - left, j - top] = arrayCache[i, j];
                }
            }

            return mergedArea;
        }

    }

}
