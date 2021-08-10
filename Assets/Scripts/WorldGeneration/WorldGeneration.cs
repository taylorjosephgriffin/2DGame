using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
  public string texture;
  int[,] map;
  Texture2D image;

  private void Start()
  {
    image = (Texture2D)Resources.Load(texture);
    map = new int[image.width * 3, image.height * 3];
    pixelReader();
  }
  // Vector2 worldSize = new Vector2(4, 4);
  // WorldRoom[,] rooms;
  // List<Vector2> takenPositions = new List<Vector2>();

  // int gridSizeX, gridSizeY, numberOfRooms = 20;

  // public GameObject roomWhiteObj;

  // private void Start()
  // {
  //     if (numberOfRooms >= (worldSize.x * 2) * (worldSize.y * 2)) 
  //     {
  //         numberOfRooms = Mathf.RoundToInt((worldSize.x * 2) * (worldSize.y * 2));
  //     }
  //     gridSizeX = Mathf.RoundToInt(worldSize.x);
  //     gridSizeY = Mathf.RoundToInt(worldSize.y);
  //     CreateRooms();
  //     SetRoomDoors();
  //     DrawMap();
  // }

  // void CreateRooms()
  // {
  //     rooms = new WorldRoom[gridSizeX * 2, gridSizeY * 2];
  //     rooms[gridSizeX, gridSizeY] = new WorldRoom(Vector2.zero, 1);
  //     takenPositions.Insert(0, Vector2.zero);
  //     Vector2 checkPos = Vector2.zero
  // }

  void pixelReader()
  {
    int greenPixels = 0;
    int blankPixels = 0;
    for (int x = 0; x < image.width; x++)
    {
      for (int y = 0; y < image.height; y++)
      {
        Color pixel = image.GetPixel(x, y);

        if (pixel == new Color32(106, 190, 48, 255))
        {
          for (int z = 0; z < 3; z++)
          {
            map[x + z, y + z] = 1;
          }
        }
        else
        {
          for (int z = 0; z < 3; z++)
          {
            map[x + z, y + z] = 0;
          }
        }
      }
    }
  }

  private void OnDrawGizmos()
  {
    if (map != null)
    {
      for (int x = 0; x < map.Length; x++)
      {
        for (int y = 0; y < map.Length; y++)
        {
          Gizmos.color = (map[x, y] == 1) ? Color.green : Color.black;
          Vector3 pos = new Vector3((-image.width / 2 + x + 0.5f), y, (-image.height / 2 + y + 0.5f));
          Gizmos.DrawCube(pos, Vector3.one);
        }
      }
    }
  }

}
