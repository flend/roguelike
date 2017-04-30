using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueBasin
{
    public static class ShuffleExtension
    {

        public static T RandomElement<T>(this IEnumerable<T> enumerable)
        {
            return RandomElementUsing<T>(enumerable, Game.Random);
        }

        public static T RandomElementUsing<T>(this IEnumerable<T> enumerable, Random rand)
        {
            int index = rand.Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }

        public static List<T> RandomElements<T>(this IEnumerable<T> enumerable, int numberOfItemsMax)
        {
            return RandomElementsUsing(enumerable, numberOfItemsMax, Game.Random);
        }

        public static List<T> RandomElementsUsing<T>(this IEnumerable<T> enumerable, int numberOfItemsMax, Random rand)
        {
            var allIndices = new HashSet<int>(Enumerable.Range(0, enumerable.Count()));
            var indicesPicked = new List<T>();

            for (int i = 0; i < numberOfItemsMax; i++)
            {
                var index = allIndices.ElementAt(rand.Next(0, allIndices.Count()));
                indicesPicked.Add(enumerable.ElementAt(index));
                allIndices.Remove(index);

                if (allIndices.Count() == 0)
                    break;
            }

            return indicesPicked;
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
                var save = elements[swapIndex];
                elements[swapIndex] = elements[i];
                elements[i] = save;
            }

            return elements.ToList();
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return Shuffle(source, Game.Random);
        }

        public static IEnumerable<T> RepeatToLength<T>(this IEnumerable<T> source, int length)
        {
            int noCopies = length / source.Count() + 1;

            IEnumerable<T> concatted = source;

            do {
                concatted = concatted.Concat(source);
            } while(concatted.Count() < length);

            return concatted.Take(length);
        }
    }
}
