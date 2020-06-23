using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Items/Weapons")]
public class Weapon : Item
{
    public int clipSize;
    public float reloadTime;
    public float bulletSpeed;
    public int damage;
    public GameObject muzzleFlash;
    public float fireCooldown;
    public GameObject projectile;
    public Sprite weaponSprite;
    public AudioClip fireSound;
    public Vector3 projectileSpawnLocation;
    public bool hasLaserSight;
    public Vector3 laserSightAnchor;

    public Vector3 leftHandAnchor;
    public Vector3 rightHandAnchor;
    
}
