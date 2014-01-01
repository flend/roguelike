using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphMap
{
    /** Carries out useful heuristics on reduced maps */
    public class MapHeuristics
    {
        public readonly MapCycleReducer mapWithoutCycles;
        public readonly int startVertex;

        public MapHeuristics(MapCycleReducer mapWithoutCycles, int startVertex)
        {
            this.mapWithoutCycles = mapWithoutCycles;
            this.startVertex = startVertex;
        }

        public IEnumerable<int> GetNodesOnStartSideOfConnection(Connection dividingConnection) {
            
            //Check the edge is in the reduced map (will throw an exception if can't find)
            var foundEdge = mapWithoutCycles.GetEdgeBetweenRoomsNoCycles(dividingConnection.Source, dividingConnection.Target);

            //Remove all areas behind the locked door
            MapSplitter allowedMap = new MapSplitter(mapWithoutCycles.mapNoCycles.Edges, foundEdge, startVertex);

            //Find the component of this broken graph that is connected to the start vertex - 
            //This component contains the vertices accessible with these clues
            return allowedMap.MapComponent(allowedMap.RoomComponentIndex(startVertex));
        }


        /** Get dictionary of terminal nodes (those with  */
        public Dictionary<int, List<int>> GetTerminalBranchNodes()
        {
            var graphMap = mapWithoutCycles.mapNoCycles;
            var originNodes = graphMap.Vertices.Where(v => graphMap.AdjacentEdges(v).Count() == 1);

            Dictionary<int, List<int>> degreeOfTerminalNodes = new Dictionary<int, List<int>>();

            foreach(var baseNode in originNodes) {

                var nextNode = baseNode;
                var adjacentEdges = graphMap.AdjacentEdges(baseNode);
                int degree = -1;

                do {
                    degree++;
                    if (!degreeOfTerminalNodes.ContainsKey(degree))
                        degreeOfTerminalNodes[degree] = new List<int>();

                    degreeOfTerminalNodes[degree].Add(nextNode);

                    nextNode = adjacentEdges.First().Target == nextNode ? adjacentEdges.First().Source : adjacentEdges.First().Target;
                    adjacentEdges = graphMap.AdjacentEdges(nextNode);

                    //Terminate when we hit a node that branches in 2 ways (3 connections)
                } while(adjacentEdges.Count() < 3);
            }

            return degreeOfTerminalNodes;
        }
    }
}
