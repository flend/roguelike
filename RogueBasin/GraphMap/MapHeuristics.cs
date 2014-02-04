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


        /** Get dictionary of terminal nodes (those that lead to a dead end, n nodes away) */
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

        /** Get dictionary of terminal connections. These are defined as the connections that lead to dead ends
         *  and are n nodes away */
        public Dictionary<int, List<Connection>> GetTerminalBranchConnections()
        {
            var graphMap = mapWithoutCycles.mapNoCycles;
            var originNodes = graphMap.Vertices.Where(v => graphMap.AdjacentEdges(v).Count() == 1);

            Dictionary<Connection, int> degreeOfConnections = new Dictionary<Connection, int>();

            foreach (var baseNode in originNodes)
            {

                int nextNode = baseNode;
                var adjacentEdges = graphMap.AdjacentEdges(baseNode);
                var nextEdge = adjacentEdges.First();
                int degree = -1;
                Connection lastConnection = null;// = new Connection(baseNode, nextEdge.Target == baseNode ? nextEdge.Source : nextEdge.Target);

                do
                {
                    //Choose our next connection, avoid going back on ourselves
                    nextEdge = adjacentEdges.First();

                    var nextEdgeAsConnection = new Connection(nextEdge.Source, nextEdge.Target).Ordered;
                    if (lastConnection != null && lastConnection == nextEdgeAsConnection)
                    {
                        nextEdge = adjacentEdges.ElementAt(1);
                        nextEdgeAsConnection = new Connection(nextEdge.Source, nextEdge.Target).Ordered;
                    }

                    //Set the degree of the next connection
                    //If we have seen the node before, but this is a lower degree, replace the degree
                    degree++;

                    if (!degreeOfConnections.ContainsKey(nextEdgeAsConnection) ||
                        (degreeOfConnections.ContainsKey(nextEdgeAsConnection) && degreeOfConnections[nextEdgeAsConnection] > degree))
                    degreeOfConnections[nextEdgeAsConnection] = degree;

                    //Move on
                    nextNode = nextEdgeAsConnection.Target == nextNode ? nextEdgeAsConnection.Source : nextEdgeAsConnection.Target;

                    adjacentEdges = graphMap.AdjacentEdges(nextNode);
                    lastConnection = nextEdgeAsConnection;

                    //Terminate when we hit a node that branches in 2 ways (3 connections) or another dead end
                } while (adjacentEdges.Count() == 2);
            }

            Dictionary<int, List<Connection>> degreeOfTerminalNodesAsList = new Dictionary<int, List<Connection>>();

            foreach (var connectionDegree in degreeOfConnections)
            {
                if (!degreeOfTerminalNodesAsList.ContainsKey(connectionDegree.Value))
                    degreeOfTerminalNodesAsList[connectionDegree.Value] = new List<Connection>();

                degreeOfTerminalNodesAsList[connectionDegree.Value].Add(connectionDegree.Key);
            }

            return degreeOfTerminalNodesAsList;
        }

    }
}
