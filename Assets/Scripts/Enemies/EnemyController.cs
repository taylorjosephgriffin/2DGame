using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState { ACTIVE, DEAD, IDLE }

public class EnemyController : MonoBehaviour
{
  private ShakeBehavior cameraShake;
  public EnemyState currentEnemyState = EnemyState.IDLE;
  public GameObject deathParticle;
  public GameObject slimeDrop;
  public AudioClip[] slimeSounds;
  public int damage = 5;
  public int health;
  public float speed;
  public EnemySpawnGroup spawnGroup;
  public Vector3 moveDirection;
  AudioSource audioSource;
  Freezer freezer;
  float playerCollisionCooldown = 0f;
  [HideInInspector]
  public GameObject player;

  bool isNavigating = false;

  Vector2 randomNavPoint;
  float idleWaitTime = 2f;
  float timeSpentNavigating;
  [HideInInspector]
  public Collider2D lastCollision;
  public GameObject lootTableObject;
  LootTable lootTable;

  public SpriteRenderer enemySpriteRenderer;
  Vector3 _origPos;

  void Start()
  {
    freezer = GameObject.Find("GameManagers").GetComponent<Freezer>();
    cameraShake = GameObject.FindWithTag("MainCamera").GetComponent<ShakeBehavior>();
    player = GameObject.FindWithTag("Player");
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

  protected void IdleState()
  {

  }


  // Update is called once per frame
  void Update()
  {
    playerCollisionCooldown -= Time.deltaTime;
    Animator animator = transform.GetComponent<Animator>();
    if (health <= 0) currentEnemyState = EnemyState.DEAD;
    if (Vector3.Distance(transform.position, player.transform.position) < 7 && currentEnemyState != EnemyState.DEAD)
    {
      currentEnemyState = EnemyState.ACTIVE;
    }
    switch (currentEnemyState)
    {
      case EnemyState.IDLE:
        if (Vector2.Distance((Vector2)transform.position, randomNavPoint) < 0.2f || timeSpentNavigating > 7)
        {
          if (idleWaitTime <= 0)
          {
            randomNavPoint = spawnGroup.GetNavigationPointWithinSpawnRadius();
            idleWaitTime = 5;
            timeSpentNavigating = 0f;
          }
          else
          {
            animator.SetFloat("IdleSpeed", 1);
            idleWaitTime -= Time.deltaTime;
          }
        }
        else
        {
          animator.SetBool("IsChasing", false);
          animator.SetFloat("IdleSpeed", 1.5f);
          Vector2 wanderMovement = Vector2.MoveTowards(transform.position, randomNavPoint, speed * Time.deltaTime);
          transform.position = wanderMovement;
          timeSpentNavigating += Time.deltaTime;
        }
        break;
      case EnemyState.ACTIVE:
        animator.SetBool("IsChasing", true);
        animator.SetFloat("IdleSpeed", 1.5f);
        Vector2 activeMovement = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
        transform.position = activeMovement;

        break;
      case EnemyState.DEAD:
        OnDeath();
        break;
    }
  }

  void OnDeath()
  {
    GameObject death = Instantiate(deathParticle, transform);
    moveDirection = player.transform.position - lastCollision.transform.position;
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

  IEnumerator FreezeAnimFrame()
  {
    Animator animator = transform.GetComponent<Animator>();
    animator.SetFloat("Speed", 0);
    yield return new WaitForSeconds(.2f);
    animator.SetFloat("Speed", 1);
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

  void HandlePlayerCollision(Collision2D player)
  {
    playerCollisionCooldown = 1f;
    player.transform.GetComponent<Move>().TakeDamage(damage, transform);
  }

  private void OnTriggerEnter2D(Collider2D collision)
  {
    if (collision.gameObject.tag == "Projectile") HandleProjectileHit(collision);
  }

  void HandleProjectileHit(Collider2D projectile)
  {
    lastCollision = projectile;
    enemySpriteRenderer.color = Color.red;
    StartCoroutine(HitColorChange());
    TakeDamage(projectile.GetComponent<Projectile>().damage, projectile.GetComponent<Collider2D>());
  }
}
