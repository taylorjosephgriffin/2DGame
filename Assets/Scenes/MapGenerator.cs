﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MapGenerator : MonoBehaviour
{
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
    private Tilemap renderMap;
    [SerializeField]  
    private RuleTile wallTile;
    [SerializeField]  
    private Tile[] groundTiles;
    [SerializeField]  
    private GameObject tree;
    [SerializeField]  
    private int treeChance;
    [SerializeField]  

    private GameObject bush;
    [SerializeField]  
    private int bushChance;
    [SerializeField]  

    private GameObject barrel;
    [SerializeField]  
    private int barrelChance;
    [SerializeField]  

    private GameObject enemy;
    

    private int wallThresholdSize = 3;
    private int roomThresholdSize = 28;
    [SerializeField]  

    private int enemyChance;

    private enum Algorithm { WALK_TOP, WALK_TOP_SMOOTH, STANDARD };
    private Algorithm currentAlgorithm = Algorithm.STANDARD;

    private List<Vector2> renderedDestructables = new List<Vector2>();
    private List<Vector2> renderedEnemies = new List<Vector2>();
    void Start()
    {
        GenerateMap();
        map = runAlgorithm();
        MovePlayer();
        RenderMap(map, renderMap, wallTile, groundTiles);
    }

    struct Coord {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
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
            renderedEnemies.Clear();
            GenerateMap();
            map = runAlgorithm();
            MovePlayer(); 
            RenderMap(map, renderMap, wallTile, groundTiles);;
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
        for (int x = 0; x < width ; x++) 
        {
            //Loop through the height of the map
            for (int y = 0; y < height; y++) 
            {
                if (map [x, y] == 0)
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

        foreach(List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY]= 0;
                } 
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);
        List<Room> survivingRooms = new List<Room>();

        foreach(List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY]= 1;
                } 
            } 
            else {
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
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom) {
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
        Debug.DrawLine(new Vector2(tileA.tileX, tileA.tileY), new Vector2(tileB.tileX, tileB.tileY), Color.green, 100);    

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
            } else 
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
                else {
                    y+= gradientStep;
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
        int tileType = map[startX,startY];

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

        ProcessMap();
    }

    public void RenderMap(int[,] map, Tilemap tilemap, TileBase wallTile, TileBase[] groundTiles)
    {
        //Clear the map (ensures we dont overlap)
        tilemap.ClearAllTiles(); 
        //Loop through the width of the map
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        GameObject colliderContainer = new GameObject();
        for (int x = 0; x < width ; x++) 
        {
            //Loop through the height of the map
            for (int y = 0; y < height; y++) 
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);
                // 1 = tile, 0 = no tile
                if (map[x, y] == 1) 
                {

                    tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                
                    if (neighbourWallTiles < 8) {
                        GameObject collider = new GameObject();
                        collider.name = "Collider";
                        collider.transform.position =  new Vector3Int(x, y, 0);
                        collider.gameObject.tag = "Collider";
                        collider.transform.SetParent(colliderContainer.transform);
                        collider.AddComponent<BoxCollider2D>();

                    }
                }
                else {
                    int randomTileIndex = UnityEngine.Random.Range(1, groundTiles.Length);
                    int randomNumber = UnityEngine.Random.Range(0, 100);
                    float scale = UnityEngine.Random.Range(1, 1.1f);
                    tilemap.SetTile(new Vector3Int(x, y, 0), groundTiles[randomTileIndex]); 
                    if (tree != null && randomNumber < treeChance && 
                        GetSurroundingObjectCount(x, y) == 0 &&
                        neighbourWallTiles < 1) {
                        GameObject newTree = Instantiate(tree, new Vector3Int(x, y, 0), new Quaternion(0,0,0,0));
                        newTree.transform.localScale = new Vector3(scale, scale, 1);
                        renderedDestructables.Add(new Vector2(x, y));
                        randomNumber = UnityEngine.Random.Range(0, 100);
                    }
                    if (bush != null && randomNumber < bushChance && 
                        GetSurroundingObjectCount(x, y) == 0 &&
                        neighbourWallTiles < 1) {
                        GameObject newbush = Instantiate(bush, new Vector3Int(x, y, 0), new Quaternion(0,0,0,0));
                        newbush.transform.localScale = new Vector3(scale, scale, 1);
                        renderedDestructables.Add(new Vector2(x, y));
                        randomNumber = UnityEngine.Random.Range(0, 100);
    
                    }
                    if (barrel != null && randomNumber < barrelChance && 
                        GetSurroundingObjectCount(x, y) == 0 &&
                        neighbourWallTiles < 1) {
                        GameObject newBarrel = Instantiate(barrel, new Vector3Int(x, y, 0), new Quaternion(0,0,0,0));
                        renderedDestructables.Add(new Vector2(x, y));
                        randomNumber = UnityEngine.Random.Range(0, 100);
                    }
                    if (enemy != null && randomNumber < enemyChance && 
                        !renderedEnemies.Contains(new Vector2(x, y - 1)) && 
                        !renderedEnemies.Contains(new Vector2(x-1, y))&& 
                        !renderedEnemies.Contains(new Vector2(x+1, y)) && 
                        !renderedEnemies.Contains(new Vector2(x, y + 1)) && 
                        !renderedEnemies.Contains(new Vector2(x, y - 2)) && 
                        !renderedEnemies.Contains(new Vector2(x-2, y))&& 
                        !renderedEnemies.Contains(new Vector2(x+2, y)) && 
                        !renderedEnemies.Contains(new Vector2(x, y + 2)) && 
                        neighbourWallTiles < 1) {
                        GameObject newEnemy = Instantiate(enemy, new Vector3Int(x, y, 0), new Quaternion(0,0,0,0));
                        renderedEnemies.Add(new Vector2(x, y));
                        randomNumber = UnityEngine.Random.Range(0, 100);
                    }
                }
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
                if (x == 0 || x == width- 1 || y == 0 || y == height - 1) map[x,y] = 1;
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
                else {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    int GetSurroundingObjectCount(int gridX, int gridY)
    {
        int objectCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (renderedDestructables.Contains(new Vector2(neighbourX, neighbourY)) && (neighbourX != gridX || neighbourY != gridY))
                {
                    objectCount++;
                }
            }
        }
        return objectCount;
    }

    class Room: IComparable<Room> {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room() {}

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
