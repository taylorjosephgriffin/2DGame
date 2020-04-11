using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Move : MonoBehaviour
{
    private GameObject player;
    private GameObject playerSprite;
    [SerializeField]
    private float speed;

    Rigidbody2D rb;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerSprite = player.transform.Find("Sprite").gameObject;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float step = speed * Time.deltaTime;

        player.transform.position = Vector3.Lerp(player.transform.position, new Vector3(player.transform.position.x + x, player.transform.position.y + y, 0), step);


        Vector3 mousePosition = Utils.GetMouseWorldPosition();
        Vector3 aimDirection = (mousePosition - transform.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        Vector3 a = Vector3.one;
        if (angle > 90 || angle < -90)
        {
            playerSprite.transform.GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            playerSprite.transform.GetComponent<SpriteRenderer>().flipX = true;
        }

        if (x < 0 || y < 0) {
            playerSprite.transform.GetComponent<Animator>().SetBool("IsWalking", true);
            if (Input.GetButton("Run"))
            {
                playerSprite.transform.GetComponent<Animator>().SetFloat("Speed", 1.5f);
                speed = 5.5f;
            }
            else
            {
                playerSprite.transform.GetComponent<Animator>().SetFloat("Speed", 1);
                speed = 3;
            }
        }
        else if (x > 0 || y > 0)
        {
            playerSprite.transform.GetComponent<Animator>().SetBool("IsWalking", true);
            if (Input.GetButton("Run"))
            {
                playerSprite.transform.GetComponent<Animator>().SetFloat("Speed", 1.5f);
                speed = 5.5f;
            }
            else
            {
                playerSprite.transform.GetComponent<Animator>().SetFloat("Speed", 1);
                speed = 3;
            }
        }
        else
        {
           playerSprite.transform.GetComponent<Animator>().SetBool("IsWalking", false);
        }

    }
}