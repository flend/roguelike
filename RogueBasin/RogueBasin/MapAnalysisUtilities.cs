using GraphMap;
using RogueBasin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraumaRL
{
    public static class MapAnalysisUtilities
    {
        public static Connection FindFreeConnectionOnPath(DoorAndClueManager manager, IEnumerable<Connection> path, Connection preferredConnectionCandidate)
        {
            if (manager.GetDoorsForEdge(preferredConnectionCandidate).Count() > 0)
            {
                //Try another edge
                var possibleEdges = path.Shuffle();
                Connection foundEdge = null;
                foreach (var edge in possibleEdges)
                {
                    if (manager.GetDoorsForEdge(preferredConnectionCandidate).Count() == 0)
                    {
                        foundEdge = edge;
                        break;
                    }
                }

                if (foundEdge == null)
                {
                    throw new ApplicationException("No free doors to place lock.");
                }

                return foundEdge;
            }

            return preferredConnectionCandidate;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself)
                // ... except we don't really need to swap it fully, as we can
                // return it immediately, and afterwards it's irrelevant.
                int swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return Shuffle(source, Game.Random).ToList();
        }
    }
}
