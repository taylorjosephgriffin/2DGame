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
    public Tile wallTile;
    public Tile groundTile;
    public GameObject tree;
    public int treeChance;

    public GameObject bush;
    public int bushChance;

    public GameObject barrel;
    public int barrelChance;

    private List<Vector2> rendererdDestructables = new List<Vector2>();
    void Start()
    {
        GenerateMap();
        RenderMap(map, renderMap, wallTile, groundTile);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Reload"))
        {
            GenerateMap();
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
            RenderMap(map, renderMap, wallTile, groundTile);;
        }
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
                    if (neighbourWallTiles < 6) {
                        GameObject collider = new GameObject();
                        collider.name = "Collider";
                        collider.transform.position =  new Vector3Int(x, y, 0);
                        collider.gameObject.tag = "Collider";
                        collider.AddComponent<BoxCollider2D>();

                    }
                }
                else {
                    tilemap.SetTile(new Vector3Int(x, y, 0), groundTile); 
                    if ((pseudoRandom.Next(0, 100) < treeChance) && 
                        !rendererdDestructables.Contains(new Vector2(x, y - 1)) && 
                        !rendererdDestructables.Contains(new Vector2(x-1, y))&& 
                        !rendererdDestructables.Contains(new Vector2(x+1, y)) && 
                        !rendererdDestructables.Contains(new Vector2(x, y + 1)) && 
                        !rendererdDestructables.Contains(new Vector2(x, y - 2)) && 
                        !rendererdDestructables.Contains(new Vector2(x-2, y))&& 
                        !rendererdDestructables.Contains(new Vector2(x+2, y)) && 
                        !rendererdDestructables.Contains(new Vector2(x, y + 2)) && 
                        neighbourWallTiles < 1) {
                        GameObject newTree = Instantiate(tree, new Vector3Int(x, y, 0), new Quaternion(0,0,0,0));
                        rendererdDestructables.Add(new Vector2(x, y));
                    }
                    if ((pseudoRandom.Next(0, 100) < bushChance) && 
                        !rendererdDestructables.Contains(new Vector2(x, y - 1)) && 
                        !rendererdDestructables.Contains(new Vector2(x-1, y))&& 
                        !rendererdDestructables.Contains(new Vector2(x+1, y)) && 
                        !rendererdDestructables.Contains(new Vector2(x, y + 1)) && 
                        !rendererdDestructables.Contains(new Vector2(x, y - 2)) && 
                        !rendererdDestructables.Contains(new Vector2(x-2, y))&& 
                        !rendererdDestructables.Contains(new Vector2(x+2, y)) && 
                        !rendererdDestructables.Contains(new Vector2(x, y + 2)) && 
                        neighbourWallTiles < 1) {
                        GameObject newbush = Instantiate(bush, new Vector3Int(x, y, 0), new Quaternion(0,0,0,0));
                        rendererdDestructables.Add(new Vector2(x, y));
                    }
                    if ((pseudoRandom.Next(0, 100) < barrelChance) && 
                        !rendererdDestructables.Contains(new Vector2(x, y - 1)) && 
                        !rendererdDestructables.Contains(new Vector2(x-1, y))&& 
                        !rendererdDestructables.Contains(new Vector2(x+1, y)) && 
                        !rendererdDestructables.Contains(new Vector2(x, y + 1)) && 
                        !rendererdDestructables.Contains(new Vector2(x, y - 2)) && 
                        !rendererdDestructables.Contains(new Vector2(x-2, y))&& 
                        !rendererdDestructables.Contains(new Vector2(x+2, y)) && 
                        !rendererdDestructables.Contains(new Vector2(x, y + 2)) && 
                        neighbourWallTiles < 1) {
                        GameObject newBarrel = Instantiate(barrel, new Vector3Int(x, y, 0), new Quaternion(0,0,0,0));
                        rendererdDestructables.Add(new Vector2(x, y));
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

    private void OnDrawGizmos()
    {
        if (map != null)    
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3((-width/2 + x + 0.5f), y, (-height/2 +y + 0.5f));
                    Gizmos.DrawCube(pos, Vector3.one);
                }   
            }
        }
    }
}
