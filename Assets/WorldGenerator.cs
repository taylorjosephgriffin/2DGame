using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
  public GameObject emptyTilemap;
  private Tilemap wallTilemap, decorationTilemap, waterTilemap, floorTilemap;
  public Tile tile;
  // Start is called before the first frame update

  void Start()
  {
    floorTilemap = emptyTilemap.transform.Find("FloorTilemap").GetComponent<Tilemap>();
    wallTilemap = emptyTilemap.transform.Find("WallTilemap").GetComponent<Tilemap>();
    decorationTilemap = emptyTilemap.transform.Find("DecorationTilemap").GetComponent<Tilemap>();
    waterTilemap = emptyTilemap.transform.Find("WaterTilemap").GetComponent<Tilemap>();
    GameObject tilemap = GameObject.Instantiate(emptyTilemap, transform.position, transform.rotation);
    for (var x = 0; x < 60; x++)
    {
      for (var y = 0; y < 60; y++)
      {
        floorTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tile);
      }
    }
  }

  // Update is called once per frame
  void Update()
  {

  }
}
