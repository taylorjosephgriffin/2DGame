using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

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
    public PlayerInput playerInput;

    public float joystickSensitivityMultiplier = 1f;

    Vector3 lastCursorPosition;

    
    public enum currentCursorEnum
    {
        Active,
        Inactive,
        Normal,
    }
    PlayerControls controls;

    public InputManager inputManager;
    private void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.Enable();
        player = GameObject.FindGameObjectWithTag("Player");
        controls.Gameplay.Aim.performed += ctx => inputDirection = ctx.ReadValue<Vector2>();
        lastCursorPosition = Vector3.zero;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        transform.position = player.transform.position;
    }

    void Update()
    {
        if (!PauseManager.isPaused)
        {
            if (inputManager.currentControlScheme == "Gamepad")
            {
                
                Vector3 movement = new Vector3(inputDirection.x, inputDirection.y, 0);
                Vector3 center = player.transform.position + Vector3.ClampMagnitude(player.transform.position, 2);
                cursorPosition = transform.position + movement * (sensitivity * joystickSensitivityMultiplier);
                Vector3 offset = cursorPosition - center;
                if (inputDirection.x == 0 && inputDirection.y == 0) transform.position = center;
                else transform.position = center + Utils.ClampMagnitudeMinMax(offset, 2, 7);
            }
            else if (inputManager.currentControlScheme == "Keyboard & Mouse")
            {
                Vector3 movement = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
                Vector3 center = player.transform.position;
                cursorPosition = transform.position + movement * sensitivity;
                Vector3 offset = cursorPosition - center;
                transform.position = center + Utils.ClampMagnitudeMinMax(offset, 2, 7);
            }
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
}
