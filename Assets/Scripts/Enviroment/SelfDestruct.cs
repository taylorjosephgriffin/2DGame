using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    float currentTime = 0f;
    public float timeUntilDestroy = 2f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime >= timeUntilDestroy)
        {
            Destroy(transform.gameObject);
        }
    }
}
