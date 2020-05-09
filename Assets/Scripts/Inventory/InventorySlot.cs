using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public Item item;

    public int quantity;

    public Text itemQuantityTextTooltip;

    public void UpdateQuantityUI()
    {
        if (quantity > 1) itemQuantityTextTooltip.text = "X" + quantity.ToString();
    }

    public void AddItem(Item newItem)
    {
        icon.overrideSprite = newItem.itemIcon;
        item = newItem;
    }

    public void RemoveItem()
    {
        icon.overrideSprite = null;
        item = null;
    }
}
