using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InputManager : MonoBehaviour
{

    public PlayerInput playerInput;
    public string currentControlScheme { get; private set; }

    void OnEnable() {
        InputUser.onChange += onInputDeviceChange;
    }
 
    void OnDisable() {
        InputUser.onChange -= onInputDeviceChange;
    }
    // Start is called before the first frame update
    void Start()
    {
        currentControlScheme = playerInput.currentControlScheme;
    }

      void onInputDeviceChange(InputUser user, InputUserChange change, InputDevice device) {
        if (change == InputUserChange.ControlSchemeChanged) {
            currentControlScheme = user.controlScheme.Value.name;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
