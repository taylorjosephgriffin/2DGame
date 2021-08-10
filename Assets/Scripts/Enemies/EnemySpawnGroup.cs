using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Enemy Spawn Group", menuName = "Enemies/Spawn Group")]
public class EnemySpawnGroup : ScriptableObject
{
  public GameObject[] enemyGroup;
  public float spawnGroupIdleRadius = 7;
  public Vector2Int spawnLocation;
  public GameObject invalidNavPoint;

  public int[,] wallMap;

  Vector2 GetPositionAroundObject(Vector2Int position, int radius)
  {
    Vector2 offset = UnityEngine.Random.insideUnitCircle * radius;
    Vector2 pos = position + offset;
    return pos;
  }
  public Vector2 GetNavigationPointWithinSpawnRadius()
  {
    Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnGroupIdleRadius;
    Vector2 pos = spawnLocation + offset;
    if (wallMap[(int)pos.x, (int)pos.y] == 1)
    {
      return GetNavigationPointWithinSpawnRadius();
    }
    return pos;
  }

  public void spawnEnemies(Vector2Int spawnLocation)
  {
    for (int i = 0; i < enemyGroup.Length; i++)
    {
      GameObject newEnemy = Instantiate(enemyGroup[i], GetPositionAroundObject(spawnLocation, 4), new Quaternion(0, 0, 0, 0));
      if (newEnemy.GetComponent<EnemyController>() != null)
      {
        newEnemy.GetComponent<EnemyController>().spawnGroup = this;
      }
      if (newEnemy.GetComponent<DroneController>() != null)
      {
        newEnemy.GetComponent<DroneController>().spawnGroup = this;
      }
    }
  }
}
