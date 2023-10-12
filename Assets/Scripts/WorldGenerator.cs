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
  public WorldGenerationController worldGenerationController;

  public KeyValuePair<int, int> currentRoomLocation = new KeyValuePair<int, int>(0, -1);

  public Direction currentDirection;

  void Start()
  {
    worldGenerationController.roomLocations.Add(new KeyValuePair<int, int>(0, 0));
    worldGenerationController.roomLocations.Add(new KeyValuePair<int, int>((int)transform.position.x, (int)transform.position.y));
    currentRoomLocation = new KeyValuePair<int, int>((int)transform.position.x, (int)transform.position.y);
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
    GameObject tilemap = GameObject.Instantiate(mapContainer, new Vector3(currentRoomLocation.Key, currentRoomLocation.Value, 10), transform.rotation);
    tilemap.GetComponent<TextMeshPro>().text = location.Key + ", " + location.Value + "\n" + currentDirection;
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
    foreach (var room in worldGenerationController.roomLocations)
    {
      Debug.Log("X: " + room.Key + " Y: " + room.Value + "\n" + "Current Room Location: " + currentRoomLocation.Key + ", " + currentRoomLocation.Value);
    }
    for (var x = 0; x < 10; x++)
    {
      System.Random rnd = new System.Random();
      int randomNumber = rnd.Next(0, 4);
      Debug.Log(randomNumber);
      currentDirection = directionDictionary[randomNumber];
      if (currentDirection == Direction.NORTH && !worldGenerationController.roomLocations.Contains(new KeyValuePair<int, int>((int)Mathf.Floor(((currentRoomLocation.Key + 60) / 60) * 60), currentRoomLocation.Value)))
      {
        currentRoomLocation = new KeyValuePair<int, int>((int)Mathf.Floor(((transform.position.x + 60) / 60) * 60), (int)transform.position.y);
        transform.position = new Vector3(currentRoomLocation.Key, currentRoomLocation.Value);
        worldGenerationController.roomLocations.Add(new KeyValuePair<int, int>(currentRoomLocation.Key, currentRoomLocation.Value));
        CreateTilemap(currentRoomLocation);
      }
      else if (currentDirection == Direction.SOUTH && !worldGenerationController.roomLocations.Contains(new KeyValuePair<int, int>((int)Mathf.Floor(((currentRoomLocation.Key - 60) / 60) * 60), currentRoomLocation.Value)))
      {
        currentRoomLocation = new KeyValuePair<int, int>((int)Mathf.Floor(((transform.position.x - 60) / 60) * 60), (int)transform.position.y);
        transform.position = new Vector3(currentRoomLocation.Key, currentRoomLocation.Value);
        worldGenerationController.roomLocations.Add(new KeyValuePair<int, int>(currentRoomLocation.Key, currentRoomLocation.Value));
        CreateTilemap(currentRoomLocation);
      }
      else if (currentDirection == Direction.EAST && !worldGenerationController.roomLocations.Contains(new KeyValuePair<int, int>(currentRoomLocation.Key, (int)Mathf.Floor(((currentRoomLocation.Value + 60) / 60) * 60))))
      {
        currentRoomLocation = new KeyValuePair<int, int>((int)transform.position.x, (int)Mathf.Floor(((transform.position.y + 60) / 60) * 60));
        transform.position = new Vector3(currentRoomLocation.Key, currentRoomLocation.Value);
        worldGenerationController.roomLocations.Add(new KeyValuePair<int, int>(currentRoomLocation.Key, currentRoomLocation.Value));
        CreateTilemap(currentRoomLocation);
      }
      else if (currentDirection == Direction.WEST && !worldGenerationController.roomLocations.Contains(new KeyValuePair<int, int>(currentRoomLocation.Key, (int)Mathf.Floor(((currentRoomLocation.Value - 60) / 60) * 60))))
      {
        currentRoomLocation = new KeyValuePair<int, int>((int)transform.position.x, (int)Mathf.Floor(((transform.position.y - 60) / 60) * 60));
        transform.position = new Vector3(currentRoomLocation.Key, currentRoomLocation.Value);
        worldGenerationController.roomLocations.Add(new KeyValuePair<int, int>(currentRoomLocation.Key, currentRoomLocation.Value));
        CreateTilemap(currentRoomLocation);
      }
    }
  }

  // Update is called once per frame
  void Update()
  {

  }
}
