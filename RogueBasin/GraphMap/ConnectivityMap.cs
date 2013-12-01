using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphMap
{
    public class ConnectivityMap
    {
        /// <summary>
        /// Input map, may contain cycles
        /// </summary>
        private UndirectedGraph<int, TaggedEdge<int, string>> baseGraph;

        public ConnectivityMap()
        {
            baseGraph = new UndirectedGraph<int, TaggedEdge<int, string>>();
        }

        public ConnectivityMap(UndirectedGraph<int, TaggedEdge<int, string>> g)
        {
            baseGraph = g;
        }

        /// <summary>
        /// Add a room connection to the map. New nodes will be created for previously unseen rooms
        /// </summary>
        /// <param name="startRoom"></param>
        /// <param name="endRoom"></param>
        public void AddRoomConnection(int startRoom, int endRoom)
        {
            baseGraph.AddVerticesAndEdge(new TaggedEdge<int, string>(startRoom, endRoom, ""));
        }

        public UndirectedGraph<int, TaggedEdge<int, string>> RoomConnectionGraph
        {
            get
            {
                return baseGraph;
            }
        }
    }
}
