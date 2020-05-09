using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSelector : MonoBehaviour, ISelectHandler
{
    private InventorySlot inventorySlot;

    void Start()
    {
        inventorySlot = GetComponent<InventorySlot>();        
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (inventorySlot.item != null)
        {
            InventoryManager.instance.itemNameTooltip.text = inventorySlot.item.itemName;
            InventoryManager.instance.itemDescriptionTooltip.text = inventorySlot.item.itemDescription;
        }
        else 
        {
            InventoryManager.instance.itemNameTooltip.text = "";
            InventoryManager.instance.itemDescriptionTooltip.text = "";
        }
    }
}
