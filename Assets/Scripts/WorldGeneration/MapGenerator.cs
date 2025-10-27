using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MapGenerator : MonoBehaviour
{
  public enum EntranceDirection
  {
    NORTH,
    SOUTH,
    EAST,
    WEST,

    NONE
  }


  [Header("Water Generation")]
  [Tooltip("Divisor used to compute baseline water generation attempts: attempts = Clamp(width*height / divisor, minAttempts, maxAttempts)")]
  [SerializeField]
  private int waterAttemptsDivisor = 400;
  [SerializeField]
  private int waterAttemptsMin = 3;
  [SerializeField]
  private int waterAttemptsMax = 20;

  [Tooltip("Minimum cluster size for a water feature (puddles/rivers)")]
  [SerializeField]
  private int waterClusterMinSize = 3;
  [Tooltip("Maximum cluster size for a water feature (puddles/rivers)")]
  [SerializeField]
  private int waterClusterMaxSize = 16;

  [Tooltip("When true, decorative water is deterministic and seeded by the room seed. When false, each regeneration uses a fresh random seed (true rain accumulation).")]
  [SerializeField]
  private bool useDeterministicWater = true;

  [Range(0f, 1f)]
  [Tooltip("How often the cluster expansion will pick a random frontier cell instead of the FIFO front. Higher = more organic, less square shapes.")]
  [SerializeField]
  private float waterFillRandomness = 0.25f;


  public EntranceDirection direction;
  public EntranceDirection[] directions;
  public EntranceDirection playerStartingPosition;

  // Decorative water support
  public Tilemap wallTilemap, floorTilemap, shadowTilemap, waterTilemap;

  // (All water generation tuning fields removed)

  [HideInInspector]
  // Controlled by WorldGenerator: when false, MapGenerator will not spawn enemies.
  public bool shouldSpawnEnemies = true;

  public UnityEngine.Transform player;

  public GeneratorConfig currentBiomeGenerator;
  [HideInInspector]
  // Controlled by WorldGenerator: when true, MapGenerator will skip decoration generation.
  public bool spawnDecorations = false;
  [SerializeField]
  private int width;
  [SerializeField]
  private int height;
  [SerializeField]
  public string seed;
  [SerializeField]
  public bool useRandomSeed;
  [Range(0, 20)]
  private int smoothIterations = 1;
  [SerializeField]
  [Range(0, 100)]
  private int randomFillPercent;
  int[,] map;
  // Backup of the generated base map (walls/floors) so editor actions can restore original layout
  private int[,] baseMapBackup;
  [SerializeField]
  private int wallThresholdSize = 3;
  [SerializeField]
  private int roomThresholdSize = 28;
  [SerializeField]
  private int enemyChance;
  private enum Algorithm { WALK_TOP, WALK_TOP_SMOOTH, STANDARD };
  private Algorithm currentAlgorithm = Algorithm.STANDARD;
  private List<Vector2> renderedDestructables = new List<Vector2>();
  private List<Vector2> renderedSpawnGroups = new List<Vector2>();
  // Generated rooms and helper data for per-room decoration spawning
  private List<Room> generatedRooms = new List<Room>();
  private int[,] roomIdMap;
  private int currentPlayerRoomId = -1;
  // world-level id assigned by WorldGenerator (optional)
  public int worldRoomId = -1;
  private GameObject decorationContainer;
  private bool decorationsCreated = false;
  // Enemy spawn selection: choose one group template per room and only spawn it once
  private bool enemyGroupSpawned = false;
  private EnemySpawnGroup selectedEnemySpawnGroupTemplate = null;
  Texture2D minimap;
  public Image miniMapSprite;
  // Cached Sprite generated from the minimap texture for quick UI assignment
  private Sprite minimapSpriteAsset;

  void Start()
  {
    Init();
  }

  void Init()
  {
    GenerateMap();
    AddDirectionalPassage();
    // Save a copy of the base map (including directional passages) so we can restore it later
    if (map != null)
      baseMapBackup = (int[,])map.Clone();
    // Choose a single enemy spawn group for this room (if the biome provides any)
    if (currentBiomeGenerator != null && currentBiomeGenerator.enemySpawnGroups != null && currentBiomeGenerator.enemySpawnGroups.Length > 0)
    {
      int sel = UnityEngine.Random.Range(0, currentBiomeGenerator.enemySpawnGroups.Length);
      selectedEnemySpawnGroupTemplate = currentBiomeGenerator.enemySpawnGroups[sel];
      enemyGroupSpawned = false;
    }
    RenderMap(baseMapBackup ?? map, floorTilemap, wallTilemap, currentBiomeGenerator.wallTile, currentBiomeGenerator.groundTiles);
    // ensure player is on the map; if not, move to a valid floor tile
    currentPlayerRoomId = GetPlayerRoomId();
    if (currentPlayerRoomId == -1)
    {
      Debug.Log("[MapGenerator] Init: player not on map; leaving player in place (no auto-teleport).");
      // Intentionally do not teleport the player. Designer/player should be placed manually.
    }

    // spawn decorations for the player's current room immediately after rendering
    SpawnDecorationsForRoom(currentPlayerRoomId);
    // for (int x = 0; x < width * 2; x++)
    // {
    //   for (int y = 0; y < height * 2; y++)
    //   {
    //     backgroundWallTilemap.SetTile(new Vector3Int(x, y, 0), currentBiomeGenerator.wallTile);
    //   }
    // }
  }

  struct Coord
  {

    public int tileX;
    public int tileY;

    public Coord(int x, int y)
    {
      tileX = x;
      tileY = y;
    }
  }

  void AddDirectionalPassage()
  {
    if (directions.Contains(EntranceDirection.NORTH))
    {
      int xIndexNorth = (int)(width / 2 + .5f);
      // start from the top-most in-bounds tile so carving reaches the edge
      Coord startTile = new Coord(xIndexNorth, height - 1);
      Coord endTile = new Coord();
      for (int y = height - 1; y >= 0; y--)
      {
        if (map[xIndexNorth, y] == 0)
        {
          endTile = new Coord(xIndexNorth, y);
          break;
        }
        else continue;
      }
      List<Coord> line = GetLine(startTile, endTile);
      Debug.Log($"[MapGenerator] AddDirectionalPassage NORTH: start={startTile.tileX},{startTile.tileY} end={endTile.tileX},{endTile.tileY} lineCount={line.Count}");
      foreach (Coord c in line)
      {
        // Explicitly carve the map cells for the corridor width before drawing tiles
        for (int ox = -2; ox <= 2; ox++)
        {
          for (int oy = -2; oy <= 2; oy++)
          {
            if (ox * ox + oy * oy <= 4)
            {
              int cx = c.tileX + ox;
              int cy = c.tileY + oy;
              if (IsInMapRange(cx, cy)) map[cx, cy] = 0;
            }
          }
        }
        DrawCircle(c, 2);
      }
    }
    if (directions.Contains(EntranceDirection.SOUTH))
    {
      int xIndexSouth = (int)(width / 2 + .5f);
      Coord startTileSouth = new Coord(xIndexSouth, 0);
      Coord endTileSouth = new Coord();
      for (int y = 0; y < height; y++)
      {
        if (map[xIndexSouth, y] == 0)
        {
          endTileSouth = new Coord(xIndexSouth, y);
          break;
        }
        else continue;
      }
      List<Coord> lineSouth = GetLine(startTileSouth, endTileSouth);
      Debug.Log($"[MapGenerator] AddDirectionalPassage SOUTH: start={startTileSouth.tileX},{startTileSouth.tileY} end={endTileSouth.tileX},{endTileSouth.tileY} lineCount={lineSouth.Count}");
      foreach (Coord c in lineSouth)
      {
        for (int ox = -2; ox <= 2; ox++)
        {
          for (int oy = -2; oy <= 2; oy++)
          {
            if (ox * ox + oy * oy <= 4)
            {
              int cx = c.tileX + ox;
              int cy = c.tileY + oy;
              if (IsInMapRange(cx, cy)) map[cx, cy] = 0;
            }
          }
        }
        DrawCircle(c, 2);
      }
    }
    if (directions.Contains(EntranceDirection.WEST))
    {
      int yIndexWest = (int)(height / 2 + .5f);
      Coord startTileWest = new Coord(0, yIndexWest);
      Coord endTileWest = new Coord();
      for (int x = 0; x < width; x++)
      {
        if (map[x, yIndexWest] == 0)
        {
          endTileWest = new Coord(x, yIndexWest);
          break;
        }
        else continue;
      }
      List<Coord> lineWest = GetLine(startTileWest, endTileWest);
      Debug.Log($"[MapGenerator] AddDirectionalPassage WEST: start={startTileWest.tileX},{startTileWest.tileY} end={endTileWest.tileX},{endTileWest.tileY} lineCount={lineWest.Count}");
      foreach (Coord c in lineWest)
      {
        for (int ox = -2; ox <= 2; ox++)
        {
          for (int oy = -2; oy <= 2; oy++)
          {
            if (ox * ox + oy * oy <= 4)
            {
              int cx = c.tileX + ox;
              int cy = c.tileY + oy;
              if (IsInMapRange(cx, cy)) map[cx, cy] = 0;
            }
          }
        }
        DrawCircle(c, 2);
      }
    }
    if (directions.Contains(EntranceDirection.EAST))
    {

      int yIndexEast = (int)(height / 2 + .5f);
      // start from the right-most in-bounds tile so carving reaches the edge
      Coord startTileEast = new Coord(width - 1, yIndexEast);
      Coord endTileEast = new Coord();
      for (int x = width - 1; x >= 0; x--)
      {
        if (map[x, yIndexEast] == 0)
        {
          endTileEast = new Coord(x, yIndexEast);
          break;
        }
        else continue;
      }
      List<Coord> lineEast = GetLine(startTileEast, endTileEast);
      Debug.Log($"[MapGenerator] AddDirectionalPassage EAST: start={startTileEast.tileX},{startTileEast.tileY} end={endTileEast.tileX},{endTileEast.tileY} lineCount={lineEast.Count}");
      foreach (Coord c in lineEast)
      {
        for (int ox = -2; ox <= 2; ox++)
        {
          for (int oy = -2; oy <= 2; oy++)
          {
            if (ox * ox + oy * oy <= 4)
            {
              int cx = c.tileX + ox;
              int cy = c.tileY + oy;
              if (IsInMapRange(cx, cy)) map[cx, cy] = 0;
            }
          }
        }
        DrawCircle(c, 2);
      }
    }
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.BackQuote))
    {
      foreach (GameObject destructable in GameObject.FindGameObjectsWithTag("Destructable"))
      {
        Destroy(destructable.gameObject);
      }
      foreach (GameObject collider in GameObject.FindGameObjectsWithTag("Collider"))
      {
        Destroy(collider.gameObject);
      }
      foreach (GameObject barrel in GameObject.FindGameObjectsWithTag("Explodable"))
      {
        Destroy(barrel.gameObject);
      }
      foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
      {
        Destroy(enemy.gameObject);
      }
      renderedDestructables.Clear();
      renderedSpawnGroups.Clear();
      Init();
    }
    // Check if player moved rooms and spawn decorations for the active room only
    int newRoom = GetPlayerRoomId();
    if (newRoom != currentPlayerRoomId)
    {
      Debug.Log($"[MapGenerator] Player room changed: from={currentPlayerRoomId} to={newRoom}");
      currentPlayerRoomId = newRoom;
      SpawnDecorationsForRoom(currentPlayerRoomId);
    }
  }

  int GetPlayerRoomId()
  {
    if (roomIdMap == null)
    {
      Debug.Log("[MapGenerator] GetPlayerRoomId: roomIdMap is null");
      return -1;
    }

    if (player == null)
    {
      // try to auto-assign
      var pgo = GameObject.FindWithTag("Player");
      if (pgo != null)
      {
        player = pgo.transform;
        Debug.Log("[MapGenerator] GetPlayerRoomId: auto-assigned player transform from tag 'Player'");
      }
      else
      {
        Debug.Log("[MapGenerator] GetPlayerRoomId: player is null and no GameObject with tag 'Player' found");
        return -1;
      }
    }

    // Preferred: use the Tilemap API to convert world position to a cell that
    // lines up with our placed tiles. This handles overlay tiles (like shadows)
    // which live on a separate tilemap but share the same cell grid.
    Vector3 playerPos = player.position;
    Vector3Int cell = floorTilemap.WorldToCell(playerPos);

    // If there's no floor tile at the exact cell, but there is a decoration
    // (shadow) tile, treat that cell as valid. Also, do a small neighborhood
    // scan if both are missing (handles slight offsets).
    TileBase floorTile = floorTilemap.GetTile(cell);
    TileBase decorTile = shadowTilemap != null ? shadowTilemap.GetTile(cell) : null;

    if (floorTile == null && decorTile == null)
    {
      // scan a 3x3 around the player cell for a matching tile
      bool found = false;
      for (int ox = -1; ox <= 1 && !found; ox++)
      {
        for (int oy = -1; oy <= 1 && !found; oy++)
        {
          Vector3Int c2 = new Vector3Int(cell.x + ox, cell.y + oy, cell.z);
          if (!IsInMapRange(c2.x - Mathf.RoundToInt(transform.position.x), c2.y - Mathf.RoundToInt(transform.position.y))) continue;
          if (floorTilemap.GetTile(c2) != null || (shadowTilemap != null && shadowTilemap.GetTile(c2) != null))
          {
            cell = c2;
            found = true;
            break;
          }
        }
      }
      if (!found)
      {
        Debug.Log($"[MapGenerator] GetPlayerRoomId: no floor or decoration tile found near player at {playerPos}");
        return -1;
      }
    }

    // Convert the tilemap cell to local map indices (map tiles were placed at
    // world = transform.position + (x,y)). So subtract the room origin.
    Vector3 origin = transform.position;
    int originX = Mathf.RoundToInt(origin.x);
    int originY = Mathf.RoundToInt(origin.y);
    int px = cell.x - originX;
    int py = cell.y - originY;

    int rid = -1;
    if (px >= 0 && px < width && py >= 0 && py < height)
    {
      rid = roomIdMap[px, py];
    }
    Debug.Log($"[MapGenerator] GetPlayerRoomId: playerPos={playerPos}, cell={cell}, origin={origin}, px={px}, py={py}, roomId={rid}");
    return rid;
  }

  void ClearDecorationContainer()
  {
    if (decorationContainer != null)
    {
      if (decorationsCreated)
      {
        // For persistent decorations, just hide the container so we can ShowDecorations later
        decorationContainer.SetActive(false);
      }
      else
      {
        // Non-persistent path: destroy the container
        Destroy(decorationContainer);
        decorationContainer = null;
      }
    }
    // Only clear the runtime lists if we're not in persistent mode
    if (!decorationsCreated)
      renderedDestructables.Clear();
  }

  // Destroy enemy GameObjects that were spawned under this MapGenerator's transform.
  // This helps when a global toggle disables enemy spawning after rooms have
  // already been generated; we remove existing spawned enemies to respect the
  // global setting.
  public void ClearSpawnedEnemies()
  {
    try
    {
      var enemies = GameObject.FindGameObjectsWithTag("Enemy");
      foreach (var e in enemies)
      {
        if (e == null) continue;
        // If the enemy is a child (any depth) of this MapGenerator's root, destroy it
        if (e.transform.IsChildOf(this.transform))
        {
          Destroy(e);
        }
      }
    }
    catch (System.Exception ex)
    {
      Debug.LogWarning($"[MapGenerator] ClearSpawnedEnemies failed: {ex}");
    }
  }

  void SpawnDecorationsForRoom(int roomId, bool clearFirst = true)
  {
    if (spawnDecorations)
    {
      Debug.Log($"[MapGenerator] SpawnDecorationsForRoom: spawnDecorations is true, skipping spawn for room {roomId}.");
      return;
    }
    if (clearFirst) ClearDecorationContainer();
    if (decorationContainer == null)
    {
      decorationContainer = new GameObject("DecorationContainer");
      decorationContainer.transform.parent = transform;
    }

    // Diagnostics and safety checks: surface why we might early-return
    if (roomId < 0)
    {
      Debug.LogWarning($"[MapGenerator:{worldRoomId}] SpawnDecorationsForRoom: invalid roomId={roomId}");
      return;
    }
    if (generatedRooms == null)
    {
      Debug.LogWarning($"[MapGenerator:{worldRoomId}] SpawnDecorationsForRoom: generatedRooms is null");
      return;
    }
    if (roomId >= generatedRooms.Count)
    {
      Debug.LogWarning($"[MapGenerator:{worldRoomId}] SpawnDecorationsForRoom: roomId {roomId} >= generatedRooms.Count {generatedRooms.Count}");
      return;
    }
    if (currentBiomeGenerator == null)
    {
      Debug.LogWarning($"[MapGenerator:{worldRoomId}] SpawnDecorationsForRoom: currentBiomeGenerator is null");
      return;
    }

    System.Random pseudoRandom = new System.Random(seed.GetHashCode());
    SortedDictionary<string, int> itemDictionary = new SortedDictionary<string, int>();
    foreach (var item in currentBiomeGenerator.spawnItems) itemDictionary.Add(item.item.name, 0);
    // Build a small set of rooms to spawn: the current room and its adjacent rooms
    List<Room> roomsToSpawn = new List<Room>();
    Room baseRoom = generatedRooms[roomId];
    roomsToSpawn.Add(baseRoom);
    Debug.Log($"[MapGenerator] SpawnDecorationsForRoom: baseRoomId={roomId}, baseSize={baseRoom.roomSize}, connectedCount={baseRoom.connectedRooms.Count}");

    // Find neighboring room IDs by scanning a perimeter around the room's bounding box.
    // This catches rooms separated by narrow corridors or 1-cell gaps that edgeTiles neighbor-check misses.
    HashSet<int> neighborIds = new HashSet<int>();
    int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
    foreach (Coord ct in baseRoom.tiles)
    {
      if (ct.tileX < minX) minX = ct.tileX;
      if (ct.tileX > maxX) maxX = ct.tileX;
      if (ct.tileY < minY) minY = ct.tileY;
      if (ct.tileY > maxY) maxY = ct.tileY;
    }

    // scan a 1-cell perimeter outside the bounding box (expand by 1)
    int scanMinX = Mathf.Max(0, minX - 1);
    int scanMaxX = Mathf.Min(width - 1, maxX + 1);
    int scanMinY = Mathf.Max(0, minY - 1);
    int scanMaxY = Mathf.Min(height - 1, maxY + 1);

    for (int sx = scanMinX; sx <= scanMaxX; sx++)
    {
      for (int sy = scanMinY; sy <= scanMaxY; sy++)
      {
        // only consider perimeter
        if (sx > minX && sx < maxX && sy > minY && sy < maxY) continue;
        int rid = roomIdMap[sx, sy];
        if (rid != -1 && rid != roomId) neighborIds.Add(rid);
      }
    }

    foreach (int nid in neighborIds)
    {
      if (nid >= 0 && nid < generatedRooms.Count)
      {
        roomsToSpawn.Add(generatedRooms[nid]);
        Debug.Log($"[MapGenerator] SpawnDecorationsForRoom: adding neighbor room {nid} (size={generatedRooms[nid].roomSize})");
      }
    }

    // Iterate each room's tiles and spawn decorations
    foreach (Room room in roomsToSpawn)
    {
      int tilesScanned = 0;
      int grassCandidates = 0;
      int itemsSpawned = 0;
      int skippedByChance = 0;
      int skippedByProximity = 0;
      int skippedByWallProximity = 0;
      int skippedByMax = 0;

      foreach (Coord t in room.tiles)
      {
        int x = t.tileX;
        int y = t.tileY;
        if (map[x, y] != 0) continue;
        tilesScanned++;

        int randomNumber = UnityEngine.Random.Range(0, 100);
        float scale = UnityEngine.Random.Range(1, 1.1f);

        var floorTile = floorTilemap.GetTile(new Vector3Int(x, y, 0));
        if (floorTile != null && floorTile.name == currentBiomeGenerator.grassSpawnTile.name)
        {
          grassCandidates++;
          Vector3 worldPos = transform.TransformPoint(new Vector3(x, y, 0));
          GameObject newItem = Instantiate(currentBiomeGenerator.grassItem.item, worldPos, Quaternion.identity, decorationContainer.transform);
          newItem.transform.localScale = new Vector3(scale, scale, 1);
          renderedDestructables.Add(new Vector2(x, y));
          itemsSpawned++;
          Debug.Log($"[MapGenerator] Instantiated grass '{currentBiomeGenerator.grassItem.item.name}' at {worldPos} for room spawn (roomId={roomId})");
        }

        foreach (var item in currentBiomeGenerator.spawnItems)
        {
          bool passedChance = pseudoRandom.Next(0, 100) < item.chance;
          bool passedProximity = ObjectAreClearFromOtherObjects(renderedDestructables, x, y, item.minRadius);
          bool passedWallClear = ObjectsAreClearFromWalls(x, y, 3);
          bool underMax = itemDictionary[item.item.name] < item.max;

          if (!passedChance) { skippedByChance++; continue; }
          if (!passedProximity) { skippedByProximity++; continue; }
          if (!passedWallClear) { skippedByWallProximity++; continue; }
          if (!underMax) { skippedByMax++; continue; }

          Vector3 worldPos = transform.TransformPoint(new Vector3(x, y, 0));
          GameObject newItem = Instantiate(item.item, worldPos, Quaternion.identity, decorationContainer.transform);
          newItem.transform.localScale = new Vector3(scale, scale, 1);
          renderedDestructables.Add(new Vector2(x, y));
          itemDictionary[item.item.name] = itemDictionary[item.item.name] + 1;
          itemsSpawned++;
          Debug.Log($"[MapGenerator] Instantiated '{item.item.name}' at {worldPos} for room spawn (roomId={roomId})");
        }
      }

      Debug.Log($"[MapGenerator] Spawn diagnostics for room (size={room.roomSize}): tilesScanned={tilesScanned}, grassCandidates={grassCandidates}, itemsSpawned={itemsSpawned}, skippedChance={skippedByChance}, skippedProximity={skippedByProximity}, skippedWall={skippedByWallProximity}, skippedMax={skippedByMax}");
    }
  }

  // Spawn all decorations for every generated room in this MapGenerator without
  // clearing between rooms. Useful when the world manager wants the entire
  // room filled (e.g., neighbor rooms).
  public void SpawnAllDecorationsInMap()
  {
    if (spawnDecorations)
    {
      Debug.Log("[MapGenerator] SpawnAllDecorationsInMap: spawnDecorations is true, skipping decoration spawn.");
      return;
    }
    // Create the decoration container and spawn decorations for all rooms.
    // This method is idempotent for repeated calls (it will recreate the
    // container each time). For persistent pre-creation use
    // SpawnAllDecorationsPersistent().
    ClearDecorationContainer();
    decorationContainer = new GameObject("DecorationContainer");
    decorationContainer.transform.parent = transform;

    if (generatedRooms == null || generatedRooms.Count == 0) return;

    for (int i = 0; i < generatedRooms.Count; i++)
    {
      // call internal spawner with clearFirst=false so it appends into the same container
      SpawnDecorationsForRoom(i, false);
    }
    decorationsCreated = true;
  }

  // Create decorations once and keep them in a persistent container. Safe to call
  // from the world manager after rooms are instantiated.
  public void SpawnAllDecorationsPersistent()
  {
    if (decorationsCreated) return;
    SpawnAllDecorationsInMap();
    decorationsCreated = true;
    if (decorationContainer != null)
      decorationContainer.SetActive(false); // start hidden
  }

  // Show or hide the decorations container for this map.
  public void ShowDecorations(bool visible)
  {
    if (decorationContainer == null)
    {
      // If decorations haven't been created yet, create them now (persistent)
      SpawnAllDecorationsPersistent();
    }
    if (decorationContainer != null)
      decorationContainer.SetActive(visible);
    // Note: shadow tiles live on `shadowTilemap` and are intentionally always
    // visible and rebuilt on RenderMap. Only the GameObject decoration
    // container is toggled here.
  }

  // Public wrapper so an external manager (WorldGenerator) can request spawning
  public void SpawnDecorationsAroundPlayer()
  {
    int rid = GetPlayerRoomId();
    SpawnDecorationsForRoom(rid);
  }

  // Spawn decorations for the room that contains the given world position.
  // This is a player-position-independent API so the world manager can call
  // it even if the MapGenerator.player reference is not set.
  public void SpawnDecorationsAtWorldPosition(Vector3 worldPos)
  {
    // Convert worldPos to a local cell and map indices using the Tilemap
    Vector3Int cell = floorTilemap.WorldToCell(worldPos);
    Vector3 origin = transform.position;
    int originX = Mathf.RoundToInt(origin.x);
    int originY = Mathf.RoundToInt(origin.y);
    int px = cell.x - originX;
    int py = cell.y - originY;

    int rid = -1;
    if (px >= 0 && px < width && py >= 0 && py < height)
    {
      rid = roomIdMap[px, py];
    }
    // Fallback: if no rid, try neighbor cells 3x3
    if (rid == -1)
    {
      for (int ox = -1; ox <= 1 && rid == -1; ox++)
      {
        for (int oy = -1; oy <= 1 && rid == -1; oy++)
        {
          int nx = px + ox;
          int ny = py + oy;
          if (nx >= 0 && nx < width && ny >= 0 && ny < height)
            rid = roomIdMap[nx, ny];
        }
      }
    }

    SpawnDecorationsForRoom(rid);
  }

  // Public wrapper to clear decorations from this map (used by external manager)
  public void ClearDecorations()
  {
    ClearDecorationContainer();
  }

  // Return true if the given world position lies within this MapGenerator's
  // tile bounds. This is used by WorldGenerator to robustly determine which
  // room the player is inside (avoids relying on rounding alone).
  public bool IsWorldPositionInsideRoom(Vector3 worldPos)
  {
    // The MapGenerator is instantiated at the room's world origin (transform.position)
    Vector3 origin = transform.position;
    float localX = worldPos.x - origin.x;
    float localY = worldPos.y - origin.y;
    int px = Mathf.FloorToInt(localX);
    int py = Mathf.FloorToInt(localY);
    return (px >= 0 && px < width && py >= 0 && py < height);
  }

  public int[,] runAlgorithm()
  {
    if (currentAlgorithm == Algorithm.WALK_TOP) return RandomWalkTop(map, seed);
    else if (currentAlgorithm == Algorithm.WALK_TOP_SMOOTH) return RandomWalkTopSmoothed(map, seed, 2);
    return map;
  }

  void MovePlayer()
  {
    // Intentionally left blank to avoid auto-teleporting the player during map generation.
    // Keep method for editor/debug convenience if explicit teleportation is desired later.
  }

  bool IsInMapRange(int x, int y)
  {
    return x >= 0 && x < width && y >= 0 && y < height;
  }

  List<List<Coord>> GetRegions(int tileType)
  {
    List<List<Coord>> regions = new List<List<Coord>>();
    int[,] mapFlags = new int[width, height];

    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
        {
          List<Coord> newRegion = GetRegionTiles(x, y);
          regions.Add(newRegion);

          foreach (Coord tile in newRegion)
          {
            mapFlags[tile.tileX, tile.tileY] = 1;
          }
        }
      }
    }
    return regions;
  }

  void ProcessMap()
  {

    List<List<Coord>> wallRegions = GetRegions(1);

    foreach (List<Coord> wallRegion in wallRegions)
    {
      if (wallRegion.Count < wallThresholdSize)
      {
        foreach (Coord tile in wallRegion)
        {
          map[tile.tileX, tile.tileY] = 0;
        }
      }
    }

    List<List<Coord>> roomRegions = GetRegions(0);
    List<Room> survivingRooms = new List<Room>();

    foreach (List<Coord> roomRegion in roomRegions)
    {
      if (roomRegion.Count < roomThresholdSize)
      {
        foreach (Coord tile in roomRegion)
        {
          map[tile.tileX, tile.tileY] = 1;
        }
      }
      else
      {
        survivingRooms.Add(new Room(roomRegion, map));
      }
    }
    survivingRooms.Sort();
    if (survivingRooms.Count > 0)
    {
      survivingRooms[0].isMainRoom = true;
      survivingRooms[0].isAccessibleFromMainRoom = true;
    }

    // store generated rooms and build a roomId map for per-room decoration spawning
    generatedRooms = survivingRooms;
    roomIdMap = new int[width, height];
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        roomIdMap[x, y] = -1;
      }
    }
    for (int i = 0; i < generatedRooms.Count; i++)
    {
      foreach (Coord t in generatedRooms[i].tiles)
      {
        roomIdMap[t.tileX, t.tileY] = i;
      }
    }

    ConnectClosestRooms(survivingRooms);

    // After connecting rooms (which may carve passages into the map), recompute regions
    // so roomIdMap reflects the final map layout (including created passages).
    List<List<Coord>> finalRoomRegions = GetRegions(0);
    List<Room> finalRooms = new List<Room>();
    foreach (List<Coord> roomRegion in finalRoomRegions)
    {
      if (roomRegion.Count < roomThresholdSize)
      {
        // small regions are ignored (they become walls)
        continue;
      }
      finalRooms.Add(new Room(roomRegion, map));
    }
    finalRooms.Sort();
    if (finalRooms.Count > 0)
    {
      finalRooms[0].isMainRoom = true;
      finalRooms[0].isAccessibleFromMainRoom = true;
    }

    generatedRooms = finalRooms;

    // Build roomIdMap from finalRooms
    roomIdMap = new int[width, height];
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        roomIdMap[x, y] = -1;
      }
    }
    for (int i = 0; i < generatedRooms.Count; i++)
    {
      foreach (Coord t in generatedRooms[i].tiles)
      {
        roomIdMap[t.tileX, t.tileY] = i;
      }
    }

    // Debug: log room connectivity for diagnosis
    try
    {
      Debug.Log($"[MapGenerator] ProcessMap: final generatedRooms={generatedRooms.Count}");
      for (int i = 0; i < generatedRooms.Count; i++)
      {
        var r = generatedRooms[i];
        Debug.Log($"[MapGenerator] Room {i}: size={r.roomSize}, connected={r.connectedRooms.Count}");
      }
    }
    catch (Exception ex)
    {
      Debug.Log("[MapGenerator] Error logging room connectivity: " + ex.Message);
    }
  }

  void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
  {
    List<Room> roomListA = new List<Room>();
    List<Room> roomListB = new List<Room>();

    if (forceAccessibilityFromMainRoom)
    {
      foreach (Room room in allRooms)
      {
        if (room.isAccessibleFromMainRoom)
        {
          roomListB.Add(room);
        }
        else
        {
          roomListA.Add(room);
        }
      }
    }
    else
    {
      roomListA = allRooms;
      roomListB = allRooms;
    }
    int bestDistance = 0;
    Coord bestTileA = new Coord();
    Coord bestTileB = new Coord();
    Room bestRoomA = new Room();
    Room bestRoomB = new Room();
    bool possibleConnectionFound = false;

    foreach (Room roomA in roomListA)
    {
      if (!forceAccessibilityFromMainRoom)
      {
        possibleConnectionFound = false;
        if (roomA.connectedRooms.Count > 0)
        {
          continue;
        }
      }

      foreach (Room roomB in roomListB)
      {
        if (roomA == roomB || roomA.IsConnected(roomB)) continue;

        for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
        {
          for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
          {
            Coord tileA = roomA.edgeTiles[tileIndexA];
            Coord tileB = roomB.edgeTiles[tileIndexB];
            int distanceBetweenRooms = (int)((tileA.tileX - tileB.tileX) * (tileA.tileX - tileB.tileX)) + ((tileA.tileY - tileB.tileY) * (tileA.tileY - tileB.tileY));

            if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
            {
              bestDistance = distanceBetweenRooms;
              possibleConnectionFound = true;
              bestTileA = tileA;
              bestTileB = tileB;
              bestRoomA = roomA;
              bestRoomB = roomB;
            }
          }
        }
      }
      if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
      {
        CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
      }
    }

    if (possibleConnectionFound && forceAccessibilityFromMainRoom)
    {
      CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
      ConnectClosestRooms(allRooms, true);
    }
    if (!forceAccessibilityFromMainRoom)
    {
      ConnectClosestRooms(allRooms, true);
    }
  }

  void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
  {
    Room.ConnectRooms(roomA, roomB);

    List<Coord> line = GetLine(tileA, tileB);
    foreach (Coord c in line)
    {
      // Carve the main path explicitly in the map so roomId detection and tile checks
      // recognize the passage even if DrawCircle later overlays tiles.
      if (IsInMapRange(c.tileX, c.tileY))
        map[c.tileX, c.tileY] = 0;
      DrawCircle(c, 2);
    }
  }

  void DrawCircle(Coord c, int r)
  {
    for (int x = -r; x <= r; x++)
    {
      for (int y = -r; y <= r; y++)
      {
        if (x * x + y * y <= r * r)
        {
          int drawX = c.tileX + x;
          int drawY = c.tileY + y;
          if (IsInMapRange(drawX, drawY))
          {
            map[drawX, drawY] = 0;
          }
        }
      }
    }
  }

  List<Coord> GetLine(Coord from, Coord to)
  {
    List<Coord> line = new List<Coord>();
    int x = from.tileX;
    int y = from.tileY;

    int dx = to.tileX - from.tileX;
    int dy = to.tileY - from.tileY;

    bool inverted = false;
    int step = Math.Sign(dx);
    int gradientStep = Math.Sign(dy);

    int longest = Mathf.Abs(dx);
    int shortest = Mathf.Abs(dy);

    if (longest < shortest)
    {
      inverted = true;
      longest = Mathf.Abs(dy);
      shortest = Mathf.Abs(dx);
      step = Math.Sign(dy);
      gradientStep = Math.Sign(dx);
    }

    int gradientAccumulation = longest / 2;
    for (int i = 0; i < longest; i++)
    {
      line.Add(new Coord(x, y));

      if (inverted)
      {
        y += step;
      }
      else
      {
        x += step;
      }
      gradientAccumulation += shortest;
      if (gradientAccumulation >= longest)
      {
        if (inverted)
        {
          x += gradientStep;
        }
        else
        {
          y += gradientStep;
        }
        gradientAccumulation -= longest;
      }
    }
    // Ensure the final destination is included so drawing operations reach the endpoint
    if (line.Count == 0 || line[line.Count - 1].tileX != to.tileX || line[line.Count - 1].tileY != to.tileY)
    {
      line.Add(new Coord(to.tileX, to.tileY));
    }
    return line;
  }

  List<Coord> GetRegionTiles(int startX, int startY)
  {
    List<Coord> tiles = new List<Coord>();
    int[,] mapFlags = new int[width, height];
    int tileType = map[startX, startY];

    Queue<Coord> queue = new Queue<Coord>();
    queue.Enqueue(new Coord(startX, startY));
    mapFlags[startX, startY] = 1;

    while (queue.Count > 0)
    {
      Coord tile = queue.Dequeue();
      tiles.Add(tile);

      for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
      {
        for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
        {
          if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
          {
            if (mapFlags[x, y] == 0 && map[x, y] == tileType)
            {
              mapFlags[x, y] = 1;
              queue.Enqueue(new Coord(x, y));
            }
          }
        }
      }
    }
    return tiles;
  }

  public int[,] RandomWalkTop(int[,] map, string seed)
  {
    //Seed our random
    System.Random rand = new System.Random(seed.GetHashCode());

    //Set our starting height
    int lastHeight = UnityEngine.Random.Range(0, map.GetUpperBound(1));

    //Cycle through our width
    for (int x = 0; x < map.GetUpperBound(0); x++)
    {
      //Flip a coin
      int nextMove = rand.Next(2);

      //If heads, and we aren't near the bottom, minus some height
      if (nextMove == 0 && lastHeight > 2)
      {
        lastHeight--;
      }
      //If tails, and we aren't near the top, add some height
      else if (nextMove == 1 && lastHeight < map.GetUpperBound(1) - 2)
      {
        lastHeight++;
      }

      //Circle through from the lastheight to the bottom
      for (int y = lastHeight; y >= 0; y--)
      {
        map[x, y] = 1;
      }
    }
    //Return the map
    return map;
  }

  public void SetEntrances(EntranceDirection[] dirs)
  {
    directions = dirs;
  }

  public int[,] RandomWalkTopSmoothed(int[,] map, string seed, int minSectionWidth)
  {
    //Seed our random
    System.Random rand = new System.Random(seed.GetHashCode());

    //Determine the start position
    int lastHeight = UnityEngine.Random.Range(0, map.GetUpperBound(1));

    //Used to determine which direction to go
    int nextMove = 0;
    //Used to keep track of the current sections width
    int sectionWidth = 0;

    //Work through the array width
    for (int x = 0; x <= map.GetUpperBound(0); x++)
    {
      //Determine the next move
      nextMove = rand.Next(2);

      //Only change the height if we have used the current height more than the minimum required section width
      if (nextMove == 0 && lastHeight > 0 && sectionWidth > minSectionWidth)
      {
        lastHeight--;
        sectionWidth = 0;
      }
      else if (nextMove == 1 && lastHeight < map.GetUpperBound(1) && sectionWidth > minSectionWidth)
      {
        lastHeight++;
        sectionWidth = 0;
      }
      //Increment the section width
      sectionWidth++;

      //Work our way from the height down to 0
      for (int y = lastHeight; y >= 0; y--)
      {
        map[x, y] = 1;
      }
    }

    //Return the modified map
    return map;
  }


  void GenerateMap()
  {
    map = new int[width, height];
    RandomFillMap();

    for (int i = 0; i < smoothIterations; i++)
    {
      SmoothMap();
    }

    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        if (x < 10 || x > width - 10 || y < 10 || y > height - 10)
        {
          map[x, y] = 1;
        }
      }
    }

    ProcessMap();
  }

  Vector2 GetPositionAroundObject(Vector2Int position, int radius)
  {
    Vector2 offset = UnityEngine.Random.insideUnitCircle * radius;
    Vector2 pos = position + offset;
    return pos;
  }

  public void RenderMap(int[,] map, Tilemap floorTilemap, Tilemap wallTilemap, TileBase wallTile, TileBase[] groundTiles)
  {
    //Clear the map (ensures we dont overlap)
    floorTilemap.ClearAllTiles();
    wallTilemap.ClearAllTiles();
    // Shadow tilemap is part of core tile rendering and should always be rebuilt
    if (shadowTilemap != null) shadowTilemap.ClearAllTiles();
    // Water decorative tilemap is decorative; clear it so we can repaint decorative rivers/puddles
    if (waterTilemap != null) waterTilemap.ClearAllTiles();
    // GameObject decorations are handled via the decoration container and
    // `spawnDecorations` / global toggles; tilemap shadows live in
    // `shadowTilemap` and are cleared above.
    //Loop through the width of the map
    System.Random pseudoRandom = new System.Random(seed.GetHashCode());
    GameObject colliderContainer = new GameObject();
    // store on the instance so other systems can reference it
    minimap = new Texture2D(width, height);
    minimap.filterMode = FilterMode.Point;
    minimap.wrapMode = TextureWrapMode.Clamp;
    SortedDictionary<string, int> itemDictionary = new SortedDictionary<string, int>();
    foreach (var item in currentBiomeGenerator.spawnItems)
    {
      itemDictionary.Add(item.item.name, 0);
    }
    for (int x = 0; x < width; x++)
    {
      //Loop through the height of the map
      for (int y = 0; y < height; y++)
      {
        int neighbourWallTiles = GetSurroundingWallCount(x, y);
        // 1 = tile, 0 = no tile
        if (map[x, y] == 1)
        {
          minimap.SetPixel(x, y, new Color32(13, 42, 81, 255));


          wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);

          // Determine whether decoration placement is allowed for this MapGenerator
          bool decorationsEnabled = !spawnDecorations && !WorldGenerator.globalDisableDecorations;

          // Safe-get the tile instance
          TileBase placedWallTile = wallTilemap.GetTile(new Vector3Int(x, y, 0));
          TileBase needShadowA = currentBiomeGenerator?.wallTileThatNeedsShadow;
          TileBase needShadowB = currentBiomeGenerator?.wallTileThatNeedsShadow2;

          // Compare by TileBase instance where possible, but fall back to sprite
          // comparison to handle editor-imported tiles that may be different
          // instances but share the same sprite asset.
          bool instanceMatch = (placedWallTile != null) && (placedWallTile == needShadowA || placedWallTile == needShadowB);
          Sprite placedSprite = wallTilemap.GetSprite(new Vector3Int(x, y, 0));
          Sprite needASprite = (needShadowA as Tile)?.sprite;
          Sprite needBSprite = (needShadowB as Tile)?.sprite;
          bool spriteMatch = (placedSprite != null) && (placedSprite == needASprite || placedSprite == needBSprite);

          bool wallNeedsShadow = decorationsEnabled && (instanceMatch || spriteMatch);

          // If the tile should form a cliff (tile below is floor and above is wall), place the cliff tile and a shadow below
          if (wallNeedsShadow && IsInMapRange(x, y - 1) && map[x, y - 1] == 0 && IsInMapRange(x, y + 1) && map[x, y + 1] != 0)
          {
            wallTilemap.SetTile(new Vector3Int(x, y, 0), currentBiomeGenerator.wallTileCliff);
            if (shadowTilemap != null && currentBiomeGenerator.wallTileShadow != null)
              shadowTilemap.SetTile(new Vector3Int(x, y - 1, 0), currentBiomeGenerator.wallTileShadow);
          }

          // If the tile should be converted to floor (sandwiched between floors), remove the wall and place a floor tile
          if (wallNeedsShadow && IsInMapRange(x, y - 1) && map[x, y - 1] == 0 && IsInMapRange(x, y + 1) && map[x, y + 1] == 0)
          {
            wallTilemap.SetTile(new Vector3Int(x, y, 0), null);
            floorTilemap.SetTile(new Vector3Int(x, y, 0), groundTiles[0]);
          }

          if (neighbourWallTiles == 0)
          {
            wallTilemap.SetTile(new Vector3Int(x, y, 0), groundTiles[0]);
          }

        }
        else
        {
          minimap.SetPixel(x, y, new Color32(54, 94, 150, 255));
          int randomNumber = UnityEngine.Random.Range(0, 100);
          float scale = UnityEngine.Random.Range(1, 1.1f);
          floorTilemap.SetTile(new Vector3Int(x, y, 0), groundTiles[pseudoRandom.Next(1, groundTiles.Length)]);
          // GameObject decorations (grass, destructables, spawn items) are deferred to per-room spawning.
          // This keeps RenderMap focused on tilemap rendering and minimap generation.
          //if (currentBiomeGenerator.tree != null && pseudoRandom.Next(0, 100) < currentBiomeGenerator.treeChance &&
          //    ObjectAreClearFromOtherObjects(renderedDestructables, x, y, 5) &&
          //    ObjectsAreClearFromWalls(x, y, 3))
          //{
          //  GameObject newTree = Instantiate(currentBiomeGenerator.tree, new Vector3Int(x, y, 0), new Quaternion(0, 0, 0, 0));
          //  newTree.transform.localScale = new Vector3(scale, scale, 1);
          //  renderedDestructables.Add(new Vector2(x, y));
          //  randomNumber = UnityEngine.Random.Range(0, 100);
          //}
          //if (currentBiomeGenerator.bush != null && pseudoRandom.Next(0, 100) < currentBiomeGenerator.bushChance &&
          //    ObjectAreClearFromOtherObjects(renderedDestructables, x, y, 3) &&
          //    ObjectsAreClearFromWalls(x, y, 3))
          //{
          //  GameObject newbush = Instantiate(currentBiomeGenerator.bush, new Vector3Int(x, y, 0), new Quaternion(0, 0, 0, 0));
          //  newbush.transform.localScale = new Vector3(scale, scale, 1);
          //  renderedDestructables.Add(new Vector2(x, y));
          //  randomNumber = UnityEngine.Random.Range(0, 100);

          //}
          //if (currentBiomeGenerator.chest != null && pseudoRandom.Next(0, 100) < currentBiomeGenerator.chestChance &&
          //    ObjectAreClearFromOtherObjects(renderedDestructables, x, y, 5) &&
          //    ObjectsAreClearFromWalls(x, y, 3) &&
          //    numberOfChests < currentBiomeGenerator.maxChests)
          //{
          //  GameObject newBarrel = Instantiate(currentBiomeGenerator.chest, new Vector3Int(x, y, 0), new Quaternion(0, 0, 0, 0));
          //  renderedDestructables.Add(new Vector2(x, y));
          //  randomNumber = UnityEngine.Random.Range(0, 100);
          //  minimap.SetPixel(x, y, new Color32(235, 255, 54, 255));
          //  numberOfChests++;
          //}
          if (shouldSpawnEnemies && !WorldGenerator.globalDisableEnemySpawns && randomNumber < enemyChance &&
                  ObjectAreClearFromOtherObjects(renderedSpawnGroups, x, y, 10) &&
                  ObjectsAreClearFromWalls(x, y, 5))
          {

            // Instantiate an EnemySpawnGroup asset so it has its own runtime data,
            // assign the spawn location and wallMap, then spawn the enemies.
            // Use the per-room selected template if available; spawn only once per room
            if (!enemyGroupSpawned && selectedEnemySpawnGroupTemplate != null)
            {
              EnemySpawnGroup runtimeGroup = Instantiate(selectedEnemySpawnGroupTemplate);
              runtimeGroup.spawnLocation = new Vector2Int(x, y);
              runtimeGroup.wallMap = map;
              // Convert local tile coordinates to world-space position using this MapGenerator's transform
              Vector3 worldTilePos = transform.TransformPoint(new Vector3(x, y, 0));
              runtimeGroup.spawnEnemiesAtWorldPosition(new Vector2(worldTilePos.x, worldTilePos.y), transform);
              renderedSpawnGroups.Add(new Vector2(x, y));
              minimap.SetPixel(x, y, new Color32(86, 255, 85, 255));
              enemyGroupSpawned = true;
              randomNumber = UnityEngine.Random.Range(0, 100);
            }
          }
        }
      }
    }
    minimap.Apply();
    // Decorative water generation (puddles/rivers) - deterministic per-seed
    try
    {
      System.Random genRandom = new System.Random(seed.GetHashCode());
      GenerateDecorativeWater(genRandom, groundTiles);
    }
    catch (Exception ex)
    {
      Debug.LogWarning($"[MapGenerator] GenerateDecorativeWater failed: {ex}");
    }
    try
    {
      minimapSpriteAsset = Sprite.Create(minimap, new Rect(0, 0, width, height), Vector2.zero, 100);
      if (miniMapSprite != null)
        miniMapSprite.sprite = minimapSpriteAsset;
    }
    catch (Exception ex)
    {
      Debug.LogWarning("[MapGenerator] Failed to create minimap sprite: " + ex.Message);
    }
  }

  // Make this room's minimap the active UI sprite (if UI Image assigned)
  public void SetMinimapToUI()
  {
    if (miniMapSprite == null)
    {
      // Nothing to set
      return;
    }
    if (minimapSpriteAsset != null)
    {
      miniMapSprite.sprite = minimapSpriteAsset;
    }
    else
    {
      // If sprite not yet generated, try to create it from texture
      if (minimap != null)
      {
        minimapSpriteAsset = Sprite.Create(minimap, new Rect(0, 0, width, height), Vector2.zero, 100);
        miniMapSprite.sprite = minimapSpriteAsset;
      }
    }
  }

  void RandomFillMap()
  {
    if (useRandomSeed)
    {
      seed = Utils.CreateRandomString();
    }

    System.Random pseudoRandom = new System.Random(seed.GetHashCode());

    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        if (x == 0 || x == width - 1 || y == 0 || y == height - 1) map[x, y] = 1;
        else map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
      }
    }
  }

  void SmoothMap()
  {
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        int neighbourWallTiles = GetSurroundingWallCount(x, y);
        if (neighbourWallTiles < 4)
        {
          map[x, y] = 0;
        }
        else if (neighbourWallTiles > 4)
        {
          map[x, y] = 1;
        }
      }
    }
  }

  int GetSurroundingWallCount(int gridX, int gridY)
  {
    int wallCount = 0;
    for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
    {
      for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
      {
        if (IsInMapRange(neighbourX, neighbourY))
        {
          if (neighbourX != gridX || neighbourY != gridY)
          {
            wallCount += map[neighbourX, neighbourY];
          }
        }
        else
        {
          wallCount++;
        }
      }
    }
    return wallCount;
  }

  bool ObjectAreClearFromOtherObjects(List<Vector2> list, int gridX, int gridY, float distance)
  {
    int objectsWithinRange = 0;
    foreach (Vector2 coordinate in list)
    {
      if (Vector2.Distance(coordinate, new Vector2(gridX, gridY)) < distance)
      {
        objectsWithinRange++;
      }
    }
    if (objectsWithinRange > 0) return false;
    else return true;
  }

  bool ObjectsAreClearFromWalls(int gridX, int gridY, float distance)
  {
    int objectsWithinRangeOfWall = 0;
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        if (map[x, y] == 1)
        {
          if (Vector2.Distance(new Vector2(x, y), new Vector2(gridX, gridY)) < distance)
          {
            objectsWithinRangeOfWall++;
          }
        }
        else continue;
      }
    }
    if (objectsWithinRangeOfWall > 0) return false;
    else return true;
  }

  class Room : IComparable<Room>
  {

    public List<Coord> tiles;
    public List<Coord> edgeTiles;
    public List<Room> connectedRooms;
    public int roomSize;
    public bool isAccessibleFromMainRoom;
    public bool isMainRoom;

    public Room() { }

    public Room(List<Coord> roomTiles, int[,] map)
    {
      tiles = roomTiles;
      roomSize = tiles.Count;
      connectedRooms = new List<Room>();
      edgeTiles = new List<Coord>();

      foreach (Coord tile in tiles)
      {
        for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
        {
          for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
          {
            if (x == tile.tileX || y == tile.tileY)
            {
              if (map[x, y] == 1)
              {
                edgeTiles.Add(tile);
              }
            }
          }
        }
      }
    }

    public void SetAccessibleFromMainRoom()
    {
      if (!isAccessibleFromMainRoom)
      {
        isAccessibleFromMainRoom = true;
        foreach (Room connectedRoom in connectedRooms)
        {
          connectedRoom.SetAccessibleFromMainRoom();
        }
      }
    }
    public static void ConnectRooms(Room roomA, Room roomB)
    {
      if (roomA.isAccessibleFromMainRoom)
      {
        roomB.SetAccessibleFromMainRoom();
      }
      else if (roomB.isAccessibleFromMainRoom)
      {
        roomA.SetAccessibleFromMainRoom();
      }
      roomA.connectedRooms.Add(roomB);
      roomB.connectedRooms.Add(roomA);
    }
    public bool IsConnected(Room otherRoom)
    {
      return connectedRooms.Contains(otherRoom);
    }

    public int CompareTo(Room otherRoom)
    {
      return otherRoom.roomSize.CompareTo(roomSize);
    }
  }

  // Decorative water generation: paint water on `waterTilemap` only and do not
  // remove or clear underlying floor tiles. Optional carving will set map cells
  // to floor and update wall/floor tilemaps, but painting water never calls
  // `floorTilemap.SetTile(..., null)`.
  private void GenerateDecorativeWater(System.Random rand, TileBase[] groundTiles)
  {
    if (waterTilemap == null) return;
    try
    {
      int attempts = Mathf.Clamp((width * height) / Math.Max(1, waterAttemptsDivisor), waterAttemptsMin, waterAttemptsMax);
      for (int a = 0; a < attempts; a++)
      {
        int sx = rand.Next(0, width);
        int sy = rand.Next(0, height);
        if (map[sx, sy] != 0) continue; // only place on floor
        Vector2Int seed = new Vector2Int(sx, sy);

        // If starting cell already has water, skip
        if (waterTilemap.GetTile(new Vector3Int(sx, sy, 0)) != null) continue;

        // Deterministic FIFO BFS expansion to a randomly chosen target size
        int targetSize = rand.Next(waterClusterMinSize, waterClusterMaxSize + 1);
        List<Coord> frontier = new List<Coord>();
        HashSet<(int, int)> visited = new HashSet<(int, int)>();
        List<(int, int)> placedCoords = new List<(int, int)>();
        frontier.Add(new Coord(seed.x, seed.y));
        visited.Add((seed.x, seed.y));
        int placed = 0;

        while (frontier.Count > 0 && placed < targetSize)
        {
          // hybrid pop: sometimes pop random to break grid patterns
          Coord c;
          if (rand.NextDouble() < waterFillRandomness)
          {
            int ridx = rand.Next(0, frontier.Count);
            c = frontier[ridx];
            frontier[ridx] = frontier[frontier.Count - 1];
            frontier.RemoveAt(frontier.Count - 1);
          }
          else
          {
            c = frontier[0];
            frontier.RemoveAt(0);
          }
          Vector3Int cell = new Vector3Int(c.tileX, c.tileY, 0);
          if (waterTilemap.GetTile(cell) != null) continue; // already filled by another cluster

          // Place unconditionally for connected fill (prevents interior holes)
          waterTilemap.SetTile(cell, currentBiomeGenerator?.waterTile);
          placedCoords.Add((c.tileX, c.tileY));
          placed++;

          // enqueue 8-neighbors in shuffled order to avoid directional bias
          var neighborOffsets = new List<(int, int)>() { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1) };
          // Fisher-Yates shuffle using the deterministic RNG
          for (int i = neighborOffsets.Count - 1; i > 0; i--)
          {
            int j = rand.Next(0, i + 1);
            var tmp = neighborOffsets[i];
            neighborOffsets[i] = neighborOffsets[j];
            neighborOffsets[j] = tmp;
          }
          foreach (var off in neighborOffsets)
          {
            int nx = c.tileX + off.Item1;
            int ny = c.tileY + off.Item2;
            if (!IsInMapRange(nx, ny)) continue;
            if (map[nx, ny] != 0) continue; // only floor
            if (visited.Contains((nx, ny))) continue;
            visited.Add((nx, ny));
            frontier.Add(new Coord(nx, ny));
          }
        }

        // If we failed to place the desired target (e.g., isolated region), rollback
        if (placed < waterClusterMinSize)
        {
          foreach (var v in placedCoords)
          {
            Vector3Int rc = new Vector3Int(v.Item1, v.Item2, 0);
            if (waterTilemap.GetTile(rc) == currentBiomeGenerator?.waterTile)
              waterTilemap.SetTile(rc, null);
          }
        }
      }
    }
    catch (Exception ex)
    {
      Debug.LogWarning($"[MapGenerator] GenerateDecorativeWater threw: {ex}");
    }
  }

  // Editor helper: regenerate decorative water from the current base map
  public void RegenerateDecorativeWater()
  {
    if (baseMapBackup != null)
    {
      // render base map then paint water
      RenderMap(baseMapBackup, floorTilemap, wallTilemap, currentBiomeGenerator.wallTile, currentBiomeGenerator.groundTiles);
    }
    else
    {
      RenderMap(map, floorTilemap, wallTilemap, currentBiomeGenerator.wallTile, currentBiomeGenerator.groundTiles);
    }
  System.Random genRandom = useDeterministicWater ? new System.Random(seed.GetHashCode()) : new System.Random(Guid.NewGuid().GetHashCode());
  GenerateDecorativeWater(genRandom, currentBiomeGenerator.groundTiles);
  }

  // Editor helper: restore base walls/floors/shadows from backup and clear water
  public void RegenerateBaseMap()
  {
    if (baseMapBackup != null)
    {
      RenderMap(baseMapBackup, floorTilemap, wallTilemap, currentBiomeGenerator.wallTile, currentBiomeGenerator.groundTiles);
    }
    else
    {
      RenderMap(map, floorTilemap, wallTilemap, currentBiomeGenerator.wallTile, currentBiomeGenerator.groundTiles);
    }
    if (waterTilemap != null) waterTilemap.ClearAllTiles();
  }

  // River support removed: use blob/puddle-only decorative water generator instead.
}
