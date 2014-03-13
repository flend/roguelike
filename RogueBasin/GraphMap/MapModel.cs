using QuickGraph;
using QuickGraph.Algorithms.Search;
using QuickGraph.Graphviz;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace GraphMap
{
    /// <summary>
    /// This should be used by clients as a way of defining an edge, instead of natively using the Edge<> class
    /// </summary>
    public sealed class Connection : Tuple<int, int>
    {
        public int Source { get { return this.Item1; } }
        public int Target { get { return this.Item2; } }

        public Connection(int origin, int target) : base(origin, target)
        {
        }

        public bool IncludesVertex(int v)
        {
            if (Source == v || Target == v)
                return true;

            return false;
        }

        /// <summary>
        /// Return the lowest index first
        /// </summary>
        public Connection Ordered
        {
            get
            {
                if (Source < Target)
                    return new Connection(Source, Target);
                else
                    return new Connection(Target, Source);
            }
        }

        public static bool operator ==(Connection a, Connection b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return (a.Source == b.Source && a.Target == b.Target) ||
                   (a.Target == b.Source && a.Source == b.Target);
        }

        public static bool operator !=(Connection a, Connection b)
        {
            return !(a == b);
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Connection p = obj as Connection;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return ((Source == p.Source) && (Target == p.Target)) ||
                ((Target == p.Source) && (Source == p.Target));
        }

        public bool Equals(Connection p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return ((Source == p.Source) && (Target == p.Target)) ||
                ((Target == p.Source) && (Source == p.Target));
        }

        public override int GetHashCode()
        {
            return Source ^ Target;
        }
    }

    /** A locked item that is unlocked by clues and produces clues when unlocked */
    public class Objective
    {
        public int LockIndex
        {
            get
            {
                return index;
            }
        }

        public List<int> OpenLockIndex { get; private set; }

        public int Vertex { get; private set; }

        /// <summary>
        /// Index into containing map
        /// </summary>
        private int index;

        /// <summary>
        /// Number of clues that are required to open the door
        /// </summary>
        public int NumCluesRequired { get; private set; }

        /** lockIndex = index of the objective itself
         *  openLockIndex = index of the objective or door that the clue this objective provides opens
         */
        public Objective(int objectiveVertex, string id, int lockIndex, List<int> openLockIndex, int numberOfCluesRequired, List<int> possibleRooms)
        {
            Id = id;
            this.index = lockIndex;
            OpenLockIndex = openLockIndex;
            Vertex = objectiveVertex;
            NumCluesRequired = numberOfCluesRequired;
            PossibleClueRoomsInFullMap = possibleRooms;
        }

        public bool CanBeOpenedWithClues(IEnumerable<Clue> clues)
        {
            var applicableClues = clues.Where(c => c.OpenLockIndex == LockIndex);
            if(applicableClues.Count() >= NumCluesRequired)
                return true;
            return false;
        }

        public List<int> PossibleClueRoomsInFullMap
        {
            get;
            private set;
        }

        public string Id
        {
            get;
            private set;
        }
    }

    /** A clue to open a locked door */
    public class Clue
    {
        /// <summary>
        /// Which door this clue locks
        /// </summary>
        private int lockIndex;

        /// <summary>
        /// Reference to door that we lock (for display only)
        /// </summary>
        public Door LockedDoor { get; private set; }

        /// <summary>
        /// Reference to objective that we lock (for display only)
        /// </summary>
        public Objective LockedObjective { get; private set; }

        public List<int> PossibleClueRoomsInFullMap
        {
            get;
            private set;
        }

        /// <summary>
        /// Construct a clue for an associated door
        /// </summary>
        /// <param name="doorIndex"></param>
        public Clue(Door matchingDoor, List<int> possibleRooms)
        {
            this.lockIndex = matchingDoor.LockIndex;
            this.LockedDoor = matchingDoor;
            this.PossibleClueRoomsInFullMap = possibleRooms;
        }

        /// <summary>
        /// Construct a clue for an associated objective
        /// </summary>
        /// <param name="doorIndex"></param>
        public Clue(Objective matchingObjective, List<int> possibleRooms)
        {
            this.lockIndex = matchingObjective.LockIndex;
            this.LockedObjective = matchingObjective;
            this.PossibleClueRoomsInFullMap = possibleRooms;
        }

        public int OpenLockIndex
        {
            get
            {
                return lockIndex;
            }
        }
    }

    /// <summary>
    /// Data carrier class to specify the requirements for a door
    /// </summary>
    public class DoorRequirements
    {
        public Connection Location { get; private set; }
        public string Id { get; private set; }
        public int NumberOfCluesRequired { get; private set; }

        /// <summary>
        /// Defaults to single clue required
        /// </summary>
        /// <param name="location"></param>
        /// <param name="id"></param>
        public DoorRequirements(Connection location, string id)
        {
            this.Location = location;
            this.Id = id;
            this.NumberOfCluesRequired = 1;
        }

        public DoorRequirements(Connection location, string id, int numberOfCluesRequired)
        {
            this.Location = location;
            this.Id = id;
            this.NumberOfCluesRequired = numberOfCluesRequired;
        }
    }

    public class ObjectiveRequirements
    {
        public int Vertex { get; private set; }
        public string Id { get; private set; }
        public int NumberOfCluesRequired { get; set; }
        public List<string> OpenLockId { get; private set; }

        /// <summary>
        /// Defaults to single clue required
        /// </summary>
        /// <param name="location"></param>
        /// <param name="id"></param>
        public ObjectiveRequirements(int vertex, string id, int numberOfCluesRequired, List<string> openLockId)
        {
            this.Vertex = vertex;
            this.Id = id;
            this.NumberOfCluesRequired = numberOfCluesRequired;
            this.OpenLockId = openLockId;
        }

        public ObjectiveRequirements(int vertex, string id, int numberOfCluesRequired)
        {
            this.Vertex = vertex;
            this.Id = id;
            this.NumberOfCluesRequired = numberOfCluesRequired;
            this.OpenLockId = new List<string>();
        }
    }

    /** A locked door requiring one or more clues to open */
    public class Door
    {
        /// <summary>
        /// Which edge we lock on the acyclic graph
        /// </summary>
        private TaggedEdge<int, string> doorEdge;

        /// <summary>
        /// Door index into containing map
        /// </summary>
        private int index;

        /// <summary>
        /// Number of clues that are required to open the door
        /// </summary>
        public int NumCluesRequired { get; private set; }

        public Door(TaggedEdge<int, string> doorEdge, Connection connectionReducedMap, Connection connectionFullMap, string id, int index, int numberOfCluesRequired)
        {
            Id = id;
            this.doorEdge = doorEdge;
            this.index = index;
            NumCluesRequired = numberOfCluesRequired;
            DoorConnectionFullMap = connectionFullMap;
            DoorConnectionReducedMap = connectionReducedMap;
        }

        public bool CanDoorBeUnlockedWithClues(IEnumerable<Clue> clues)
        {
            var cluesForThisDoor = clues.Where(c => c.OpenLockIndex == this.LockIndex).Count();
            return cluesForThisDoor >= this.NumCluesRequired;
        }

        /// <summary>
        /// Edge in reduced no-cycle map
        /// </summary>
        public TaggedEdge<int, string> DoorEdge
        {
            get
            {
                return doorEdge;
            }
        }

        /// <summary>
        /// Connection in full, non-reduced map
        /// </summary>
        public Connection DoorConnectionFullMap
        {
            get;
            private set;
        }

        public Connection DoorConnectionReducedMap
        {
            get;
            private set;
        }

        public int LockIndex
        {
            get
            {
                return index;
            }
        }

        public string Id
        {
            get;
            private set;
        }
    }

    /** Sets up a model of the input map and has state-changing methods */
    public class MapModel
    {
        /// <summary>
        /// Input map, may contain cycles
        /// </summary>
        private UndirectedGraph<int, TaggedEdge<int, string>> baseGraph;

        private ConnectivityMap inputMap;

        /// <summary>
        /// Where the player starts. This node determines the unlocked side of a locked door
        /// </summary>
        private int startVertex;

        MapCycleReducer graphNoCycles;

        DoorAndClueManager doorAndClueManager;

        Random random;

        public MapCycleReducer GraphNoCycles { get { return graphNoCycles; } }

        public UndirectedGraph<int, TaggedEdge<int, string>> BaseGraph { get { return baseGraph; } }

        public DoorAndClueManager DoorAndClueManager { get { return doorAndClueManager; } }

        public ConnectivityMap FullMap { get { return inputMap;  } }

        /// <summary>
        /// Constructed with the inputMap and the room id of the PC start location
        /// </summary>
        /// <param name="inputMap"></param>
        /// <param name="startVertex"></param>
        public MapModel(ConnectivityMap inputMap, int startVertex)
        {
            this.inputMap = inputMap;

            baseGraph = new UndirectedGraph<int, TaggedEdge<int, string>>();
            this.startVertex = startVertex;

            //Clone the input graph (edges only)
            baseGraph.AddVerticesAndEdgeRange(inputMap.RoomConnectionGraph.Edges);

            //Build cycle-free map
            graphNoCycles = new MapCycleReducer(baseGraph.Edges);

            //Build Door and Clue Manager
            //Ensure we pass on the mapped (to no cycles) version of the start vertex
            doorAndClueManager = new DoorAndClueManager(graphNoCycles, graphNoCycles.roomMappingFullToNoCycleMap[startVertex]);

            //Build a random generator (don't keep instantiating them, because they all give the same number if during the same tick!
            random = new Random();
        }
     
        /// <summary>
        /// Return a random edge in the reduced graph
        /// </summary>
        /// <returns></returns>
        public TaggedEdge<int, string> GetRandomUnlockedEdgeInReducedGraph()
        {
            var gReduced = graphNoCycles.mapNoCycles;

            int edgeToGet;

            //Check if all edges are locked, if so just return a random locked edge
            if (gReduced.EdgeCount <= doorAndClueManager.DoorMap.Count)
                return gReduced.Edges.ElementAt(random.Next(gReduced.EdgeCount));

            //If there are unlocked edges, return one of these
            do
            {
                edgeToGet = random.Next(gReduced.EdgeCount);
            } while (doorAndClueManager.GetDoorsForEdge(gReduced.Edges.ElementAt(edgeToGet)).Count() > 1);

            return gReduced.Edges.ElementAt(edgeToGet);
        }

        public IEnumerable<Connection> GetPathBetweenVerticesInReducedMap(int startVertex, int endVertex)
        {
            var tryGetPath = graphNoCycles.mapNoCycles.ShortestPathsDijkstra(x => 1, startVertex);

            IEnumerable<TaggedEdge<int, string>> path;
            if (tryGetPath(endVertex, out path))
            {
                return path.Select(e => new Connection(e.Source, e.Target));
            }
            else
            {
                return new List<Connection>();
            }
        }

        public IEnumerable<Connection> GetPathBetweenVerticesInFullMap(int startVertex, int endVertex)
        {
            var tryGetPath = baseGraph.ShortestPathsDijkstra(x => 1, startVertex);

            IEnumerable<TaggedEdge<int, string>> path;
            if (tryGetPath(endVertex, out path))
            {
                return path.Select(e => new Connection(e.Source, e.Target));
            }
            else
            {
                return new List<Connection>();
            }
        }

        public Dictionary<int, int> GetDistanceOfVerticesFromParticularVertexInReducedMap(int startVertex, IEnumerable<int> verticesToCheck)
        {
            var vertexDistances = verticesToCheck.Select(v => GetPathBetweenVerticesInReducedMap(startVertex, v).Count());
            return verticesToCheck.Zip(vertexDistances, (v, d) => new { Key = v, Value = d })
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public Dictionary<int, int> GetDistanceOfVerticesFromParticularVertexInFullMap(int startVertex, IEnumerable<int> verticesToCheck)
        {
            var vertexDistances = verticesToCheck.Select(v => GetPathBetweenVerticesInFullMap(startVertex, v).Count());
            return verticesToCheck.Zip(vertexDistances, (v, d) => new { Key = v, Value = d })
                .ToDictionary(x => x.Key, x => x.Value);
        }

        /** Lock an edge and place a random clue */

        public void LockEdgeRandomClue(DoorRequirements doorReq)
        {    
            //Check that edge is in reduced map
            if (!graphNoCycles.IsEdgeInRoomsNoCycles(doorReq.Location.Source, doorReq.Location.Target))
                throw new ApplicationException("Edge not in non-cycle map");

            var validRoomsForClue = doorAndClueManager.GetValidRoomsToPlaceClueForDoor(doorReq.Location);

            //Place the clue in a random valid room
            int clueVertex = validRoomsForClue.ElementAt(random.Next(validRoomsForClue.Count()));

            Console.WriteLine(String.Format("LockEdgeRandomClue. Candidate rooms: {0}, placing at: {1}", validRoomsForClue.Count(), clueVertex));

            doorAndClueManager.PlaceDoorAndClue(doorReq, clueVertex);
        }

        /** Lock a random edge and place a random clue */

        public void LockRandomEdgeRandomClue(string doorId)
        {
            //Generate a random edge
            var edgeToLock = graphNoCycles.NoCycleEdges.ElementAt(random.Next(graphNoCycles.NoCycleEdges.Count()));

            //Lock with a random clue
            LockEdgeRandomClue(new DoorRequirements(new Connection(edgeToLock.Source, edgeToLock.Target), doorId));
        }

        /// <summary>
        /// Return the start vertex (in the full map)
        /// </summary>
        public int StartVertex { get { return startVertex; } }


        /// <summary>
        /// Return the start vertex (in the no cycles map)
        /// </summary>
        public int StartVertexNoCycleMap { get { return GraphNoCycles.roomMappingFullToNoCycleMap[startVertex]; } }
    }



    /** Immutable class for finding the MST of a map */
    public class MapMST
    {
        readonly UndirectedGraph<int, TaggedEdge<int, string>> baseGraph;

        /// <summary>
        /// A minimum spanning tree for the input map
        /// </summary>
        public readonly UndirectedGraph<int, TaggedEdge<int, string>> mst = new UndirectedGraph<int, TaggedEdge<int, string>>();
        /// <summary>
        /// edge predecessor to each vertex making up the MST
        /// </summary>
        public readonly Dictionary<int, TaggedEdge<int, string>> vertexPredecessors = new Dictionary<int, TaggedEdge<int, string>>();

        public readonly List<int> verticesInDFSOrder = new List<int>();

        public MapMST(IEnumerable<TaggedEdge<int, string>> edges)
        {
            this.baseGraph = new UndirectedGraph<int, TaggedEdge<int, string>>();
            baseGraph.AddVerticesAndEdgeRange(edges);

            Process();
        }

        void Process()
        {
            //Do a depth first search
            //Catagorize edges as tree edges or 'back/forward/inner' edges (that don't make a MST)

            // create algorithm

            var dfs = new UndirectedDepthFirstSearchAlgorithm<int, TaggedEdge<int, string>>(baseGraph);

            dfs.TreeEdge += treeEdge;

            dfs.DiscoverVertex += discoverVertex;

            //do the search
            dfs.Compute();

            //Build graph representation of MST for further processing

            //We have a dictionary of edges, so just iterate over it and build a new graph
            mst.AddVerticesAndEdgeRange(vertexPredecessors.Values);
        }

        private void discoverVertex(int vertex)
        {
            verticesInDFSOrder.Add(vertex);
        }

        private void treeEdge(object sender, UndirectedEdgeEventArgs<int, TaggedEdge<int, string>> e)
        {
            var vertexTarget = e.Target;

            //Associate vertex with predecessor edge
            vertexPredecessors.Add(vertexTarget, e.Edge);
        }

    }

    /** Immutable one-method class used to reduce cycles from a map */
    public class MapCycleReducer {

        readonly UndirectedGraph<int, TaggedEdge<int, string>> baseGraph;
        public bool CycleDebug { get; set; }
        public readonly UndirectedGraph<int, TaggedEdge<int, string>> mapNoCycles = new UndirectedGraph<int,TaggedEdge<int,string>>();

        //TODO: These should be ReadOnly dictionaries, but that is a .net 4.5 feature

        /// <summary>
        /// Mapping from room vertex in non-reduced map to node in no-cycle reduced map
        /// </summary>
        public readonly Dictionary<int, int> roomMappingFullToNoCycleMap;

        /// <summary>
        /// Mapping from room vertex in no-cycle reduced map to node in full non-reduced map
        /// </summary>
        public readonly Dictionary<int, List<int>> roomMappingNoCycleToFullMap;

        /// <summary>
        /// Mapping from edge connection in no-cycle reduced map to edge connection in full non-reduced map
        /// </summary>
        public readonly Dictionary<Connection, Connection> edgeMappingNoCycleToFullMap;

        public MapMST mapMST;

        private List<List<Connection>> allCycles = new List<List<Connection>>();

        public MapCycleReducer(IEnumerable<TaggedEdge<int, string>> edges)
        {
            this.baseGraph = new UndirectedGraph<int, TaggedEdge<int, string>>();
            baseGraph.AddVerticesAndEdgeRange(edges);

            //Find minimum spanning tree
            mapMST = new MapMST(baseGraph.Edges);
            
            //Find all cycles within tree

            if (CycleDebug)
                Console.WriteLine("--cycle finding--");

            //Find all edges not in MST
            var allEdges = baseGraph.Edges;
            var edgesInMST = mapMST.vertexPredecessors.Values;

            var backEdges = allEdges.Except(edgesInMST);

            if (CycleDebug)
            {
                Console.WriteLine("No of back edges: {0}", backEdges.Count());

                foreach (var edge in backEdges)
                {
                    Console.WriteLine(edge);
                }
            }

            //Calculate [all? - expensive?] shortest paths. Each edge has a distance of 1

            var cycleList = new List<IEnumerable<TaggedEdge<int, string>>>();

            //Find the shortest cycle for each back edge

            foreach (var backEdge in backEdges)
            {

                int startVertex = backEdge.Source;
                int endVertex = backEdge.Target;

                var tryGetPath = mapMST.mst.ShortestPathsDijkstra(x => 1, startVertex);

                IEnumerable<TaggedEdge<int, string>> path;
                if (tryGetPath(endVertex, out path))
                {
                    cycleList.Add(path);
                    AddToAllCycles(path, backEdge);
                }
                else
                {
                    Console.WriteLine(String.Format("no path found for cycle, start: {0}, end: {1}", startVertex, endVertex));
                }
            }

            //Output to console all cycles
            if (CycleDebug)
            {
                foreach (var cycle in cycleList)
                {
                    Console.WriteLine("Cycle: ");

                    foreach (var edge in cycle)
                    {
                        Console.Write(String.Format("{0}\t", edge));
                    }

                    Console.Write("\r\n");
                }
            }

            //Combine any cycles that share a vertex
            //There will be at least 2 ways of getting to each vertex
            //Therefore these vertexes must be collasped

            //make a (probably unconnected) graph of all the vertexes in the cycles
            //(no need to add back-edges - if they are connected by this then they will be connected by 2 vertices)

            var cycleGraph = new UndirectedGraph<int, TaggedEdge<int, string>>();
            var allCycleEdges = cycleList.SelectMany(lst => lst);
            cycleGraph.AddVerticesAndEdgeRange(allCycleEdges);

            //find the connected components
            //this gives us n sets of connected nodes which will be reduced to n single nodes in the final acyclic graph
            var components = new Dictionary<int, int>();
            int componentCount = cycleGraph.ConnectedComponents<int, TaggedEdge<int, string>>(components);

            if (componentCount != 0)
            {
                if (CycleDebug)
                    Console.WriteLine("Graph contains {0} strongly connected components", componentCount);
                foreach (var kv in components)
                {
                    if (CycleDebug)
                        Console.WriteLine("Vertex {0} is connected to subgraph {1}", kv.Key, kv.Value);
                }
            }

            //Replace each connected component with a single vertex, and remember which nodes were rolled-up

            mapNoCycles.AddVerticesAndEdgeRange(baseGraph.Edges);

            //Maintain a map of vertex number mappings after cycle removal
            //Initialise with no-change case

            var roomMappingToNoCyclesWork = new Dictionary<int, int>();

            foreach (var vertex in baseGraph.Vertices)
                roomMappingToNoCyclesWork[vertex] = vertex;

            //Maintain a map of all edges after cycle removal to initial map
            var edgeMappingNoCycleToFullMapWork = new Dictionary<Connection, Connection>();

            //For each cycle
            for (int i = 0; i < componentCount; i++)
            {
                //Get all vertices in this cycle
                //Get all vertices (keys) with value (cycle no)
                var verticesInCycle = components.Where(kv => kv.Value == i).Select(kv => kv.Key);

                //First vertex (to be kept)
                //Other vertices (to be removed)
                var sortedVertices = verticesInCycle.ToList();
                sortedVertices.Sort();

                //First vertex defined as minimum to ease repeatibility
                var firstVertex = sortedVertices.First();
                var verticesInCycleNotFirst = sortedVertices.Skip(1);

                //Get all non-internal edges from vertices in the cycle

                //Get all adjacent edges to vertices to remove in the cycle
                var edgesFromCycle = verticesInCycleNotFirst.SelectMany(v => baseGraph.AdjacentEdges(v));

                //Discard all edges between vertices
                //(we need to maintain edges that are either sourced from the cycle or target it)
                var exteriorEdges = edgesFromCycle.Where(edge => !verticesInCycle.Contains(edge.Source) || !verticesInCycle.Contains(edge.Target));

                //Remove all cycle vertices but first from graph
                foreach (int vertex in verticesInCycleNotFirst)
                {
                    //Update vertex map
                    roomMappingToNoCyclesWork[vertex] = firstVertex;

                    //Remove vertex from graph
                    mapNoCycles.RemoveVertex(vertex);
                }

                //Add all exterior edges onto the remaining cycle vertex
                foreach (var edge in exteriorEdges)
                {
                    //Maintain reverse mapping. Edges which are collasped map back to the original rooms (where the door actually is)
                    //Store in lowest node first ordering
                    edgeMappingNoCycleToFullMapWork.Add(new Connection(roomMappingToNoCyclesWork[edge.Source], roomMappingToNoCyclesWork[edge.Target]).Ordered, new Connection(edge.Source, edge.Target).Ordered);

                    //Rewrite edge
                    //Use mapped vertex indices, since those in this cycle (and other source cycles) will have been reduced
                    mapNoCycles.AddEdge(new TaggedEdge<int, string>(roomMappingToNoCyclesWork[edge.Source], roomMappingToNoCyclesWork[edge.Target], edge.Tag));
                }
            }

            //Fill in any unchanged edges in the edge mapping
            foreach (var edge in mapNoCycles.Edges)
            {
                if(!edgeMappingNoCycleToFullMapWork.ContainsKey(new Connection(edge.Source, edge.Target).Ordered))
                    edgeMappingNoCycleToFullMapWork[new Connection(edge.Source, edge.Target).Ordered] = new Connection(edge.Source, edge.Target).Ordered;
            }
            
            edgeMappingNoCycleToFullMap = edgeMappingNoCycleToFullMapWork.AsReadOnly();

            roomMappingFullToNoCycleMap = roomMappingToNoCyclesWork.AsReadOnly();
            //Reverse the above mapping
            var roomMappingFullToNoCycleGrouped = roomMappingToNoCyclesWork.GroupBy(kv => kv.Value);
            roomMappingNoCycleToFullMap = roomMappingFullToNoCycleGrouped.ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList()).AsReadOnly();
                
            if (CycleDebug)
                Console.WriteLine(String.Format("Cycle reduction - Cycles removed: {2}, Vertices before: {0}, vertices after: {1}", baseGraph.Vertices.Count(), mapNoCycles.Vertices.Count(), componentCount));
        }

        private void AddToAllCycles(IEnumerable<TaggedEdge<int, string>> path, TaggedEdge<int, string> backEdge)
        {
            var connectionList = path.Select(edge => new Connection(edge.Source, edge.Target)).ToList();
            connectionList.Add(new Connection(backEdge.Source, backEdge.Target));

            allCycles.Add(connectionList);
        }

        public bool IsEdgeInRoomsNoCycles(int startRoom, int endRoom)
        {
            try
            {
                var foundEdge = GetEdgeBetweenRoomsNoCycles(startRoom, endRoom);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public TaggedEdge<int, String> GetEdgeBetweenRoomsNoCycles(int startRoom, int endRoom)
        {
            TaggedEdge<int, string> possibleEdge = null;

            try
            {
                mapNoCycles.TryGetEdge(startRoom, endRoom, out possibleEdge);

                if (possibleEdge != null)
                {
                    return possibleEdge;
                }
                else
                {
                    throw new ApplicationException("Edge not in map after cycle reduction");
                }
            }
            catch(Exception) {
                throw new ApplicationException("Edge not in map after cycle reduction");
            }
        }

        public IEnumerable<TaggedEdge<int, String>> NoCycleEdges 
        {
            get
            {
                return mapNoCycles.Edges;
            }

        }

        public List<List<Connection>> AllCycles { get { return allCycles; } }
    }

    /** Immutable one-method class used to split a map into locked / unlocked sections */
    public class MapSplitter
    {
        readonly UndirectedGraph<int, TaggedEdge<int, string>> map;
        readonly IEnumerable<TaggedEdge<int, string>> edgesToSplitOn;
        readonly int originVertex;
        readonly Dictionary<int, int> components = new Dictionary<int,int>();

        public int OriginComponentIndex { get; private set; }
        public int NonOriginComponentIndex
        {
            get
            {
                return OriginComponentIndex == 1 ? 0 : 1;
            }
        }

        public int RoomComponentIndex(int nodeIndex)
        {
            return components[nodeIndex];
        }

        public MapSplitter(IEnumerable<TaggedEdge<int, string>> edges, IEnumerable<TaggedEdge<int, string>> edgesToSplitOn, int originVertex)
        {
            this.map = new UndirectedGraph<int, TaggedEdge<int, string>>();
            map.AddVerticesAndEdgeRange(edges);

            this.edgesToSplitOn = edgesToSplitOn;
            this.originVertex = originVertex;

            Process();
        }

        public MapSplitter(IEnumerable<TaggedEdge<int, string>> edges, TaggedEdge<int, string> edgeToSplitOn, int originVertex)
        {
            this.map = new UndirectedGraph<int, TaggedEdge<int, string>>();
            map.AddVerticesAndEdgeRange(edges);

            this.edgesToSplitOn = new List<TaggedEdge<int, string>>(new TaggedEdge<int, string>[]{ edgeToSplitOn });
            this.originVertex = originVertex;

            Process();
        }

        private void Process()
        {
            //Break tree on all edges and label as unlocked (contains start vertex) and locked tree

            foreach (var edgeToSplitOn in edgesToSplitOn)
            {
                map.RemoveEdge(edgeToSplitOn);
            }

            //We need to get the 2 subtrees created by this divide
            //This could be done by DFS from the source and target of the edge
            //Unfortunately, we don't have a top-level method to do that, so we get the connected components instead (may be slower)

            int componentCount = map.ConnectedComponents<int, TaggedEdge<int, string>>(components);

            //Which tree is the unlocked one?
            OriginComponentIndex = components[originVertex];
        }

        /// <summary>
        /// Returns all vertices in a component
        /// </summary>
        /// <param name="componentIndex"></param>
        /// <returns></returns>
        public IEnumerable<int> MapComponent(int componentIndex)
        {
            return components.Where(kv => kv.Value == componentIndex).Select(kv => kv.Key);
        }

    }

    static class DictionaryExtension
    {
        public static Dictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return new Dictionary<TKey, TValue>(dictionary);
        }
    }
    
    
}
