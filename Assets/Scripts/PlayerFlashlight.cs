using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class PlayerFlashlight : MonoBehaviour
{
  public bool flashlightIsOn = true;
  Light2D playerLight;
  PlayerControls controls;
  // Start is called before the first frame update
  void Awake()
  {
    controls = new PlayerControls();
    controls.Gameplay.Enable();
    controls.Gameplay.Flashlight.performed += ctx => ToggleFlashlight();
    playerLight = GetComponent<Light2D>();
    playerLight.transform.gameObject.SetActive(flashlightIsOn);
  }

  void ToggleFlashlight()
  {
    flashlightIsOn = !flashlightIsOn;
    playerLight.transform.gameObject.SetActive(flashlightIsOn);
  }

  // Update is called once per frame
  void Update()
  {

  }
}
