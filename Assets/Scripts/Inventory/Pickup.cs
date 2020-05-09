using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public GameObject inventory;

    InventoryManager inventoryManager;
    public GameObject itemSprite;
    public ItemObject item;

    private void Start()
    {
        inventoryManager = inventory.GetComponent<InventoryManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(transform.gameObject);
        }
    }
}
