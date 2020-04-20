using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Move : MonoBehaviour
{
    private GameObject player;
    private GameObject playerSprite;
    [SerializeField]
    private float normalSpeed;
    [SerializeField]
    private float runSpeed;
    float currentSpeed;
    /// <summary>
     /// Dodge distance/speed that gets processed every frame. Might wanna keep this low! 
     /// </summary>
     private float _dodgeSpeed = 60000;
     
     /// <summary>
     /// How long dodges last.
     /// </summary>
     private float _dodgeTime = 0.5f; 
     
     /// <summary>
     /// Reference to the dodging coroutine.
     /// </summary>
     private Coroutine _dodging; 
     bool canDash = true;

     Vector2 inputDirection;

    Rigidbody2D rb;

        PlayerControls controls;
    // Start is called before the first frame update

    GameObject cursor;
    Vector3 cursorPosition;
    bool isRunning;
    private void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.Enable();
        cursor = GameObject.Find("Cursor");
        cursorPosition = new Vector2(cursor.transform.position.x, cursor.transform.position.y);
        controls.Gameplay.Move.performed += ctx => inputDirection = ctx.ReadValue<Vector2>();
        controls.Gameplay.Run.performed += ctx => isRunning = true;
        controls.Gameplay.Run.canceled += ctx => isRunning = false;
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerSprite = player.transform.Find("Sprite").gameObject;
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.E))
        //  {
        //      StartCoroutine(DodgeCoroutine());
        //  }
    }
    
    // public IEnumerator DodgeCoroutine ()
    // {
    //     Vector3 mousePosition = Utils.GetMouseWorldPosition();
    //     Vector3 aimDirection = (mousePosition - transform.position).normalized;
    //     float time = 0; //create float to store the time this coroutine is operating
    //     canDash = false; //set canBoost to false so that we can't keep boosting while boosting
 
    //     while(_dodgeTime > time) //we call this loop every frame while our custom boostDuration is a higher value than the "time" variable in this coroutine
    //     {
    //         time += Time.deltaTime; //Increase our "time" variable by the amount of time that it has been since the last update
    //         rb.velocity = aimDirection * 20; //set our rigidbody velocity to a custom velocity every frame, so that we get a steady boost direction like in Megaman
    //         yield return 0; //go to next frame
    //     }
    //      yield return new WaitForSeconds(_dodgeTime); //Cooldown time for being able to boost again, if you'd like.
    //      canDash = true; //set back to true so that we can boost again.
    // }

    void FixedUpdate()
    {
        
   
        float step = currentSpeed * Time.deltaTime;
        Vector3 aimDirection = (cursor.transform.position - transform.position).normalized;
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

        if (canDash)
        {
            rb.MovePosition(rb.position + new Vector2(inputDirection.x, inputDirection.y) * currentSpeed * Time.fixedDeltaTime);   
        }

        if (inputDirection.x < 0 || inputDirection.y < 0) {
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
        else if (inputDirection.x > 0 || inputDirection.y > 0)
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
}