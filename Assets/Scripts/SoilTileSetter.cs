using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoilTileSetter : MonoBehaviour
{
    public Sprite[] soilTiles;
    
    void Start()
    {
        int randomIndex = Random.Range(0, soilTiles.Length);
        transform.GetComponent<SpriteRenderer>().sprite = soilTiles[randomIndex];    
    }
}
