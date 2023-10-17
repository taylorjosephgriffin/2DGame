using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Move : MonoBehaviour
{
  private GameObject player;
  public GameObject playerSprite;
  public GameObject walkDust;
  [SerializeField]
  private float normalSpeed;
  [SerializeField]
  private float runSpeed;
  float currentSpeed;
  bool canDash = true;

  Vector2 inputDirection;

  public float footstepTimer = 0f;
  float timer = 0f;

  Rigidbody2D rb;

  PlayerControls controls;

  GameObject cursor;
  Vector3 cursorPosition;
  bool isRunning;
  public float health = 100;

  public float maxHealth = 100;
  private ShakeBehavior shake;
  public Transform footAnchor;

  public Progress healthBarUI;
  public AudioClip[] footstepSounds;
  private void Awake()
  {
    controls = new PlayerControls();
    controls.Gameplay.Enable();
    cursor = GameObject.Find("MouseCursor");
    cursorPosition = new Vector2(cursor.transform.position.x, cursor.transform.position.y);
    controls.Gameplay.Move.performed += ctx => inputDirection = ctx.ReadValue<Vector2>();
    controls.Gameplay.Run.performed += ctx => isRunning = true;
    controls.Gameplay.Run.canceled += ctx => isRunning = false;
    shake = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ShakeBehavior>();
    //Physics2D.IgnoreLayerCollision(8, 10);
    Physics2D.IgnoreLayerCollision(4, 10);
    Physics2D.IgnoreLayerCollision(8, 14);
  }

  void Start()
  {
    player = GameObject.FindWithTag("Player");
    rb = GetComponent<Rigidbody2D>();
  }
  IEnumerator whitecolor()
  {
    yield return new WaitForSeconds(0.05f);
    playerSprite.GetComponent<SpriteRenderer>().color = Color.white;
  }

  public void TakeDamage(int damage, Transform enemy)
  {
    StartCoroutine(shake.Shake(.05f, .05f));
    playerSprite.GetComponent<SpriteRenderer>().color = Color.red;
    health -= damage;
    healthBarUI.UpdateHealthUI(health);
    StartCoroutine(whitecolor());
    Vector3 force = transform.position - enemy.position;
    force.Normalize();
    rb.AddForce(force * 500);
  }

  IEnumerator RenderWalkDust()
  {
    timer = 0f;
    Instantiate(walkDust, footAnchor.position, footAnchor.rotation);
    yield return new WaitForSeconds(.1f);
    Instantiate(walkDust, footAnchor.position, footAnchor.rotation);
  }

  void FixedUpdate()
  {
    float step = currentSpeed * Time.deltaTime;
    Vector3 aimDirection = (cursor.transform.position - transform.position).normalized;
    bool isMovingNegatively = inputDirection.x < 0 || inputDirection.y < 0;
    bool isMovingPositively = inputDirection.x > 0 || inputDirection.y > 0;
    bool isMoving = isMovingPositively || isMovingNegatively;

    aimDirection.z = 0f;

    float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
    if (angle > 90 || angle < -90)
    {
      playerSprite.transform.GetComponent<SpriteRenderer>().flipX = false;
    }
    else
    {
      playerSprite.transform.GetComponent<SpriteRenderer>().flipX = true;
    }

    rb.MovePosition(rb.position + new Vector2(inputDirection.x, inputDirection.y) * currentSpeed * Time.fixedDeltaTime);


    if (isMoving)
    {
      var audioSource = GetComponent<AudioSource>();
      timer += Time.deltaTime;
      if (timer >= footstepTimer)
      {
        StartCoroutine(RenderWalkDust());
        audioSource.clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        audioSource.Play();
      }
    }

    if (isMovingNegatively)
    {

      playerSprite.transform.GetComponent<Animator>().SetBool("IsWalking", true);
      if (isRunning)
      {
        playerSprite.transform.GetComponent<Animator>().SetFloat("Speed", 1.5f);
        currentSpeed = runSpeed;
      }
      else
      {
        playerSprite.transform.GetComponent<Animator>().SetFloat("Speed", 1);
        currentSpeed = normalSpeed;
      }
    }
    else if (isMovingPositively)
    {
      playerSprite.transform.GetComponent<Animator>().SetBool("IsWalking", true);
      if (isRunning)
      {
        playerSprite.transform.GetComponent<Animator>().SetFloat("Speed", 1.5f);
        currentSpeed = runSpeed;
      }
      else
      {
        playerSprite.transform.GetComponent<Animator>().SetFloat("Speed", 1);
        currentSpeed = normalSpeed;
      }
    }
    else
    {
      playerSprite.transform.GetComponent<Animator>().SetBool("IsWalking", false);
    }
  }

  private void OnTriggerEnter2D(Collider2D other)
  {

  }

  private void OnParticleCollision(GameObject other)
  {

  }
}