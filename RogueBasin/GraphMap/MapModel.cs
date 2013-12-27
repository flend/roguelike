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
    }

    /** A clue to open a locked door */
    public class Clue
    {
        /// <summary>
        /// Which door this clue locks
        /// </summary>
        private int doorIndex;

        /// <summary>
        /// Construct a clue for the associated door
        /// </summary>
        /// <param name="doorIndex"></param>
        public Clue(int doorIndex)
        {
            this.doorIndex = doorIndex;
        }

        public int DoorIndex
        {
            get
            {
                return doorIndex;
            }
        }
    }

    /// <summary>
    /// Data carrier class to specify the requirements for a door
    /// </summary>
    public class DoorRequirements
    {
        public Connection Location { get; set; }
        public string Id { get; set; }
        public int NumberOfCluesRequired { get; set; }

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

        public Door(TaggedEdge<int, string> doorEdge, string id, int index, int numberOfCluesRequired)
        {
            Id = id;
            this.doorEdge = doorEdge;
            this.index = index;
            NumCluesRequired = numberOfCluesRequired;
        }

        public bool CanDoorBeUnlockedWithClues(IEnumerable<Clue> clues)
        {
            var cluesForThisDoor = clues.Where(c => c.DoorIndex == this.DoorIndex).Count();
            return cluesForThisDoor >= this.NumCluesRequired;
        }

        public TaggedEdge<int, string> DoorEdge
        {
            get
            {
                return doorEdge;
            }
        }

        public Connection DoorConnection
        {
            get
            {
                return new Connection(DoorEdge.Source, DoorEdge.Target);
            }
        }

        public int DoorIndex
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

        /// <summary>
        /// Where the player starts. This node determines the unlocked side of a locked door
        /// </summary>
        private int startVertex;

        MapCycleReducer graphNoCycles;

        DoorAndClueManager doorAndClueManager;

        Random random;

        public MapCycleReducer GraphNoCycles { get { return graphNoCycles; } }

        public DoorAndClueManager DoorAndClueManager { get { return doorAndClueManager; } }

        /// <summary>
        /// Constructed with the inputMap and the room id of the PC start location
        /// </summary>
        /// <param name="inputMap"></param>
        /// <param name="startVertex"></param>
        public MapModel(ConnectivityMap inputMap, int startVertex)
        {
            baseGraph = new UndirectedGraph<int, TaggedEdge<int, string>>();
            this.startVertex = startVertex;

            //Clone the input graph (edges only)
            baseGraph.AddVerticesAndEdgeRange(inputMap.RoomConnectionGraph.Edges);

            //Build cycle-free map
            graphNoCycles = new MapCycleReducer(baseGraph.Edges);

            //Build Door and Clue Manager
            //Ensure we pass on the mapped (to no cycles) version of the start vertex
            doorAndClueManager = new DoorAndClueManager(graphNoCycles, graphNoCycles.roomMappingToNoCycles[startVertex]);

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
            } while (doorAndClueManager.GetDoorForEdge(gReduced.Edges.ElementAt(edgeToGet)) != null);

            return gReduced.Edges.ElementAt(edgeToGet);
        }

        /** Lock an edge and place a random clue */

        public void LockEdgeRandomClue(DoorRequirements doorReq)
        {    
            //Check that edge is in reduced map
            if (!graphNoCycles.IsEdgeInRoomsNoCycles(doorReq.Location.Source, doorReq.Location.Target))
                throw new ApplicationException("Edge not in non-cycle map");

            var validRoomsForClue = doorAndClueManager.GetValidRoomsToPlaceClue(doorReq.Location);

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
        public int StartVertexNoCycleMap { get { return GraphNoCycles.roomMappingToNoCycles[startVertex]; } }
    }

    /** Manages doors and clues in a map.
     *  Should be constructed with a map without cycles, which should not change subsequently.
     *  Provides utility methods for finding valid places to put clues and interrogating the
     *  clue/door dependency DAG
     */
    public class DoorAndClueManager {

        readonly MapCycleReducer mapNoCycles;
        readonly int startVertex;

        private int nextDoorIndex = 0;

        /** Door dependency graph
         *
         *  Directed graph.
         *  Children of a node depend upon it (opposite to arrow direction).
         *  All clues dependent upon a door may be found by searching children (out-edges) of vertex
         *  
         *  vertex number <int> = door index
        */
        private AdjacencyGraph<int, Edge<int>> doorDependencyGraph;

        /** Door map
         * 
         *  key = unique identifier for door
         *  Door = information, including Edge. Only 1 door per edge.
         */
        private Dictionary<int, Door> doorMap;

        /** Clue map
          * 
          *  key = vertex where clue is located
          *  List<Clue> = all Clues at this vertex
          */
        private Dictionary<int, List<Clue>> clueMap;

        public DoorAndClueManager(MapCycleReducer reducedMap, int startVertex)
        {
            this.mapNoCycles = reducedMap;
            this.startVertex = startVertex;

            doorDependencyGraph = new AdjacencyGraph<int, Edge<int>>();
            doorMap = new Dictionary<int, Door>();
            clueMap = new Dictionary<int, List<Clue>>();
        }

        /** Clue map
          * 
          *  key = vertex where clue is located
          *  List<Clue> = all Clues at this vertex
          */
        public Dictionary<int, List<Clue>> ClueMap
        {
            get { return clueMap; }
        }

        /** Door map
         * 
         *  key = unique identifier for door
         *  Door = information, including Edge. Only 1 door per edge.
         */
        public Dictionary<int, Door> DoorMap
        {
            get { return doorMap; }
        }

        public AdjacencyGraph<int, Edge<int>> DoorDependencyGraph
        {
            get { return doorDependencyGraph; }
        }

        /** Return the list of valid rooms in the cycle-free map to place a clue for a locked edge */
        public IEnumerable<int> GetValidRoomsToPlaceClue(Connection edgeForDoor)
        {
            //Check the edge is in the reduced map (will throw an exception if can't find)
            var foundEdge = mapNoCycles.GetEdgeBetweenRoomsNoCycles(edgeForDoor.Source, edgeForDoor.Target);

            //Traverse the locked tree and find all clues that will be behind the locked door
            var newlyLockedClues = GetCluesBehindLockedEdge(foundEdge);

            //Find all doors that depend on any door with a locked clue.
            //We can't place a clue for our new door behind any of these
            var allLockedClueDoors = newlyLockedClues.Select(c => c.DoorIndex);
            var allDoorsDependentOnLockedClueDoors = newlyLockedClues.SelectMany(c => GetDependentDoorIndices(c.DoorIndex));
            var allInaccessibleDoors = allLockedClueDoors.Union(allDoorsDependentOnLockedClueDoors).Distinct();

            //Retrieve the door edges in the forbidden list
            var forbiddenDoorEdges = allInaccessibleDoors.Select(doorIndex => doorMap[doorIndex].DoorEdge);
            //Add this edge (can't put clue behind our own door) - NB: hacky way to union with a single item
            var allForbiddenDoorEdges = forbiddenDoorEdges.Union(Enumerable.Repeat(foundEdge, 1)).Distinct();

            //Remove all areas behind any locked door
            MapSplitter allowedMap = new MapSplitter(mapNoCycles.mapNoCycles.Edges, allForbiddenDoorEdges, startVertex);

            //Find the component of this broken graph that is connected to the start vertex - this is the candidate subtree
            var allowedNodes = allowedMap.MapComponent(allowedMap.RoomComponentIndex(startVertex));

            Console.WriteLine("Nodes in candidate graph");
            foreach (var node in allowedNodes)
            {
                Console.Write("{0} ", node);
            }
            Console.WriteLine();

            return allowedNodes;
        }

        /// <summary>
        /// Add a clue for a doorId at requested room / vertex.
        /// No checks and no updates to the dependency graph. Therefore this function is not safe for anything other than test cases.
        /// Adding further doors after using this function can lead to broken maps
        /// </summary>
        /// <param name="room"></param>
        /// <param name="doorId"></param>
        private Clue PlaceClue(int room, string doorId)
        {
            int doorIndex = GetDoorById(doorId).DoorIndex;
            Clue newClue = new Clue(doorIndex);

            List<Clue> clueListAtVertex;
            clueMap.TryGetValue(room, out clueListAtVertex);

            if (clueListAtVertex == null)
            {
                clueMap[room] = new List<Clue>();
            }

            clueMap[room].Add(newClue);

            return newClue;
        }

        /// <summary>
        /// Lock an edge with an id, no checks, no dependency updates.
        /// Returns the door id
        /// </summary>
        /// <param name="edgeForDoorSource"></param>
        /// <param name="edgeForDoorTarget"></param>
        /// <param name="doorId"></param>
        private Door LockDoor(DoorRequirements doorReqs)
        {
            var edgeForDoor = doorReqs.Location;
            var foundEdge = mapNoCycles.GetEdgeBetweenRoomsNoCycles(edgeForDoor.Source, edgeForDoor.Target);

            int thisDoorIndex = nextDoorIndex;
            nextDoorIndex++;
            
            Door newDoor = new Door(foundEdge, doorReqs.Id, thisDoorIndex, doorReqs.NumberOfCluesRequired);
            doorMap.Add(thisDoorIndex, newDoor);
            doorDependencyGraph.AddVertex(thisDoorIndex);

            return newDoor;
        }

        /// <summary>
        /// Place a door and multiple clues for the door.
        /// Ensures that dependency graph is correctly updated (dependencies for new door and changes to dependencies for
        /// existing doors)
        /// Does no checking at all, so impossible situations can be created
        /// </summary>
        public IEnumerable<Clue> PlaceDoorAndCluesNoChecks(DoorRequirements doorReq, List<int> clueVertices)
        {
            //Check the edge is in the reduced map (will throw an exception if can't find)
            var edgeForDoor = doorReq.Location;
            var foundEdge = mapNoCycles.GetEdgeBetweenRoomsNoCycles(edgeForDoor.Source, edgeForDoor.Target);
            
            //Add locked door on this edge
            int thisDoorIndex = LockDoor(doorReq).DoorIndex;
            //Add clues
            var clues = new List<Clue>();
            foreach (var clueVertex in clueVertices)
                clues.Add(PlaceClue(clueVertex, doorReq.Id));

            //BUG: this seems to work under debug mode but fail in release builds
            //var clues = clueVertices.Select(vertex => PlaceClue(vertex, doorReq.Id));

            //Console.WriteLine("Placing door id: {0}, (index: {1}) at {2}->{3}", doorId, thisDoorIndex, edgeForDoor.Source, edgeForDoor.Target);
            //Console.WriteLine("Placing clue index: {0} at {1}", thisDoorIndex, clueVertex);
            
            var tryGetPath = mapNoCycles.mapNoCycles.ShortestPathsDijkstra(x => 1, startVertex);
            IEnumerable<TaggedEdge<int, string>> path;

            //Find path on MST from start location to all clues. Any doors which we traverse become doors we DEPEND on
            foreach (var clueVertex in clueVertices)
            {
                if (clueVertex != startVertex)
                {
                    if (tryGetPath(clueVertex, out path))
                    {
                        foreach (var edge in path)
                        {
                            //Very slow, need to hash this
                            int doorIndex = GetDoorIndexForEdge(edge);
                            if (doorIndex != -1)
                            {
                                doorDependencyGraph.AddVerticesAndEdge(new Edge<int>(doorIndex, thisDoorIndex));
                                Console.WriteLine(String.Format("Door: {1}, now depends on: {0}", doorMap[doorIndex].Id, doorMap[thisDoorIndex].Id));
                            }

                        }
                    }
                    else
                    {
                        Console.WriteLine(String.Format("BUG: no path found for between start and clue, start: {0}, end: {1}", startVertex, clueVertex));
                        throw new ApplicationException(String.Format("BUG: no path found for between start and clue, start: {0}, end: {1}", startVertex, clueVertex));
                    }
                }
            }

            //Find all clues now locked by this door, these clues depend on new door
            var newlyLockedClues = GetCluesBehindLockedEdge(foundEdge);
            var lockedCluesDoorIndices = newlyLockedClues.Select(clue => clue.DoorIndex);

            Console.WriteLine("Doors with clues behind this door");
            foreach (var door in lockedCluesDoorIndices.Distinct().Select(ind => doorMap[ind]))
            {
                Console.WriteLine("Id: {0} door loc: {1}", door.Id, door.DoorEdge.Source);
            }

            //Add dependency on new door to all these clues
            foreach (var door in lockedCluesDoorIndices)
            {
                //Edge goes FROM new door TO old door. Old door now DEPENDS on new door, since old door's clue is locked behind new door. New door must be opened first.
                doorDependencyGraph.AddEdge(new Edge<int>(thisDoorIndex, door));
            }

            return clues;
        }

        /// <summary>
        /// Place a door and multiple clues.
        /// Ensures that dependency graph is correctly updated (dependencies for new door and changes to dependencies for
        /// existing doors)
        /// Does a (slow) check that all clues are being placed in valid areas. If GetValidRoomsToPlaceClue() has been used
        /// outside the function, then use the NoChecks version.
        /// Doesn't check that sufficient clues are being placed for the door, but this should be ensured
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="doorId"></param>
        /// <param name="clueVertex"></param>
        public IEnumerable<Clue> PlaceDoorAndClues(DoorRequirements doorReq, List <int> clueVertices)
        {
            //Check all clues are in the valid placement area
            if (!GetValidRoomsToPlaceClue(doorReq.Location).Except(clueVertices).Any())
                throw new ApplicationException(String.Format("Can't put clues: {0}, behind door at {1}:{2}", GetValidRoomsToPlaceClue(doorReq.Location).Except(clueVertices).ToString(), doorReq.Location.Source, doorReq.Location.Target));

            return PlaceDoorAndCluesNoChecks(doorReq, clueVertices);
        }

        /// <summary>
        /// See PlaceDoorAndClues
        /// </summary>
        /// <param name="doorReq"></param>
        /// <param name="clueVertex"></param>
        /// <returns></returns>
        public Clue PlaceDoorAndClue(DoorRequirements doorReq, int clueVertex)
        {
            return PlaceDoorAndClues(doorReq, new List<int>(new int[] { clueVertex })).First();
        }

        private IEnumerable<Clue> GetCluesBehindLockedEdge(TaggedEdge<int,string> foundEdge)
        {
            MapSplitter splitMap = new MapSplitter(mapNoCycles.mapNoCycles.Edges, foundEdge, startVertex);
            
            //Lists of all clues in vertices which are in the locked tree
            var newlyLockedCluesLists = clueMap.Where(kv => splitMap.RoomComponentIndex(kv.Key) == splitMap.NonOriginComponentIndex).Select(kv => kv.Value);
            //Flattened to one long list
            return newlyLockedCluesLists.SelectMany(clue => clue);
        }


        /// <summary>
        /// Return the ids of all doors that depend on this door (not including itself)
        /// </summary>
        public List<string> GetDependentDoorIds(string doorId)
        {
            try
            {
                var doorIndex = GetDoorById(doorId).DoorIndex;

                var dependentDoorIndices = GetDependentDoorIndices(doorIndex);

                return dependentDoorIndices.Select(x => doorMap[x].Id).ToList();
            }
            catch (NullReferenceException)
            {
                throw new ApplicationException("Can't find doorId.");
            }
        }

        //On a dfs search of the dependency tree, each hit vertex calls this
        private List<int> foundVertices;

        private void dfsDependencyVertexAction(int vertex)
        {
            foundVertices.Add(vertex);
        }
        /// <summary>
        /// Return the indices of all doors that depend on this door (not including itself)
        /// </summary>
        /// <param name="parentDoorId"></param>
        /// <returns></returns>
        public List<int> GetDependentDoorIndices(int parentDoorId)
        {
            try
            {
                var dfs = new DepthFirstSearchAlgorithm<int, Edge<int>>(doorDependencyGraph);
                dfs.DiscoverVertex += dfsDependencyVertexAction;

                foundVertices = new List<int>();
                dfs.Compute(parentDoorId);

                foundVertices.Remove(parentDoorId);

                return foundVertices;
            }
            catch (Exception)
            {
                throw new ApplicationException("Can't find door index");
            }
        }

        /// <summary>
        /// Do we need to open dependencyParentDoorId before we can open dependentDoorId?
        /// </summary>
        /// <param name="dependencyParentDoorId"></param>
        /// <param name="dependentDoorId"></param>
        /// <returns></returns>
        public bool IsDoorDependentOnParentDoor(string targetDoorId, string parentDoorId)
        {
            try
            {
                return GetDependencyEdge(parentDoorId, targetDoorId) != null;
            }
            catch (ApplicationException)
            {
                //Edge not in tree
                return false;
            }
        }

        /// <summary>
        /// Get an edge between doors in the depedency tree
        /// Refactor to isDependent would be better as an interface for users
        /// </summary>
        /// <param name="doorId1"></param>
        /// <param name="doorId2"></param>
        /// <returns></returns>
        private Edge<int> GetDependencyEdge(string dependencyParentDoorId, string dependentDoorId) {
            try
            {
                var dependencyParentIndex = GetDoorById(dependencyParentDoorId).DoorIndex;
                var dependentDoorIndex = GetDoorById(dependentDoorId).DoorIndex;

                QuickGraph.Edge<int> depEdge;
                doorDependencyGraph.TryGetEdge(dependencyParentIndex, dependentDoorIndex, out depEdge);
                if (depEdge == null)
                {
                    throw new ApplicationException(String.Format("Dependency {0} on {1} is not in tree", dependentDoorId, dependencyParentDoorId));
                }

                return depEdge;
            }
            catch (Exception)
            {
                throw new ApplicationException(String.Format("Dependency {0} on {1} is not in tree", dependentDoorId, dependencyParentDoorId));
            }
        }


        /// <summary>
        /// Return door-ids corresponding to clues present at vertex or empty if none
        /// </summary>
        /// <param name="vertexIndex"></param>
        /// <returns></returns>
        public IEnumerable<string> GetClueIdForVertex(int vertexIndex)
        {
            List<Clue> foundClue;

            clueMap.TryGetValue(vertexIndex, out foundClue);

            if (foundClue == null)
                return new List<string>();

            return foundClue.Select(c => doorMap[c.DoorIndex].Id);
        }

        public Door GetDoorByIndex(int doorIndex)
        {
            return doorMap[doorIndex];
        }

        /// <summary>
        /// Get Door on edge, or return null. Suboptimal, should be constructed as we go if used a lot
        /// </summary>
        /// <param name="edgeToFind"></param>
        /// <returns></returns>
        public int GetDoorIndexForEdge(TaggedEdge<int, string> edgeToFind)
        {
            foreach (var door in doorMap)
            {
                if (door.Value.DoorEdge == edgeToFind)
                {
                    return door.Value.DoorIndex;
                }
            }
            return -1;
        }

        public Door GetDoorById(string id)
        {
            foreach (var door in doorMap)
            {
                if (door.Value.Id == id)
                {
                    return door.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Get Door on edge, or return null. Suboptimal, should be constructed as we go if used a lot
        /// </summary>
        /// <param name="edgeToFind"></param>
        /// <returns></returns>
        public Door GetDoorForEdge(TaggedEdge<int, string> edgeToFind)
        {

            foreach (var door in doorMap)
            {
                if (door.Value.DoorEdge == edgeToFind)
                {
                    return door.Value;
                }
            }
            return null;
        }

        public Door GetDoorForEdge(Connection edge)
        {
            foreach (var door in doorMap)
            {
                if (door.Value.DoorEdge.Source == edge.Source &&
                    door.Value.DoorEdge.Target == edge.Target)
                {
                    return door.Value;
                }
            }
            return null;
        }

        public IEnumerable<int> GetAccessibleVerticesWithClues(IEnumerable<Clue> clues)
        {
            //Find all the locked edges not accessible by our clues
            var allDoors = doorMap.Keys;

            //How many keys we have for each door index
            var noCluesForDoors = clues.GroupBy(c => c.DoorIndex).ToDictionary(g => g.Key, g => g.Count());

            var unlockedDoors = doorMap.Where(d => noCluesForDoors.ContainsKey(d.Value.DoorIndex) &&
                                                   noCluesForDoors[d.Value.DoorIndex] >= d.Value.NumCluesRequired)
                                       .Select(d => d.Key);
            
            var lockedDoors = allDoors.Except(unlockedDoors);

            //Remove all areas behind any locked door
            var lockedEdges = lockedDoors.Select(d => doorMap[d].DoorEdge);
            MapSplitter allowedMap = new MapSplitter(mapNoCycles.mapNoCycles.Edges, lockedEdges, startVertex);

            //Find the component of this broken graph that is connected to the start vertex - 
            //This component contains the vertices accessible with these clues
            return allowedMap.MapComponent(allowedMap.RoomComponentIndex(startVertex));
        }
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

            //do the search
            dfs.Compute();

            //Build graph representation of MST for further processing

            //We have a dictionary of edges, so just iterate over it and build a new graph
            mst.AddVerticesAndEdgeRange(vertexPredecessors.Values);
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

        public readonly Dictionary<int, int> roomMappingToNoCycles = new Dictionary<int,int>();

        public MapCycleReducer(IEnumerable<TaggedEdge<int, string>> edges)
        {
            this.baseGraph = new UndirectedGraph<int, TaggedEdge<int, string>>();
            baseGraph.AddVerticesAndEdgeRange(edges);

            Process();
        }

        private void Process()
        {
            //Find minimum spanning tree
            MapMST mapMST = new MapMST(baseGraph.Edges);
            
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
            foreach (var vertex in baseGraph.Vertices)
                roomMappingToNoCycles[vertex] = vertex;

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
                    roomMappingToNoCycles[vertex] = firstVertex;

                    //Remove vertex from graph
                    mapNoCycles.RemoveVertex(vertex);
                }

                //Add all exterior edges onto the remaining cycle vertex
                foreach (var edge in exteriorEdges)
                {
                    //Rewrite edge
                    //Use mapped vertex indices, since those in this cycle (and other source cycles) will have been reduced
                    mapNoCycles.AddEdge(new TaggedEdge<int, string>(roomMappingToNoCycles[edge.Source], roomMappingToNoCycles[edge.Target], edge.Tag));
                }
            }
            if (CycleDebug)
                Console.WriteLine(String.Format("Cycle reduction - Cycles removed: {2}, Vertices before: {0}, vertices after: {1}", baseGraph.Vertices.Count(), mapNoCycles.Vertices.Count(), componentCount));
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
}
