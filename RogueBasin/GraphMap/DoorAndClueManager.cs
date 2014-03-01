using QuickGraph;
using QuickGraph.Algorithms.Search;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphMap
{

    /** Manages doors and clues in a map.
     *  Should be constructed with a map without cycles, which should not change subsequently.
     *  Provides utility methods for finding valid places to put clues and interrogating the
     *  clue/door dependency DAG
     */
    public class DoorAndClueManager
    {

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
            objectiveMap = new Dictionary<int, Objective>();
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

        public Dictionary<int, List<Objective>> ObjectiveRoomMap
        {
            get { return objectiveRoomMap; }
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
        public IEnumerable<int> GetValidRoomsToPlaceClueForDoor(Connection edgeForDoor, List<string> doorsToAvoidIds)
        {

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
            return GetValidRoomsToPlaceClueForObjective(objectiveId, new List<string>());
        }


        public IEnumerable<int> GetValidRoomsToPlaceClueForObjective(string objectiveId, List<string> doorsToAvoidIds)
        {
            var obj = GetObjectiveById(objectiveId);
            if (obj == null)
                throw new ApplicationException("Objective id not valid");

            return GetValidRoomsToPlaceObjectiveOrClueForObjective(obj.OpenLockIndex, doorsToAvoidIds);
        }

        private IEnumerable<int> GetValidRoomsToPlaceClueForObjective(List<int> locksLockedByObjective)
        {
            return GetValidRoomsToPlaceObjectiveOrClueForObjective(locksLockedByObjective, new List<string>());
        }


        /** Return the list of valid rooms in the cycle-free map to place a clue for a locked edge,
         * specifying also that we want to not place the clue behind any door in the list doorsToAvoid. 
           This function is also used when working out where to place the objective itself - it must also
           be accessible in the same way as its clues*/
        private IEnumerable<int> GetValidRoomsToPlaceObjectiveOrClueForObjective(List<int> locksLockedByObjective, List<string> doorsToAvoidIds)
        {
            var allowedNodes = GetValidRoomsToPlaceClue(doorsToAvoidIds, null, locksLockedByObjective);

            return allowedNodes;
        }

        /** Get valid rooms to place a clue, where rooms behind edgeToLock are inaccessible and rooms behind doors needing lockedClues are inaccessible */
        private IEnumerable<int> GetValidRoomsToPlaceClue(List<string> doorsToAvoidIds, Connection edgeToLock, IEnumerable<int> lockedClues)
        {
            //Check the edge is in the reduced map (will throw an exception if can't find)
            List<TaggedEdge<int, string>> foundEdge = new List<TaggedEdge<int, string>>(); ;
            if (edgeToLock != null)
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
        public IEnumerable<int> GetValidRoomsToPlaceClueForDoor(Connection edgeForDoor)
        {
            return GetValidRoomsToPlaceClueForDoor(edgeForDoor, new List<string>());
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
                if (door != null)
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
        /// Only really public for tests
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
                UpdateDependencyGraphWhenClueIsPlaced(clueVertex, new List<int> { thisDoorIndex });
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
        public IEnumerable<Clue> PlaceDoorAndClues(DoorRequirements doorReq, List<int> clueVertices)
        {
            //Check all clues are in the valid placement area
            if (GetValidRoomsToPlaceClueForDoor(doorReq.Location).Intersect(clueVertices).Count() < clueVertices.Count())
                throw new ApplicationException(String.Format("Can't put clues: {0}, behind door at {1}:{2}", GetValidRoomsToPlaceClueForDoor(doorReq.Location).Except(clueVertices).ToString(), doorReq.Location.Source, doorReq.Location.Target));

            return PlaceDoorAndCluesNoChecks(doorReq, clueVertices);
        }

        /// <summary>
        /// See PlaceDoorAndClues
        /// </summary>
        public void PlaceDoor(DoorRequirements doorReq)
        {
            PlaceDoorAndClues(doorReq, new List<int>());
        }

        /// <summary>
        /// See PlaceDoorAndClues
        /// </summary>
        public Clue PlaceDoorAndClue(DoorRequirements doorReq, int clueVertex)
        {
            return PlaceDoorAndClues(doorReq, new List<int> { clueVertex }).First();
        }

        private IEnumerable<Clue> GetCluesBehindLockedEdge(TaggedEdge<int, string> foundEdge)
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
        private List<int> GetDependentDoorIndices(int parentDoorId)
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
        private Edge<int> GetDependencyEdge(string dependencyParentDoorId, string dependentDoorId)
        {
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

        public IEnumerable<int> GetAccessibleVerticesWithClues(IEnumerable<int> lockedDoorsForCluesAndObjectives)
        {
            //Find all the locked edges not accessible by our clues
            var allDoors = doorMap.Keys;

            //How many keys we have for each locked item
            var allClues = lockedDoorsForCluesAndObjectives.ToList();
            Dictionary<int, int> noCluesForDoors;

            //Test objectives in a n^2 loop and add any liberated clues
            var openedObjectives = new HashSet<Objective>();
            int openedObjectivesCount;

            do {
                openedObjectivesCount = openedObjectives.Count();
                noCluesForDoors =  allClues.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
                var unlockedObjectives = objectiveMap.Where(obj => noCluesForDoors.ContainsKey(obj.Value.LockIndex) &&
                                               noCluesForDoors[obj.Value.LockIndex] >= obj.Value.NumCluesRequired)
                                               .Select(d => d.Value);

                var newlyUnlockedObjectives = unlockedObjectives.Except(openedObjectives);
                foreach(var obj in newlyUnlockedObjectives) openedObjectives.Add(obj);
                
                allClues.AddRange(newlyUnlockedObjectives.SelectMany(obj => obj.OpenLockIndex));
            } while(openedObjectivesCount != openedObjectives.Count());

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

            if (GetValidRoomsToPlaceClueForDoor(door.DoorConnectionReducedMap).Intersect(newClueVertices).Count() < newClueVertices.Count())
                throw new ApplicationException(String.Format("Can't put clues: {0}, behind door at {1}:{2}", GetValidRoomsToPlaceClueForDoor(door.DoorConnectionReducedMap).Except(newClueVertices).ToString(), door.DoorConnectionReducedMap.Source, door.DoorConnectionReducedMap.Target));

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
}
