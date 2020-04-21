using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public bool useLevelGenerator = false;
    public GameObject levelGenerator;
    // Start is called before the first frame update
    void Awake()
    {
        if (!useLevelGenerator)
        {
            levelGenerator.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
