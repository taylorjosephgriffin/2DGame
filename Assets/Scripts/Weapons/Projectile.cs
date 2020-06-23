using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileState { FIRED, HIT };

public class Projectile : MonoBehaviour
{
    public ProjectileState currentProjectileState = ProjectileState.FIRED;
    public float projectileSpeed;
    public float spawnTime = 5;
    public int damage = 5;
    Vector3 mousePosition;
    Vector3 aimDirection;
    public Vector3 aimDirectionOverride;
    GameObject cursor;
    Vector3 cursorPosition;
    bool isColliding;

    public GameObject projectileDeathPrefab;

    private void Awake()
    {
        cursor = GameObject.Find("MouseCursor");
        cursorPosition = cursor.transform.position;
    }
    // Start is called before the first frame update
     void Start()
    {
        CalculateMousePosition();
    }

    
    void killProjectile()
    {
        GameObject projectileExplosion = Instantiate(projectileDeathPrefab, transform.position, transform.rotation);
        Destroy(transform.gameObject);
    }

    private void CalculateMousePosition()
    {
        aimDirection = (cursorPosition - transform.position);
        aimDirection.z = 0;
        aimDirection.Normalize();

    }

    // Update is called once per frame
    void Update()
    {
        spawnTime -= Time.deltaTime;
        if (spawnTime <= 0)
        {
            Destroy(transform.gameObject);
        }
        if (currentProjectileState == ProjectileState.FIRED)
        {
            GetComponent<Rigidbody2D>().velocity = aimDirection * projectileSpeed;
        }
        if (currentProjectileState == ProjectileState.HIT)
        {
            killProjectile();
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isColliding) return;
        if (collision.CompareTag("Explodable"))
        {
            transform.GetComponent<CircleCollider2D>().enabled = false;
            collision.GetComponent<Explodable>().explode();
        }
        if (!collision.CompareTag("Pickup") && !collision.CompareTag("Destructable") && !collision.CompareTag("Explodable") && !collision.CompareTag("Player"))
        {
            transform.GetComponent<CircleCollider2D>().enabled = false;
            currentProjectileState = ProjectileState.HIT;
        }
    }
}
