using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractKeyTooltipRenderer : MonoBehaviour
{
    public GameObject gamepadTooltip;
    public GameObject keyboardTooltip;

    GameObject currentTooltip;
    Animator tooltipAnimator;
    
    public InputManager inputManager;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if (inputManager.currentControlScheme == "Gamepad") currentTooltip = gamepadTooltip;
        else currentTooltip = keyboardTooltip;
        GameObject tooltip = Instantiate(currentTooltip);
        tooltipAnimator = tooltip.GetComponent<Animator>();
        tooltip.transform.SetParent(transform);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            tooltipAnimator.SetBool("showTooltip", true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            
            tooltipAnimator.SetBool("showTooltip", false);
        }
    }
}
