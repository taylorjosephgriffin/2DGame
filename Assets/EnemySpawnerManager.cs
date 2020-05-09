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
        enemySpawnGroup.spawnEnemies(new Vector2Int((int)transform.position.x, (int)transform.position.y));
        enemySpawnGroup.spawnLocation = new Vector2Int((int)transform.position.x, (int)transform.position.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
