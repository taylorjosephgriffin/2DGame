using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public GameObject dropItemShadow;
    public GameObject dropItemIcon;
    public GameObject dropItemLight;
    public int pickupRadius;
    public float autoPickupSpeedMultiplier = 1;

    public float forceTime = .1f;
    float forceTimer = 0;

    public float disabledTime = .3f;
    float disabledTimer = 0;

    GameObject player;

    Vector3 randomPoint;

    private void Start()
    {
        dropItemShadow.GetComponent<SpriteRenderer>().sprite = item.itemDropShadow;
        dropItemIcon.GetComponent<SpriteRenderer>().sprite = item.itemDropIcon;
        dropItemLight.GetComponent<Light2D>().color = item.itemDropLightColor;
        player = GameObject.FindWithTag("Player");
        Physics2D.IgnoreLayerCollision(12, 14);
        Physics2D.IgnoreLayerCollision(12, 12);
        Physics2D.IgnoreLayerCollision(12, 8);
        randomPoint = Random.insideUnitSphere;
    }

    private void Update()
    {
        float speed;
        float distance = Vector3.Distance(transform.position, player.transform.position);
        forceTimer += Time.deltaTime;
        disabledTimer += Time.deltaTime;

        if (forceTimer < forceTime) {
            GetComponent<Rigidbody2D>().AddForce(randomPoint * 4f, ForceMode2D.Impulse);
        }
        if (disabledTimer > disabledTime) {
            GetComponent<CircleCollider2D>().enabled = true;
            if (distance < pickupRadius && !InventoryManager.instance.InventoryIsFull()) {
                speed = (pickupRadius - distance) * autoPickupSpeedMultiplier;
                transform.position = Vector2.Lerp(transform.position, player.transform.position, speed * Time.deltaTime);
            }
        } else {
            GetComponent<CircleCollider2D>().enabled = false;
        }
    }

    public void Interact()
    {
        InventoryManager.instance.AddItem(item, gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Interact();
        }
    }
}
