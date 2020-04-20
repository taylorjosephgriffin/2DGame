using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState { ACTIVE, DEAD }

public class EnemyController : MonoBehaviour
{

    GameObject player;
    public GameObject Hitbox;
    public int health;
    private ShakeBehavior cameraShake;
    public EnemyState currentEnemyState;
    public GameObject deathParticle;
    Vector3 moveDirection;
    public GameObject slimeDrop;

    // Start is called before the first frame update
    void Start()
    {
        cameraShake = GameObject.FindWithTag("MainCamera").GetComponent<ShakeBehavior>();
        player = GameObject.FindWithTag("Player");
    }

    IEnumerator whitecolor()
    {
        yield return new WaitForSeconds(0.05f);
        transform.parent.GetComponent<SpriteRenderer>().color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentEnemyState == EnemyState.ACTIVE)
        {
            transform.parent.GetComponent<Animator>().SetBool("IsChasing", true);
            Vector3 direction = player.transform.position - transform.parent.position;
            transform.parent.position += direction * .3f * Time.deltaTime;

            transform.Translate(new Vector3(2f * Time.deltaTime, 0, 0));
            if (health <= 0)
            {
                currentEnemyState = EnemyState.DEAD;
            }
        }
        else
        {
            GameObject slimeDeath = Instantiate(deathParticle, transform);
            slimeDeath.transform.SetParent(null);
            float rng = Random.Range(1, 100);
            if (rng < 50) 
            {
                GameObject drop = Instantiate(slimeDrop, transform);
                drop.transform.SetParent(null);
            }
            Destroy(transform.parent.gameObject);
        
        }
    }

    void TakeDamage(int damage, Collider2D collision)
    {
        moveDirection = player.transform.position - collision.transform.position;
        StartCoroutine(cameraShake.Shake(.05f, .05f));
        if (collision.GetComponent<Projectile>())
        {
            collision.GetComponent<Projectile>().currentProjectileState = ProjectileState.HIT;
            transform.parent.GetComponent<Rigidbody2D>().AddForce(moveDirection * -4f, ForceMode2D.Impulse);
            health -= damage;
        }
        if (collision.GetComponent<Bolt>() && collision.GetComponent<Bolt>().currentBoltState != BoltState.STUCK)
        {
            transform.parent.GetComponent<Rigidbody2D>().AddForce(moveDirection * -4f, ForceMode2D.Impulse);
            
            health -= damage;
            if (health > collision.GetComponent<Bolt>().damage)
            {
                collision.transform.parent = transform;
                collision.GetComponent<Bolt>().currentBoltState = BoltState.STUCK;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Projectile")
        {
            if (collision.GetComponent<Bolt>() && collision.GetComponent<Bolt>().currentBoltState == BoltState.FIRED) 
            {
                transform.parent.GetComponent<SpriteRenderer>().color = Color.red;
                StartCoroutine(whitecolor());
                TakeDamage(collision.GetComponent<Bolt>().damage, collision.GetComponent<Collider2D>());
            }
            if (collision.GetComponent<Projectile>())
            {
                transform.parent.GetComponent<SpriteRenderer>().color = Color.red;
                StartCoroutine(whitecolor());
                TakeDamage(collision.GetComponent<Projectile>().damage, collision.GetComponent<Collider2D>());  
            } 
        }
    }
}
