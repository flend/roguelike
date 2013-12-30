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


        /** Get nodes depth nodes away from leaves (dead ends) */
        public IEnumerable<int> GetTerminalBranchNodes(int depth)
        {
            if(depth > 0)
                throw new NotImplementedException();

            var graphMap = mapWithoutCycles.mapNoCycles;
            return graphMap.Vertices.Where(v => graphMap.AdjacentEdges(v).Count() == 1);
        }
    }
}
