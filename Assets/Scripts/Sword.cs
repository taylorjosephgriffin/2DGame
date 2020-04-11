using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public GameObject projectile;
    public Transform projectileAnchor;
    private GameObject newProjectile;
    float cooldown = .2f;
    private ShakeBehavior cameraShake;
    // Start is called before the first frame update
    void Start()
    {
        cameraShake = GameObject.FindWithTag("MainCamera").GetComponent<ShakeBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        cooldown -= Time.deltaTime;
        if (Input.GetButtonDown("Fire1") && cooldown <= 0)
        {
            cooldown = .2f;
            newProjectile = Instantiate(projectile);
            newProjectile.transform.position = projectileAnchor.transform.position;
            newProjectile.transform.rotation = projectileAnchor.transform.rotation;
            transform.GetComponent<Animator>().SetTrigger("Attack");
            newProjectile.GetComponent<Projectile>().currentProjectileState = ProjectileState.FIRED;
            StartCoroutine(cameraShake.Shake(.05f, .05f));
        }

    }
}
