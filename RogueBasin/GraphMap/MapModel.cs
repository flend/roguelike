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
            } while (doorAndClueManager.GetDoorForEdge(gReduced.Edges.ElementAt(edgeToGet)) != null);

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
        public int StartVertexNoCycleMap { get { return GraphNoCycles.roomMappingFullToNoCycleMap[startVertex]; } }
    }

    /** Manages doors and clues in a map.
     *  Should be constructed with a map without cycles, which should not change subsequently.
     *  Provides utility methods for finding valid places to put clues and interrogating the
     *  clue/door dependency DAG
     */
    public class DoorAndClueManager {

        readonly MapCycleReducer mapNoCycles;
        readonly int startVertex;

        private int nextLockIndex = 0;

        /** Door dependency graph
         *
         *  Directed graph.
         *  Children of a node depend upon it (opposite to arrow direction).
         *  All clues dependent upon a door may be found by searching children (out-edges) of vertex
         *  
         *  vertex number <int> = door index
        */
        private AdjacencyGraph<int, Edge<int>> lockDependencyGraph;

        /** Door map
         * 
         *  key = lock index, unique between doors & objectives
         *  Door = information, including Edge. Only 1 door per edge.
         */
        private Dictionary<int, Door> doorMap;

        /** Objective map
         * 
         *  key = lock index, unique between doors & objectives
         *  Objective = information
         */
        private Dictionary<int, Objective> objectiveMap;

        /** Objective room map
         * 
         *  key = vertex where objective is located
         *  List<Objective> = all Objectives at this vertex
         */
        private Dictionary<int, List<Objective>> objectiveRoomMap;

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

            lockDependencyGraph = new AdjacencyGraph<int, Edge<int>>();
            doorMap = new Dictionary<int, Door>();
            clueMap = new Dictionary<int, List<Clue>>();
            objectiveMap = new Dictionary<int,Objective>();
            objectiveRoomMap = new Dictionary<int, List<Objective>>();
        }

        public Dictionary<int, List<Clue>> ClueMap
        {
            get { return clueMap; }
        }

        public Dictionary<int, Door> DoorMap
        {
            get { return doorMap; }
        }

        public Dictionary<int, Objective> ObjectiveMap
        {
            get { return objectiveMap; }
        }

        public AdjacencyGraph<int, Edge<int>> DoorDependencyGraph
        {
            get { return lockDependencyGraph; }
        }

        public IEnumerable<int> GetValidRoomsToPlaceClueForDoor(string doorId)
        {
            return GetValidRoomsToPlaceClueForDoor(doorId, new List<string>());
        }

        public IEnumerable<int> GetValidRoomsToPlaceClueForDoor(string doorId, List<string> doorsToAvoidIds)
        {
            var door = GetDoorById(doorId);
            if (door == null)
                throw new ApplicationException("Can't find door id: " + doorId);

            return GetValidRoomsToPlaceClueForDoor(door.DoorConnectionReducedMap, doorsToAvoidIds);
        }

        /** Return the list of valid rooms in the cycle-free map to place a clue for a locked edge,
         * specifying also that we want to not place the clue behind any door in the list doorsToAvoid*/
        public IEnumerable<int> GetValidRoomsToPlaceClueForDoor(Connection edgeForDoor, List<string> doorsToAvoidIds) {

            //Traverse the locked tree and find all clues that will be behind the locked door
            var foundEdge = mapNoCycles.GetEdgeBetweenRoomsNoCycles(edgeForDoor.Source, edgeForDoor.Target);
            var newlyLockedClues = GetCluesBehindLockedEdge(foundEdge);
            var newlyLockedObjectives = GetObjectivesBehindLockedEdge(foundEdge);

            var newlyLockedCluesIndices = newlyLockedClues.Select(c => c.OpenLockIndex).Union(newlyLockedObjectives.SelectMany(o => o.OpenLockIndex));

            var allowedNodes = GetValidRoomsToPlaceClue(doorsToAvoidIds, edgeForDoor, newlyLockedCluesIndices);

            return allowedNodes;
        }

        public IEnumerable<int> GetValidRoomsToPlaceClueForObjective(string objectiveId)
        {
            var obj = GetObjectiveById(objectiveId);
            if (obj == null)
                throw new ApplicationException("Objective id not valid");

            return GetValidRoomsToPlaceObjectiveOrClueForObjective(obj.OpenLockIndex, new List<string>());
        }

        public IEnumerable<int> GetValidRoomsToPlaceClueForObjective(List<int> locksLockedByObjective)
        {
            return GetValidRoomsToPlaceObjectiveOrClueForObjective(locksLockedByObjective, new List<string>());
        }


        /** Return the list of valid rooms in the cycle-free map to place a clue for a locked edge,
         * specifying also that we want to not place the clue behind any door in the list doorsToAvoid. 
           This function is also used when working out where to place the objective itself - it must also
           be accessible in the same way as its clues*/
        public IEnumerable<int> GetValidRoomsToPlaceObjectiveOrClueForObjective(List<int> locksLockedByObjective, List<string> doorsToAvoidIds)
        {
            var allowedNodes = GetValidRoomsToPlaceClue(doorsToAvoidIds, null, locksLockedByObjective);

            return allowedNodes;
        }

        /** Get valid rooms to place a clue, where rooms behind edgeToLock are inaccessible and rooms behind doors needing lockedClues are inaccessible */
        private IEnumerable<int> GetValidRoomsToPlaceClue(List<string> doorsToAvoidIds, Connection edgeToLock, IEnumerable<int> lockedClues)
        {
            //Check the edge is in the reduced map (will throw an exception if can't find)
            List<TaggedEdge<int, string>> foundEdge = new List<TaggedEdge<int,string>>();;
            if(edgeToLock != null)
                foundEdge.Add(mapNoCycles.GetEdgeBetweenRoomsNoCycles(edgeToLock.Source, edgeToLock.Target));

            //Find all doors that depend on any door with a locked clue.
            //We can't place a clue for our new door behind any of these
            var allDoorsDependentOnLockedClueDoors = lockedClues.SelectMany(c => GetDependentDoorIndices(c));
            var allInaccessibleDoors = lockedClues.Union(allDoorsDependentOnLockedClueDoors).Distinct();

            //Add to the list any doors we want to avoid (this can be used to localise clues to parts of levels etc.)
            var allDoorsToAvoid = doorsToAvoidIds.Select(id => GetDoorById(id).LockIndex);
            var allInaccessibleDoorsAndAvoidedDoors = allInaccessibleDoors.Union(allDoorsToAvoid);

            //Retrieve the door edges in the forbidden list
            //Note that some ids correspond to objectives, so these are avoided by the ugly SelectMany
            var forbiddenDoorEdges = allInaccessibleDoorsAndAvoidedDoors.SelectMany(
                doorIndex => doorMap.ContainsKey(doorIndex) ? new List<TaggedEdge<int, string>> { doorMap[doorIndex].DoorEdge } : new List<TaggedEdge<int, string>>());
            //Add this edge (can't put clue behind our own door) - NB: hacky way to union with a single item
            var allForbiddenDoorEdges = forbiddenDoorEdges.Union(foundEdge).Distinct();

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


        /** Return the list of valid rooms in the cycle-free map to place a clue for a locked edge */
        public IEnumerable<int> GetValidRoomsToPlaceClue(Connection edgeForDoor)
        {
            return GetValidRoomsToPlaceClueForDoor(edgeForDoor, new List<string>());
        }

        public IEnumerable<int> GetValidRoomsToPlaceClueForExistingDoor(string doorId)
        {
            return GetValidRoomsToPlaceClueForExistingDoor(doorId, new List<string>());
        }

        public IEnumerable<int> GetValidRoomsToPlaceClueForExistingDoor(string doorId, List<string> doorsToAvoidIds)
        {
            var door = GetDoorById(doorId);

            if (doorId == null)
            {
                throw new ApplicationException("Can't find door id " + doorId);
            }

            return GetValidRoomsToPlaceClueForDoor(door.DoorConnectionReducedMap, doorsToAvoidIds);
        }

        /// <summary>
        /// Add a clue for a doorId at requested room / vertex.
        /// No checks and no updates to the dependency graph.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="doorId"></param>
        private Clue BuildAndAddClueToMap(int room, Door thisDoor, Objective thisObjective)
        {           
            //Find the possible rooms that this clue could be placed in the real map
            var possibleRooms = mapNoCycles.roomMappingNoCycleToFullMap[room];

            Clue newClue;
            if (thisDoor != null)
                newClue = new Clue(thisDoor, possibleRooms);
            else
                newClue = new Clue(thisObjective, possibleRooms);

            List<Clue> clueListAtVertex;
            clueMap.TryGetValue(room, out clueListAtVertex);

            if (clueListAtVertex == null)
            {
                clueMap[room] = new List<Clue>();
            }

            clueMap[room].Add(newClue);

            return newClue;
        }

        private int GetLockIndexById(string id)
        {
            return GetLockIndicesByIds(new List<string> { id }).First();
        }

        private List<int> GetLockIndicesByIds(List<string> ids)
        {
            var indices = new List<int>();

            foreach (var id in ids)
            {
                var door = GetDoorById(id);
                if(door != null)
                    indices.Add(door.LockIndex);

                var obj = GetObjectiveById(id);

                if (obj != null)
                    indices.Add(obj.LockIndex);
            }

            return indices;
        }

        private Objective BuildAndAddObjectiveToMap(ObjectiveRequirements thisObj)
        {

            //Find the possible rooms that this objective could be placed in the real map
            var possibleRooms = mapNoCycles.roomMappingNoCycleToFullMap[thisObj.Vertex];

            int thisLockIndex = nextLockIndex;
            nextLockIndex++;

            var newObj = new Objective(thisObj.Vertex, thisObj.Id, thisLockIndex, GetLockIndicesByIds(thisObj.OpenLockId), thisObj.NumberOfCluesRequired, possibleRooms);

            //add to lock map
            objectiveMap.Add(thisLockIndex, newObj);
            lockDependencyGraph.AddVertex(thisLockIndex);

            //add to room map
            List<Objective> objListAtVertex;
            objectiveRoomMap.TryGetValue(thisObj.Vertex, out objListAtVertex);

            if (objListAtVertex == null)
            {
                objectiveRoomMap[thisObj.Vertex] = new List<Objective>();
            }

            objectiveRoomMap[thisObj.Vertex].Add(newObj);

            return newObj;
        }

        /// <summary>
        /// Lock an edge with an id, no checks, no dependency updates.
        /// Returns the door id
        /// </summary>
        /// <param name="edgeForDoorSource"></param>
        /// <param name="edgeForDoorTarget"></param>
        /// <param name="doorId"></param>
        private Door BuildDoorAndAddToMap(DoorRequirements doorReqs)
        {
            var edgeForDoor = doorReqs.Location;
            var foundEdge = mapNoCycles.GetEdgeBetweenRoomsNoCycles(edgeForDoor.Source, edgeForDoor.Target);

            int thisDoorIndex = nextLockIndex;
            nextLockIndex++;

            var doorEdgeInFullMap = mapNoCycles.edgeMappingNoCycleToFullMap[new Connection(edgeForDoor.Source, edgeForDoor.Target).Ordered];

            Door newDoor = new Door(foundEdge, edgeForDoor, doorEdgeInFullMap, doorReqs.Id, thisDoorIndex, doorReqs.NumberOfCluesRequired);
            doorMap.Add(thisDoorIndex, newDoor);
            lockDependencyGraph.AddVertex(thisDoorIndex);

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
            Door thisDoor = PlaceDoorAndUpdateDependencyGraph(doorReq);
            var clues = PlaceCluesAndUpdateDependencyGraph(clueVertices, thisDoor, null);

            return clues;
        }

        private Door PlaceDoorAndUpdateDependencyGraph(DoorRequirements doorReq)
        {
            //Check the edge is in the reduced map (will throw an exception if can't find)
            var edgeForDoor = doorReq.Location;
            var foundEdge = mapNoCycles.GetEdgeBetweenRoomsNoCycles(edgeForDoor.Source, edgeForDoor.Target);

            //Add locked door on this edge
            Door thisDoor = BuildDoorAndAddToMap(doorReq);
            int thisDoorIndex = thisDoor.LockIndex;

            //Find all clues now locked by this door, these clues depend on new door
            var newlyLockedClues = GetCluesBehindLockedEdge(foundEdge);
            var lockedCluesDoorIndices = newlyLockedClues.Select(clue => clue.OpenLockIndex);

            //Find all objectives now locked by this door, these objectives depend on new door
            var newlyLockedObj = GetObjectivesBehindLockedEdge(foundEdge);
            var lockedObjDoorIndices = newlyLockedObj.SelectMany(o => o.OpenLockIndex);

            var allLockedIndices = lockedCluesDoorIndices.Union(lockedObjDoorIndices);

            Console.WriteLine("Doors with clues behind this door");
            foreach (var door in lockedCluesDoorIndices.Distinct().Select(ind => doorMap[ind]))
            {
                Console.WriteLine("Id: {0} door loc: {1}", door.Id, door.DoorEdge.Source);
            }

            AddLockDependencyToExistingLocks(thisDoorIndex, allLockedIndices);
            return thisDoor;
        }

        private void AddLockDependencyToExistingLocks(int newDependency, IEnumerable<int> existingLockIndices)
        {
            //Add dependency on new door to all these clues
            foreach (var door in existingLockIndices)
            {
                //Edge goes FROM new door TO old door. Old door now DEPENDS on new door, since old door's clue is locked behind new door. New door must be opened first.
                lockDependencyGraph.AddEdge(new Edge<int>(newDependency, door));
            }
        }

        private List<Clue> PlaceCluesAndUpdateDependencyGraph(List<int> clueVertices, Door doorLockedByClues, Objective objectiveLockedByClues)
        {
            //Add clues
            var clues = new List<Clue>();
            foreach (var clueVertex in clueVertices)
                clues.Add(BuildAndAddClueToMap(clueVertex, doorLockedByClues, objectiveLockedByClues));

            int thisDoorIndex;
            if (doorLockedByClues != null)
                thisDoorIndex = doorLockedByClues.LockIndex;
            else
                thisDoorIndex = objectiveLockedByClues.LockIndex;

            //BUG: this seems to work under debug mode but fail in release builds
            //var clues = clueVertices.Select(vertex => PlaceClue(vertex, doorReq.Id));

            foreach (var clueVertex in clueVertices)
            {
                UpdateDependencyGraphWhenClueIsPlaced(clueVertex, new List<int>{ thisDoorIndex });
            }
            return clues;
        }

        private void UpdateDependencyGraphWhenClueIsPlaced(int clueVertex, List<int> locksLockedByClue)
        {
            //Find path on MST from start location to all clues. Any doors which we traverse become doors we DEPEND on
            
            var tryGetPath = mapNoCycles.mapNoCycles.ShortestPathsDijkstra(x => 1, startVertex);
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
                            locksLockedByClue.ForEach(
                                lockIndex => lockDependencyGraph.AddVerticesAndEdge(new Edge<int>(doorIndex, lockIndex))
                            );
                            Console.WriteLine(String.Format("Door: {1}, now depends on: {0}", doorMap[doorIndex].Id, locksLockedByClue));
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
            if (GetValidRoomsToPlaceClue(doorReq.Location).Intersect(clueVertices).Count() < clueVertices.Count())
                throw new ApplicationException(String.Format("Can't put clues: {0}, behind door at {1}:{2}", GetValidRoomsToPlaceClue(doorReq.Location).Except(clueVertices).ToString(), doorReq.Location.Source, doorReq.Location.Target));

            return PlaceDoorAndCluesNoChecks(doorReq, clueVertices);
        }

        /// <summary>
        /// See PlaceDoorAndClues
        /// </summary>
        public void PlaceDoor(DoorRequirements doorReq)
        {
            PlaceDoorAndClues(doorReq, new List<int> ());
        }

        /// <summary>
        /// See PlaceDoorAndClues
        /// </summary>
        public Clue PlaceDoorAndClue(DoorRequirements doorReq, int clueVertex)
        {
            return PlaceDoorAndClues(doorReq, new List<int>{ clueVertex }).First();
        }

        private IEnumerable<Clue> GetCluesBehindLockedEdge(TaggedEdge<int,string> foundEdge)
        {
            MapSplitter splitMap = new MapSplitter(mapNoCycles.mapNoCycles.Edges, foundEdge, startVertex);
            
            //Lists of all clues in vertices which are in the locked tree
            var newlyLockedCluesLists = clueMap.Where(kv => splitMap.RoomComponentIndex(kv.Key) == splitMap.NonOriginComponentIndex).Select(kv => kv.Value);
            //Flattened to one long list
            return newlyLockedCluesLists.SelectMany(clue => clue);
        }

        private IEnumerable<Objective> GetObjectivesBehindLockedEdge(TaggedEdge<int, string> foundEdge)
        {
            MapSplitter splitMap = new MapSplitter(mapNoCycles.mapNoCycles.Edges, foundEdge, startVertex);

            //Lists of all clues in vertices which are in the locked tree
            var newlyLockedObjectiveLists = objectiveRoomMap.Where(kv => splitMap.RoomComponentIndex(kv.Key) == splitMap.NonOriginComponentIndex).Select(kv => kv.Value);
            //Flattened to one long list
            return newlyLockedObjectiveLists.SelectMany(o => o);
        }


        /// <summary>
        /// Return the ids of all doors that depend on this door (not including itself)
        /// </summary>
        public List<string> GetDependentDoorIds(string doorId)
        {
            try
            {
                var doorIndex = GetDoorById(doorId).LockIndex;

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
                var dfs = new DepthFirstSearchAlgorithm<int, Edge<int>>(lockDependencyGraph);
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
        public bool IsLockDependentOnParentLock(string targetDoorId, string parentDoorId)
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
                var dependencyParentIndex = GetLockIndexById(dependencyParentDoorId);
                var dependentDoorIndex = GetLockIndexById(dependentDoorId);

                QuickGraph.Edge<int> depEdge;
                lockDependencyGraph.TryGetEdge(dependencyParentIndex, dependentDoorIndex, out depEdge);
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

            return foundClue.Select(c => doorMap[c.OpenLockIndex].Id);
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
                    return door.Value.LockIndex;
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

        public Objective GetObjectiveById(string id)
        {
            foreach (var obj in objectiveMap)
            {
                if (obj.Value.Id == id)
                {
                    return obj.Value;
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
                    door.Value.DoorEdge.Target == edge.Target ||
                    
                    door.Value.DoorEdge.Source == edge.Target &&
                    door.Value.DoorEdge.Target == edge.Source)
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
            var noCluesForDoors = clues.GroupBy(c => c.OpenLockIndex).ToDictionary(g => g.Key, g => g.Count());

            var unlockedDoors = doorMap.Where(d => noCluesForDoors.ContainsKey(d.Value.LockIndex) &&
                                                   noCluesForDoors[d.Value.LockIndex] >= d.Value.NumCluesRequired)
                                       .Select(d => d.Key);
            
            var lockedDoors = allDoors.Except(unlockedDoors);

            //Remove all areas behind any locked door
            var lockedEdges = lockedDoors.Select(d => doorMap[d].DoorEdge);
            MapSplitter allowedMap = new MapSplitter(mapNoCycles.mapNoCycles.Edges, lockedEdges, startVertex);

            //Find the component of this broken graph that is connected to the start vertex - 
            //This component contains the vertices accessible with these clues
            return allowedMap.MapComponent(allowedMap.RoomComponentIndex(startVertex));
        }

        public List<Clue> AddCluesToExistingDoor(string doorId, List<int> newClueVertices)
        {
            var door = GetDoorById(doorId);
            if (door == null)
                throw new ApplicationException("Can't find door id " + doorId);

            if (GetValidRoomsToPlaceClue(door.DoorConnectionReducedMap).Intersect(newClueVertices).Count() < newClueVertices.Count())
                throw new ApplicationException(String.Format("Can't put clues: {0}, behind door at {1}:{2}", GetValidRoomsToPlaceClue(door.DoorConnectionReducedMap).Except(newClueVertices).ToString(), door.DoorConnectionReducedMap.Source, door.DoorConnectionReducedMap.Target));

            return PlaceCluesAndUpdateDependencyGraph(newClueVertices, door, null);
        }

        public List<Clue> AddCluesToExistingObjective(string objectiveId, List<int> newClueVertices)
        {
            var objective = GetObjectiveById(objectiveId);
            if (objective == null)
                throw new ApplicationException("Can't find obj id " + objectiveId);

            var validRoomsForObjectiveClues = GetValidRoomsToPlaceClueForObjective(objective.OpenLockIndex);
            if (validRoomsForObjectiveClues.Intersect(newClueVertices).Count() < newClueVertices.Count())
                throw new ApplicationException(String.Format("Can't put clues: {0}, for objective at {1}", validRoomsForObjectiveClues.Except(newClueVertices).ToString(), objective.Vertex));

            return PlaceCluesAndUpdateDependencyGraph(newClueVertices, null, objective);
        }

        public void PlaceObjective(ObjectiveRequirements objectiveRequirements)
        {
            PlaceObjective(objectiveRequirements, new List<string>());
        }

        public void PlaceObjective(ObjectiveRequirements objectiveRequirements, List<string> doorsToAvoidIds)
        {
 	        //Objectives are like clues in so much as they may be a 'clue' to lock a door

            var locksLockedByObjectiveIndices = GetLockIndicesByIds(objectiveRequirements.OpenLockId);
            if (locksLockedByObjectiveIndices.Count() != objectiveRequirements.OpenLockId.Count())
            {
                throw new ApplicationException("Lock for objective does not exist");
            }

            if (!GetValidRoomsToPlaceObjectiveOrClueForObjective(locksLockedByObjectiveIndices, doorsToAvoidIds).Contains(objectiveRequirements.Vertex))
                throw new ApplicationException("Can't place objective " + objectiveRequirements.Id + " at vertex " + objectiveRequirements.Vertex);

            var objective = BuildAndAddObjectiveToMap(objectiveRequirements);

            //An objective acts like
            //a) placing clues for the lock that depends on it (if any)
            UpdateDependencyGraphWhenClueIsPlaced(objective.Vertex, objective.OpenLockIndex);
            //b) locking these clues with a virtual door that is the objective
            AddLockDependencyToExistingLocks(objective.LockIndex, objective.OpenLockIndex);
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

        private List<List<Connection>> allCycles = new List<List<Connection>>();

        public MapCycleReducer(IEnumerable<TaggedEdge<int, string>> edges)
        {
            this.baseGraph = new UndirectedGraph<int, TaggedEdge<int, string>>();
            baseGraph.AddVerticesAndEdgeRange(edges);

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
            roomMappingNoCycleToFullMap = roomMappingToNoCyclesWork.GroupBy(kv => kv.Value).ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList()).AsReadOnly();
                
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
