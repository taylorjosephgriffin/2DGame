using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [HideInInspector]
    public InventorySlot[] slots;
    public InventoryManager inventoryManager;

    public InventorySlot selectedSlot;

    public GameObject contextMenu;

    // Start is called before the first frame update
    
    public void Init()
    {
        slots = GetComponentsInChildren<InventorySlot>();
        InventoryManager.instance.onItemAddCallback += UpdateInventoryAdd;
        InventoryManager.instance.onItemRemoveCallback += UpdateInventoryRemove;
        SelectFirstSlot();
    }


    void SelectFirstSlot() {
        foreach (InventorySlot slot in slots) {
            if (slot.name == EventSystem.current.firstSelectedGameObject.name)
            {
                slot.GetComponent<Button>().Select();
                slot.GetComponent<Button>().OnSelect(null);
                if (slot.item != null) {
                    inventoryManager.itemDescriptionTooltip.text = slot.item.itemDescription;
                    inventoryManager.itemNameTooltip.text = slot.item.itemName;
                }
            }
        }
    }

    private void OnEnable()
    {
        SelectFirstSlot();
    }

    private void Update()
    {
        if (selectedSlot != null) {
            contextMenu.SetActive(true);
            contextMenu.transform.SetParent(selectedSlot.transform);
            contextMenu.GetComponent<RectTransform>().localPosition = new Vector3(400, 100, 0);
        }
    }

    private int? GetNextEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == null) return i;
        } 
        return null;
    }

    private int? GetMatchingSlot(Item newItem)
    {
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slots[i].item == newItem) return i;
        } 
        return null;
    }

    public void UpdateInventoryAdd(Item newItem)
    {

        int? matchingItemIndex = GetMatchingSlot(newItem);
        int? emptyItemIndex = GetNextEmptySlot();
    
        if (GetMatchingSlot(newItem) == null || slots[(int)matchingItemIndex].quantity == newItem.stackLimit)
        {
            InventorySlot emptySlot = slots[(int)emptyItemIndex];
            emptySlot.AddItem(newItem);
            emptySlot.quantity = 1;

        } else {
            InventorySlot matchingSlot = slots[(int)matchingItemIndex];
            matchingSlot.quantity += 1;
            matchingSlot.UpdateQuantityUI();

        }
    }

    public void UpdateInventoryRemove(Item newItem)
    {
        slots[(int)GetMatchingSlot(newItem)].RemoveItem();
    }
}
