using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class WorldGenerator : MonoBehaviour
{
  public GameObject mapContainer; // Prefab or container for rooms
  public GeneratorConfig biomeGenerator;
  public WorldGenerationController worldGenerationController;
  public UnityEngine.Transform player;
  [Tooltip("How many room hops away to spawn decorations (1 = current + immediate neighbors)")]
  public int spawnRoomRadius = 1;

  // Biome configs
  public GeneratorConfig forestConfig;
  public GeneratorConfig desertConfig;
  public GeneratorConfig tundraConfig;
  public int targetRooms = 10; // Number of rooms to generate

  // Room size (should match MapGenerator's width/height)
  public int roomWidth = 60;
  public int roomHeight = 60;
  [Header("Debug")]
  [Tooltip("If true, MapGenerators will skip decoration spawning (tile shadows + per-room GameObject decorations).")]
  public bool disableDecorationsGlobally = false;
  [Tooltip("If true, MapGenerators will skip enemy spawning.")]
  public bool disableEnemySpawnsGlobally = false;
  // Static globals so MapGenerator can check even if toggles are applied after Start
  public static bool globalDisableDecorations = false;
  public static bool globalDisableEnemySpawns = false;

  [System.Serializable]
  public class StaticRoom
  {
    public Vector2Int position; // anchor (e.g., top-left or center)
    public GameObject prefab;
    public Vector2Int size = new Vector2Int(1, 1); // width, height
    [Tooltip("Mandatory branch directions for this static room. Only directions listed here will be allowed to branch from this room.")]
    public MapGenerator.EntranceDirection[] mandatoryBranchDirections = new MapGenerator.EntranceDirection[0];
    [Tooltip("If true, this static room will not spawn enemies regardless of global/world settings.")]
    public bool disableEnemySpawns = false;
  }
  public List<StaticRoom> staticRooms = new List<StaticRoom>();

  // Directions: N, S, E, W
  private Vector2Int[] cardinalDirections;
  // For tracking which directions each room should have entrances
  private Dictionary<Vector2Int, HashSet<Vector2Int>> roomEntrances = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
  private Dictionary<Vector2Int, MapGenerator> roomMapGens = new Dictionary<Vector2Int, MapGenerator>();
  // Track which rooms currently have decorations active
  private HashSet<Vector2Int> activeDecorationRooms = new HashSet<Vector2Int>();
  private Vector2Int currentPlayerRoom = new Vector2Int(int.MinValue, int.MinValue);
  // World-level room id mappings
  private Dictionary<Vector2Int, int> roomOriginToId = new Dictionary<Vector2Int, int>();
  private Dictionary<int, Vector2Int> roomIdToOrigin = new Dictionary<int, Vector2Int>();
  private Dictionary<int, HashSet<int>> roomAdjacency = new Dictionary<int, HashSet<int>>();
  private int nextRoomId = 0;
  // Previous values to detect inspector/runtime changes
  private bool prevDisableDecorationsGlobally = false;
  private bool prevDisableEnemySpawnsGlobally = false;
  void Start()
  {
    cardinalDirections = new Vector2Int[] {
        new Vector2Int(0, roomHeight),    // Up
        new Vector2Int(0, -roomHeight),   // Down
        new Vector2Int(roomWidth, 0),     // Right
        new Vector2Int(-roomWidth, 0)     // Left
    };

    // Clear previous room locations
    if (worldGenerationController.roomLocations == null)
      worldGenerationController.roomLocations = new List<KeyValuePair<int, int>>();
    else
      worldGenerationController.roomLocations.Clear();

    Vector2Int startPos = new Vector2Int(0, 0);
    // Do NOT call CreateRoomAt here!
    StartCoroutine(GenerateRoomLayoutWithStaticsCoroutine());
  }

  List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, HashSet<Vector2Int> blocked)
  {
    Queue<Vector2Int> queue = new Queue<Vector2Int>();
    Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
    queue.Enqueue(start);
    cameFrom[start] = start;

    Vector2Int[] dirs = cardinalDirections;

    while (queue.Count > 0)
    {
      var current = queue.Dequeue();
      if (current == end)
      {
        // Reconstruct path
        List<Vector2Int> path = new List<Vector2Int>();
        while (current != start)
        {
          path.Add(current);
          current = cameFrom[current];
        }
        path.Reverse();
        return path;
      }
      foreach (var dir in dirs)
      {
        Vector2Int next = current + dir;
        if (!cameFrom.ContainsKey(next) && !blocked.Contains(next))
        {
          queue.Enqueue(next);
          cameFrom[next] = current;
        }
      }
    }
    return null; // No path found
  }

  System.Collections.IEnumerator GenerateRoomLayoutWithStaticsCoroutine()
  {
    HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
    Dictionary<Vector2Int, GameObject> staticRoomPrefabs = new Dictionary<Vector2Int, GameObject>();
    // Track which static origins explicitly opt out of enemy spawns
    HashSet<Vector2Int> staticOriginsDisableEnemySpawns = new HashSet<Vector2Int>();
    roomEntrances.Clear();

    Vector2Int startPos = Vector2Int.zero;

    // 1. Place all static rooms (if any)
    foreach (var staticRoom in staticRooms)
    {
      if (staticRoom.prefab == null)
      {
        Debug.LogError($"Static room at {staticRoom.position} is missing a prefab. All static rooms must have a prefab.");
        continue;
      }
      // Snap position to the nearest room grid cell in world coordinates.
      // This handles inputs that are already world coordinates (e.g. 380) or cell indices.
      int gridX = Mathf.RoundToInt(staticRoom.position.x / (float)roomWidth);
      int gridY = Mathf.RoundToInt(staticRoom.position.y / (float)roomHeight);
      Vector2Int worldPos = new Vector2Int(gridX * roomWidth, gridY * roomHeight);
      if (worldPos != staticRoom.position)
      {
        Debug.Log($"WorldGenerator: snapping static room {staticRoom.position} -> grid ({gridX},{gridY}) -> world {worldPos}");
      }

      roomPositions.Add(worldPos);
      staticRoomPrefabs[worldPos] = staticRoom.prefab;
      if (staticRoom.disableEnemySpawns)
      {
        staticOriginsDisableEnemySpawns.Add(worldPos);
      }
      if (!roomEntrances.ContainsKey(worldPos))
        roomEntrances[worldPos] = new HashSet<Vector2Int>();
      // If the static room specifies mandatoryBranchDirections, convert them to room grid offsets and store
      if (staticRoom.mandatoryBranchDirections != null && staticRoom.mandatoryBranchDirections.Length > 0)
      {
        foreach (var dir in staticRoom.mandatoryBranchDirections)
        {
          Vector2Int offset = Vector2Int.zero;
          if (dir == MapGenerator.EntranceDirection.NORTH) offset = new Vector2Int(0, roomHeight);
          else if (dir == MapGenerator.EntranceDirection.SOUTH) offset = new Vector2Int(0, -roomHeight);
          else if (dir == MapGenerator.EntranceDirection.EAST) offset = new Vector2Int(roomWidth, 0);
          else if (dir == MapGenerator.EntranceDirection.WEST) offset = new Vector2Int(-roomWidth, 0);
          if (offset != Vector2Int.zero) roomEntrances[worldPos].Add(offset);
        }
      }
    }
    // Log normalized static rooms for debugging
    Debug.Log($"WorldGenerator: staticRooms provided={staticRooms.Count}, normalized placed={staticRoomPrefabs.Count}");
    foreach (var kv in staticRoomPrefabs)
    {
      Debug.Log($"WorldGenerator: static room at {kv.Key} -> prefab={(kv.Value != null ? kv.Value.name : "<null>")}");
    }

    // If there are no static rooms, seed with a start position so procedural generation can proceed
    if (roomPositions.Count == 0)
    {
      roomPositions.Add(startPos);
      if (!roomEntrances.ContainsKey(startPos))
        roomEntrances[startPos] = new HashSet<Vector2Int>();
    }

    // 2. Connect static rooms with simple axis-aligned paths (pairwise).
    // Step in room-grid cells (divide positions by roomWidth/roomHeight) to avoid mismatch and infinite loops.
    if (staticRooms != null && staticRooms.Count >= 2)
    {
      for (int i = 0; i < staticRooms.Count - 1; i++)
      {
        Vector2Int fromWorld = staticRooms[i].position;
        Vector2Int toWorld = staticRooms[i + 1].position;
        // Convert to cell coords
        int fromCellX = Mathf.RoundToInt((float)fromWorld.x / roomWidth);
        int fromCellY = Mathf.RoundToInt((float)fromWorld.y / roomHeight);
        int toCellX = Mathf.RoundToInt((float)toWorld.x / roomWidth);
        int toCellY = Mathf.RoundToInt((float)toWorld.y / roomHeight);

        Vector2Int curCell = new Vector2Int(fromCellX, fromCellY);
        int safety = 0;
        int maxSteps = 1000;
        // Step horizontally then vertically in cell space
        while (curCell.x != toCellX && safety < maxSteps)
        {
          safety++;
          int sign = (toCellX > curCell.x) ? 1 : -1;
          curCell.x += sign;
          Vector2Int worldPos = new Vector2Int(curCell.x * roomWidth, curCell.y * roomHeight);
          roomPositions.Add(worldPos);
          if (!roomEntrances.ContainsKey(worldPos)) roomEntrances[worldPos] = new HashSet<Vector2Int>();
          if ((safety & 63) == 0) yield return null; // yield occasionally
        }
        safety = 0;
        while (curCell.y != toCellY && safety < maxSteps)
        {
          safety++;
          int sign = (toCellY > curCell.y) ? 1 : -1;
          curCell.y += sign;
          Vector2Int worldPos = new Vector2Int(curCell.x * roomWidth, curCell.y * roomHeight);
          roomPositions.Add(worldPos);
          if (!roomEntrances.ContainsKey(worldPos)) roomEntrances[worldPos] = new HashSet<Vector2Int>();
          if ((safety & 63) == 0) yield return null; // yield occasionally
        }
      }
    }

    // 3. Add more procedural rooms up to targetRooms
    System.Random rnd = new System.Random();
    if (roomPositions.Count == 0)
    {
      roomPositions.Add(startPos);
      if (!roomEntrances.ContainsKey(startPos))
        roomEntrances[startPos] = new HashSet<Vector2Int>();
    }

    int maxAttempts = Mathf.Min(targetRooms * 100, 10000);
    int attempts = 0;
    int innerLoopCounter = 0;
    while (roomPositions.Count < targetRooms && attempts < maxAttempts)
    {
      bool addedAny = false;
      var baseRoomList = new List<Vector2Int>(roomPositions);
      if (baseRoomList.Count == 0)
        break;
      Debug.Log($"WorldGenerator: attempt={attempts}, rooms={roomPositions.Count}, baseRooms={baseRoomList.Count}");

      foreach (var baseRoom in baseRoomList)
      {
        var possibleDirs = new List<Vector2Int>(cardinalDirections);
        // Shuffle directions for randomness
        for (int i = possibleDirs.Count - 1; i > 0; i--)
        {
          int j = rnd.Next(i + 1);
          var temp = possibleDirs[i];
          possibleDirs[i] = possibleDirs[j];
          possibleDirs[j] = temp;
        }
        foreach (var dir in possibleDirs)
        {
          Vector2Int newRoomOrigin = baseRoom + dir;
          if (!roomPositions.Contains(newRoomOrigin))
          {
            roomPositions.Add(newRoomOrigin);
            if (!roomEntrances.ContainsKey(newRoomOrigin))
              roomEntrances[newRoomOrigin] = new HashSet<Vector2Int>();
            addedAny = true;
            Debug.Log($"WorldGenerator: added room {newRoomOrigin} from base {baseRoom}");
            break;
          }
        }
        if (roomPositions.Count >= targetRooms)
          break;
      }
      attempts++;
      innerLoopCounter++;
      if ((innerLoopCounter & 127) == 0) // yield occasionally to avoid freezing (every 128 iterations)
      {
        yield return null;
      }
      if (!addedAny)
      {
        if (roomPositions.Count < targetRooms)
        {
          Debug.LogWarning($"WorldGenerator: Only able to generate {roomPositions.Count} rooms out of requested {targetRooms}. Expansion blocked by static room layout or map boundaries.");
        }
        break;
      }
    }

    // 4. Build entrance sets for all rooms (mutual)
    // Respect any pre-populated mandatory branch directions for static rooms by
    // taking a snapshot of the initial allowed direction sets. If a room has an
    // explicit non-empty set, treat it as a whitelist: only those directions are
    // permitted to branch from that room. When creating mutual entrances, both
    // sides must permit the connection (either unrestricted or explicitly allowed).
    var initialAllowed = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    foreach (var kv in roomEntrances)
    {
      initialAllowed[kv.Key] = new HashSet<Vector2Int>(kv.Value);
    }

    foreach (var pos in roomPositions)
    {
      if (!roomEntrances.ContainsKey(pos))
        roomEntrances[pos] = new HashSet<Vector2Int>();

      foreach (var dir in cardinalDirections)
      {
        Vector2Int neighbor = pos + dir;
        if (!roomPositions.Contains(neighbor)) continue;

        // Determine if origin allows this dir (empty whitelist => unrestricted)
        bool originHasWhitelist = initialAllowed.ContainsKey(pos) && initialAllowed[pos].Count > 0;
        bool originAllows = !originHasWhitelist || initialAllowed[pos].Contains(dir);

        // Determine if neighbor allows the opposite dir
        bool neighborHasWhitelist = initialAllowed.ContainsKey(neighbor) && initialAllowed[neighbor].Count > 0;
        bool neighborAllows = !neighborHasWhitelist || initialAllowed[neighbor].Contains(-dir);

        if (originAllows && neighborAllows)
        {
          roomEntrances[pos].Add(dir);
          if (!roomEntrances.ContainsKey(neighbor))
            roomEntrances[neighbor] = new HashSet<Vector2Int>();
          roomEntrances[neighbor].Add(-dir);
        }
      }
    }

    // 5. Prune entrances that do not connect to an actual room
    foreach (var kvp in new List<KeyValuePair<Vector2Int, HashSet<Vector2Int>>>(roomEntrances))
    {
      Vector2Int roomPos = kvp.Key;
      var toRemove = new List<Vector2Int>();
      foreach (var dir in kvp.Value)
      {
        Vector2Int neighbor = roomPos + dir;
        if (!roomPositions.Contains(neighbor))
          toRemove.Add(dir);
      }
      foreach (var dir in toRemove)
        kvp.Value.Remove(dir);
    }

    // 6. Instantiate rooms (static prefabs where provided, skip if prefab is null)
    foreach (var pos in roomPositions)
    {
      GameObject prefab = staticRoomPrefabs.ContainsKey(pos) ? staticRoomPrefabs[pos] : mapContainer;
  CreateRoomAt(pos, roomEntrances.ContainsKey(pos) ? roomEntrances[pos] : new HashSet<Vector2Int>(), prefab, isStartRoom: false, disableEnemySpawnsForThisRoom: staticOriginsDisableEnemySpawns.Contains(pos));
      // small yield to avoid long frame when instantiating many rooms
      yield return null;
    }

    // After creating all rooms, build adjacency from the recorded origins to ensure
    // static rooms (and any ordering differences) have mutual adjacency entries.
    BuildAdjacencyFromOrigins();
    // Ensure our global toggles are applied to all created rooms
    ApplyGlobalDebugTogglesToAllRooms();
    // Pre-create persistent decorations for all rooms to avoid spikes when player
    // enters a room. Start them hidden by default.
    foreach (var kv in roomMapGens)
    {
      try { kv.Value.SpawnAllDecorationsPersistent(); } catch (System.Exception ex) { Debug.LogError($"WorldGenerator: error creating persistent decorations for {kv.Key}: {ex}"); }
    }

    yield break;
  }

  GeneratorConfig GetBiomeForY(int y)
  {
    if (y < -roomHeight) return desertConfig;
    if (y > roomHeight) return tundraConfig;
    return forestConfig;
  }

  void GenerateRoomLayout(Vector2Int startPos)
  {
    System.Random rnd = new System.Random();
    HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int> { startPos };
    List<(Vector2Int pos, Vector2Int? fromDir)> frontier = new List<(Vector2Int, Vector2Int?)>();
    HashSet<Vector2Int> frontierSet = new HashSet<Vector2Int>();
    roomEntrances.Clear();

    // Add all cardinal neighbors of the start room to the frontier, with their direction
    foreach (var dir in cardinalDirections)
    {
      Vector2Int neighbor = startPos + dir;
      frontier.Add((neighbor, dir));
      frontierSet.Add(neighbor);
    }

    int roomsCreated = 1;
    while (roomsCreated < targetRooms && frontier.Count > 0)
    {
      int pickIdx = rnd.Next(frontier.Count);
      var (newRoomPos, prevDir) = frontier[pickIdx];
      frontier.RemoveAt(pickIdx);
      frontierSet.Remove(newRoomPos);

      bool isAdjacent = false;
      foreach (var dir in cardinalDirections)
      {
        if (roomPositions.Contains(newRoomPos + dir))
        {
          isAdjacent = true;
          break;
        }
      }
      if (!isAdjacent)
        continue;

      List<Vector2Int> possibleDirs = new List<Vector2Int>();
      foreach (var dir in cardinalDirections)
      {
        Vector2Int neighbor = newRoomPos + dir;
        if (!roomPositions.Contains(neighbor) && !frontierSet.Contains(neighbor))
        {
          if (prevDir.HasValue && dir == prevDir.Value)
          {
            possibleDirs.Add(dir);
            possibleDirs.Add(dir);
            possibleDirs.Add(dir);
          }
          possibleDirs.Add(dir);
        }
      }

      List<Vector2Int> branchDirs = new List<Vector2Int>();
      Vector2Int? mainDir = null;
      if (possibleDirs.Count > 0)
      {
        mainDir = possibleDirs[rnd.Next(possibleDirs.Count)];
        branchDirs.Add(mainDir.Value);
      }

      if (rnd.NextDouble() < 0.2)
      {
        List<Vector2Int> forkDirs = new List<Vector2Int>(cardinalDirections);
        forkDirs.RemoveAll(d =>
            (mainDir.HasValue && d == mainDir.Value) ||
            roomPositions.Contains(newRoomPos + d) ||
            frontierSet.Contains(newRoomPos + d)
        );
        int extraBranches = rnd.Next(1, 3);
        for (int i = 0; i < extraBranches && forkDirs.Count > 0; i++)
        {
          int forkIdx = rnd.Next(forkDirs.Count);
          Vector2Int forkDir = forkDirs[forkIdx];
          forkDirs.RemoveAt(forkIdx);
          branchDirs.Add(forkDir);
        }
      }

      roomPositions.Add(newRoomPos);
      worldGenerationController.roomLocations.Add(new KeyValuePair<int, int>(newRoomPos.x, newRoomPos.y));

      if (!roomEntrances.ContainsKey(newRoomPos))
        roomEntrances[newRoomPos] = new HashSet<Vector2Int>();
      if (prevDir.HasValue)
        roomEntrances[newRoomPos].Add(-prevDir.Value);

      foreach (var dir in cardinalDirections)
      {
        Vector2Int neighbor = newRoomPos + dir;
        if (roomPositions.Contains(neighbor))
        {
          roomEntrances[newRoomPos].Add(dir);
          if (!roomEntrances.ContainsKey(neighbor))
            roomEntrances[neighbor] = new HashSet<Vector2Int>();
          roomEntrances[neighbor].Add(-dir);
        }
      }

      foreach (var dir in branchDirs)
      {
        roomEntrances[newRoomPos].Add(dir);
      }

      foreach (var dir in branchDirs)
      {
        Vector2Int neighbor = newRoomPos + dir;
        if (!roomEntrances.ContainsKey(neighbor))
          roomEntrances[neighbor] = new HashSet<Vector2Int>();
        roomEntrances[neighbor].Add(-dir);

        if (!roomPositions.Contains(neighbor) && !frontierSet.Contains(neighbor))
        {
          frontier.Add((neighbor, dir));
          frontierSet.Add(neighbor);
        }
      }

      roomsCreated++;
    }

    // PRUNE: Remove entrances that do not connect to an actual placed room
    foreach (var kvp in roomEntrances)
    {
      Vector2Int roomPos = kvp.Key;
      var toRemove = new List<Vector2Int>();
      foreach (var dir in kvp.Value)
      {
        Vector2Int neighbor = roomPos + dir;
        if (!roomPositions.Contains(neighbor))
        {
          toRemove.Add(dir);
        }
      }
      foreach (var dir in toRemove)
      {
        kvp.Value.Remove(dir);
      }
    }

    // Instantiate all rooms (including start room) after pruning
    foreach (var roomPos in roomPositions)
    {
      bool isStartRoom = roomPos == startPos;
      // CreateRoomAt(roomPos, roomEntrances[roomPos], prefab);
    }
  }

  // Unified CreateRoomAt: accepts a prefab (optional), an isStartRoom flag and a
  // per-room disableEnemySpawnsForThisRoom flag. This replaces previous
  // overloaded/duplicate implementations so callers can consistently opt-out
  // static rooms from enemy spawning.
  void CreateRoomAt(Vector2Int gridPos, HashSet<Vector2Int> entrances, GameObject prefab, bool isStartRoom = false, bool disableEnemySpawnsForThisRoom = false)
  {
    // Fallback to mapContainer when prefab is null
    GameObject toInstantiate = prefab != null ? prefab : mapContainer;
    Vector3 worldPos = new Vector3(gridPos.x, gridPos.y, 0);
    GameObject roomObj = Instantiate(toInstantiate, worldPos, Quaternion.identity);

    // If this is the mapContainer prefab, unhide it (set active)
    if (toInstantiate == mapContainer && roomObj != null)
    {
      roomObj.SetActive(true);
    }

    MapGenerator mapGen = roomObj.GetComponentInChildren<MapGenerator>();
    if (mapGen == null)
    {
      Debug.LogError("MapGenerator component not found in room prefab!");
      return;
    }

    // Use biome based on Y
    mapGen.currentBiomeGenerator = GetBiomeForY(gridPos.y);

    mapGen.seed = System.Guid.NewGuid().ToString();
    mapGen.useRandomSeed = false;

    // Ensure the MapGenerator has a player reference for player-aware spawning
    if (this.player != null)
    {
      mapGen.player = this.player;
    }
    else
    {
      var pgo = GameObject.FindWithTag("Player");
      if (pgo != null)
        mapGen.player = pgo.transform;
    }

    // Apply world-level debug toggles to the MapGenerator instance. Also
    // respect static room opt-out which forces no enemy spawns for this room.
    try
    {
      mapGen.spawnDecorations = disableDecorationsGlobally;
      bool shouldSpawn = !disableEnemySpawnsGlobally;
      if (disableEnemySpawnsForThisRoom) shouldSpawn = false;
      mapGen.shouldSpawnEnemies = shouldSpawn;
    }
    catch (System.Exception ex)
    {
      Debug.LogWarning($"WorldGenerator: failed to apply debug toggles to MapGenerator at {gridPos}: {ex}");
    }

    // Convert Vector2Int directions to MapGenerator.EntranceDirection[]
    var entranceDirs = new List<MapGenerator.EntranceDirection>();
    foreach (var dir in entrances)
    {
      var entrance = DirectionToEntrance(dir);
      if (entrance != null)
        entranceDirs.Add(entrance.Value);
    }

    // Pass entrance directions to MapGenerator
    mapGen.SetEntrances(entranceDirs.ToArray());

    // Assign world-level id mapping for this room origin
    Vector2Int origin = new Vector2Int((int)worldPos.x, (int)worldPos.y);
    if (!roomOriginToId.ContainsKey(origin))
    {
      int assignedId = nextRoomId++;
      roomOriginToId[origin] = assignedId;
      roomIdToOrigin[assignedId] = origin;
      roomMapGens[origin] = mapGen;
      if (!roomAdjacency.ContainsKey(assignedId)) roomAdjacency[assignedId] = new HashSet<int>();
    }
    mapGen.worldRoomId = roomOriginToId[origin];

    // record adjacency based on entrances (will be filled mutually when neighbors are created)
    int thisId = mapGen.worldRoomId;
    if (!roomAdjacency.ContainsKey(thisId)) roomAdjacency[thisId] = new HashSet<int>();
    foreach (var dir in entrances)
    {
      Vector2Int neighborOrigin = origin + dir;
      if (roomOriginToId.ContainsKey(neighborOrigin))
      {
        int nid = roomOriginToId[neighborOrigin];
        roomAdjacency[thisId].Add(nid);
        if (!roomAdjacency.ContainsKey(nid)) roomAdjacency[nid] = new HashSet<int>();
        roomAdjacency[nid].Add(thisId);
      }
    }
  }

  // Helper to convert Vector2Int to MapGenerator.EntranceDirection
  MapGenerator.EntranceDirection? DirectionToEntrance(Vector2Int dir)
  {
    if (dir == new Vector2Int(0, roomHeight)) return MapGenerator.EntranceDirection.NORTH;
    if (dir == new Vector2Int(roomWidth, 0)) return MapGenerator.EntranceDirection.EAST;
    if (dir == new Vector2Int(0, -roomHeight)) return MapGenerator.EntranceDirection.SOUTH;
    if (dir == new Vector2Int(-roomWidth, 0)) return MapGenerator.EntranceDirection.WEST;
    return null;
  }
  void Update()
  {
    // Detect changes to debug toggles in the Inspector and reapply them immediately
    if (prevDisableDecorationsGlobally != disableDecorationsGlobally || prevDisableEnemySpawnsGlobally != disableEnemySpawnsGlobally)
    {
      prevDisableDecorationsGlobally = disableDecorationsGlobally;
      prevDisableEnemySpawnsGlobally = disableEnemySpawnsGlobally;
      ApplyGlobalDebugTogglesToAllRooms();
    }
    if (player == null)
    {
      var pgo = GameObject.FindWithTag("Player");
      if (pgo != null) player = pgo.transform;
      else return;
    }

    // Compute a snapped room origin near the player, then verify with a containment test
    int cellX = Mathf.FloorToInt(player.position.x / (float)roomWidth);
    int cellY = Mathf.FloorToInt(player.position.y / (float)roomHeight);
    Vector2Int baseOrigin = new Vector2Int(cellX * roomWidth, cellY * roomHeight);

    // Search nearby origins (3x3 grid) to handle edge cases where rounding or
    // passage carving might place the player near the border between rooms.
    Vector2Int foundOrigin = new Vector2Int(int.MinValue, int.MinValue);
    for (int dx = -1; dx <= 1 && foundOrigin.x == int.MinValue; dx++)
    {
      for (int dy = -1; dy <= 1 && foundOrigin.x == int.MinValue; dy++)
      {
        Vector2Int candidate = baseOrigin + new Vector2Int(dx * roomWidth, dy * roomHeight);
        if (roomMapGens.TryGetValue(candidate, out var mg))
        {
          if (mg.IsWorldPositionInsideRoom(player.position))
          {
            foundOrigin = candidate;
            break;
          }
        }
      }
    }

    Vector2Int playerRoomWorld = foundOrigin.x == int.MinValue ? baseOrigin : foundOrigin;

    if (playerRoomWorld != currentPlayerRoom)
    {
      // Clear previous decorations by origin
      var prev = new HashSet<Vector2Int>(activeDecorationRooms);
      foreach (var origin in prev)
      {
        if (roomMapGens.TryGetValue(origin, out var mg))
        {
          mg.ClearDecorations();
        }
      }
      activeDecorationRooms.Clear();

      // find the player room id
      if (!roomOriginToId.TryGetValue(playerRoomWorld, out int playerRoomId))
      {
        currentPlayerRoom = playerRoomWorld;
        return;
      }

      // BFS adjacency up to spawnRoomRadius to collect room ids to activate
      HashSet<int> idsToActivate = new HashSet<int>();
      Queue<(int id, int depth)> q = new Queue<(int, int)>();
      q.Enqueue((playerRoomId, 0));
      idsToActivate.Add(playerRoomId);
      while (q.Count > 0)
      {
        var (id, depth) = q.Dequeue();
        if (depth >= spawnRoomRadius) continue;
        if (!roomAdjacency.TryGetValue(id, out var neigh)) continue;
        foreach (var nid in neigh)
        {
          if (!idsToActivate.Contains(nid))
          {
            idsToActivate.Add(nid);
            q.Enqueue((nid, depth + 1));
          }
        }
      }

  // Debug: log which rooms (ids) we're about to activate
  Debug.Log($"WorldGenerator: playerRoomId={playerRoomId}, idsToActivate=[{string.Join(",", new List<int>(idsToActivate))}]");

  // Activate rooms: spawn decorations and track active origins
      // Capture player position once and defensively handle null
      Vector3 playerPos = Vector3.zero;
      if (player == null)
      {
        var pgo = GameObject.FindWithTag("Player");
        if (pgo != null) player = pgo.transform;
      }
      if (player != null) playerPos = player.position;
      else
      {
        Debug.LogWarning("WorldGenerator: player transform is null when activating decoration rooms");
      }

      // Show decorations only for activated rooms; hide previously active rooms
      foreach (var origin in activeDecorationRooms)
      {
        if (roomMapGens.TryGetValue(origin, out var oldMg))
        {
          try { oldMg.ShowDecorations(false); } catch (System.Exception ex) { Debug.LogError($"WorldGenerator: error hiding decorations for {origin}: {ex}"); }
        }
      }
      activeDecorationRooms.Clear();

      foreach (var id in idsToActivate)
      {
        if (!roomIdToOrigin.TryGetValue(id, out var origin))
        {
          Debug.LogWarning($"WorldGenerator: no origin for room id {id}");
          continue;
        }
        if (!roomMapGens.TryGetValue(origin, out var mg))
        {
          Debug.LogWarning($"WorldGenerator: no MapGenerator at origin {origin} for room id {id}");
          continue;
        }
        try
        {
          mg.ShowDecorations(true);
        }
        catch (System.Exception ex)
        {
          Debug.LogError($"WorldGenerator: error showing decorations for {origin}: {ex}");
        }
        activeDecorationRooms.Add(origin);
      }

      // Update the UI minimap to show the room the player is currently in (if available)
      if (roomMapGens.TryGetValue(playerRoomWorld, out var currentRoomMapGen))
      {
        try
        {
          currentRoomMapGen.SetMinimapToUI();
        }
        catch (System.Exception ex)
        {
          Debug.LogWarning($"WorldGenerator: failed to set minimap for room {playerRoomWorld}: {ex}");
        }
      }

      currentPlayerRoom = playerRoomWorld;
    }
  }

  // Apply current WorldGenerator debug toggles to all known MapGenerator instances.
  void ApplyGlobalDebugTogglesToAllRooms()
  {
    foreach (var kv in roomMapGens)
    {
      var origin = kv.Key;
      var mg = kv.Value;
      if (mg == null) continue;
      try
      {
        mg.spawnDecorations = disableDecorationsGlobally;
        mg.shouldSpawnEnemies = !disableEnemySpawnsGlobally;
        // update static globals for MapGenerator checks
        WorldGenerator.globalDisableDecorations = disableDecorationsGlobally;
        WorldGenerator.globalDisableEnemySpawns = disableEnemySpawnsGlobally;
        // If decorations are disabled, ensure any persistent decorations are hidden/cleared
        if (disableDecorationsGlobally)
        {
          try { mg.ClearDecorations(); } catch (System.Exception) { }
        }
        // If enemy spawns are disabled, remove already-spawned enemies under this room
        if (disableEnemySpawnsGlobally)
        {
          try { mg.ClearSpawnedEnemies(); } catch (System.Exception) { }
        }
      }
      catch (System.Exception ex)
      {
        Debug.LogWarning($"WorldGenerator: failed to apply debug toggles to MapGenerator at {origin}: {ex}");
      }
    }
  }

  // Rebuild adjacency by scanning neighbors of every known origin. This makes
  // adjacency robust when rooms (especially static rooms) were created in any order.
  void BuildAdjacencyFromOrigins()
  {
    roomAdjacency.Clear();
    foreach (var kv in roomOriginToId)
    {
      var origin = kv.Key;
      int id = kv.Value;
      if (!roomAdjacency.ContainsKey(id)) roomAdjacency[id] = new HashSet<int>();
      foreach (var dir in cardinalDirections)
      {
        var neighbor = origin + dir;
        if (roomOriginToId.TryGetValue(neighbor, out var nid))
        {
          roomAdjacency[id].Add(nid);
        }
      }
    }
    Debug.Log($"WorldGenerator: Built adjacency for {roomAdjacency.Count} rooms");
  }
}

