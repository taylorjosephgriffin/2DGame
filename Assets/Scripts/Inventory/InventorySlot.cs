using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public RectTransform iconTransform;
    public Item item;

    public enum slotTypeEnum {
        INVENTORY,
        EQUIPMENT,
        SHOP,
    }

    public slotTypeEnum slotType;
    public int quantity;

    public Text itemQuantityTextTooltip;

    public void UpdateQuantityUI()
    {
        if (quantity > 1) itemQuantityTextTooltip.text = "X" + quantity.ToString();
    }

    public void AddItem(Item newItem)
    {
        icon.overrideSprite = newItem.itemIcon;
        iconTransform.localScale = new Vector3(newItem.inventoryIconScale, newItem.inventoryIconScale, newItem.inventoryIconScale);
        item = newItem;
    }

    public void RemoveItem()
    {
        icon.overrideSprite = null;
        item = null;
    }
}
