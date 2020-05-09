using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateEmitter : MonoBehaviour
{
    public int rotationSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate ( Vector3.forward * (rotationSpeed * Time.deltaTime) );
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<Move>().TakeDamage(1, transform);
        }
    }
}
