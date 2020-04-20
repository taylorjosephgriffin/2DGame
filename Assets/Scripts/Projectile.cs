using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileState { FIRED, HIT };

public class Projectile : MonoBehaviour
{
    public ProjectileState currentProjectileState = ProjectileState.FIRED;
    public float projectileSpeed = 10;
    public float spawnTime = 5;
    public int damage = 5;
    Vector3 mousePosition;
    Vector3 aimDirection;
    GameObject cursor;
    Vector3 cursorPosition;

    private void Awake()
    {
        cursor = GameObject.Find("Cursor");
        cursorPosition = cursor.transform.position;
    }
    // Start is called before the first frame update
     void Start()
    {
        CalculateMousePosition();
    }

    IEnumerator killProjectile()
    {
        transform.GetComponent<Animator>().SetTrigger("AnimDeath");
        yield return new WaitForSeconds(0.25f);
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
            StartCoroutine(killProjectile());
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Explodable"))
        {
            collision.GetComponent<Explodable>().explode();
        }
        if (!collision.CompareTag("Pickup") && !collision.CompareTag("Explodable") && !collision.CompareTag("Player"))
        {
            currentProjectileState = ProjectileState.HIT;
        }
    }
}
