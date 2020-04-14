using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MapGenerator : MonoBehaviour
{
    public int width;
    public int height;
    public string seed;
    public bool useRandomSeed;

    [Range(0, 20)]
    public int smoothIterations;

    [Range(0, 100)]
    public int randomFillPercent;
    int[,] map;

    public Tilemap renderMap;

    public Tile artWallTile;
    public Tile wallTile;
    public Tile groundTile;
    public GameObject tree;
    public int treeChance;

    public GameObject bush;
    public int bushChance;

    public GameObject barrel;
    public int barrelChance;

    public GameObject enemy;

    public int enemyChance;

    public enum Algorithm { WALK_TOP, WALK_TOP_SMOOTH, STANDARD };
    public Algorithm currentAlgorithm = Algorithm.STANDARD;

    private List<Vector2> renderedDestructables = new List<Vector2>();
    private List<Vector2> renderedEnemies = new List<Vector2>();
    void Start()
    {
        GenerateMap();
        map = runAlgorithm();
        MovePlayer();
        RenderMap(map, renderMap, wallTile, groundTile);
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
            GenerateMap();
            map = runAlgorithm();
            MovePlayer();
            RenderMap(map, renderMap, wallTile, groundTile);;
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
                }
            }
        }
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
    }

    public void RenderMap(int[,] map, Tilemap tilemap, TileBase wallTile, TileBase groundTile)
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
                bool surroundingClear = !renderedDestructables.Contains(new Vector2(x, y - 1)) && 
                        !renderedDestructables.Contains(new Vector2(x-1, y))&& 
                        !renderedDestructables.Contains(new Vector2(x+1, y)) && 
                        !renderedDestructables.Contains(new Vector2(x, y + 1)) && 
                        !renderedDestructables.Contains(new Vector2(x, y - 2)) && 
                        !renderedDestructables.Contains(new Vector2(x-2, y))&& 
                        !renderedDestructables.Contains(new Vector2(x+2, y)) && 
                        !renderedDestructables.Contains(new Vector2(x, y + 2));
                int neighbourWallTiles = GetSurroundingWallCount(x, y);
                // 1 = tile, 0 = no tile
                if (map[x, y] == 1) 
                {
                    if (neighbourWallTiles < 7) {
                        tilemap.SetTile(new Vector3Int(x, y, 0), artWallTile);
                    } else {
                        tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                    }
                
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
                    tilemap.SetTile(new Vector3Int(x, y, 0), groundTile); 
                    if ((pseudoRandom.Next(0, 100) < treeChance) && 
                        surroundingClear &&
                        neighbourWallTiles < 1) {
                        GameObject newTree = Instantiate(tree, new Vector3Int(x, y, 0), new Quaternion(0,0,0,0));
                        renderedDestructables.Add(new Vector2(x, y));
                    }
                    if ((pseudoRandom.Next(0, 100) < bushChance) && 
                        surroundingClear &&
                        neighbourWallTiles < 1) {
                        GameObject newbush = Instantiate(bush, new Vector3Int(x, y, 0), new Quaternion(0,0,0,0));
                        renderedDestructables.Add(new Vector2(x, y));
                    }
                    if ((pseudoRandom.Next(0, 100) < barrelChance) && 
                        surroundingClear &&
                        neighbourWallTiles < 1) {
                        GameObject newBarrel = Instantiate(barrel, new Vector3Int(x, y, 0), new Quaternion(0,0,0,0));
                        renderedDestructables.Add(new Vector2(x, y));
                    }
                    if ((pseudoRandom.Next(0, 100) < enemyChance) && 
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
                if (neighbourWallTiles > 4)
                {
                    map[x, y] = 1;
                } 
                else if (neighbourWallTiles < 4)
                {
                    map[x, y] = 0;
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
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
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

    // private void OnDrawGizmos()
    // {
    //     if (map != null)    
    //     {
    //         for (int x = 0; x < width; x++)
    //         {
    //             for (int y = 0; y < height; y++)
    //             {
    //                 Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
    //                 Vector3 pos = new Vector3((-width/2 + x + 0.5f), y, (-height/2 +y + 0.5f));
    //                 Gizmos.DrawCube(pos, Vector3.one);
    //             }   
    //         }
    //     }
    // }
}
