using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAimWeapon : MonoBehaviour
{
  public Transform aimTransform;
  GameObject cursor;
  Vector3 cursorPosition;
  public Vector3 aimDirection;

  private void Awake()
  {
    aimTransform = transform.Find("Aim");
    cursor = GameObject.Find("MouseCursor");
    cursorPosition = new Vector2(cursor.transform.position.x, cursor.transform.position.y);
  }

  // Update is called once per frame
  void Update()
  {
    if (!PauseManager.isPaused) HandleAiming();
  }


  private void HandleAiming()
  {
    aimDirection = (cursor.transform.position - transform.position).normalized;
    aimDirection.z = 0f;
    float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
    aimTransform.right = aimDirection;

    Vector3 a = Vector3.one;
    if (angle > 90 || angle < -90)
    {
      a.y = -1f;
    }
    else
    {
      a.y = +1f;
    }
    aimTransform.localScale = a;
  }
}
