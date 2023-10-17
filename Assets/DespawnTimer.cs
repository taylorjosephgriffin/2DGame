using UnityEngine;

public class DespawnTimer : MonoBehaviour
{
    public float despawnTime = 20.0f; // Time in seconds before despawning

    void Start()
    {
        // Invoke the method to despawn after the specified time
        Invoke("Despawn", despawnTime);
    }

    void Despawn()
    {
        // Destroy the game object after the specified time
        Destroy(gameObject);
    }
}
