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

    public GameObject player;

    public MouseCursor cursor;
    
    public PlayerAimWeapon playerAim;

    public GameObject currentHitObject;

    public LayerMask layerMask;

    Vector3 origin;
    Vector3 origin2;

    Vector3 origin3;

    Vector3 direction;


    public int sphereRadius;

    private float currentHitDistance;

    public float maxDistance;

    Vector3 currentHitLocation;

    [Range(0, 100)]
    public int aimAssistAmount;

    Vector3 raycastHitPoint;
    
    private void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.Enable();
        controls.Gameplay.Fire1.performed += ctx => Attack();
        cameraShake = GameObject.FindWithTag("MainCamera").GetComponent<ShakeBehavior>();
    }

    private void Start()
    {

    }

    private void GetPlayerAttackedID()
    {
    
        
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.white;
    //     Gizmos.DrawLine(origin, direction);
    //     Gizmos.DrawLine(origin2, direction + (projectileAnchor.transform.up * (aimAssistAmount * .01f)));
    //     Gizmos.DrawLine(origin3, direction - (projectileAnchor.transform.up * (aimAssistAmount * .01f)));
      
    // }


    void Update()
    {

        cooldown -= Time.deltaTime;


    }
    public void Attack()
    {
        if (cooldown <= 0 && !PauseManager.isPaused)
        {
            cooldown = .2f;
            GetPlayerAttackedID();
            StartCoroutine(cameraShake.Shake(.05f, .05f));
            muzzleFlash.GetComponent<ParticleSystem>().Play();
            gunAnimator.SetTrigger("Recoil");
            newProjectile = Instantiate(projectile);
            newProjectile.transform.position = projectileAnchor.transform.position;
            newProjectile.transform.rotation = projectileAnchor.transform.rotation;
            // newProjectile.GetComponent<Projectile>().aimDirection = currentHitLocation;
            newProjectile.GetComponent<Projectile>().currentProjectileState = ProjectileState.FIRED;
            fireEvent.Invoke();
            gunAnimator.ResetTrigger("Recoil");
        }
    }
}
