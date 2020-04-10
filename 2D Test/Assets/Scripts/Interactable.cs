using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public GameObject Cursor;
    public GameObject Player;
    public int InteractDistance = 4;
    public bool IsInteractable;
    private bool IsHovering;

    private void Start()
    {
        Cursor.GetComponent<MouseCursor>().currentCursor = MouseCursor.currentCursorEnum.Normal;
    }

    private void Update()
    {
        if (IsHovering)
        {
            if (Vector3.Distance(Player.transform.position, transform.position) <= InteractDistance)
            {
                IsInteractable = true;
                Cursor.GetComponent<MouseCursor>().currentCursor = MouseCursor.currentCursorEnum.Active;
            }
            else
            {
                IsInteractable = false;
                Cursor.GetComponent<MouseCursor>().currentCursor = MouseCursor.currentCursorEnum.Inactive;
            }
        }
    }

    private void OnMouseEnter()
    {
        IsHovering = true;
    }

    private void OnMouseExit()
    {
        IsHovering = false;
        Cursor.GetComponent<MouseCursor>().currentCursor = MouseCursor.currentCursorEnum.Normal;
    }
}
