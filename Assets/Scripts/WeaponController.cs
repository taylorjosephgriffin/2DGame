using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private GameObject Player;

    private void Start()
    {
        Player = GameObject.FindWithTag("Player");
    }
    private void LateUpdate()
    {
        transform.GetComponent<Renderer>().sortingOrder = Player.GetComponent<Renderer>().sortingOrder + 1;
    }
}
