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
    // Defensive: ensure wallMap exists and has dimensions
    if (wallMap == null)
    {
      Debug.LogWarning("EnemySpawnGroup:GetNavigationPointWithinSpawnRadius - wallMap is null, returning spawnLocation");
      return spawnLocation;
    }
    try
    {
      int dim0 = wallMap.GetLength(0);
      int dim1 = wallMap.GetLength(1);
      if (dim0 == 0 || dim1 == 0)
      {
        Debug.LogWarning("EnemySpawnGroup:GetNavigationPointWithinSpawnRadius - wallMap has zero dimension, returning spawnLocation");
        return spawnLocation;
      }

      int maxAttempts = 20;
      for (int attempt = 0; attempt < maxAttempts; attempt++)
      {
        Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnGroupIdleRadius;
        Vector2 pos = spawnLocation + offset;

        int ix = Mathf.RoundToInt(pos.x);
        int iy = Mathf.RoundToInt(pos.y);

        // Bounds check
        if (ix < 0 || iy < 0 || ix >= dim0 || iy >= dim1)
        {
          continue;
        }

        // If the cell exists and is not a wall (0 == free), return it
        if (wallMap[ix, iy] == 0)
        {
          return pos;
        }

        // occupied by wall, try again
      }
    }
    catch (System.Exception ex)
    {
      Debug.LogWarning($"EnemySpawnGroup:GetNavigationPointWithinSpawnRadius - exception: {ex}. Returning spawnLocation as fallback.");
      return spawnLocation;
    }

    // Fallback if no valid nav point found
    Debug.LogWarning("EnemySpawnGroup:GetNavigationPointWithinSpawnRadius - failed to find clear nav point after attempts, returning spawnLocation");
    return spawnLocation;
  }

  public void spawnEnemies(Vector2Int spawnLocation, Transform parent = null)
  {
    if (enemyGroup == null || enemyGroup.Length == 0)
    {
      Debug.LogWarning("EnemySpawnGroup:spawnEnemies called but enemyGroup is null or empty");
      return;
    }
    for (int i = 0; i < enemyGroup.Length; i++)
    {
      GameObject prefab = enemyGroup[i];
      if (prefab == null)
      {
        Debug.LogWarning($"EnemySpawnGroup:spawnEnemies - enemy prefab at index {i} is null, skipping");
        continue;
      }
      Vector2 pos2 = GetPositionAroundObject(spawnLocation, 4);
      Vector3 spawnPos = new Vector3(pos2.x, pos2.y, 0f);
  GameObject newEnemy = parent != null ? Instantiate(prefab, spawnPos, Quaternion.identity, parent) : Instantiate(prefab, spawnPos, Quaternion.identity);
      var ec = newEnemy.GetComponent<EnemyController>();
      if (ec != null)
      {
        ec.spawnGroup = this;
      }
      var dc = newEnemy.GetComponent<DroneController>();
      if (dc != null)
      {
        dc.spawnGroup = this;
      }
    }
  }

  // Spawn enemies around a world-space position (used when MapGenerator wants to
  // place enemies at the room's world coordinates). This avoids confusion
  // between local tile indices and world-space coordinates.
  public void spawnEnemiesAtWorldPosition(Vector2 worldPosition, Transform parent = null)
  {
    if (enemyGroup == null || enemyGroup.Length == 0)
    {
      Debug.LogWarning("EnemySpawnGroup:spawnEnemiesAtWorldPosition called but enemyGroup is null or empty");
      return;
    }
    for (int i = 0; i < enemyGroup.Length; i++)
    {
      GameObject prefab = enemyGroup[i];
      if (prefab == null)
      {
        Debug.LogWarning($"EnemySpawnGroup:spawnEnemiesAtWorldPosition - enemy prefab at index {i} is null, skipping");
        continue;
      }
      Vector2 offset = UnityEngine.Random.insideUnitCircle * 4f;
      Vector3 spawnPos = new Vector3(worldPosition.x + offset.x, worldPosition.y + offset.y, 0f);
  GameObject newEnemy = parent != null ? Instantiate(prefab, spawnPos, Quaternion.identity, parent) : Instantiate(prefab, spawnPos, Quaternion.identity);
      var ec = newEnemy.GetComponent<EnemyController>();
      if (ec != null)
      {
        ec.spawnGroup = this;
      }
      var dc = newEnemy.GetComponent<DroneController>();
      if (dc != null)
      {
        dc.spawnGroup = this;
      }
    }
  }
}