using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaponController : MonoBehaviour
{
    public Transform projectileAnchor;
    public Transform laserSightAnchor;
    float cooldown;
    private ShakeBehavior cameraShake;
    public class FireEvent : UnityEvent {}
    public FireEvent fireEvent = new FireEvent();
    public GameObject reloadingText;
    PlayerControls controls;
    // Start is called before the first frame update

    Animator gunAnimator;
    public Weapon weapon;
    public int currentBulletsInClip;
    public bool isReloading = false;
    SpriteRenderer gunSpriteRenderer;
    AudioSource gunAudioSource;
    ParticleSystem muzzleFlashParticleSystem;
    public GameObject leftHand;
    public GameObject rightHand;
    
    private void Awake()
    {
        cameraShake = GameObject.FindWithTag("MainCamera").GetComponent<ShakeBehavior>();
        cooldown = weapon.fireCooldown;
        SetupControls();
        SetupGun();
        SetupAnchors();
    }


    void SetupAnchors() {
        projectileAnchor.localPosition = weapon.projectileSpawnLocation;
        laserSightAnchor.localPosition = weapon.laserSightAnchor;
        leftHand.transform.localPosition = weapon.leftHandAnchor;
        rightHand.transform.localPosition = weapon.rightHandAnchor;
        if (weapon.hasLaserSight) laserSightAnchor.transform.gameObject.SetActive(true);
    }

    void SetupControls() {
        controls = new PlayerControls();
        controls.Gameplay.Enable();
        controls.Gameplay.Fire1.performed += ctx => Attack();
    }

    void SetupGun() {
        currentBulletsInClip = weapon.clipSize;
        gunAnimator = transform.GetComponent<Animator>();
        muzzleFlashParticleSystem = weapon.muzzleFlash.GetComponent<ParticleSystem>();
        SetupGunAudio();
        SetupGunSprite();
    }

    void SetupGunSprite() {
        gunSpriteRenderer = transform.GetComponent<SpriteRenderer>();
        gunSpriteRenderer.sprite = weapon.weaponSprite;
    }

    void SetupGunAudio() {
        gunAudioSource = transform.GetComponent<AudioSource>();
        gunAudioSource.clip = weapon.fireSound;
    }

    IEnumerator Reload() {
        isReloading = true;
        reloadingText.SetActive(true);
        yield return new WaitForSeconds(weapon.reloadTime);
        isReloading = false;
        reloadingText.SetActive(false);
        currentBulletsInClip = weapon.clipSize;
    }

    void Update()
    {
        bool shouldReload = currentBulletsInClip == 0 && !isReloading;

        StartCooldown();
        if (shouldReload) StartCoroutine(Reload());
    }

    void InstantiateProjectile() {
        GameObject newProjectile = Instantiate(weapon.projectile);
        Projectile newProjectileScript = newProjectile.GetComponent<Projectile>();
        newProjectile.transform.position = projectileAnchor.transform.position;
        newProjectile.transform.rotation = projectileAnchor.transform.rotation;
        newProjectileScript.projectileSpeed = weapon.bulletSpeed;
        newProjectileScript.currentProjectileState = ProjectileState.FIRED;
        newProjectileScript.damage = weapon.damage;
    }

    void StartCameraShake() {
        StartCoroutine(cameraShake.Shake(.05f, .05f));
    }

    IEnumerator TriggerRecoilAnim() {
        gunAnimator.SetTrigger("Recoil");
        yield return new WaitForSeconds(1f);
        gunAnimator.ResetTrigger("Recoil");
    }

    void InvokeFireEvent() {
        fireEvent.Invoke();
    }

    void StartCooldown() {
        cooldown -= Time.deltaTime;
    }

    void ResetCooldown() {
        cooldown = weapon.fireCooldown;
    }

    public void Attack()
    {
        bool cannotFire = cooldown <= 0 && !PauseManager.isPaused && !isReloading;

        if (cannotFire)
        {
            ResetCooldown();
            currentBulletsInClip--;
            StartCameraShake();
            gunAudioSource.Play();
            InstantiateProjectile();
            muzzleFlashParticleSystem.Play();
            InvokeFireEvent();
            StartCoroutine(TriggerRecoilAnim());
        }
    }
}
