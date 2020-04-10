﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileState { FIRED, HIT };

public class Projectile : MonoBehaviour
{
    public ProjectileState currentProjectileState = ProjectileState.FIRED;
    public float projectileSpeed = 10;
    public float spawnTime = 5;
    public int damage;
    Vector3 mousePosition;
    Vector3 aimDirection;
    // Start is called before the first frame update
     void Start()
    {
        CalculateMousePosition();
    }

    IEnumerator killProjectile()
    {
        transform.GetComponent<Animator>().SetTrigger("AnimDeath");
        yield return new WaitForSeconds(0.25f);
        Destroy(transform.gameObject);
    }

    private void CalculateMousePosition()
    {
        mousePosition = Utils.GetMouseWorldPosition();
        aimDirection = (mousePosition - transform.position);
        aimDirection.z = 0;
        aimDirection.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        spawnTime -= Time.deltaTime;
        if (spawnTime <= 0)
        {
            Destroy(transform.gameObject);
        }
        if (currentProjectileState == ProjectileState.FIRED)
        {
            GetComponent<Rigidbody2D>().velocity = aimDirection * projectileSpeed;
        }
        if (currentProjectileState == ProjectileState.HIT)
        {
            StartCoroutine(killProjectile());
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Pickup"))
        {
            Debug.Log(collision.tag);
            currentProjectileState = ProjectileState.HIT;
        }
    }
}
