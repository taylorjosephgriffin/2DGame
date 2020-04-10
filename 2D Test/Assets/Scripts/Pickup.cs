using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    private InventoryManager inventory;
    public GameObject itemSprite;

    private void Start()
    {
        inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<InventoryManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("entered");
        if (collision.CompareTag("Player"))
        {
            for (int i = 0; i< inventory.slots.Length; i++)
            {
                if (inventory.isFull[i] == false)
                {
                    inventory.isFull[i] = true;
                    inventory.SetColorOfSlot(i, "ADD");
                    GameObject item = Instantiate(itemSprite, inventory.slots[i].transform, false);
                    item.transform.Rotate(new Vector3(0, 0, 45));
                    Destroy(transform.gameObject);
                    break;
                }
            }
        }
    }
}
