using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCursor : MonoBehaviour
{
    private SpriteRenderer rend;
    public Sprite cursor;
    public Sprite cursorActive;
    public Sprite cursorInactive;
    public currentCursorEnum currentCursor = currentCursorEnum.Normal;
    public enum currentCursorEnum
    {
        Active,
        Inactive,
        Normal,
    }
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        rend = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = cursorPos;

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
