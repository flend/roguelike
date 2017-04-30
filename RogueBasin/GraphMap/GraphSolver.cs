using System.Collections.Generic;
using System.Linq;

namespace GraphMap
{
    public class GraphSolver
    {
        MapModel model;
        DoorAndClueManager doorAndClueManager;

        public GraphSolver(MapModel model, DoorAndClueManager doorAndClueManager)
        {
            this.model = model;
            this.doorAndClueManager = doorAndClueManager;
        }

        public bool MapCanBeSolved()
        {
            var cluesFound = new HashSet<Clue>();
            var doorManager = doorAndClueManager;
            //Door & Clue manager works on the reduced map, so we use this here
            //Potentially could be problematic if something is wrong with map reduction
            var totalVerticesReducedMap = model.GraphNoCycles.mapNoCycles.VertexCount;

            int lastTimeAccessibleVertices = 0;
            int noAccessibleVertices = 0;
            do
            {
                lastTimeAccessibleVertices = noAccessibleVertices;

                var accessibleVertices = doorManager.GetAccessibleVerticesWithClues(cluesFound.Select(c => c.OpenLockIndex));

                //Add any clues in these vertices to the clues we have (hashset ensures we don't add twice)
                var cluesAtVertices = accessibleVertices.SelectMany(v => doorManager.ClueMap.ContainsKey(v) ? doorManager.ClueMap[v] : new List<Clue> ());
                foreach (var clue in cluesAtVertices) { cluesFound.Add(clue); }

                noAccessibleVertices = accessibleVertices.Count();
            } while (noAccessibleVertices != lastTimeAccessibleVertices);

            //Couldn't touch all vertices - map is not solvable
            if (noAccessibleVertices < totalVerticesReducedMap)
                return false;

            return true;
        }
    }
}
