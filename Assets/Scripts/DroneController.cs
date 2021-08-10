using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    public enum EnemyState { ACTIVE, DEAD, IDLE, ATTACKING, CHASING}
 private ShakeBehavior cameraShake;
    public EnemyState currentEnemyState = EnemyState.IDLE;
    public GameObject deathParticle;
    public GameObject slimeDrop;
    public AudioClip [] slimeSounds;
    public int damage = 5;
    public int health;
    public float speed;
    public EnemySpawnGroup spawnGroup;
    public Vector3 moveDirection;
    AudioSource audioSource;
    Freezer freezer;
    float playerCollisionCooldown = 0f;

    GameObject player;

    bool isNavigating = false;

    Vector2 randomNavPoint;
    float idleWaitTime = 2f;
    float timeSpentNavigating;
    [HideInInspector]
    public Collider2D lastCollision;
    public GameObject lootTableObject;
    LootTable lootTable;

    public ParticleSystem bulletEmitter;

    public SpriteRenderer enemySpriteRenderer;
    Vector3 _origPos;

    public Animator[] droneAnimators;
    public AudioClip droneWindDown;
    public AudioClip droneLowSpeedAudio;
    public AudioClip dronePowerUp;
    public AudioClip droneSoundNormal;
    public GameObject droneProjectile;

    bool isAttacking = false;

    void Start()
    {
        freezer = GameObject.Find("GameManagers").GetComponent<Freezer>();
        cameraShake = GameObject.FindWithTag("MainCamera").GetComponent<ShakeBehavior>();
        player = GameObject.Find("Player");
        audioSource = GetComponent<AudioSource>();
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        currentEnemyState = EnemyState.IDLE;
        randomNavPoint = spawnGroup.GetNavigationPointWithinSpawnRadius();
        lootTable = lootTableObject.GetComponent<LootTable>();
        Physics2D.IgnoreLayerCollision(11, 12);
    }

    IEnumerator HitColorChange()
    {
        enemySpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.05f);
        enemySpriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        enemySpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.05f);
        enemySpriteRenderer.color = Color.white;
    }

    protected void IdleState() {

    }


    // Update is called once per frame
    void Update()
    {
        ParticleSystem.EmissionModule bulletEmitterModule = bulletEmitter.emission;
        playerCollisionCooldown -= Time.deltaTime;
        if (health <= 0) {
            currentEnemyState = EnemyState.DEAD;
            bulletEmitterModule.enabled = false;
        } else if (Vector3.Distance(transform.position, player.transform.position) < 7 && currentEnemyState != EnemyState.DEAD) {
            currentEnemyState = EnemyState.ATTACKING;
            bulletEmitterModule.enabled = true;
        } else if (Vector3.Distance(transform.position, player.transform.position) < 10 && currentEnemyState != EnemyState.DEAD) {
            currentEnemyState = EnemyState.CHASING;
            bulletEmitterModule.enabled = false;
            foreach (Animator animator in droneAnimators) {
                animator.SetBool("IsAttacking", false);
            }
            isAttacking = false;
        } else {
            foreach (Animator animator in droneAnimators) {
                animator.SetBool("IsAttacking", false);
            }
            bulletEmitterModule.enabled = false;
            isAttacking = false;
            currentEnemyState = EnemyState.IDLE;
        }
        switch (currentEnemyState) {
            case EnemyState.IDLE:
                StartCoroutine(droneAudioIdle());
                if (Vector2.Distance((Vector2)transform.position, randomNavPoint) < 0.2f || timeSpentNavigating > 7) {
                    if (idleWaitTime <= 0) {
                        randomNavPoint = spawnGroup.GetNavigationPointWithinSpawnRadius();
                        idleWaitTime = 5;
                        timeSpentNavigating = 0f;
                    } else {

                        idleWaitTime -= Time.deltaTime;
                    }
                    bulletEmitterModule.enabled = false;
                }
                else {
                    Vector2 wanderMovement = Vector2.MoveTowards(transform.position, randomNavPoint, speed * Time.deltaTime);
                    transform.position = wanderMovement;
                    timeSpentNavigating += Time.deltaTime;
                }
                break;
            case EnemyState.CHASING: 
                Vector2 activeMovement = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
                transform.position = activeMovement;
                foreach (Animator animator in droneAnimators) {
                    animator.SetBool("IsAttacking", false);
                }
                bulletEmitterModule.enabled = false;
                break;
            case EnemyState.ATTACKING:
                 if (!isAttacking) {
                    isAttacking = true;
                    droneAudioAttacking();
                    foreach (Animator animator in droneAnimators) {
                        animator.SetBool("IsAttacking", true);
                    }
                    bulletEmitter.Play();
                    bulletEmitterModule.enabled = true;
                    InsantiateProjectiles();
                }
                
                break;
            case EnemyState.DEAD:
                OnDeath();
                bulletEmitterModule.enabled = false;
                break;
        }
    }

    void InsantiateProjectiles() {
        int RaysToShoot = 5;
        float angle = 0;
        for (int i = 0; i < RaysToShoot; i++) {
            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);
            angle += 2 * Mathf.PI / RaysToShoot;
        
            Vector3 dir = new Vector3(transform.position.x + x, transform.position.y + y, 0);
            GameObject newProjectile = Instantiate(droneProjectile, transform);
            newProjectile.transform.right = dir;
        }
    }
    
    /*

      private int RaysToShoot = 30;
        float angle = 0;
        for (int i = 0; i<5; i++) {
            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);
            angle += 2 * Mathf.PI / RaysToShoot;
        
            Vector3 dir = new Vector3(transform.position.x + x, transform.position.y + y, 0);
            RaycastHit hit;
            Debug.DrawLine (transform.position, dir, Color.red);
        }



    */

    void OnDeath()
    {
        GameObject death = Instantiate(deathParticle, transform);
        
        freezer.Freeze();
        death.transform.SetParent(null);
        lootTableObject.transform.SetParent(null);
        lootTable.enemyCollisionPosition = lastCollision.transform.position;
        lootTable.GenerateAndInstaniateLoot();
        Destroy(transform.gameObject);
    }

    void PlayJumpingAudio()
    {
        audioSource.clip = slimeSounds[Random.Range(0, slimeSounds.Length)];
        audioSource.Play();
    }

    IEnumerator droneAudioIdle() {
        audioSource.clip = dronePowerUp;
        yield return new WaitForSeconds(dronePowerUp.length);
        audioSource.clip = droneSoundNormal;
    }

    IEnumerator droneAudioAttacking() {
        audioSource.clip = droneWindDown;
        yield return new WaitForSeconds(droneWindDown.length);
        audioSource.clip = droneLowSpeedAudio;
    }

    IEnumerator FreezeAnimFrame()
    {
        droneAnimators[0].SetFloat("Speed", 0);
        yield return new WaitForSeconds(.2f);
        droneAnimators[0].SetFloat("Speed", 1);
    }

    void TakeDamage(int damage, Collider2D collision)
    {
        moveDirection = player.transform.position - lastCollision.transform.position;
        StartCoroutine(cameraShake.Shake(.05f, .05f));
        if (damage < health) StartCoroutine(FreezeAnimFrame());
        collision.GetComponent<Projectile>().currentProjectileState = ProjectileState.HIT;
        transform.GetComponent<Rigidbody2D>().AddForce(moveDirection * -4f, ForceMode2D.Impulse);
        health -= damage;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player" && playerCollisionCooldown <= 0) HandlePlayerCollision(collision);
    }

    void HandlePlayerCollision(Collision2D player) {
        playerCollisionCooldown = 1f;
        player.transform.GetComponent<Move>().TakeDamage(damage, transform);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Projectile") HandleProjectileHit(collision);
    }

    void HandleProjectileHit(Collider2D projectile) {
        lastCollision = projectile;
        enemySpriteRenderer.color = Color.red;
        StartCoroutine(HitColorChange());
        TakeDamage(projectile.GetComponent<Projectile>().damage, projectile.GetComponent<Collider2D>());
    }
}
