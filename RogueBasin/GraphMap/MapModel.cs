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
    /** The public interface to the map algorithms for locking doors, adding clues etc. */
    /*public class MapPuzzleLayer
    {

        private MapModel mapModel;

        public MapPuzzleLayer(ConnectivityMap inputMap)
        {
            mapModel = new MapModel(inputMap);

            //Do initial processing
            mapModel.EliminateCyclesInMap();
        }

        /// <summary>
        /// Dictionary of input room id -> model node
        /// Many rooms may map to the same node, if cycles are removed
        /// </summary>
        public Dictionary<int, int> RoomMappingToNoCycleMap
        {
            get
            {
                return mapModel.VertexMapping;
            }
        }

    }*/

    /** A clue to open a locked door */
    public class Clue
    {
        /// <summary>
        /// Where we are located (clue map index)
        /// </summary>
        private int locationVertex;

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

    /** A locked door requiring one or more clues to open */
    public class Door
    {
        /// <summary>
        /// Which edge we lock on the acyclic graph
        /// </summary>
        private TaggedEdge<int, string> doorEdge;

        /// <summary>
        /// Indentifying index (door map index)
        /// </summary>
        private int locationVertex;

        /// <summary>
        /// Door index into containing map
        /// </summary>
        private int index;

        public Door(TaggedEdge<int, string> doorEdge, string id, int index)
        {
            Id = id;
            this.doorEdge = doorEdge;
            this.index = index;
        }

        public TaggedEdge<int, string> DoorEdge
        {
            get
            {
                return doorEdge;
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


    /** Carries the state of a map being processed */
    public class MapModel
    {

        public bool CycleDebug
        {
            get;
            set;
        }

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

        /// <summary>
        /// Next available door index
        /// </summary>
        private int nextDoorIndex = 0;

        public MapModel(ConnectivityMap inputMap, int startVertex)
        {
            baseGraph = new UndirectedGraph<int, TaggedEdge<int, string>>();
            this.StartVertex = startVertex;

            //Clone the input graph (edges only)
            baseGraph.AddVerticesAndEdgeRange(inputMap.RoomConnectionGraph.Edges);

            //Build cycle-free map
            graphNoCycles = new MapCycleReducer(baseGraph.Edges);

            //Build Door and Clue Manager
            doorAndClueManager = new DoorAndClueManager(graphNoCycles, startVertex);

        }

        /// <summary>
        /// Set the player's start vertex. Must be called before locking doors etc.
        /// </summary>
        public int StartVertex
        {
            get
            {
                return startVertex;
            }
            private set
            {
                startVertex = value;
            }
        }

        /*
        public UndirectedGraph<int, TaggedEdge<int, string>> MinimumSpanningTree
        {
            get
            {
                return mst;
            }
        }

        public UndirectedGraph<int, TaggedEdge<int, string>> BaseGraph
        {
            get
            {
                return baseGraph;
            }
        }

        public UndirectedGraph<int, TaggedEdge<int, string>> GraphNoCycles
        {
            get
            {
                return gReduced;
            }
        }

        /// <summary>
        /// Dictionary of input room id -> model node
        /// Many rooms may map to the same node, if cycles are removed
        /// </summary>
        public Dictionary<int, int> VertexMapping
        {
            get
            {
                return reducedMapping;
            }
        }
         */
                
        /// <summary>
        /// Return a random edge in the reduced graph
        /// </summary>
        /// <returns></returns>
        public TaggedEdge<int, string> GetRandomUnlockedEdgeInReducedGraph()
        {
            Random r = new Random();
            var gReduced = graphNoCycles.mapNoCycles;

            int edgeToGet;

            //Check if all edges are locked, if so just return a random locked edge
            if (gReduced.EdgeCount <= doorAndClueManager.DoorMap.Count)
                return gReduced.Edges.ElementAt(r.Next(gReduced.EdgeCount));

            //If there are unlocked edges, return one of these
            do
            {
                edgeToGet = r.Next(gReduced.EdgeCount);

            } while (doorAndClueManager.GetDoorForEdge(gReduced.Edges.ElementAt(edgeToGet)) != null);

            return gReduced.Edges.ElementAt(edgeToGet);
        }


        /// <summary>
        /// Print the vertex mapping after cycle reduction
        /// </summary>
        public void PrintVertexMapping()
        {
            Console.WriteLine("Vertex mapping after cycle reduction");

            foreach (var map in graphNoCycles.roomMappingToNoCycles.OrderBy(x => x.Key))
            {
                Console.WriteLine(map.Key.ToString() + "->" + map.Value.ToString());
            }
        }

        /*
        public GetValidNodesToPlaceClue(UndirectedGraph<int, TaggedEdge<int, string>> mapNoCycles, Dictionary<int, List<Clue>> thisClueMap, int startVertex, TaggedEdge<int, string> edgeToLock, AdjacencyGraph<int, Edge<int>> doorDependencyGraph) {

            //Break tree on to-lock edge
            MapSplitter mapSplitter = new MapSplitter(mapNoCycles.Edges, edgeToLock, startVertex);
            int lockedTreeIndex = mapSplitter.NonOriginComponentIndex;

            //Traverse the locked tree and find all clues that will be behind the locked door

            //We do this simply by finding all clues in locked tree vertices

            //Lists of all clues in vertices which are in the locked tree
            var newlyLockedCluesLists = thisClueMap.Where(kv => mapSplitter.RoomComponentIndex(kv.Key) == lockedTreeIndex).Select(kv => kv.Value);
            //Flattened to one long list
            var newlyLockedClues = newlyLockedCluesLists.SelectMany(clue => clue);

            //We can't traverse any of the doors that these clues open when we place the clue for this new locked door
            //We also can't traverse any doors that depend on the doors in the first list
            var lockedCluesDoorIndices = newlyLockedClues.Select(clue => clue.DoorIndex);

            Console.WriteLine("Doors with clues behind this door");
            foreach (var door in lockedCluesDoorIndices.Distinct().Select(ind => doorMap[ind]))
            {
                Console.WriteLine("Id: {0} door loc: {1}", door.Id, door.DoorEdge.Source);
            }

            //Find all doors that depend on each of these doors
            var dfs = new DepthFirstSearchAlgorithm<int, Edge<int>>(doorDependencyGraph);
            dfs.DiscoverVertex += dfsDependencyVertexAction;

            foundVertices = new List<int>();
            foreach (var forbiddenDoorIndex in lockedCluesDoorIndices.Distinct())
            {
                //Do DFS with root being the inaccessible clue/door. This catches all dependent clue / doors
                //This will include the inaccessible clues themselves
                dfs.Compute(forbiddenDoorIndex);
            }

            //We now have all the doors we're not allowed to pass through when placing clues
            var lockedClueDoorsAndDependentsIndices = foundVertices.Distinct();
            var forbiddenDoorsList = lockedClueDoorsAndDependentsIndices.Select(ind => doorMap[ind]);

            Console.WriteLine("Doors with clues behind this door AND doors dependent on them");
            foreach (var door in forbiddenDoorsList)
            {
                Console.WriteLine("Id: {0} door loc: {1}", door.Id, door.DoorEdge.Source);
            }

            //Add the new door to the door dependency graph (TODO: tie up with door adding below)
            int thisDoorIndex = nextDoorIndex;
            doorDependencyGraph.AddVertex(thisDoorIndex);
            foreach (var door in lockedCluesDoorIndices)
            {
                //Edge goes FROM new door TO old door. Old door now DEPENDS on new door, since old door's clue is locked behind new door. New door must be opened first.
                doorDependencyGraph.AddEdge(new Edge<int>(thisDoorIndex, door));
            }

            //Place the new door clue in an allowed area

            //Retrieve the door edges in the forbidden list
            var forbiddenDoorEdges = forbiddenDoorsList.Select(door => door.DoorEdge);

            //Break all forbidden door edges

            var candidateTree = new UndirectedGraph<int, TaggedEdge<int, string>>();
            candidateTree.AddVerticesAndEdgeRange(gReduced.Edges);

            //Remove new door edge
            candidateTree.RemoveEdge(edge);

            foreach (var doorEdge in forbiddenDoorEdges)
            {
                candidateTree.RemoveEdge(doorEdge);
            }

            //Find the component of this broken graph that is connected to the start vertex - this is the candidate subtree
            var candidateComponents = new Dictionary<int, int>();
            int candidateComponentsCount = candidateTree.ConnectedComponents<int, TaggedEdge<int, string>>(candidateComponents);
            int candidateTreeIndex = candidateComponents[startVertex];

            //Get all nodes in the candidate graph
            var candidateNodes = candidateComponents.Where(kv => kv.Value == candidateTreeIndex).Select(kv => kv.Key);

            Console.WriteLine("Nodes in candidate graph");
            foreach (var node in candidateNodes)
            {
                Console.Write("{0} ", node);
            }
            Console.WriteLine();

        }*/

        /** Lock an edge and place a random clue.
         *  Edge must be from GraphNoCycles.
         *  Therefore EliminateCyclesInMap() must be run */
        public void LockEdgeRandomClue(TaggedEdge<int, string> edge, string doorId)
        {

            var gReduced = graphNoCycles.mapNoCycles;

            Console.WriteLine("---New door id: {0} at {1}->{2}---", doorId, edge.Source, edge.Target);

            //Check that edge is in reduced map
            if (!gReduced.ContainsEdge(edge))
                throw new Exception("Edge not in reduced map");

            //Break tree on this edge and label as unlocked (contains start vertex) and locked tree
            var brokenTree = new UndirectedGraph<int, TaggedEdge<int, string>>();
            brokenTree.AddVerticesAndEdgeRange(gReduced.Edges);

            brokenTree.RemoveEdge(edge);

            //We need to get the 2 subtrees created by this divide
            //This could be done by DFS from the source and target of the edge
            //Unfortunately, we don't have a top-level method to do that, so we get the connected components instead (may be slower)

            var components = new Dictionary<int, int>();
            int componentCount = brokenTree.ConnectedComponents<int, TaggedEdge<int, string>>(components);

            if (componentCount != 2)
            {
                throw new Exception("Must be 2 connected components after breaking on door");
            }

            //Which tree is the unlocked one?
            int unlockedTreeIndex = components[startVertex];
            int lockedTreeIndex = unlockedTreeIndex == 1 ? 0 : 1;

            //Traverse the locked tree and find all clues that will be behind the locked door

            //We do this simply by finding all clues in locked tree vertices

            var clueMap = doorAndClueManager.ClueMap;
            var doorMap = doorAndClueManager.DoorMap;

            //Lists of all clues in vertices which are in the locked tree
            var newlyLockedCluesLists = clueMap.Where(kv => components[kv.Key] == lockedTreeIndex).Select(kv => kv.Value);
            //Flattened to one long list
            var newlyLockedClues = newlyLockedCluesLists.SelectMany(clue => clue);

            //We can't traverse any of the doors that these clues open when we place the clue for this new locked door
            //We also can't traverse any doors that depend on the doors in the first list
            var lockedCluesDoorIndices = newlyLockedClues.Select(clue => clue.DoorIndex);

            Console.WriteLine("Doors with clues behind this door");
            foreach (var door in lockedCluesDoorIndices.Distinct().Select(ind => doorMap[ind]))
            {
                Console.WriteLine("Id: {0} door loc: {1}", door.Id, door.DoorEdge.Source);
            }

            //TODO: refactor - this function shouldn't be writing in this directly
            var doorDependencyGraph = doorAndClueManager.DoorDependencyGraph;

            //Find all doors that depend on each of these doors
            var dfs = new DepthFirstSearchAlgorithm<int, Edge<int>>(doorDependencyGraph);
            dfs.DiscoverVertex += dfsDependencyVertexAction;

            foundVertices = new List<int>();
            foreach (var forbiddenDoorIndex in lockedCluesDoorIndices.Distinct())
            {
                //Do DFS with root being the inaccessible clue/door. This catches all dependent clue / doors
                //This will include the inaccessible clues themselves
                dfs.Compute(forbiddenDoorIndex);
            }

            //We now have all the doors we're not allowed to pass through when placing clues
            var lockedClueDoorsAndDependentsIndices = foundVertices.Distinct();
            var forbiddenDoorsList = lockedClueDoorsAndDependentsIndices.Select(ind => doorMap[ind]);

            Console.WriteLine("Doors with clues behind this door AND doors dependent on them");
            foreach (var door in forbiddenDoorsList)
            {
                Console.WriteLine("Id: {0} door loc: {1}", door.Id, door.DoorEdge.Source);
            }

            //Add the new door to the door dependency graph (TODO: tie up with door adding below)
            int thisDoorIndex = nextDoorIndex;
            doorDependencyGraph.AddVertex(thisDoorIndex);
            foreach (var door in lockedCluesDoorIndices)
            {
                //Edge goes FROM new door TO old door. Old door now DEPENDS on new door, since old door's clue is locked behind new door. New door must be opened first.
                doorDependencyGraph.AddEdge(new Edge<int>(thisDoorIndex, door));
            }

            //Place the new door clue in an allowed area

            //Retrieve the door edges in the forbidden list
            var forbiddenDoorEdges = forbiddenDoorsList.Select(door => door.DoorEdge);

            //Break all forbidden door edges

            var candidateTree = new UndirectedGraph<int, TaggedEdge<int, string>>();
            candidateTree.AddVerticesAndEdgeRange(gReduced.Edges);

            //Remove new door edge
            candidateTree.RemoveEdge(edge);

            foreach (var doorEdge in forbiddenDoorEdges)
            {
                candidateTree.RemoveEdge(doorEdge);
            }

            //Find the component of this broken graph that is connected to the start vertex - this is the candidate subtree
            var candidateComponents = new Dictionary<int, int>();
            int candidateComponentsCount = candidateTree.ConnectedComponents<int, TaggedEdge<int, string>>(candidateComponents);
            int candidateTreeIndex = candidateComponents[startVertex];

            //Get all nodes in the candidate graph
            var candidateNodes = candidateComponents.Where(kv => kv.Value == candidateTreeIndex).Select(kv => kv.Key);

            Console.WriteLine("Nodes in candidate graph");
            foreach (var node in candidateNodes)
            {
                Console.Write("{0} ", node);
            }
            Console.WriteLine();

            //Place the clue in a random node in the candidate graph
            Random r = new Random();
            int clueVertex = candidateNodes.ElementAt(r.Next(candidateNodes.Count()));

            doorAndClueManager.PlaceDoorAndClue(edge.Source, edge.Target, doorId, clueVertex);
        }

        //On a dfs search of the dependency tree, each hit vertex calls this
        private List<int> foundVertices;

        private void dfsDependencyVertexAction(int vertex)
        {
            foundVertices.Add(vertex);
        }
    }

    /** Manages doors and clues in a map.
     *  Should be constructed with a map without cycles, which should not change subsequently.
     *  Also pass in a pre-calculated MST for speed
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

        public Dictionary<int, List<Clue>> ClueMap
        {
            get { return clueMap; }
        }

        public Dictionary<int, Door> DoorMap
        {
            get { return doorMap; }
        }

        public AdjacencyGraph<int, Edge<int>> DoorDependencyGraph
        {
            get { return doorDependencyGraph; }
        }

        /// <summary>
        /// Place a door and clue. No checks, so can easily be used to make impossible situations
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="doorId"></param>
        /// <param name="clueVertex"></param>
        public void PlaceDoorAndClue(int edgeForDoorSource, int edgeForDoorTarget, string doorId, int clueVertex)
        {
            //Check the edge is in the reduced map (will throw an exception if can't find)
            var foundEdge = mapNoCycles.GetEdgeBetweenRoomsNoCycles(edgeForDoorSource, edgeForDoorTarget);

            //Check we can route to the clueVertex without going through the to-be-locked edge
            var tryGetPath = mapNoCycles.mapNoCycles.ShortestPathsDijkstra(x => 1, startVertex);

            IEnumerable<TaggedEdge<int, string>> path;
            if (clueVertex != startVertex)
            {
                if (tryGetPath(clueVertex, out path))
                {
                    foreach (var edge in path)
                    {
                        if (edge.Source == edgeForDoorSource &&
                            edge.Target == edgeForDoorTarget)
                        {
                            Console.WriteLine(String.Format("Can't put clue: {0}, behind it's door at {1}:{2}", clueVertex, edgeForDoorSource, edgeForDoorTarget));
                            throw new ApplicationException(String.Format("Can't put clue: {0}, behind it's door at {1}:{2}", clueVertex, edgeForDoorSource, edgeForDoorTarget));
                        }
                    }
                }
            }

            //Add clue at vertex

            List<Clue> clueListAtVertex;
            clueMap.TryGetValue(clueVertex, out clueListAtVertex);

            if (clueListAtVertex == null)
            {
                clueMap[clueVertex] = new List<Clue>();
            }

            int thisDoorIndex = nextDoorIndex;
            nextDoorIndex++;
            clueMap[clueVertex].Add(new Clue(thisDoorIndex));

            Console.WriteLine("Placing door id: {0}, (index: {1}) at {2}->{3}", doorId, thisDoorIndex, edgeForDoorSource, edgeForDoorTarget);
            Console.WriteLine("Placing clue index: {0} at {1}", thisDoorIndex, clueVertex);

            //Add locked door on this edge
            doorMap.Add(thisDoorIndex, new Door(foundEdge, doorId, thisDoorIndex));

            //Find path on MST from start location to clue. Any doors which we traverse become doors we DEPEND on
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

        /// <summary>
        /// Get an edge between doors in the depedency tree
        /// Refactor to isDependent would be better as an interface for users
        /// </summary>
        /// <param name="doorId1"></param>
        /// <param name="doorId2"></param>
        /// <returns></returns>
        public Edge<int> GetDependencyEdge(string dependencyParentDoorId, string dependentDoorId) {
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

        public Door GetDoorForEdge(int source, int target)
        {
            foreach (var door in doorMap)
            {
                if (door.Value.DoorEdge.Source == source &&
                    door.Value.DoorEdge.Target == target)
                {
                    return door.Value;
                }
            }
            return null;
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
    }

    /** Immutable one-method class used to split a map into locked / unlocked sections */
    public class MapSplitter
    {
        readonly UndirectedGraph<int, TaggedEdge<int, string>> map;
        readonly TaggedEdge<int, string> edgeToSplitOn;
        readonly int originVertex;
        readonly Dictionary<int, int> components = new Dictionary<int,int>();
        private IEnumerable<TaggedEdge<int, string>> enumerable;

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

        public MapSplitter(IEnumerable<TaggedEdge<int, string>> edges, TaggedEdge<int, string> edgeToSplitOn, int originVertex)
        {

            this.map = new UndirectedGraph<int, TaggedEdge<int, string>>();
            map.AddVerticesAndEdgeRange(edges);

            this.edgeToSplitOn = edgeToSplitOn;
            this.originVertex = originVertex;

            Process();
        }

        private void Process()
        {
            //Break tree on this edge and label as unlocked (contains start vertex) and locked tree

            map.RemoveEdge(edgeToSplitOn);

            //We need to get the 2 subtrees created by this divide
            //This could be done by DFS from the source and target of the edge
            //Unfortunately, we don't have a top-level method to do that, so we get the connected components instead (may be slower)

            int componentCount = map.ConnectedComponents<int, TaggedEdge<int, string>>(components);

            if (componentCount != 2)
            {
                throw new Exception("Must be 2 connected components after breaking on requested edge");
            }

            //Which tree is the unlocked one?
            OriginComponentIndex = components[originVertex];
        }
    }      
}