/*

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class WorldGenerator : MonoBehaviour
{
    public GameObject mapContainer; // Prefab or container for rooms
    public GeneratorConfig biomeGenerator;
    public WorldGenerationController worldGenerationController;

    // Biome configs
    public GeneratorConfig forestConfig;
    public GeneratorConfig desertConfig;
    public GeneratorConfig tundraConfig;
    public int targetRooms = 10; // Number of rooms to generate

    // Room size (should match MapGenerator's width/height)
    public int roomWidth = 60;
    public int roomHeight = 60;

    // Directions: N, S, E, W
    private Vector2Int[] cardinalDirections;
    // For tracking which directions each room should have entrances
    private Dictionary<Vector2Int, HashSet<Vector2Int>> roomEntrances = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

    void Start()
    {
        cardinalDirections = new Vector2Int[] {
            new Vector2Int(0, roomHeight),    // Up
            new Vector2Int(0, -roomHeight),   // Down
            new Vector2Int(roomWidth, 0),     // Right
            new Vector2Int(-roomWidth, 0)     // Left
        };

        // Clear previous room locations
        if (worldGenerationController.roomLocations == null)
            worldGenerationController.roomLocations = new List<KeyValuePair<int, int>>();
        else
            worldGenerationController.roomLocations.Clear();

        // Start at (0,0)
        Vector2Int startPos = new Vector2Int(0, 0);
        worldGenerationController.roomLocations.Add(new KeyValuePair<int, int>(startPos.x, startPos.y));
        roomEntrances[startPos] = new HashSet<Vector2Int>();
  CreateRoomAt(startPos, roomEntrances[startPos], mapContainer, isStartRoom: true, disableEnemySpawnsForThisRoom: false);

        GenerateRoomLayout(startPos);
    }

    GeneratorConfig GetBiomeForY(int y)
    {
        if (y < -roomHeight) return desertConfig;
        if (y > roomHeight) return tundraConfig;
        return forestConfig;
    }

    void GenerateRoomLayout(Vector2Int startPos)
    {
        System.Random rnd = new System.Random();
        HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int> { startPos };
        List<(Vector2Int pos, Vector2Int? fromDir)> frontier = new List<(Vector2Int, Vector2Int?)>();
        HashSet<Vector2Int> frontierSet = new HashSet<Vector2Int>();

        // Add all cardinal neighbors of the start room to the frontier, with their direction
        foreach (var dir in cardinalDirections)
        {
            Vector2Int neighbor = startPos + dir;
            frontier.Add((neighbor, dir));
            frontierSet.Add(neighbor);
        }

        int roomsCreated = 1;
        while (roomsCreated < targetRooms && frontier.Count > 0)
        {
            // Pick a random position from the frontier
            int pickIdx = rnd.Next(frontier.Count);
            var (newRoomPos, prevDir) = frontier[pickIdx];
            frontier.RemoveAt(pickIdx);
            frontierSet.Remove(newRoomPos);

            // Only place if adjacent to an existing room
            bool isAdjacent = false;
            foreach (var dir in cardinalDirections)
            {
                if (roomPositions.Contains(newRoomPos + dir))
                {
                    isAdjacent = true;
                    break;
                }
            }
            if (!isAdjacent)
                continue;

            // Place the new room
            roomPositions.Add(newRoomPos);
            worldGenerationController.roomLocations.Add(new KeyValuePair<int, int>(newRoomPos.x, newRoomPos.y));

            // Track entrance direction for this room (from parent)
            if (!roomEntrances.ContainsKey(newRoomPos))
                roomEntrances[newRoomPos] = new HashSet<Vector2Int>();
            if (prevDir.HasValue)
                roomEntrances[newRoomPos].Add(-prevDir.Value);

            // Also, add the reverse direction to the parent room
            Vector2Int parent = newRoomPos + (prevDir.HasValue ? prevDir.Value : Vector2Int.zero);
            if (roomEntrances.ContainsKey(parent) && prevDir.HasValue)
                roomEntrances[parent].Add(prevDir.Value);

            CreateRoomAt(newRoomPos, roomEntrances[newRoomPos], mapContainer, isStartRoom: false, disableEnemySpawnsForThisRoom: false);
            roomsCreated++;

            // Build a weighted list of possible directions
            List<Vector2Int> possibleDirs = new List<Vector2Int>();
            foreach (var dir in cardinalDirections)
            {
                Vector2Int neighbor = newRoomPos + dir;
                if (!roomPositions.Contains(neighbor) && !frontierSet.Contains(neighbor))
                {
                    // Weight previous direction higher
                    if (prevDir.HasValue && dir == prevDir.Value)
                    {
                        // Add the previous direction multiple times for higher chance
                        possibleDirs.Add(dir);
                        possibleDirs.Add(dir);
                        possibleDirs.Add(dir); // 3x weight
                    }
                    possibleDirs.Add(dir); // 1x for all directions
                }
            }

            // Pick the main branch direction with bias
            if (possibleDirs.Count > 0)
            {
                var mainDir = possibleDirs[rnd.Next(possibleDirs.Count)];
                Vector2Int mainBranchNeighbor = newRoomPos + mainDir;
                frontier.Add((mainBranchNeighbor, mainDir));
                frontierSet.Add(mainBranchNeighbor);
            }

            // With a small chance, add 1–2 extra random neighbors (forks)
            if (rnd.NextDouble() < 0.2) // 20% chance to branch
            {
                List<Vector2Int> forkDirs = new List<Vector2Int>(cardinalDirections);
                // Remove the main direction if it was added
                forkDirs.RemoveAll(d => roomPositions.Contains(newRoomPos + d) || frontierSet.Contains(newRoomPos + d));
                int extraBranches = rnd.Next(1, 3); // 1 or 2 extra branches
                for (int i = 0; i < extraBranches && forkDirs.Count > 0; i++)
                {
                    int forkIdx = rnd.Next(forkDirs.Count);
                    Vector2Int forkDir = forkDirs[forkIdx];
                    forkDirs.RemoveAt(forkIdx);
                    Vector2Int neighbor = newRoomPos + forkDir;
                    frontier.Add((neighbor, forkDir));
                    frontierSet.Add(neighbor);
                }
            }
        }
    }



    // Helper to convert Vector2Int to MapGenerator.EntranceDirection
    MapGenerator.EntranceDirection? DirectionToEntrance(Vector2Int dir)
    {
        if (dir == new Vector2Int(0, roomHeight)) return MapGenerator.EntranceDirection.Up;
        if (dir == new Vector2Int(roomWidth, 0)) return MapGenerator.EntranceDirection.Right;
        if (dir == new Vector2Int(0, -roomHeight)) return MapGenerator.EntranceDirection.Down;
        if (dir == new Vector2Int(-roomWidth, 0)) return MapGenerator.EntranceDirection.Left;
        return null;
    }

    void Update()
    {
        // (Optional) Add runtime logic here
    }
}
*/