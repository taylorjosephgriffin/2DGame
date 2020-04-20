using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCursor : MonoBehaviour
{
    private SpriteRenderer rend;
    public Sprite cursor;
    public Sprite cursorActive;

    public Vector3 startPos;
    public Sprite cursorInactive;
    public float sensitivity = 2f;
    public currentCursorEnum currentCursor = currentCursorEnum.Normal;

    public Vector3 cursorPosition;
    Vector3 inputDirection;
    GameObject player;
    
    public enum currentCursorEnum
    {
        Active,
        Inactive,
        Normal,
    }
    PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.Enable();
        player = GameObject.FindGameObjectWithTag("Player");
        controls.Gameplay.Aim.performed += ctx => inputDirection = ctx.ReadValue<Vector2>();
    }
    void Start()
    {
        Cursor.visible = false;
        rend = GetComponent<SpriteRenderer>();
        transform.position = player.transform.position;
    }

    private void FixedUpdate()
    {
        // if (Vector3.Distance(player.transform.position, cursorPosition) < 60)
        // {
        //     transform.Translate(inputDirection * sensitivity * Time.fixedDeltaTime, Space.World);
        //     cursorPosition = transform.position;
        // }
        Vector3 movement = new Vector3(inputDirection.x, inputDirection.y, 0);
        Vector3 center = player.transform.position;
        Vector3 newPos = transform.position + movement;
        Vector3 offset = newPos - center;

        transform.position = center + Vector3.ClampMagnitude(offset, 7);

        Debug.Log(inputDirection);
    
    }

    // Update is called once per frame
    void Update()
    {
        if (currentCursor == currentCursorEnum.Active)
        {
            rend.sprite = cursorActive;
        }
        if (currentCursor == currentCursorEnum.Inactive)
        {
            rend.sprite = cursorInactive;
        }
        if (currentCursor == currentCursorEnum.Normal)
        {
            rend.sprite = cursor;
        }
    }
}
