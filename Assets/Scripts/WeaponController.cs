using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public GameObject Player;

    private void LateUpdate()
    {
        transform.GetComponent<Renderer>().sortingOrder = Player.GetComponent<Renderer>().sortingOrder + 1;
    }
}
