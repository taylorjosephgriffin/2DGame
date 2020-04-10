using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAimWeapon : MonoBehaviour
{
    private Transform aimTransform;

    private void Awake()
    {
        aimTransform = transform.Find("Aim");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleAiming();
    }


    private void HandleAiming()
    {
        Vector3 mousePosition = Utils.GetMouseWorldPosition();
        Vector3 aimDirection = (mousePosition - transform.position).normalized;
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
