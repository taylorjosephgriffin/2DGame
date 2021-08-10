using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class WorldGenerator : MonoBehaviour
{
  public GameObject mapContainer;
  GameObject emptyTilemap;
  private Tilemap wallTilemap, decorationTilemap, waterTilemap, floorTilemap;
  public Tile tile;
  public GeneratorConfig biomeGenerator;
  // Start is called before the first frame update
  public enum Direction
  {
    NORTH,
    SOUTH,
    WEST,
    EAST
  }
  public Dictionary<int, Direction> directionDictionary = new Dictionary<int, Direction>()
  {
      {0, Direction.NORTH},
      {1, Direction.SOUTH},
      {2, Direction.EAST},
      {3, Direction.WEST}
  };

  //  public List<int[,]> roomLocations = new List<int[,]>();
  public List<KeyValuePair<int, int>> roomLocations = new List<KeyValuePair<int, int>>();

  public KeyValuePair<int, int> currentRoomLocation = new KeyValuePair<int, int>(0, -1);

  public Direction currentDirection;

  void Start()
  {
    roomLocations.Add(new KeyValuePair<int, int>(0, 0));
    currentRoomLocation = new KeyValuePair<int, int>(0, -1);
    currentDirection = Direction.WEST;
    CreateTilemap(currentRoomLocation);
    CreateTilemapLocations();
  }

  void CreateTilemap(KeyValuePair<int, int> location)
  {
    emptyTilemap = mapContainer.transform.Find("EmptyTilemap").gameObject;
    emptyTilemap.GetComponent<MapGenerator>().currentBiomeGenerator = biomeGenerator;
    floorTilemap = emptyTilemap.transform.Find("FloorTilemap").GetComponent<Tilemap>();
    wallTilemap = emptyTilemap.transform.Find("WallTilemap").GetComponent<Tilemap>();
    decorationTilemap = emptyTilemap.transform.Find("DecorationTilemap").GetComponent<Tilemap>();
    waterTilemap = emptyTilemap.transform.Find("WaterTilemap").GetComponent<Tilemap>();
    GameObject tilemap = GameObject.Instantiate(mapContainer, transform.position, transform.rotation);
    tilemap.GetComponent<TextMeshPro>().text = location.Key + ", " + location.Value + "\b" + currentDirection;
    for (var x = 0; x < 60; x++)
    {
      for (var y = 0; y < 60; y++)
      {
        floorTilemap.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tile);
      }
    }
  }

  void CreateTilemapLocations()
  {
    for (var x = 0; x < 20; x++)
    {
      int randomNumber = Random.Range(0, 4);
      Debug.Log(randomNumber);
      currentDirection = directionDictionary[randomNumber];
      Debug.Log(currentDirection);
      if (currentDirection == Direction.NORTH && !roomLocations.Contains(new KeyValuePair<int, int>(currentRoomLocation.Key + 1, currentRoomLocation.Value)))
      {
        transform.position = new Vector3(transform.position.x + 60, transform.position.y, 0);
        currentRoomLocation = new KeyValuePair<int, int>(currentRoomLocation.Key + 1, currentRoomLocation.Value);
        roomLocations.Add(new KeyValuePair<int, int>(currentRoomLocation.Key + 1, currentRoomLocation.Value));
        CreateTilemap(currentRoomLocation);
      }
      else if (currentDirection == Direction.SOUTH && !roomLocations.Contains(new KeyValuePair<int, int>(currentRoomLocation.Key - 1, currentRoomLocation.Value)))
      {
        transform.position = new Vector3(transform.position.x - 60, transform.position.y, 0);
        currentRoomLocation = new KeyValuePair<int, int>(currentRoomLocation.Key - 1, currentRoomLocation.Value);
        roomLocations.Add(new KeyValuePair<int, int>(currentRoomLocation.Key - 1, currentRoomLocation.Value));
        CreateTilemap(currentRoomLocation);
      }
      else if (currentDirection == Direction.EAST && !roomLocations.Contains(new KeyValuePair<int, int>(currentRoomLocation.Key, currentRoomLocation.Value + 1)))
      {
        transform.position = new Vector3(transform.position.x, transform.position.y + 60, 0);
        currentRoomLocation = new KeyValuePair<int, int>(currentRoomLocation.Key, currentRoomLocation.Value + 1);
        roomLocations.Add(new KeyValuePair<int, int>(currentRoomLocation.Key, currentRoomLocation.Value + 1));
        CreateTilemap(currentRoomLocation);
      }
      else if (currentDirection == Direction.WEST && !roomLocations.Contains(new KeyValuePair<int, int>(currentRoomLocation.Key, currentRoomLocation.Value - 1)))
      {
        transform.position = new Vector3(transform.position.x, transform.position.y - 60, 0);
        currentRoomLocation = new KeyValuePair<int, int>(currentRoomLocation.Key, currentRoomLocation.Value - 1);
        roomLocations.Add(new KeyValuePair<int, int>(currentRoomLocation.Key, currentRoomLocation.Value - 1));
        CreateTilemap(currentRoomLocation);
      }
    }
  }

  // Update is called once per frame
  void Update()
  {

  }
}
