using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState { ACTIVE, DEAD, IDLE}

public class EnemyController : MonoBehaviour
{
    private ShakeBehavior cameraShake;
    public EnemyState currentEnemyState = EnemyState.IDLE;
    public GameObject deathParticle;
    public GameObject slimeDrop;
    public AudioClip [] slimeSounds;
    public int damage = 5;
    public int health;
    public float speed;
    public EnemySpawnGroup spawnGroup;
    Vector3 moveDirection;
    AudioSource slimeAudioSource;
    Freezer freezer;
    float playerCollisionCooldown = 0f;
    GameObject player;

    bool isNavigating = false;

    Vector2 randomNavPoint;
    float idleWaitTime = 2f;
    float timeSpentNavigating;
    Collider2D lastCollision;

    void Start()
    {
        freezer = GameObject.Find("GameManager").GetComponent<Freezer>();
        cameraShake = GameObject.FindWithTag("MainCamera").GetComponent<ShakeBehavior>();
        player = GameObject.FindWithTag("Player");
        slimeAudioSource = GetComponent<AudioSource>();
        currentEnemyState = EnemyState.IDLE;
        randomNavPoint = spawnGroup.GetNavigationPointWithinSpawnRadius();
    }

    IEnumerator HitColorChange()
    {
        transform.GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.05f);
        transform.GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(0.05f);
        transform.GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.05f);
        transform.GetComponent<SpriteRenderer>().color = Color.white;
    }


    // Update is called once per frame
    void Update()
    {
        playerCollisionCooldown -= Time.deltaTime;
        Animator slimeAnimator = transform.GetComponent<Animator>();
        if (health <= 0) currentEnemyState = EnemyState.DEAD;
        switch (currentEnemyState) {
            case EnemyState.IDLE:
                if (Vector2.Distance((Vector2)transform.position, randomNavPoint) < 0.2f || timeSpentNavigating > 7) {
                    if (idleWaitTime <= 0) {
                        randomNavPoint = spawnGroup.GetNavigationPointWithinSpawnRadius();
                        idleWaitTime = 5;
                        timeSpentNavigating = 0f;
                    } else {
                        slimeAnimator.SetFloat("IdleSpeed", 1);
                        idleWaitTime -= Time.deltaTime;
                    }
                }
                else {
                    slimeAnimator.SetBool("IsChasing", false);
                    slimeAnimator.SetFloat("IdleSpeed", 1.5f);
                    Vector2 movement = Vector2.MoveTowards(transform.position, randomNavPoint, speed * Time.deltaTime);
                    transform.position = movement;
                    timeSpentNavigating += Time.deltaTime;
                }
                break;
            case EnemyState.ACTIVE:
                if (Vector3.Distance(transform.position, player.transform.position) < 7) {
                    slimeAnimator.SetBool("IsChasing", false);
                    slimeAnimator.SetFloat("IdleSpeed", 1.5f);
                    Vector2 movement = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
                    transform.position = movement;
                }
                break;
            case EnemyState.DEAD:
                OnDeath();
                break;
        }
    }

    void OnDeath()
    {
        GameObject slimeDeath = Instantiate(deathParticle, transform);
        moveDirection = player.transform.position - lastCollision.transform.position;
        freezer.Freeze();
        slimeDeath.transform.SetParent(null);
        float rng = Random.Range(1, 100);
        if (rng < 50) {
            GameObject drop = Instantiate(slimeDrop, transform);
            drop.transform.SetParent(null);
            drop.GetComponent<Rigidbody2D>().AddForce(moveDirection * -2f, ForceMode2D.Impulse);
        }
        Destroy(transform.gameObject);
    }

    void PlayJumpingAudio()
    {
        slimeAudioSource.clip = slimeSounds[Random.Range(0, slimeSounds.Length)];
        slimeAudioSource.Play();
    }

    IEnumerator FreezeAnimFrame()
    {
        Animator slimeAnimator = transform.GetComponent<Animator>();
        slimeAnimator.SetFloat("Speed", 0);

        yield return new WaitForSeconds(.2f);

        slimeAnimator.SetFloat("Speed", 1);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 7);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(new Vector3(spawnGroup.spawnLocation.x, spawnGroup.spawnLocation.y, 0), spawnGroup.spawnGroupIdleRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(randomNavPoint, 1);
    }

    void TakeDamage(int damage, Collider2D collision)
    {
        moveDirection = player.transform.position - lastCollision.transform.position;
        StartCoroutine(cameraShake.Shake(.05f, .05f));
        if (damage < health) StartCoroutine(FreezeAnimFrame());
        if (collision.GetComponent<Projectile>()) {
            collision.GetComponent<Projectile>().currentProjectileState = ProjectileState.HIT;
            transform.GetComponent<Rigidbody2D>().AddForce(moveDirection * -4f, ForceMode2D.Impulse);
            health -= damage;
        }
        if (collision.GetComponent<Bolt>() && collision.GetComponent<Bolt>().currentBoltState != BoltState.STUCK) {
            transform.GetComponent<Rigidbody2D>().AddForce(moveDirection * -4f, ForceMode2D.Impulse);
            
            health -= damage;
            if (health > collision.GetComponent<Bolt>().damage) {
                collision.transform.parent = transform;
                collision.GetComponent<Bolt>().currentBoltState = BoltState.STUCK;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player" && playerCollisionCooldown <= 0) {
            playerCollisionCooldown = 1f;
            collision.transform.GetComponent<Move>().TakeDamage(5, transform);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Projectile") {
            lastCollision = collision;
            transform.GetComponent<SpriteRenderer>().color = Color.red;
            StartCoroutine(HitColorChange());
            TakeDamage(collision.GetComponent<Projectile>().damage, collision.GetComponent<Collider2D>());  
        }
    }
}
