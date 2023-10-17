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
    WEST
  }
  public EntranceDirection direction;
  public EntranceDirection[] directions;
  public EntranceDirection playerStartingPosition;

  public Tilemap wallTilemap, floorTilemap, decorationTilemap, backgroundWallTilemap;

  public bool shouldSpawnEnemies;

  public UnityEngine.Transform player;

  public GeneratorConfig currentBiomeGenerator;
  [SerializeField]
  private int width;
  [SerializeField]
  private int height;
  [SerializeField]
  private string seed;
  [SerializeField]
  private bool useRandomSeed;
  [Range(0, 20)]
  private int smoothIterations = 1;
  [SerializeField]
  [Range(0, 100)]
  private int randomFillPercent;
  int[,] map;
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
  Texture2D minimap;
  public Image miniMapSprite;

  void Start()
  {
    Init();
  }

  void Init()
  {
    GenerateMap();
    AddDirectionalPassage();
    RenderMap(map, floorTilemap, wallTilemap, currentBiomeGenerator.wallTile, currentBiomeGenerator.groundTiles);
    for (int x = 0; x < width * 2; x++)
    {
      for (int y = 0; y < height * 2; y++)
      {
        backgroundWallTilemap.SetTile(new Vector3Int(x, y, 0), currentBiomeGenerator.wallTile);
      }
    }
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
      if (playerStartingPosition == EntranceDirection.NORTH) player.position = new Vector3Int(xIndexNorth, height - 1, 0);
      Coord startTile = new Coord(xIndexNorth, (int)height);
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
      foreach (Coord c in line)
      {
        DrawCircle(c, 2);
      }
    }
    if (directions.Contains(EntranceDirection.SOUTH))
    {
      int xIndexSouth = (int)(width / 2 + .5f);
      if (playerStartingPosition == EntranceDirection.SOUTH) player.position = new Vector3Int(xIndexSouth, 0, 0);
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
      foreach (Coord c in lineSouth)
      {
        DrawCircle(c, 2);
      }
    }
    if (directions.Contains(EntranceDirection.WEST))
    {
      int yIndexWest = (int)(height / 2 + .5f);
      if (playerStartingPosition == EntranceDirection.WEST) player.position = new Vector3Int(0, yIndexWest, 0);
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
      foreach (Coord c in lineWest)
      {
        DrawCircle(c, 2);
      }
    }
    if (directions.Contains(EntranceDirection.EAST))
    {

      int yIndexEast = (int)(height / 2 + .5f);
      if (playerStartingPosition == EntranceDirection.EAST) player.position = new Vector3Int(width - 1, yIndexEast, 0);
      Coord startTileEast = new Coord((int)width, yIndexEast);
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
      foreach (Coord c in lineEast)
      {
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
  }

  public int[,] runAlgorithm()
  {
    if (currentAlgorithm == Algorithm.WALK_TOP) return RandomWalkTop(map, seed);
    else if (currentAlgorithm == Algorithm.WALK_TOP_SMOOTH) return RandomWalkTopSmoothed(map, seed, 2);
    return map;
  }

  void MovePlayer()
  {
    for (int x = 0; x < width; x++)
    {
      //Loop through the height of the map
      for (int y = 0; y < height; y++)
      {
        if (map[x, y] == 0)
        {
          GameObject.FindWithTag("Player").transform.position = new Vector3Int(x, y, 0);
          GameObject.FindWithTag("MainCamera").transform.position = new Vector3Int(x, y, -10);
        }
      }
    }
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
    survivingRooms[0].isMainRoom = true;
    survivingRooms[0].isAccessibleFromMainRoom = true;
    ConnectClosestRooms(survivingRooms);
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
    decorationTilemap.ClearAllTiles();
    //Loop through the width of the map
    System.Random pseudoRandom = new System.Random(seed.GetHashCode());
    GameObject colliderContainer = new GameObject();
    Texture2D minimap = new Texture2D(width, height);
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

          if ((wallTilemap.GetSprite(new Vector3Int(x, y, 0)) == currentBiomeGenerator.wallTileThatNeedsShadow.sprite || wallTilemap.GetSprite(new Vector3Int(x, y, 0)) == currentBiomeGenerator.wallTileThatNeedsShadow2.sprite) &&
              IsInMapRange(x, y - 1) &&
              map[x, y - 1] == 0 && IsInMapRange(x, y
              + 1) && map[x, y + 1] != 0)
          {
            wallTilemap.SetTile(new Vector3Int(x, y, 0), currentBiomeGenerator.wallTileCliff);
            decorationTilemap.SetTile(new Vector3Int(x, y - 1, 0), currentBiomeGenerator.wallTileShadow);
          }

          if ((wallTilemap.GetSprite(new Vector3Int(x, y, 0)) == currentBiomeGenerator.wallTileThatNeedsShadow.sprite || wallTilemap.GetSprite(new Vector3Int(x, y, 0)) == currentBiomeGenerator.wallTileThatNeedsShadow2.sprite) &&
              IsInMapRange(x, y - 1) &&
              map[x, y - 1] == 0 && IsInMapRange(x, y
              + 1) && map[x, y + 1] == 0)
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
          int dictval;
          floorTilemap.SetTile(new Vector3Int(x, y, 0), groundTiles[pseudoRandom.Next(1, groundTiles.Length)]);
          if (floorTilemap.GetTile(new Vector3Int(x, y, 0)).name == currentBiomeGenerator.grassSpawnTile.name)
          {
            GameObject newItem = Instantiate(currentBiomeGenerator.grassItem.item, new Vector3Int(x + (int)transform.GetComponentInParent<UnityEngine.Transform>().position.x, y + (int)transform.GetComponentInParent<UnityEngine.Transform>().position.y, 0), new Quaternion(0, 0, 0, 0));
            newItem.transform.localScale = new Vector3(scale, scale, 1);
          }
          foreach (var item in currentBiomeGenerator.spawnItems)
          {
            if (pseudoRandom.Next(0, 100) < item.chance && ObjectAreClearFromOtherObjects(renderedDestructables, x, y, item.minRadius) &&
              ObjectsAreClearFromWalls(x, y, 3) && itemDictionary[item.item.name] < item.max)
            {
              if (itemDictionary.TryGetValue(item.item.name, out dictval))
              {
                if (itemDictionary[item.item.name] < item.max)
                {
                  GameObject newItem = Instantiate(item.item, new Vector3Int(x + (int)transform.GetComponentInParent<UnityEngine.Transform>().position.x, y + (int)transform.GetComponentInParent<UnityEngine.Transform>().position.y, 0), new Quaternion(0, 0, 0, 0));
                  newItem.transform.localScale = new Vector3(scale, scale, 1);
                  renderedDestructables.Add(new Vector2(x, y));
                  randomNumber = UnityEngine.Random.Range(0, 100);
                }
              }
              if (itemDictionary.TryGetValue(item.item.name, out dictval))
              {
                itemDictionary.Remove(item.item.name);
                itemDictionary.Add(item.item.name, dictval + 1);
              }

            }
          }
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
          if (shouldSpawnEnemies && randomNumber < enemyChance &&
              ObjectAreClearFromOtherObjects(renderedSpawnGroups, x, y, 10) &&
              ObjectsAreClearFromWalls(x, y, 5))
          {

            EnemySpawnGroup newEnemy = Instantiate(currentBiomeGenerator.enemySpawnGroups[0]);
            newEnemy.spawnEnemies(new Vector2Int(x, y));
            newEnemy.spawnLocation = new Vector2Int(x, y);
            newEnemy.wallMap = map;
            renderedSpawnGroups.Add(new Vector2(x, y));
            minimap.SetPixel(x, y, new Color32(86, 255, 85, 255));
            randomNumber = UnityEngine.Random.Range(0, 100);
          }
        }
      }
    }
    minimap.Apply();
    miniMapSprite.sprite = Sprite.Create(minimap, new Rect(0, 0, width, height), Vector2.zero, 100);
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

}
