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

namespace graphtestc
{
    /** Provides */
    class MapModel
    {

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
            public Clue(int doorIndex) {
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
        /// Minimum spanning tree for input map (internal)
        /// </summary>
        private UndirectedGraph<int, TaggedEdge<int, string>> mst;

        /// <summary>
        /// Edge predecessor to each vertex making up an MST (internal)
        /// </summary>
        private Dictionary<int, TaggedEdge<int, string>> vertexPredecessors = new Dictionary<int, TaggedEdge<int, string>>();

        /// <summary>
        /// Map with cycles removed, used to place doors etc.
        /// </summary>
        private UndirectedGraph<int, TaggedEdge<int, string>> gReduced;

        /// <summary>
        /// baseGraph -> reducedGraph vertex mapping
        /// </summary>
        private Dictionary<int, int> reducedMapping;
        
        /// <summary>
        /// Where the player starts. This node determines the unlocked side of a locked door
        /// </summary>
        private int startVertex;

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

        /// <summary>
        /// Next available door index
        /// </summary>
        private int nextDoorIndex = 0;

        public MapModel(UndirectedGraph<int, TaggedEdge<int, string>> inputGraph, int startVertex)
        {
            baseGraph = new UndirectedGraph<int, TaggedEdge<int, string>>();
            clueMap = new Dictionary<int, List<Clue>>();
            doorMap = new Dictionary<int, Door>();
            doorDependencyGraph = new AdjacencyGraph<int, Edge<int>>();

            //Clone the input graph (edges only)
            baseGraph.AddVerticesAndEdgeRange(inputGraph.Edges);

            this.startVertex = startVertex;
        }

        public AdjacencyGraph<int, Edge<int>> DoorDependencyGraph
        {
            get
            {
                return doorDependencyGraph;
            }
        }

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

        public Dictionary<int, int> VertexMapping
        {
            get
            {
                return reducedMapping;
            }
        }

        /** Produce acyclic graph and vertex mapping. Use accessors in class */
        public void EliminateCyclesInMap() {

            CalculateSpanningTree();
            ReduceCycles();
        }

        /// <summary>
        /// Get a map edge from the reduced (acyclic) tree. Returns null if fails
        /// </summary>
        /// <param name="startVertex"></param>
        /// <param name="endVertex"></param>
        /// <returns></returns>
        public TaggedEdge<int, string> GetReducedMapEdge(int startVertex, int endVertex)
        {
            TaggedEdge<int, string> edgeToFind;
            gReduced.TryGetEdge(startVertex, endVertex, out edgeToFind);

            return edgeToFind;
        }
        
        /// <summary>
        /// Get Door on edge, or return null. Suboptimal, should be constructed as we go if used a lot
        /// </summary>
        /// <param name="edgeToFind"></param>
        /// <returns></returns>
        public Door GetDoorForEdge(TaggedEdge<int, string> edgeToFind) {

            foreach (var door in doorMap)
            {
                if (door.Value.DoorEdge == edgeToFind)
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


        private void ReduceCycles()
        {
            //Find all cycles within tree

            if(CycleDebug)
                Console.WriteLine("--cycle finding--");

            //Find all edges not in MST
            var allEdges = baseGraph.Edges;
            var edgesInMST = vertexPredecessors.Values;

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

                var tryGetPath = mst.ShortestPathsDijkstra(x => 1, startVertex);

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

            //Duplicate the full graph (mutable)
            gReduced = new UndirectedGraph<int, TaggedEdge<int, string>>();
            gReduced.AddVerticesAndEdgeRange(baseGraph.Edges);

            //Maintain a map of vertex number mappings after cycle removal
            //Initialise with no-change case
            reducedMapping = new Dictionary<int, int>();
            foreach (var vertex in baseGraph.Vertices)
                reducedMapping[vertex] = vertex;

            //For each cycle

            for (int i = 0; i < componentCount; i++)
            {
                //Get all vertices in this cycle
                //Get all vertices (keys) with value (cycle no)
                var verticesInCycle = components.Where(kv => kv.Value == i).Select(kv => kv.Key);

                //First vertex (to be kept)
                //Other vertices (to be removed)
                var firstVertex = verticesInCycle.First();
                var verticesInCycleNotFirst = verticesInCycle.Skip(1);

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
                    reducedMapping[vertex] = firstVertex;

                    //Remove vertex from graph
                    gReduced.RemoveVertex(vertex);
                }

                //Add all exterior edges onto the remaining cycle vertex
                foreach (var edge in exteriorEdges)
                {
                    //Rewrite edge
                    //Use mapped vertex indices, since those in this cycle (and other source cycles) will have been reduced
                    gReduced.AddEdge(new TaggedEdge<int, string>(reducedMapping[edge.Source], reducedMapping[edge.Target], edge.Tag));
                }
            }
            Console.WriteLine(String.Format("Cycle reduction - Cycles removed: {2}, Vertices before: {0}, vertices after: {1}", baseGraph.Vertices.Count(), gReduced.Vertices.Count(), componentCount));
        }

        private void CalculateSpanningTree()
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
            mst = new UndirectedGraph<int, TaggedEdge<int, string>>();
            mst.AddVerticesAndEdgeRange(vertexPredecessors.Values);

        }

        private void treeEdge(object sender, UndirectedEdgeEventArgs<int, TaggedEdge<int, string>> e)
        {
            var vertexTarget = e.Target;

            //Associate vertex with predecessor edge
            vertexPredecessors.Add(vertexTarget, e.Edge);
        }

        /// <summary>
        /// Print the vertex mapping after cycle reduction
        /// </summary>
        public void PrintVertexMapping()
        {
            Console.WriteLine("Vertex mapping after cycle reduction");

            foreach (var map in reducedMapping.OrderBy(x => x.Key))
            {
                Console.WriteLine(map.Key.ToString() + "->" + map.Value.ToString());
            }
        }

        /** Lock an edge and place a random clue.
         *  Edge must be from GraphNoCycles.
         *  Therefore EliminateCyclesInMap() must be run */
        public void LockEdgeRandomClue(TaggedEdge<int, string> edge, string doorId)
        {
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

            foreach(var doorEdge in forbiddenDoorEdges) {
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

            PlaceDoorAndClue(edge, doorId, clueVertex);
        }

        //On a dfs search of the dependency tree, each hit vertex calls this
        private List<int> foundVertices;

        private void dfsDependencyVertexAction(int vertex)
        {
            foundVertices.Add(vertex);
        }

        /// <summary>
        /// Place a door and clue. No checks, so can easily be used to make impossible situations
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="doorId"></param>
        /// <param name="clueVertex"></param>
        public void PlaceDoorAndClue(TaggedEdge<int, string> edgeForDoor, string doorId, int clueVertex)
        {
            List<Clue> clueListAtVertex;
            clueMap.TryGetValue(clueVertex, out clueListAtVertex);

            if (clueListAtVertex == null)
            {
                clueMap[clueVertex] = new List<Clue>();
            }

            int thisDoorIndex = nextDoorIndex;
            nextDoorIndex++;
            clueMap[clueVertex].Add(new Clue(thisDoorIndex));

            Console.WriteLine("Placing door id: {0}, (index: {1}) at {2}->{3}", doorId, thisDoorIndex, edgeForDoor.Source, edgeForDoor.Target);
            Console.WriteLine("Placing clue index: {0} at {1}", thisDoorIndex, clueVertex);

            //Add door on this edge
            doorMap.Add(thisDoorIndex, new Door(edgeForDoor,doorId, thisDoorIndex));

            //Find path on MST from start location to clue. Any doors which we traverse become doors we DEPEND on
            var tryGetPath = mst.ShortestPathsDijkstra(x => 1, startVertex);

            IEnumerable<TaggedEdge<int, string>> path;
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
                            doorDependencyGraph.AddEdge(new Edge<int>(doorIndex, thisDoorIndex));
                            Console.WriteLine(String.Format("Door: {1}, now depends on: {0}", doorMap[doorIndex].Id, doorMap[thisDoorIndex].Id));
                        }

                    }
                }
                else
                {
                    Console.WriteLine(String.Format("BUG: no path found for between start and clue, start: {0}, end: {1}", startVertex, clueVertex));
                }
            }

        }

        public Door GetDoorByIndex(int doorIndex)
        {
            return doorMap[doorIndex];
        }
    }
}
