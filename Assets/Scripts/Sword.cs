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
    public GameObject muzzleFlash;
    PlayerControls controls;
    // Start is called before the first frame update
    [SerializeField]
    private Animator gunAnimator;
    

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.Enable();
        controls.Gameplay.Fire1.performed += ctx => Attack();
        cameraShake = GameObject.FindWithTag("MainCamera").GetComponent<ShakeBehavior>();
    }
    void Start()
    {
    
    }

    void Update()
    {
        cooldown -= Time.deltaTime;

    }
    public void Attack()
    {
        if (cooldown <= 0)
        {
            cooldown = .2f;
            StartCoroutine(cameraShake.Shake(.05f, .05f));
            muzzleFlash.GetComponent<ParticleSystem>().Play();
            gunAnimator.SetTrigger("Recoil");
            newProjectile = Instantiate(projectile);
            newProjectile.transform.position = projectileAnchor.transform.position;
            newProjectile.transform.rotation = projectileAnchor.transform.rotation;
            transform.GetComponent<Animator>().SetTrigger("Attack");
            newProjectile.GetComponent<Projectile>().currentProjectileState = ProjectileState.FIRED;
            fireEvent.Invoke();
            gunAnimator.ResetTrigger("Recoil");
        }
    }
}
