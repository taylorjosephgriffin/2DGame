using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawnerManager : MonoBehaviour
{
    public EnemySpawnGroup enemySpawnGroup;
    // Start is called before the first frame update
    void Start()
    {
        Vector2 worldPos = new Vector2(transform.position.x, transform.position.y);
        enemySpawnGroup.spawnLocation = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        enemySpawnGroup.spawnEnemiesAtWorldPosition(worldPos, this.transform);
    }
}
