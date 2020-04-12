using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Sword : MonoBehaviour
{
    [SerializeField]
    private GameObject projectile;
    [SerializeField]
    private Transform projectileAnchor;
    private GameObject newProjectile;
    float cooldown = .2f;
    private ShakeBehavior cameraShake;
    [Serializable]
    public class FireEvent : UnityEvent {}
    public FireEvent fireEvent = new FireEvent();
    void Start()
    {
        cameraShake = GameObject.FindWithTag("MainCamera").GetComponent<ShakeBehavior>();
    }

    void Update()
    {
        cooldown -= Time.deltaTime;
        if (Input.GetButtonDown("Fire1") && cooldown <= 0)
        {
            Attack();
        }

    }
    private void Attack()
    {
        cooldown = .2f;
        newProjectile = Instantiate(projectile);
        newProjectile.transform.position = projectileAnchor.transform.position;
        newProjectile.transform.rotation = projectileAnchor.transform.rotation;
        transform.GetComponent<Animator>().SetTrigger("Attack");
        newProjectile.GetComponent<Projectile>().currentProjectileState = ProjectileState.FIRED;
        StartCoroutine(cameraShake.Shake(.05f, .05f));
        fireEvent.Invoke();
    }
}
