using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public bool useLevelGenerator = false;
    public GameObject levelGenerator;

    public GameObject minimap;
    // Start is called before the first frame update
    void Awake()
    {
        if (!useLevelGenerator) {
            levelGenerator.SetActive(false);
            minimap.SetActive(false);
        } else {
            levelGenerator.SetActive(true);
            minimap.SetActive(true);
        }
    }
}
