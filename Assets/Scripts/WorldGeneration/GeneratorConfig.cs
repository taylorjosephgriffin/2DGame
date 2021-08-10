using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Generator Config", menuName = "World Generation/Generator Config")]
public class GeneratorConfig : ScriptableObject
{
  public enum BiomeType
  {
    FOREST,
    DESERT,
    TUNDRA,
    LAB
  }
  public BiomeType biomeType;

  public RuleTile wallTile;

  public Tile[] groundTiles;
  public SpawnItem[] spawnItems;
  public SpawnItem grassItem;
  public Tile grassSpawnTile;
  public EnemySpawnGroup[] enemySpawnGroups;

  public Tile wallTileTop, wallTileThatNeedsShadow, wallTileThatNeedsShadow2, wallTileCliff, wallTileShadow;
}
